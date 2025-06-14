using Grpc.Core;
using Microsoft.Extensions.Logging;
using ContactsApi.Grpc;
using ContactsApi.Models;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsApi.Services
{
    public class LookupsGrpcService : ContactsApi.Grpc.LookupsService.LookupsServiceBase
    {
        private readonly ILogger<LookupsGrpcService> _logger;
        private readonly Utilities _utilities;

        public LookupsGrpcService(ILogger<LookupsGrpcService> logger, Utilities utilities)
        {
            _logger = logger;
            _utilities = utilities;
        }

        public override async Task<GetLookupsResponse> GetPrefixes(Empty request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Received GetPrefixes request.");
                var prefixDtos = await _utilities.GetPrefixesAsync();
                var grpcLookups = prefixDtos.Select(MapLookupDtoToGrpcLookup).ToList();
                return new GetLookupsResponse { Lookups = { grpcLookups } };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prefixes.");
                throw new RpcException(new Status(StatusCode.Internal, $"Error getting prefixes: {ex.Message}"));
            }
        }

        public override async Task<GetLookupsResponse> GetSuffixes(Empty request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Received GetSuffixes request.");
                var suffixDtos = await _utilities.GetSuffixesAsync();
                var grpcLookups = suffixDtos.Select(MapLookupDtoToGrpcLookup).ToList();
                return new GetLookupsResponse { Lookups = { grpcLookups } };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suffixes.");
                throw new RpcException(new Status(StatusCode.Internal, $"Error getting suffixes: {ex.Message}"));
            }
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
