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
using Microsoft.Extensions.Configuration;

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
            
            var mockConfig = new Mock<IConfiguration>();
            // --- REVISED IConfiguration MOCK SETUP for GetConnectionString ---
            // This explicitly mocks the chain of GetSection calls that GetConnectionString uses.
            var mockDefaultConnectionSection = new Mock<IConfigurationSection>();
            mockDefaultConnectionSection.Setup(s => s.Value).Returns("DataSource=file::memory:?cache=shared");

            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(s => s.GetSection("DefaultConnection")).Returns(mockDefaultConnectionSection.Object);

            mockConfig.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);
            // --- END REVISED IConfiguration MOCK SETUP ---

            // Initialize _utilitiesMock, passing the configured mockConfig to its constructor.
            _utilitiesMock = new Mock<Utilities>(mockConfig.Object); 

            // Setup default behaviors for Utilities methods called by LookupsGrpcService
            _utilitiesMock.Setup(u => u.GetPrefixesAsync()).ReturnsAsync(new List<LookupDto>());
            _utilitiesMock.Setup(u => u.GetSuffixesAsync()).ReturnsAsync(new List<LookupDto>());

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
            // Override the default setup for this specific test
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
            // Override the default setup for this specific test
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
