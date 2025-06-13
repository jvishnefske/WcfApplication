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
            _utilitiesMock = new Mock<Utilities>(MockBehavior.Strict, new Mock<IConfiguration>().Object); // Pass a mock IConfiguration
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
            var response = await _service.UpdateContact(contact, TestServerCallContext.Create());

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);
            _utilitiesMock.Verify(u => u.UpdateContactAsync(contact.Uid, contact.FirstName, contact.LastName, contact.PrefixId, contact.SuffixId, contact.Address, contact.City, contact.State, contact.Zip), Times.Once);
        }
    }

    // Helper class for creating a dummy ServerCallContext for gRPC service tests
    public class TestServerCallContext : ServerCallContext
    {
        private TestServerCallContext() { }

        public static TestServerCallContext Create() => new TestServerCallContext();

        // Corrected implementation for abstract members
        protected override ContextPropagationToken CreateContextPropagationTokenCore(ContextPropagationOptions? options) => throw new NotImplementedException();
        protected override string MethodCore => "Test";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "localhost";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => new Metadata();
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseHeadersCore { get; set; } = new Metadata();
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new AuthContext(null, new List<AuthProperty>());

        // Corrected StatusCore and ResponseTrailersCore implementations
        protected override Status StatusCore { get; set; } // Must have a public setter
        protected override Metadata ResponseTrailersCore { get; } = new Metadata(); // Must have a public getter

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            ResponseHeadersCore = responseHeaders;
            return Task.CompletedTask;
        }

        // Corrected ReadMessageCore and WriteMessageCore to avoid internal types
        protected override Task<byte[]> ReadMessageCore() => throw new NotImplementedException();
        protected override Task WriteMessageCore(byte[] message, WriteOptions writeOptions) => throw new NotImplementedException();
    }
}
