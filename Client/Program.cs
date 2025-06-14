using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactsApi.Grpc; // Corrected namespace for generated gRPC types
using Google.Protobuf.WellKnownTypes;
using Grpc.Core; // For RpcException

namespace Client // Corrected namespace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Contacts Client Application");
            Console.WriteLine("---------------------------");

            try
            {
                await RunClientOperations();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An unhandled error occurred: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                ApiClient.Shutdown();
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
            }
        }

        static async Task RunClientOperations()
        {
            while (true)
            {
                Console.WriteLine("\nChoose an operation:");
                Console.WriteLine("1. List All Contacts");
                Console.WriteLine("2. Get Contact by UID");
                Console.WriteLine("3. Add New Contact");
                Console.WriteLine("4. Update Existing Contact");
                Console.WriteLine("5. Exit");
                Console.Write("Enter choice: ");

                string? choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await ListAllContacts();
                            break;
                        case "2":
                            await GetContactByUid();
                            break;
                        case "3":
                            await AddNewContact();
                            break;
                        case "4":
                            await UpdateExistingContact();
                            break;
                        case "5":
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (RpcException rpcEx)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"gRPC Error (Status: {rpcEx.Status.StatusCode}): {rpcEx.Status.Detail}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An unexpected error occurred during operation: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        static async Task ListAllContacts()
        {
            Console.WriteLine("\n--- Listing All Contacts ---");
            var response = await ApiClient.ContactsClient.GetAllContactsAsync(new Empty());
            if (response.Contacts.Count == 0)
            {
                Console.WriteLine("No contacts found.");
                return;
            }

            foreach (var contact in response.Contacts)
            {
                Console.WriteLine($"UID: {contact.Uid}, Name: {contact.FirstName} {contact.LastName}, Address: {contact.Address}, City: {contact.City}, State: {contact.State}, Zip: {contact.Zip}");
            }
        }

        static async Task GetContactByUid()
        {
            Console.WriteLine("\n--- Get Contact by UID ---");
            int uid = ReadIntInput("Enter Contact UID: ", 0);
            if (uid <= 0)
            {
                Console.WriteLine("Invalid UID.");
                return;
            }

            var request = new GetContactRequest { Uid = uid };
            var contact = await ApiClient.ContactsClient.GetContactAsync(request);
            
            // If RpcException with NotFound status is thrown, it's caught by RunClientOperations
            Console.WriteLine($"UID: {contact.Uid}, Name: {contact.FirstName} {contact.LastName}, Address: {contact.Address}, City: {contact.City}, State: {contact.State}, Zip: {contact.Zip}");
        }

        static async Task AddNewContact()
        {
            Console.WriteLine("\n--- Add New Contact ---");
            var personRequest = await GetPersonRequestFromConsole(null);
            if (personRequest == null) return;

            var response = await ApiClient.ContactsClient.InsertContactAsync(personRequest);
            if (response.Success)
            {
                Console.WriteLine($"Success: {response.Message}");
            }
            else
            {
                Console.WriteLine($"Failed: {response.Message}");
            }
        }

        static async Task UpdateExistingContact()
        {
            Console.WriteLine("\n--- Update Existing Contact ---");
            int uid = ReadIntInput("Enter Contact UID to update: ", 0);
            if (uid <= 0)
            {
                Console.WriteLine("Invalid UID.");
                return;
            }

            var existingContact = await ApiClient.ContactsClient.GetContactAsync(new GetContactRequest { Uid = uid });
            if (existingContact == null)
            {
                Console.WriteLine($"Contact with UID {uid} not found.");
                return;
            }

            var personRequest = await GetPersonRequestFromConsole(existingContact);
            if (personRequest == null) return;

            var contactToUpdate = new Contact
            {
                Uid = uid,
                FirstName = personRequest.FirstName,
                LastName = personRequest.LastName,
                PrefixId = personRequest.PrefixId,
                SuffixId = personRequest.SuffixId,
                Address = personRequest.Address,
                City = personRequest.City,
                State = personRequest.State,
                Zip = personRequest.Zip
            };

            var response = await ApiClient.ContactsClient.UpdateContactAsync(contactToUpdate);
            if (response.Success)
            {
                Console.WriteLine($"Success: {response.Message}");
            }
            else
            {
                Console.WriteLine($"Failed: {response.Message}");
            }
        }

        static async Task<PersonRequest?> GetPersonRequestFromConsole(Contact? existingContact)
        {
            var prefixes = await GetLookups((request, options) => ApiClient.LookupsClient.GetPrefixesAsync(request, options));
            var suffixes = await GetLookups((request, options) => ApiClient.LookupsClient.GetSuffixesAsync(request, options));
 
            string firstName = ReadInput("First Name", existingContact?.FirstName ?? string.Empty);
            string lastName = ReadInput("Last Name", existingContact?.LastName ?? string.Empty);
            int prefixId = GetLookupIdFromConsole("Prefix", prefixes, existingContact?.PrefixId ?? 0);
            int suffixId = GetLookupIdFromConsole("Suffix", suffixes, existingContact?.SuffixId ?? 0);
            string address = ReadInput("Address", existingContact?.Address ?? string.Empty);
            string city = ReadInput("City", existingContact?.City ?? string.Empty);
            string state = ReadInput("State", existingContact?.State ?? string.Empty);
            string zip = ReadInput("Zip", existingContact?.Zip ?? string.Empty);

            return new PersonRequest
            {
                FirstName = firstName,
                LastName = lastName,
                PrefixId = prefixId,
                SuffixId = suffixId,
                Address = address,
                City = city,
                State = state,
                Zip = zip
            };
        }

        static async Task<List<Lookup>> GetLookups(Func<Empty, CallOptions?, AsyncUnaryCall<GetLookupsResponse>> getLookupsFunc)
        {
            try
            {
                var response = await getLookupsFunc(new Empty(), null);
                return response.Lookups.ToList();
            }
            catch (RpcException rpcEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error fetching lookups (Status: {rpcEx.Status.StatusCode}): {rpcEx.Status.Detail}");
                Console.ResetColor();
                return new List<Lookup>();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An unexpected error occurred fetching lookups: {ex.Message}");
                Console.ResetColor();
                return new List<Lookup>();
            }
        }

        static int GetLookupIdFromConsole(string lookupType, List<Lookup> lookups, int currentValue)
        {
            if (lookups == null || !lookups.Any())
            {
                Console.WriteLine($"No {lookupType} options available.");
                return 0;
            }

            Console.WriteLine($"\nAvailable {lookupType}s:");
            foreach (var lookup in lookups)
            {
                Console.WriteLine($"{lookup.Id}. {lookup.Description}");
            }

            int selectedId = ReadIntInput($"Enter {lookupType} ID (current: {currentValue})", currentValue);
            while (!lookups.Any(l => l.Id == selectedId))
            {
                Console.WriteLine("Invalid ID. Please choose from the list above.");
                selectedId = ReadIntInput($"Enter {lookupType} ID (current: {currentValue})", currentValue);
            }
            return selectedId;
        }

        static string ReadInput(string prompt, string currentValue)
        {
            Console.Write($"{prompt} (current: {currentValue}): ");
            string? input = Console.ReadLine();
            return string.IsNullOrEmpty(input) ? currentValue : input;
        }

        static int ReadIntInput(string prompt, int currentValue)
        {
            Console.Write($"{prompt} (current: {currentValue}): ");
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int result))
            {
                return result;
            }
            return currentValue; // Return current value if input is invalid
        }
    }
}
