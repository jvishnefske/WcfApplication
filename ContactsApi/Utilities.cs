using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient; // CHANGE THIS LINE from System.Data.SqlClient
using System.Diagnostics;
using ContactsApi.Models;

namespace ContactsApi
{
    public class Utilities
    {
        static Utilities()
        {
            System.Diagnostics.Trace.WriteLine("starting tracing");
        }

        // IMPORTANT: Update this connection string to your actual SQL Server details
        // For local SQL Express: "Data Source=.\\SQLEXPRESS;Initial Catalog=ContactsDb;Integrated Security=True;TrustServerCertificate=True;"
        // For Docker/Linux SQL Server: "Server=localhost,1433;Database=ContactsDb;User Id=SA;Password=YourStrongPassword;TrustServerCertificate=True;"
        // Or retrieve from configuration (e.g., appsettings.json)
        const String connectionString =
            "Server=localhost;Database=personnel;User Id=sa;Password=7529;TrustServerCertificate=True;"; // UPDATED connection string

        private static SqlConnection getConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static ContactDto GetContact(int uid)
        {
            ContactDto contact = null;
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                // UPDATED query and parameter handling
                SqlCommand cmd = new SqlCommand("SELECT Uid, PrefixId, FirstName, LastName, SuffixId, Address, City, State, Zip FROM Contacts WHERE Uid = @Uid", conn);
                cmd.Parameters.AddWithValue("@Uid", uid);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        contact = new ContactDto
                        {
                            Uid = reader.GetInt32(0),
                            PrefixId = reader.GetInt32(1),
                            FirstName = reader.GetString(2),
                            LastName = reader.GetString(3),
                            SuffixId = reader.GetInt32(4),
                            Address = reader.IsDBNull(5) ? null : reader.GetString(5), // Handle DBNull
                            City = reader.IsDBNull(6) ? null : reader.GetString(6),     // Handle DBNull
                            State = reader.IsDBNull(7) ? null : reader.GetString(7),   // Handle DBNull
                            Zip = reader.IsDBNull(8) ? null : reader.GetString(8)       // Handle DBNull
                        };
                    }
                }
            }
            return contact;
        }

        public static List<ContactDto> GetAllContacts()
        {
            System.Diagnostics.Trace.WriteLine("Refreshing contacts.");

            List<ContactDto> contacts = new List<ContactDto>();
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                // UPDATED query
                SqlCommand cmd = new SqlCommand("SELECT Uid, PrefixId, FirstName, LastName, SuffixId, Address, City, State, Zip FROM Contacts", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contacts.Add(new ContactDto
                        {
                            Uid = reader.GetInt32(0),
                            PrefixId = reader.GetInt32(1),
                            FirstName = reader.GetString(2),
                            LastName = reader.GetString(3),
                            SuffixId = reader.GetInt32(4),
                            Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                            City = reader.IsDBNull(6) ? null : reader.GetString(6),
                            State = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Zip = reader.IsDBNull(8) ? null : reader.GetString(8)
                        });
                    }
                }
            }
            return contacts;
        }

        public static List<LookupDto> GetPrefixes()
        {
            List<LookupDto> prefixes = new List<LookupDto>();
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                // UPDATED query
                SqlCommand cmd = new SqlCommand("SELECT Id, Description FROM Prefixes", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        prefixes.Add(new LookupDto
                        {
                            Id = reader.GetInt32(0),
                            Description = reader.GetString(1)
                        });
                    }
                }
                return prefixes;
            }
        }

        public static List<LookupDto> GetSuffixes()
        {
            List<LookupDto> suffixes = new List<LookupDto>();
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                // UPDATED query
                SqlCommand cmd = new SqlCommand("SELECT Id, Description FROM Suffixes", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suffixes.Add(new LookupDto
                        {
                            Id = reader.GetInt32(0),
                            Description = reader.GetString(1)
                        });
                    }
                }
                return suffixes;
            }
        }

        // UPDATED signature to include Address, City, State, Zip
        public static void UpdateContact(Int32 uid, String firstName, String lastName, Int32 prefix, Int32 suffix, String address, String city, String state, String zip)
        {
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                // UPDATED query and parameters
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Contacts SET FirstName = @FirstName, LastName = @LastName, PrefixId = @PrefixId, SuffixId = @SuffixId, Address = @Address, City = @City, State = @State, Zip = @Zip WHERE Uid = @Uid", conn);
                cmd.Parameters.AddWithValue("@Uid", uid);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@PrefixId", prefix);
                cmd.Parameters.AddWithValue("@SuffixId", suffix);
                cmd.Parameters.AddWithValue("@Address", (object)address ?? DBNull.Value); // Handle nulls for optional fields
                cmd.Parameters.AddWithValue("@City", (object)city ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@State", (object)state ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Zip", (object)zip ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        // UPDATED signature to include Address, City, State, Zip
        public static void InsertContact(String firstName, String lastName, Int32 prefix, Int32 suffix, String address, String city, String state, String zip)
        {
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                // UPDATED query and parameters
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Contacts (FirstName, LastName, PrefixId, SuffixId, Address, City, State, Zip) VALUES (@FirstName, @LastName, @PrefixId, @SuffixId, @Address, @City, @State, @Zip)", conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@PrefixId", prefix);
                cmd.Parameters.AddWithValue("@SuffixId", suffix);
                cmd.Parameters.AddWithValue("@Address", (object)address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@City", (object)city ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@State", (object)state ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Zip", (object)zip ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
