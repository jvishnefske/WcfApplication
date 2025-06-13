using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Moq;
using ContactsApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ContactsApi.Tests
{
    // Implement IAsyncLifetime for asynchronous setup/teardown
    public class UtilitiesTests : IAsyncLifetime
    {
        private readonly SqliteConnection _connection;
        private readonly Utilities _utilities;
        private readonly IConfiguration _configuration; // Store configuration for Utilities

        public UtilitiesTests()
        {
            // Use an in-memory SQLite database for testing with shared cache
            _connection = new SqliteConnection("DataSource=file::memory:?cache=shared"); // UPDATED CONNECTION STRING
            
            // Create a real IConfiguration instance with an in-memory connection string
            // Fix CS8620 warning by making values nullable
            var inMemorySettings = new Dictionary<string, string?> {
                {"ConnectionStrings:DefaultConnection", _connection.ConnectionString}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Instantiate the real Utilities class with the configured IConfiguration
            _utilities = new Utilities(_configuration);
        }

        // InitializeAsync is called before each test method
        public async Task InitializeAsync()
        {
            _connection.Open(); // Open the connection for the Utilities class to use

            // Initialize the database schema using the Utilities class's method
            await _utilities.InitializeDatabaseAsync();
            
            // Populate initial data after schema is created
            await PopulateTestDataAsync();
        }

        // DisposeAsync is called after each test method
        public async Task DisposeAsync()
        {
            // Clean up data if necessary (optional, as in-memory DB is cleared per test)
            // For persistent DBs, you might truncate tables here.
            _connection.Close();
            await _connection.DisposeAsync(); // Use async dispose
        }

        private async Task PopulateTestDataAsync()
        {
            using (var setupConnection = new SqliteConnection(_connection.ConnectionString))
            {
                await setupConnection.OpenAsync();
                var command = setupConnection.CreateCommand();

                // Clear existing data before populating for a clean test state
                command.CommandText = "DELETE FROM Contacts; DELETE FROM Prefixes; DELETE FROM Suffixes;";
                await command.ExecuteNonQueryAsync();

                // Populate Prefixes
                command.CommandText = @"
                    INSERT INTO Prefixes (Id, Description) VALUES
                    (1, 'Mr.'), (2, 'Ms.'), (3, 'Mrs.');";
                await command.ExecuteNonQueryAsync();

                // Populate Suffixes
                command.CommandText = @"
                    INSERT INTO Suffixes (Id, Description) VALUES
                    (1, 'Jr.'), (2, 'Sr.');";
                await command.ExecuteNonQueryAsync();

                // Populate Contacts
                command.CommandText = @"
                    INSERT INTO Contacts (PrefixId, FirstName, LastName, SuffixId, Address, City, State, Zip) VALUES
                    (1, 'John', 'Doe', 1, '123 Main St', 'Anytown', 'CA', '90210'),
                    (2, 'Jane', 'Smith', 2, '456 Oak Ave', 'Otherville', 'NY', '10001');";
                await command.ExecuteNonQueryAsync();
            }
        }

        [Fact]
        public async Task GetAllContactsAsync_ReturnsAllContacts()
        {
            // Act
            var contacts = await _utilities.GetAllContactsAsync();

            // Assert
            Assert.NotNull(contacts);
            Assert.Equal(2, contacts.Count); // Expecting 2 contacts from setup
            Assert.Contains(contacts, c => c.FirstName == "John" && c.LastName == "Doe");
            Assert.Contains(contacts, c => c.FirstName == "Jane" && c.LastName == "Smith");
        }

        [Fact]
        public async Task GetContactAsync_ReturnsCorrectContact()
        {
            // Arrange: Get a known UID from the initial data
            var allContacts = await _utilities.GetAllContactsAsync();
            var johnDoe = allContacts.First(c => c.FirstName == "John");

            // Act
            var contact = await _utilities.GetContactAsync(johnDoe.Uid);

            // Assert
            Assert.NotNull(contact);
            Assert.Equal("John", contact.FirstName);
            Assert.Equal("Doe", contact.LastName);
            Assert.Equal(1, contact.PrefixId);
            Assert.Equal(1, contact.SuffixId);
            Assert.Equal("123 Main St", contact.Address);
            Assert.Equal("Anytown", contact.City);
            Assert.Equal("CA", contact.State);
            Assert.Equal("90210", contact.Zip);
        }

        [Fact]
        public async Task GetContactAsync_ReturnsNullForNonExistentContact()
        {
            // Act
            var contact = await _utilities.GetContactAsync(999); // Non-existent UID

            // Assert
            Assert.Null(contact);
        }

        [Fact]
        public async Task InsertContactAsync_AddsNewContact()
        {
            // Arrange
            var initialCount = (await _utilities.GetAllContactsAsync()).Count;
            string newFirstName = "Alice";
            string newLastName = "Brown";
            int newPrefixId = 2; // Ms.
            int newSuffixId = 1; // Jr.
            string newAddress = "789 Pine Ln";
            string newCity = "Newtown";
            string newState = "TX";
            string newZip = "77001";

            // Act
            await _utilities.InsertContactAsync(newFirstName, newLastName, newPrefixId, newSuffixId, newAddress, newCity, newState, newZip);

            // Assert
            var contacts = await _utilities.GetAllContactsAsync();
            Assert.Equal(initialCount + 1, contacts.Count);
            Assert.Contains(contacts, c => c.FirstName == newFirstName && c.LastName == newLastName);
        }

        [Fact]
        public async Task UpdateContactAsync_UpdatesExistingContact()
        {
            // Arrange: Get a contact to update
            var allContacts = await _utilities.GetAllContactsAsync();
            var janeSmith = allContacts.First(c => c.FirstName == "Jane");
            
            string updatedFirstName = "Janet";
            string updatedLastName = "Jones";
            int updatedPrefixId = 3; // Mrs.
            int updatedSuffixId = 1; // Jr.
            string updatedAddress = "Updated Address";
            string updatedCity = "Updated City";
            string updatedState = "UP";
            string updatedZip = "11111";

            // Act
            await _utilities.UpdateContactAsync(janeSmith.Uid, updatedFirstName, updatedLastName, updatedPrefixId, updatedSuffixId, updatedAddress, updatedCity, updatedState, updatedZip);

            // Assert
            var updatedContact = await _utilities.GetContactAsync(janeSmith.Uid);
            Assert.NotNull(updatedContact);
            Assert.Equal(updatedFirstName, updatedContact.FirstName);
            Assert.Equal(updatedLastName, updatedContact.LastName);
            Assert.Equal(updatedPrefixId, updatedContact.PrefixId);
            Assert.Equal(updatedSuffixId, updatedContact.SuffixId);
            Assert.Equal(updatedAddress, updatedContact.Address);
            Assert.Equal(updatedCity, updatedContact.City);
            Assert.Equal(updatedState, updatedContact.State);
            Assert.Equal(updatedZip, updatedContact.Zip);
        }

        [Fact]
        public async Task GetPrefixesAsync_ReturnsAllPrefixes()
        {
            // Act
            var prefixes = await _utilities.GetPrefixesAsync();

            // Assert
            Assert.NotNull(prefixes);
            Assert.Equal(3, prefixes.Count); // Expecting 3 prefixes from setup
            Assert.Contains(prefixes, p => p.Description == "Mr.");
            Assert.Contains(prefixes, p => p.Description == "Ms.");
            Assert.Contains(prefixes, p => p.Description == "Mrs.");
        }

        [Fact]
        public async Task GetSuffixesAsync_ReturnsAllSuffixes()
        {
            // Act
            var suffixes = await _utilities.GetSuffixesAsync();

            // Assert
            Assert.NotNull(suffixes);
            Assert.Equal(2, suffixes.Count); // Expecting 2 suffixes from setup
            Assert.Contains(suffixes, s => s.Description == "Jr.");
            Assert.Contains(suffixes, s => s.Description == "Sr.");
        }
    }
}
