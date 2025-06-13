using Xunit;
using Moq;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ContactsApi.Services;
using ContactsApi.Grpc;
using ContactsApi.Models;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration; // Added for Mock<IConfiguration>
using System.Threading; // Added for CancellationToken
using System; // Added for NotImplementedException
using Grpc.Net.Client.Testing; // ADD THIS USING DIRECTIVE

namespace ContactsApi.Tests
{
    public class ContactsGrpcServiceTests
    {
        private readonly Mock<ILogger<ContactsGrpcService>> _loggerMock;
        private readonly Mock<Utilities> _utilitiesMock;
        private readonly ContactsGrpcService _service;

        public ContactsGrpcServiceTests()
        {
            _loggerMock = new Mock<ILogger<ContactsGrpcService>>();
            // Ensure MockBehavior.Strict is used if you want to verify all setups
            _utilitiesMock = new Mock<Utilities>(MockBehavior.Strict, new Mock<IConfiguration>().Object); 
            _service = new ContactsGrpcService(_loggerMock.Object, _utilitiesMock.Object);
        }

        [Fact]
        public async Task GetAllContacts_ReturnsAllContacts()
        {
            // Arrange
            var mockContacts = new List<ContactDto>
            {
                new ContactDto { Uid = 1, FirstName = "John", LastName = "Doe", PrefixId = 1, SuffixId = 1, Address = "123 Main", City = "Anytown", State = "CA", Zip = "90210" },
                new ContactDto { Uid = 2, FirstName = "Jane", LastName = "Smith", PrefixId = 2, SuffixId = 2, Address = "456 Oak", City = "Otherville", State = "NY", Zip = "10001" }
            };
            _utilitiesMock.Setup(u => u.GetAllContactsAsync()).ReturnsAsync(mockContacts);

            // Act
            // Use the TestServerCallContext from Grpc.Net.Client.Testing
            var response = await _service.GetAllContacts(new Empty(), TestServerCallContext.Create());

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Contacts.Count);
            Assert.Equal("John", response.Contacts[0].FirstName);
            Assert.Equal("Jane", response.Contacts[1].FirstName);
            _utilitiesMock.Verify(u => u.GetAllContactsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetContact_ReturnsSpecificContact()
        {
            // Arrange
            var mockContact = new ContactDto { Uid = 1, FirstName = "John", LastName = "Doe", PrefixId = 1, SuffixId = 1, Address = "123 Main", City = "Anytown", State = "CA", Zip = "90210" };
            _utilitiesMock.Setup(u => u.GetContactAsync(1)).ReturnsAsync(mockContact);

            // Act
            var request = new GetContactRequest { Uid = 1 };
            // Use the TestServerCallContext from Grpc.Net.Client.Testing
            var contact = await _service.GetContact(request, TestServerCallContext.Create());

            // Assert
            Assert.NotNull(contact);
            Assert.Equal(1, contact.Uid);
            Assert.Equal("John", contact.FirstName);
            _utilitiesMock.Verify(u => u.GetContactAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetContact_ThrowsNotFoundForNonExistentContact()
        {
            // Arrange
            _utilitiesMock.Setup(u => u.GetContactAsync(999)).ReturnsAsync((ContactDto?)null);

            // Act & Assert
            var request = new GetContactRequest { Uid = 999 };
            // Use the TestServerCallContext from Grpc.Net.Client.Testing
            var exception = await Assert.ThrowsAsync<RpcException>(() => _service.GetContact(request, TestServerCallContext.Create()));
            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
            _utilitiesMock.Verify(u => u.GetContactAsync(999), Times.Once);
        }

        [Fact]
        public async Task InsertContact_CallsUtilitiesAndReturnsSuccess()
        {
            // Arrange
            var personRequest = new PersonRequest { FirstName = "New", LastName = "Person", PrefixId = 1, SuffixId = 1, Address = "A", City = "B", State = "C", Zip = "D" };
            _utilitiesMock.Setup(u => u.InsertContactAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(Task.CompletedTask);

            // Act
            // Use the TestServerCallContext from Grpc.Net.Client.Testing
            var response = await _service.InsertContact(personRequest, TestServerCallContext.Create());

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);
            _utilitiesMock.Verify(u => u.InsertContactAsync(personRequest.FirstName, personRequest.LastName, personRequest.PrefixId, personRequest.SuffixId, personRequest.Address, personRequest.City, personRequest.State, personRequest.Zip), Times.Once);
        }

        [Fact]
        public async Task UpdateContact_CallsUtilitiesAndReturnsSuccess()
        {
            // Arrange
            var contact = new Contact { Uid = 1, FirstName = "Updated", LastName = "Name", PrefixId = 1, SuffixId = 1, Address = "A", City = "B", State = "C", Zip = "D" };
            _utilitiesMock.Setup(u => u.UpdateContactAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(Task.CompletedTask);

            // Act
            // Use the TestServerCallContext from Grpc.Net.Client.Testing
            var response = await _service.UpdateContact(contact, TestServerCallContext.Create());

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);
            _utilitiesMock.Verify(u => u.UpdateContactAsync(contact.Uid, contact.FirstName, contact.LastName, contact.PrefixId, contact.SuffixId, contact.Address, contact.City, contact.State, contact.Zip), Times.Once);
        }
    }
}
