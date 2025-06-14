using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Grpc.Net.Client;
using ContactsApi.Grpc; // This namespace contains the generated client types
using ContactsApi;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using Grpc.Core; // For RpcException
using System; // For Uri

namespace ContactsApi.Tests
{
    // IClassFixture ensures the WebApplicationFactory is created once for all tests in this class
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        // Corrected client type references: ContactsServiceClient and LookupsServiceClient are directly under ContactsApi.Grpc
        private readonly ContactsServiceClient _contactsClient;
        private readonly LookupsServiceClient _lookupsClient;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;

            // Create a gRPC client that targets the in-memory test server
            // The base address is arbitrary as the channel uses the factory's test server
            var client = _factory.CreateDefaultClient(new Uri("http://localhost"));
            var channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions { HttpClient = client });

            // Corrected client instantiation
            _contactsClient = new ContactsServiceClient(channel);
            _lookupsClient = new LookupsServiceClient(channel);
        }

        [Fact]
        public async Task GetAllContacts_ReturnsContacts()
        {
            // Act
            var response = await _contactsClient.GetAllContactsAsync(new Empty());

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Contacts.Any()); // Should have initial data from Utilities.InitializeDatabaseAsync
            Assert.Contains(response.Contacts, c => c.FirstName == "John" || c.FirstName == "Jane");
        }

        [Fact]
        public async Task GetContact_ReturnsSpecificContact()
        {
            // Arrange: Insert a contact first to ensure it exists
            var insertRequest = new PersonRequest
            {
                FirstName = "Test",
                LastName = "User",
                PrefixId = 1,
                SuffixId = 1,
                Address = "1 Test St",
                City = "Testville",
                State = "TS",
                Zip = "12345"
            };
            await _contactsClient.InsertContactAsync(insertRequest);

            // Act: Get all contacts to find the UID of the inserted contact
            var allContacts = await _contactsClient.GetAllContactsAsync(new Empty());
            var testContact = allContacts.Contacts.FirstOrDefault(c => c.FirstName == "Test" && c.LastName == "User");

            Assert.NotNull(testContact); // Ensure the contact was inserted and found

            var getRequest = new GetContactRequest { Uid = testContact.Uid };
            var retrievedContact = await _contactsClient.GetContactAsync(getRequest);

            // Assert
            Assert.NotNull(retrievedContact);
            Assert.Equal("Test", retrievedContact.FirstName);
            Assert.Equal("User", retrievedContact.LastName);
        }

        [Fact]
        public async Task GetContact_ReturnsNotFoundForNonExistentUid()
        {
            // Act & Assert
            var request = new GetContactRequest { Uid = 99999 }; // A UID that should not exist
            var exception = await Assert.ThrowsAsync<RpcException>(() => _contactsClient.GetContactAsync(request));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Fact]
        public async Task InsertContact_AddsNewContact()
        {
            // Arrange
            var initialCountResponse = await _contactsClient.GetAllContactsAsync(new Empty());
            int initialCount = initialCountResponse.Contacts.Count;

            var request = new PersonRequest
            {
                FirstName = "New",
                LastName = "Entry",
                PrefixId = 1,
                SuffixId = 1,
                Address = "1 New St",
                City = "New City",
                State = "NC",
                Zip = "00000"
            };

            // Act
            var response = await _contactsClient.InsertContactAsync(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Contact inserted successfully.", response.Message);

            var finalCountResponse = await _contactsClient.GetAllContactsAsync(new Empty());
            Assert.Equal(initialCount + 1, finalCountResponse.Contacts.Count);
            Assert.Contains(finalCountResponse.Contacts, c => c.FirstName == "New" && c.LastName == "Entry");
        }

        [Fact]
        public async Task UpdateContact_ModifiesExistingContact()
        {
            // Arrange: Insert a contact to update
            var insertRequest = new PersonRequest
            {
                FirstName = "Original",
                LastName = "Name",
                PrefixId = 1,
                SuffixId = 1,
                Address = "1 Original St",
                City = "Original City",
                State = "OR",
                Zip = "11111"
            };
            await _contactsClient.InsertContactAsync(insertRequest);

            var allContacts = await _contactsClient.GetAllContactsAsync(new Empty());
            var contactToUpdate = allContacts.Contacts.First(c => c.FirstName == "Original");

            var updateRequest = new Contact
            {
                Uid = contactToUpdate.Uid,
                FirstName = "Updated",
                LastName = "Person",
                PrefixId = 2, // Ms.
                SuffixId = 2, // Sr.
                Address = "2 Updated Ave",
                City = "Updated City",
                State = "UP",
                Zip = "22222"
            };

            // Act
            var response = await _contactsClient.UpdateContactAsync(updateRequest);

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Contact updated successfully.", response.Message);

            var retrievedContact = await _contactsClient.GetContactAsync(new GetContactRequest { Uid = contactToUpdate.Uid });
            Assert.Equal("Updated", retrievedContact.FirstName);
            Assert.Equal("Person", retrievedContact.LastName);
            Assert.Equal(2, retrievedContact.PrefixId);
            Assert.Equal(2, retrievedContact.SuffixId);
            Assert.Equal("2 Updated Ave", retrievedContact.Address);
            Assert.Equal("Updated City", retrievedContact.City);
            Assert.Equal("UP", retrievedContact.State);
            Assert.Equal("22222", retrievedContact.Zip);
        }

        [Fact]
        public async Task GetPrefixes_ReturnsAllPrefixes()
        {
            // Act
            var response = await _lookupsClient.GetPrefixes(new Empty(), null);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Lookups.Count >= 3); // Expecting at least initial prefixes
            Assert.Contains(response.Lookups, l => l.Description == "Mr.");
            Assert.Contains(response.Lookups, l => l.Description == "Ms.");
            Assert.Contains(response.Lookups, l => l.Description == "Mrs.");
        }

        [Fact]
        public async Task GetSuffixes_ReturnsAllSuffixes()
        {
            // Act
            var response = await _lookupsClient.GetSuffixes(new Empty(), null);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Lookups.Count >= 2); // Expecting at least initial suffixes
            Assert.Contains(response.Lookups, l => l.Description == "Jr.");
            Assert.Contains(response.Lookups, l => l.Description == "Sr.");
        }
    }
}
