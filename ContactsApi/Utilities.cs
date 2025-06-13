using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using ContactsApi.Models;
using System.IO;
using Microsoft.Extensions.Configuration; // ADD THIS USING

namespace ContactsApi
{
    public class Utilities
    {
        private readonly string _connectionString;

        public Utilities(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                                throw new InvalidOperationException("DefaultConnection connection string not found.");
            System.Diagnostics.Trace.WriteLine("Utilities initialized with connection string from config.");
        }

        private SqliteConnection getConnection()
        {
            return new SqliteConnection(_connectionString); // Use the injected connection string
        }

        public void InitializeDatabase()
        {
            using (var connection = getConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Create Contacts table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Contacts (
                        Uid INTEGER PRIMARY KEY AUTOINCREMENT,
                        PrefixId INTEGER NOT NULL,
                        FirstName TEXT NOT NULL,
                        LastName TEXT NOT NULL,
                        SuffixId INTEGER NOT NULL,
                        Address TEXT,
                        City TEXT,
                        State TEXT,
                        Zip TEXT
                    );";
                command.ExecuteNonQuery();

                // Create Prefixes table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Prefixes (
                        Id INTEGER PRIMARY KEY,
                        Description TEXT NOT NULL UNIQUE
                    );";
                command.ExecuteNonQuery();

                // Create Suffixes table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Suffixes (
                        Id INTEGER PRIMARY KEY,
                        Description TEXT NOT NULL UNIQUE
                    );";
                command.ExecuteNonQuery();

                // Populate Prefixes if empty
                command.CommandText = "SELECT COUNT(*) FROM Prefixes;";
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Prefixes (Id, Description) VALUES
                        (1, 'Mr.'),
                        (2, 'Ms.'),
                        (3, 'Mrs.'),
                        (4, 'Dr.'),
                        (5, 'Prof.');";
                    command.ExecuteNonQuery();
                }

                // Populate Suffixes if empty
                command.CommandText = "SELECT COUNT(*) FROM Suffixes;";
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Suffixes (Id, Description) VALUES
                        (1, 'Jr.'),
                        (2, 'Sr.'),
                        (3, 'II'),
                        (4, 'III'),
                        (5, 'IV');";
                    command.ExecuteNonQuery();
                }
            }
            System.Diagnostics.Trace.WriteLine($"SQLite database initialized at: {_connectionString}");
        }


        public ContactDto? GetContact(int uid) // Changed return type to nullable
        {
            ContactDto? contact = null;
            using (SqliteConnection conn = getConnection())
            {
                conn.Open();
                SqliteCommand cmd = new SqliteCommand("SELECT Uid, PrefixId, FirstName, LastName, SuffixId, Address, City, State, Zip FROM Contacts WHERE Uid = @Uid", conn);
                cmd.Parameters.AddWithValue("@Uid", uid);
                using (SqliteDataReader reader = cmd.ExecuteReader())
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
                            Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                            City = reader.IsDBNull(6) ? null : reader.GetString(6),
                            State = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Zip = reader.IsDBNull(8) ? null : reader.GetString(8)
                        };
                    }
                }
            }
            return contact;
        }

        public List<ContactDto> GetAllContacts()
        {
            System.Diagnostics.Trace.WriteLine("Refreshing contacts.");

            List<ContactDto> contacts = new List<ContactDto>();
            using (SqliteConnection conn = getConnection())
            {
                conn.Open();
                SqliteCommand cmd = new SqliteCommand("SELECT Uid, PrefixId, FirstName, LastName, SuffixId, Address, City, State, Zip FROM Contacts", conn);
                using (SqliteDataReader reader = cmd.ExecuteReader())
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

        public List<LookupDto> GetPrefixes()
        {
            List<LookupDto> prefixes = new List<LookupDto>();
            using (SqliteConnection conn = getConnection())
            {
                conn.Open();
                SqliteCommand cmd = new SqliteCommand("SELECT Id, Description FROM Prefixes", conn);
                using (SqliteDataReader reader = cmd.ExecuteReader())
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

        public List<LookupDto> GetSuffixes()
        {
            List<LookupDto> suffixes = new List<LookupDto>();
            using (SqliteConnection conn = getConnection())
            {
                conn.Open();
                SqliteCommand cmd = new SqliteCommand("SELECT Id, Description FROM Suffixes", conn);
                using (SqliteDataReader reader = cmd.ExecuteReader())
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

        public void UpdateContact(Int32 uid, String firstName, String lastName, Int32 prefix, Int32 suffix, String address, String city, String state, String zip)
        {
            using (SqliteConnection conn = getConnection())
            {
                conn.Open();
                SqliteCommand cmd = new SqliteCommand(
                    "UPDATE Contacts SET FirstName = @FirstName, LastName = @LastName, PrefixId = @PrefixId, SuffixId = @SuffixId, Address = @Address, City = @City, State = @State, Zip = @Zip WHERE Uid = @Uid", conn);
                cmd.Parameters.AddWithValue("@Uid", uid);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@PrefixId", prefix);
                cmd.Parameters.AddWithValue("@SuffixId", suffix);
                cmd.Parameters.AddWithValue("@Address", address); // SQLite handles null directly
                cmd.Parameters.AddWithValue("@City", city);
                cmd.Parameters.AddWithValue("@State", state);
                cmd.Parameters.AddWithValue("@Zip", zip);

                cmd.ExecuteNonQuery();
            }
        }

        public void InsertContact(String firstName, String lastName, Int32 prefix, Int32 suffix, String address, String city, String state, String zip)
        {
            using (SqliteConnection conn = getConnection())
            {
                conn.Open();
                // Removed Uid from INSERT as it's AUTOINCREMENT
                SqliteCommand cmd = new SqliteCommand(
                    "INSERT INTO Contacts (FirstName, LastName, PrefixId, SuffixId, Address, City, State, Zip) VALUES (@FirstName, @LastName, @PrefixId, @SuffixId, @Address, @City, @State, @Zip)", conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@PrefixId", prefix);
                cmd.Parameters.AddWithValue("@SuffixId", suffix);
                cmd.Parameters.AddWithValue("@Address", address); // SQLite handles null directly
                cmd.Parameters.AddWithValue("@City", city);
                cmd.Parameters.AddWithValue("@State", state);
                cmd.Parameters.AddWithValue("@Zip", zip);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
