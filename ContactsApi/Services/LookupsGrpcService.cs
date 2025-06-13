using Grpc.Core;
using ContactsApi.Grpc;
using ContactsApi.Models;
using Google.Protobuf.WellKnownTypes;
using ContactsApi; // ADD THIS USING for Utilities

namespace ContactsApi.Services
{
    public class LookupsGrpcService : ContactsApi.Grpc.LookupsService.LookupsServiceBase
    {
        private readonly ILogger<LookupsGrpcService> _logger;
        private readonly Utilities _utilities; // ADD THIS FIELD

        public LookupsGrpcService(ILogger<LookupsGrpcService> logger, Utilities utilities) // ADD Utilities to constructor
        {
            _logger = logger;
            _utilities = utilities; // ASSIGN Utilities
        }

        public override async Task<GetLookupsResponse> GetPrefixes(Empty request, ServerCallContext context) // ADD ASYNC
        {
            _logger.LogInformation("Getting prefixes via gRPC.");
            var prefixes = await _utilities.GetPrefixesAsync(); // CALL non-static method ASYNC
            var response = new GetLookupsResponse();
            foreach (var lookupDto in prefixes)
            {
                response.Lookups.Add(MapLookupDtoToGrpcLookup(lookupDto));
            }
            return response; // No Task.FromResult needed for async methods
        }

        public override async Task<GetLookupsResponse> GetSuffixes(Empty request, ServerCallContext context) // ADD ASYNC
        {
            _logger.LogInformation("Getting suffixes via gRPC.");
            var suffixes = await _utilities.GetSuffixesAsync(); // CALL non-static method ASYNC
            var response = new GetLookupsResponse();
            foreach (var lookupDto in suffixes)
            {
                response.Lookups.Add(MapLookupDtoToGrpcLookup(lookupDto));
            }
            return response; // No Task.FromResult needed for async methods
        }

        private ContactsApi.Grpc.Lookup MapLookupDtoToGrpcLookup(LookupDto dto)
        {
            return new ContactsApi.Grpc.Lookup
            {
                Id = dto.Id,
                Description = dto.Description ?? string.Empty
            };
        }
    }
}
