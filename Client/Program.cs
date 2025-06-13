using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes; // For Empty message
using Client.Grpc; // Namespace for generated gRPC client code

namespace Client // Corrected namespace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Contacts gRPC Client");
            Console.WriteLine("--------------------");

            try
            {
                await RunClientOperations();
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"gRPC Error: {ex.Status.StatusCode} - {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                ApiClient.Shutdown(); // Ensure the gRPC channel is disposed
                Console.WriteLine("\nPress any key to exit...");
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
                Console.WriteLine("4. Update Contact");
                Console.WriteLine("5. Exit");
                Console.Write("Enter your choice: ");

                string? choice = Console.ReadLine();
                Console.WriteLine();

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
                        return; // Exit the application
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static async Task ListAllContacts()
        {
            Console.WriteLine("--- All Contacts ---");
            try
            {
                var response = await ApiClient.ContactsClient.GetAllContactsAsync(new Empty());
                if (response.Contacts.Any())
                {
                    foreach (var contact in response.Contacts)
                    {
                        Console.WriteLine($"UID: {contact.Uid}, Name: {contact.FirstName} {contact.LastName}, Address: {contact.Address}, {contact.City}, {contact.State} {contact.Zip}");
                    }
                }
                else
                {
                    Console.WriteLine("No contacts found.");
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Error listing contacts: {ex.Status.Detail}");
            }
        }

        static async Task GetContactByUid()
        {
            Console.Write("Enter Contact UID: ");
            if (int.TryParse(Console.ReadLine(), out int uid))
            {
                try
                {
                    var request = new GetContactRequest { Uid = uid };
                    var contact = await ApiClient.ContactsClient.GetContactAsync(request);
                    Console.WriteLine($"--- Contact Details (UID: {contact.Uid}) ---");
                    Console.WriteLine($"First Name: {contact.FirstName}");
                    Console.WriteLine($"Last Name: {contact.LastName}");
                    Console.WriteLine($"Prefix ID: {contact.PrefixId}");
                    Console.WriteLine($"Suffix ID: {contact.SuffixId}");
                    Console.WriteLine($"Address: {contact.Address}");
                    Console.WriteLine($"City: {contact.City}");
                    Console.WriteLine($"State: {contact.State}");
                    Console.WriteLine($"Zip: {contact.Zip}");
                }
                catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound)
                {
                    Console.WriteLine($"Contact with UID {uid} not found.");
                }
                catch (RpcException ex)
                {
                    Console.WriteLine($"Error getting contact: {ex.Status.Detail}");
                }
            }
            else
            {
                Console.WriteLine("Invalid UID.");
            }
        }

        static async Task AddNewContact()
        {
            Console.WriteLine("--- Add New Contact ---");
            var personRequest = await GetPersonRequestFromConsole();
            if (personRequest == null) return;

            try
            {
                var response = await ApiClient.ContactsClient.InsertContactAsync(personRequest);
                if (response.Success)
                {
                    Console.WriteLine("Contact added successfully!");
                }
                else
                {
                    Console.WriteLine($"Failed to add contact: {response.Message}");
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Error adding contact: {ex.Status.Detail}");
            }
        }

        static async Task UpdateExistingContact()
        {
            Console.Write("Enter UID of contact to update: ");
            if (!int.TryParse(Console.ReadLine(), out int uid))
            {
                Console.WriteLine("Invalid UID.");
                return;
            }

            try
            {
                // First, get the existing contact details
                var existingContact = await ApiClient.ContactsClient.GetContactAsync(new GetContactRequest { Uid = uid });

                Console.WriteLine($"--- Update Contact (UID: {uid}) ---");
                Console.WriteLine("Enter new values (leave blank to keep current value):");

                var updatedContact = new Contact
                {
                    Uid = uid,
                    FirstName = ReadInput($"First Name ({existingContact.FirstName}): ", existingContact.FirstName),
                    LastName = ReadInput($"Last Name ({existingContact.LastName}): ", existingContact.LastName),
                    PrefixId = ReadIntInput($"Prefix ID ({existingContact.PrefixId}): ", existingContact.PrefixId),
                    SuffixId = ReadIntInput($"Suffix ID ({existingContact.SuffixId}): ", existingContact.SuffixId),
                    Address = ReadInput($"Address ({existingContact.Address}): ", existingContact.Address),
                    City = ReadInput($"City ({existingContact.City}): ", existingContact.City),
                    State = ReadInput($"State ({existingContact.State}): ", existingContact.State),
                    Zip = ReadInput($"Zip ({existingContact.Zip}): ", existingContact.Zip)
                };

                var response = await ApiClient.ContactsClient.UpdateContactAsync(updatedContact);
                if (response.Success)
                {
                    Console.WriteLine("Contact updated successfully!");
                }
                else
                {
                    Console.WriteLine($"Failed to update contact: {response.Message}");
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound)
            {
                Console.WriteLine($"Contact with UID {uid} not found.");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Error updating contact: {ex.Status.Detail}");
            }
        }

        static async Task<PersonRequest?> GetPersonRequestFromConsole()
        {
            var prefixes = await GetLookups(ApiClient.LookupsClient.GetPrefixesAsync);
            var suffixes = await GetLookups(ApiClient.LookupsClient.GetSuffixesAsync);

            Console.Write("First Name: ");
            string? firstName = Console.ReadLine();
            Console.Write("Last Name: ");
            string? lastName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                Console.WriteLine("First Name and Last Name are required.");
                return null;
            }

            int prefixId = GetLookupIdFromConsole("Prefix", prefixes);
            int suffixId = GetLookupIdFromConsole("Suffix", suffixes);

            Console.Write("Address: ");
            string? address = Console.ReadLine();
            Console.Write("City: ");
            string? city = Console.ReadLine();
            Console.Write("State: ");
            string? state = Console.ReadLine();
            Console.Write("Zip: ");
            string? zip = Console.ReadLine();

            return new PersonRequest
            {
                FirstName = firstName,
                LastName = lastName,
                PrefixId = prefixId,
                SuffixId = suffixId,
                Address = address ?? string.Empty,
                City = city ?? string.Empty,
                State = state ?? string.Empty,
                Zip = zip ?? string.Empty
            };
        }

        static async Task<List<Lookup>> GetLookups(Func<Empty, CallOptions?, AsyncUnaryCall<GetLookupsResponse>> getLookupsFunc)
        {
            try
            {
                var response = await getLookupsFunc(new Empty(), null);
                return response.Lookups.ToList();
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Error loading lookups: {ex.Status.Detail}");
                return new List<Lookup>();
            }
        }

        static int GetLookupIdFromConsole(string lookupType, List<Lookup> lookups)
        {
            if (!lookups.Any())
            {
                Console.WriteLine($"No {lookupType} options available.");
                return 0; // Or handle as an error
            }

            Console.WriteLine($"Available {lookupType}s:");
            foreach (var lookup in lookups)
            {
                Console.WriteLine($"  {lookup.Id}. {lookup.Description}");
            }

            while (true)
            {
                Console.Write($"Enter {lookupType} ID: ");
                if (int.TryParse(Console.ReadLine(), out int id) && lookups.Any(l => l.Id == id))
                {
                    return id;
                }
                Console.WriteLine("Invalid ID. Please choose from the list.");
            }
        }

        static string ReadInput(string prompt, string currentValue)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? currentValue : input;
        }

        static int ReadIntInput(string prompt, int currentValue)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) return currentValue;
            if (int.TryParse(input, out int result)) return result;
            Console.WriteLine("Invalid number. Keeping current value.");
            return currentValue;
        }
    }
}
