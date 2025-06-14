using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Grpc.Net.Client;
using Grpc.Net.ClientFactory;
using ContactsApi.Grpc;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core; // For RpcException
using System.Collections.Generic; // For Dictionary
using Microsoft.Extensions.Configuration; // For ConfigureAppConfiguration

namespace ContactsApi.Tests
{
    // IClassFixture ensures a single instance of WebApplicationFactory is shared across all tests in this class.
    // This means the API application is started once for all tests in this fixture.
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ContactsService.ContactsServiceClient _contactsClient;
        private readonly LookupsService.LookupsServiceClient _lookupsClient;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Configure the WebApplicationFactory to use an in-memory SQLite database for tests.
            // This overrides the appsettings.json connection string for the test host.
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Clear existing configuration sources and add an in-memory one for tests.
                    // This ensures that the test environment uses a clean, in-memory database.
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {"ConnectionStrings:DefaultConnection", "DataSource=file::memory:?cache=shared"}
                    });
                });
            });

            // Create an HttpClient that targets the TestServer provided by the factory.
            var httpClient = _factory.CreateClient();
            
            // Create a gRPC channel using the HttpClient.
            // The BaseAddress of the HttpClient will be the address of the TestServer.
            var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions { HttpClient = httpClient });

            // Create gRPC clients using the channel.
            _contactsClient = new ContactsService.ContactsServiceClient(channel);
            _lookupsClient = new LookupsService.LookupsServiceClient(channel);
        }

        // IAsyncLifetime methods for per-test setup/teardown
        public async Task InitializeAsync()
        {
            // Get a scope to resolve services from the test server
            using (var scope = _factory.Services.CreateScope())
            {
                var utilities = scope.ServiceProvider.GetRequiredService<Utilities>();
                await utilities.ClearContactsAsync(); // Clear contacts before each test
            }
        }

        // DisposeAsync is called after each test method
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task GetAllContacts_ReturnsEmptyListInitially()
        {
            // Arrange: The in-memory database starts empty for contacts for each test run
            // (due to WithWebHostBuilder config and Utilities.InitializeDatabaseAsync).
            // Utilities.InitializeDatabaseAsync populates prefixes/suffixes but not contacts.

            // Act
            var response = await _contactsClient.GetAllContactsAsync(new Empty());

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Contacts); // Expecting an empty list as no contacts are added by default
        }

        [Fact]
        public async Task InsertContact_AddsNewContactAndCanBeRetrieved()
        {
            // Arrange
            var newContactRequest = new PersonRequest
            {
                FirstName = "Integration",
                LastName = "Test",
                PrefixId = 1, // Assuming Mr. exists from Utilities initialization
                SuffixId = 1, // Assuming Jr. exists from Utilities initialization
                Address = "101 Test St",
                City = "Testville",
                State = "TS",
                Zip = "12345"
            };

            // Act: Insert the contact
            var insertResponse = await _contactsClient.InsertContactAsync(newContactRequest);

            // Assert insert success
            Assert.NotNull(insertResponse);
            Assert.True(insertResponse.Success);
            Assert.Contains("Contact inserted successfully.", insertResponse.Message);

            // Act: Retrieve all contacts to verify insertion
            var allContactsResponse = await _contactsClient.GetAllContactsAsync(new Empty());

            // Assert: Verify the new contact is in the list
            Assert.NotNull(allContactsResponse);
            Assert.Single(allContactsResponse.Contacts); // Should be exactly one contact now
            var retrievedContact = allContactsResponse.Contacts[0];
            Assert.Equal("Integration", retrievedContact.FirstName);
            Assert.Equal("Test", retrievedContact.LastName);
            Assert.Equal("101 Test St", retrievedContact.Address);
        }

        [Fact]
        public async Task GetContact_ReturnsNotFoundForNonExistentContact()
        {
            // Arrange
            var request = new GetContactRequest { Uid = 999 }; // A UID that definitely doesn't exist

            // Act & Assert
            // We expect an RpcException with StatusCode.NotFound
            var exception = await Assert.ThrowsAsync<RpcException>(() => _contactsClient.GetContactAsync(request).ResponseAsync);
            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
            Assert.Contains("Contact with Uid 999 not found.", exception.Status.Detail);
        }

        [Fact]
        public async Task GetPrefixes_ReturnsPopulatedList()
        {
            // Act
            var response = await _lookupsClient.GetPrefixesAsync(new Empty());

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Lookups.Count > 0); // Expecting prefixes to be populated by Utilities
            Assert.Contains(response.Lookups, l => l.Description == "Mr.");
            Assert.Contains(response.Lookups, l => l.Description == "Ms.");
        }

        [Fact]
        public async Task GetSuffixes_ReturnsPopulatedList()
        {
            // Act
            var response = await _lookupsClient.GetSuffixesAsync(new Empty());

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Lookups.Count > 0); // Expecting suffixes to be populated by Utilities
            Assert.Contains(response.Lookups, l => l.Description == "Jr.");
            Assert.Contains(response.Lookups, l => l.Description == "Sr.");
        }
    }
}
