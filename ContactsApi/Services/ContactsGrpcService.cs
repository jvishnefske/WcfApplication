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
    public class ContactsGrpcService : ContactsApi.Grpc.ContactsService.ContactsServiceBase
    {
        private readonly ILogger<ContactsGrpcService> _logger;
        private readonly Utilities _utilities;

        public ContactsGrpcService(ILogger<ContactsGrpcService> logger, Utilities utilities)
        {
            _logger = logger;
            _utilities = utilities;
        }

        public override async Task<GetAllContactsResponse> GetAllContacts(Empty request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Received GetAllContacts request.");
                var contactDtos = await _utilities.GetAllContactsAsync();
                var grpcContacts = contactDtos.Select(MapContactDtoToGrpcContact).ToList();
                return new GetAllContactsResponse { Contacts = { grpcContacts } };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all contacts.");
                throw new RpcException(new Status(StatusCode.Internal, $"Error getting all contacts: {ex.Message}"));
            }
        }

        public override async Task<Contact> GetContact(GetContactRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Received GetContact request for Uid: {Uid}", request.Uid);
                var contactDto = await _utilities.GetContactAsync(request.Uid);
                if (contactDto == null)
                {
                    _logger.LogWarning("Contact with Uid {Uid} not found.", request.Uid);
                    throw new RpcException(new Status(StatusCode.NotFound, $"Contact with Uid {request.Uid} not found."));
                }
                return MapContactDtoToGrpcContact(contactDto);
            }
            catch (RpcException)
            {
                throw; // Re-throw RpcException as it's already a gRPC error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact with Uid: {Uid}", request.Uid);
                throw new RpcException(new Status(StatusCode.Internal, $"Error getting contact: {ex.Message}"));
            }
        }

        public override async Task<OperationResponse> InsertContact(PersonRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Received InsertContact request for {FirstName} {LastName}", request.FirstName, request.LastName);
                await _utilities.InsertContactAsync(
                    request.FirstName,
                    request.LastName,
                    request.PrefixId,
                    request.SuffixId,
                    request.Address,
                    request.City,
                    request.State,
                    request.Zip
                );
                return new OperationResponse { Success = true, Message = "Contact inserted successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting contact: {FirstName} {LastName}", request.FirstName, request.LastName);
                throw new RpcException(new Status(StatusCode.Internal, $"Error inserting contact: {ex.Message}"));
            }
        }

        public override async Task<OperationResponse> UpdateContact(Contact request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Received UpdateContact request for Uid: {Uid}", request.Uid);
                await _utilities.UpdateContactAsync(
                    request.Uid,
                    request.FirstName,
                    request.LastName,
                    request.PrefixId,
                    request.SuffixId,
                    request.Address,
                    request.City,
                    request.State,
                    request.Zip
                );
                return new OperationResponse { Success = true, Message = "Contact updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact with Uid: {Uid}", request.Uid);
                throw new RpcException(new Status(StatusCode.Internal, $"Error updating contact: {ex.Message}"));
            }
        }

        private ContactsApi.Grpc.Contact MapContactDtoToGrpcContact(ContactDto dto)
        {
            return new ContactsApi.Grpc.Contact
            {
                Uid = dto.Uid,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PrefixId = dto.PrefixId,
                SuffixId = dto.SuffixId,
                Address = dto.Address ?? string.Empty,
                City = dto.City ?? string.Empty,
                State = dto.State ?? string.Empty,
                Zip = dto.Zip ?? string.Empty
            };
        }
    }
}
