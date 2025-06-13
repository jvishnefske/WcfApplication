using Grpc.Core;
using ContactsApi.Grpc;
using ContactsApi.Models;
using Google.Protobuf.WellKnownTypes;
using ContactsApi; // ADD THIS USING for Utilities

namespace ContactsApi.Services
{
    public class ContactsGrpcService : ContactsApi.Grpc.ContactsService.ContactsServiceBase
    {
        private readonly ILogger<ContactsGrpcService> _logger;
        private readonly Utilities _utilities; // ADD THIS FIELD

        public ContactsGrpcService(ILogger<ContactsGrpcService> logger, Utilities utilities) // ADD Utilities to constructor
        {
            _logger = logger;
            _utilities = utilities; // ASSIGN Utilities
        }

        public override Task<GetAllContactsResponse> GetAllContacts(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("Getting all contacts via gRPC.");
            var contacts = _utilities.GetAllContacts(); // CALL non-static method
            var response = new GetAllContactsResponse();
            foreach (var contactDto in contacts)
            {
                response.Contacts.Add(MapContactDtoToGrpcContact(contactDto));
            }
            return Task.FromResult(response);
        }

        public override Task<Contact> GetContact(GetContactRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Getting contact with UID {request.Uid} via gRPC.");
            var contactDto = _utilities.GetContact(request.Uid); // CALL non-static method
            if (contactDto == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Contact with UID {request.Uid} not found."));
            }
            return Task.FromResult(MapContactDtoToGrpcContact(contactDto));
        }

        public override Task<OperationResponse> InsertContact(PersonRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Inserting new contact via gRPC.");
            try
            {
                _utilities.InsertContact( // CALL non-static method
                    request.FirstName,
                    request.LastName,
                    request.PrefixId,
                    request.SuffixId,
                    request.Address,
                    request.City,
                    request.State,
                    request.Zip
                );
                return Task.FromResult(new OperationResponse { Success = true, Message = "Contact inserted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting contact via gRPC.");
                throw new RpcException(new Status(StatusCode.Internal, $"Failed to insert contact: {ex.Message}"));
            }
        }

        public override Task<OperationResponse> UpdateContact(Contact request, ServerCallContext context)
        {
            _logger.LogInformation($"Updating contact with UID {request.Uid} via gRPC.");
            try
            {
                _utilities.UpdateContact( // CALL non-static method
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
                return Task.FromResult(new OperationResponse { Success = true, Message = "Contact updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact via gRPC.");
                throw new RpcException(new Status(StatusCode.Internal, $"Failed to update contact: {ex.Message}"));
            }
        }

        private ContactsApi.Grpc.Contact MapContactDtoToGrpcContact(ContactDto dto)
        {
            return new ContactsApi.Grpc.Contact
            {
                Uid = dto.Uid,
                PrefixId = dto.PrefixId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                SuffixId = dto.SuffixId,
                Address = dto.Address ?? string.Empty,
                City = dto.City ?? string.Empty,
                State = dto.State ?? string.Empty,
                Zip = dto.Zip ?? string.Empty
            };
        }
    }
}
