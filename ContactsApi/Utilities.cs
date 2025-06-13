using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using ContactsApi.Models; // Add this line to use DTOs

namespace ContactsApi
{
    public class Utilities
    {
        static Utilities()
        {
            System.Diagnostics.Trace.WriteLine("starting tracing");
        }

        // IMPORTANT: Update this connection string to your actual SQL Server details
        const String connectionString =
            "server=BAKER\\SQLEXPRESS;" +
            "Database=personnel;" +
            "User Id=sa;" +
            "Password=7529;";

        private static SqlConnection getConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static ContactDto GetContact(int uid)
        {
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText =
                    @"select uid,prefixid,first_name,last_name,suffixid,address,city,state,zip
                    from personnel.dbo.employees 
                    where uid = @uid;";
                command.Parameters.AddWithValue("@uid", uid);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ContactDto
                        {
                            Uid = reader.GetInt32(reader.GetOrdinal("uid")),
                            PrefixId = reader.GetInt32(reader.GetOrdinal("prefixid")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            SuffixId = reader.GetInt32(reader.GetOrdinal("suffixid")),
                            Address = reader.GetString(reader.GetOrdinal("address")),
                            City = reader.GetString(reader.GetOrdinal("city")),
                            State = reader.GetString(reader.GetOrdinal("state")),
                            Zip = reader.GetString(reader.GetOrdinal("zip"))
                        };
                    }
                    return null;
                }
            }
        }

        public static List<ContactDto> GetAllContacts()
        {
            System.Diagnostics.Trace.WriteLine("Refreshing contacts.");

            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                SqlCommand command = conn.CreateCommand();
                command.CommandText =
                    @"select uid,prefixid,first_name,last_name,suffixid,address,city,state,zip
                    from personnel.dbo.employees;";

                List<ContactDto> contacts = new List<ContactDto>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contacts.Add(new ContactDto
                        {
                            Uid = reader.GetInt32(reader.GetOrdinal("uid")),
                            PrefixId = reader.GetInt32(reader.GetOrdinal("prefixid")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            SuffixId = reader.GetInt32(reader.GetOrdinal("suffixid")),
                            Address = reader.GetString(reader.GetOrdinal("address")),
                            City = reader.GetString(reader.GetOrdinal("city")),
                            State = reader.GetString(reader.GetOrdinal("state")),
                            Zip = reader.GetString(reader.GetOrdinal("zip"))
                        });
                    }
                }
                return contacts;
            }
        }

        public static List<LookupDto> GetPrefixes()
        {
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                const String query = "select * from personnel.prefixes";
                SqlCommand command = new SqlCommand(query, conn);
                List<LookupDto> prefixes = new List<LookupDto>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        prefixes.Add(new LookupDto
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("prefixid")),
                            Description = reader.GetString(reader.GetOrdinal("description"))
                        });
                    }
                }
                return prefixes;
            }
        }

        public static List<LookupDto> GetSuffixes()
        {
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                const String query = "select * from personnel.suffixes";
                SqlCommand command = new SqlCommand(query, conn);
                List<LookupDto> suffixes = new List<LookupDto>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suffixes.Add(new LookupDto
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("suffixid")),
                            Description = reader.GetString(reader.GetOrdinal("description"))
                        });
                    }
                }
                return suffixes;
            }
        }

        public static void UpdateContact(Int32 uid, String firstName, String lastName, Int32 prefix, Int32 suffix, String address, String city, String state, String zip)
        {
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                const String query =
                    @"UPDATE personnel.dbo.employees 
                      SET prefixid = @prefixid, first_name = @firstName, last_name = @lastName, 
                          suffixid = @suffixid, address = @address, city = @city, state = @state, zip = @zip
                      WHERE uid = @uid;";

                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@uid", uid);
                command.Parameters.AddWithValue("@prefixid", prefix);
                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@suffixid", suffix);
                command.Parameters.AddWithValue("@address", address);
                command.Parameters.AddWithValue("@city", city);
                command.Parameters.AddWithValue("@state", state);
                command.Parameters.AddWithValue("@zip", zip);

                command.ExecuteNonQuery();
            }
        }

        public static void InsertContact(String firstName, String lastName, Int32 prefix, Int32 suffix, String address, String city, String state, String zip)
        {
            using (SqlConnection conn = getConnection())
            {
                conn.Open();
                const String query =
                    @"INSERT INTO personnel.dbo.employees 
                      (prefixid, first_name, last_name, suffixid, address, city, state, zip)
                      VALUES (@prefixid, @firstName, @lastName, @suffixid, @address, @city, @state, @zip);";

                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@prefixid", prefix);
                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@suffixid", suffix);
                command.Parameters.AddWithValue("@address", address);
                command.Parameters.AddWithValue("@city", city);
                command.Parameters.AddWithValue("@state", state);
                command.Parameters.AddWithValue("@zip", zip);

                command.ExecuteNonQuery();
            }
        }
    }
}
