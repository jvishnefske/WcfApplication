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

        public override Task<GetLookupsResponse> GetPrefixes(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("Getting prefixes via gRPC.");
            var prefixes = _utilities.GetPrefixes(); // CALL non-static method
            var response = new GetLookupsResponse();
            foreach (var lookupDto in prefixes)
            {
                response.Lookups.Add(MapLookupDtoToGrpcLookup(lookupDto));
            }
            return Task.FromResult(response);
        }

        public override Task<GetLookupsResponse> GetSuffixes(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("Getting suffixes via gRPC.");
            var suffixes = _utilities.GetSuffixes(); // CALL non-static method
            var response = new GetLookupsResponse();
            foreach (var lookupDto in suffixes)
            {
                response.Lookups.Add(MapLookupDtoToGrpcLookup(lookupDto));
            }
            return Task.FromResult(response);
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
