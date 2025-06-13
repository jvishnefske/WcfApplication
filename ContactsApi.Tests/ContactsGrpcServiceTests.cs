using Xunit;
using Moq;
using Grpc.Core; // Ensure this is present
using Microsoft.Extensions.Logging;
using ContactsApi.Services;
using ContactsApi.Grpc;
using ContactsApi.Models;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System;
// REMOVED: using Grpc.Net.Client.Testing; 

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
            // Use the custom TestServerCallContext
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
            // Use the custom TestServerCallContext
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
            // Use the custom TestServerCallContext
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
            // Use the custom TestServerCallContext
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
            // Use the custom TestServerCallContext
            var response = await _service.UpdateContact(contact, TestServerCallContext.Create());

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);
            _utilitiesMock.Verify(u => u.UpdateContactAsync(contact.Uid, contact.FirstName, contact.LastName, contact.PrefixId, contact.SuffixId, contact.Address, contact.City, contact.State, contact.Zip), Times.Once);
        }
    }

    // REVISED CUSTOM TestServerCallContext CLASS DEFINITION
    public class TestServerCallContext : ServerCallContext
    {
        // Private fields to back the properties
        private Metadata _responseHeaders = new Metadata();
        private Status _status;
        private WriteOptions? _writeOptions;

        private TestServerCallContext() { }

        public static TestServerCallContext Create() => new TestServerCallContext();

        // Implement protected abstract properties (all are getter-only except StatusCore and WriteOptionsCore)
        protected override AuthContext AuthContextCore => new AuthContext(null, new List<AuthProperty>());
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override string HostCore => "localhost";
        protected override string MethodCore => "Test";
        protected override string PeerCore => "localhost";
        protected override Metadata RequestHeadersCore => new Metadata();
        protected override Metadata ResponseTrailersCore => new Metadata(); 

        // Corrected: ResponseHeadersCore is getter-only in the abstract base.
        // We will store the headers in the private _responseHeaders field when WriteResponseHeadersAsyncCore is called.
        protected override Metadata ResponseHeadersCore { get; } = new Metadata(); 

        // These properties must have both get and set, and match the abstract signature
        protected override Status StatusCore { get => _status; set => _status = value; }
        protected override WriteOptions? WriteOptionsCore { get => _writeOptions; set => _writeOptions = value; }

        // Implement protected abstract methods
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            // This method is abstract in Grpc.Core 2.x.
            // For testing, it's usually safe to throw NotImplementedException if not directly used.
            throw new NotImplementedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            // This method is called by the gRPC infrastructure to set response headers.
            // We store them in our private field.
            _responseHeaders = responseHeaders; 
            return Task.CompletedTask;
        }

        // CORRECTED: ReadMessageCore and WriteMessageCore *DO* take Serializer/Deserializer parameters in Grpc.Core 2.x abstract definition
        protected override Task<byte[]> ReadMessageCore(Grpc.Core.Deserializer<byte[]> deserializer)
        {
            throw new NotImplementedException();
        }

        protected override Task WriteMessageCore(byte[] message, Grpc.Core.Serializer<byte[]> serializer, WriteOptions writeOptions)
        {
            throw new NotImplementedException();
        }
    }
}
