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
    public class LookupsGrpcServiceTests
    {
        private readonly Mock<ILogger<LookupsGrpcService>> _loggerMock;
        private readonly Mock<Utilities> _utilitiesMock;
        private readonly LookupsGrpcService _service;

        public LookupsGrpcServiceTests()
        {
            _loggerMock = new Mock<ILogger<LookupsGrpcService>>();
            _utilitiesMock = new Mock<Utilities>(MockBehavior.Strict, new Mock<IConfiguration>().Object); // Pass a mock IConfiguration
            _service = new LookupsGrpcService(_loggerMock.Object, _utilitiesMock.Object);
        }

        [Fact]
        public async Task GetPrefixes_ReturnsAllPrefixes()
        {
            // Arrange
            var mockPrefixes = new List<LookupDto>
            {
                new LookupDto { Id = 1, Description = "Mr." },
                new LookupDto { Id = 2, Description = "Ms." }
            };
            _utilitiesMock.Setup(u => u.GetPrefixesAsync()).ReturnsAsync(mockPrefixes);

            // Act
            var response = await _service.GetPrefixes(new Empty(), TestServerCallContext.Create());

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Lookups.Count);
            Assert.Equal("Mr.", response.Lookups[0].Description);
            Assert.Equal("Ms.", response.Lookups[1].Description);
            _utilitiesMock.Verify(u => u.GetPrefixesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetSuffixes_ReturnsAllSuffixes()
        {
            // Arrange
            var mockSuffixes = new List<LookupDto>
            {
                new LookupDto { Id = 1, Description = "Jr." },
                new LookupDto { Id = 2, Description = "Sr." }
            };
            _utilitiesMock.Setup(u => u.GetSuffixesAsync()).ReturnsAsync(mockSuffixes);

            // Act
            var response = await _service.GetSuffixes(new Empty(), TestServerCallContext.Create());

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Lookups.Count);
            Assert.Equal("Jr.", response.Lookups[0].Description);
            Assert.Equal("Sr.", response.Lookups[1].Description);
            _utilitiesMock.Verify(u => u.GetSuffixesAsync(), Times.Once);
        }
    }
}
