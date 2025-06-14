using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using ContactsApi.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

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
            return new SqliteConnection(_connectionString);
        }

        public async Task InitializeDatabaseAsync()
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
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
                await command.ExecuteNonQueryAsync();

                // Create Prefixes table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Prefixes (
                        Id INTEGER PRIMARY KEY,
                        Description TEXT NOT NULL UNIQUE
                    );";
                await command.ExecuteNonQueryAsync();

                // Create Suffixes table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Suffixes (
                        Id INTEGER PRIMARY KEY,
                        Description TEXT NOT NULL UNIQUE
                    );";
                await command.ExecuteNonQueryAsync();

                // Populate Prefixes if empty
                command.CommandText = "SELECT COUNT(*) FROM Prefixes;";
                if (Convert.ToInt32(await command.ExecuteScalarAsync()) == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Prefixes (Id, Description) VALUES
                        (1, 'Mr.'),
                        (2, 'Ms.'),
                        (3, 'Mrs.'),
                        (4, 'Dr.'),
                        (5, 'Prof.');";
                    await command.ExecuteNonQueryAsync();
                }

                // Populate Suffixes if empty
                command.CommandText = "SELECT COUNT(*) FROM Suffixes;";
                if (Convert.ToInt32(await command.ExecuteScalarAsync()) == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Suffixes (Id, Description) VALUES
                        (1, 'Jr.'),
                        (2, 'Sr.'),
                        (3, 'II'),
                        (4, 'III'),
                        (5, 'IV');";
                    await command.ExecuteNonQueryAsync();
                }
            }
            System.Diagnostics.Trace.WriteLine($"SQLite database initialized at: {_connectionString}");
        }

        // ADD VIRTUAL KEYWORD TO ALL PUBLIC ASYNC METHODS
        public virtual async Task<ContactDto?> GetContactAsync(int uid)
        {
            ContactDto? contact = null;
            using (SqliteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                SqliteCommand cmd = new SqliteCommand("SELECT Uid, PrefixId, FirstName, LastName, SuffixId, Address, City, State, Zip FROM Contacts WHERE Uid = @Uid", conn);
                cmd.Parameters.AddWithValue("@Uid", uid);
                using (SqliteDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
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

        // ADD VIRTUAL KEYWORD
        public virtual async Task<List<ContactDto>> GetAllContactsAsync()
        {
            System.Diagnostics.Trace.WriteLine("Refreshing contacts.");

            List<ContactDto> contacts = new List<ContactDto>();
            using (SqliteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                SqliteCommand cmd = new SqliteCommand("SELECT Uid, PrefixId, FirstName, LastName, SuffixId, Address, City, State, Zip FROM Contacts", conn);
                using (SqliteDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
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

        // ADD VIRTUAL KEYWORD
        public virtual async Task<List<LookupDto>> GetPrefixesAsync()
        {
            List<LookupDto> prefixes = new List<LookupDto>();
            using (SqliteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                SqliteCommand cmd = new SqliteCommand("SELECT Id, Description FROM Prefixes", conn);
                using (SqliteDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        prefixes.Add(new LookupDto
                        {
                            Id = reader.GetInt32(0),
                            Description = reader.IsDBNull(1) ? null : reader.GetString(1)
                        });
                    }
                }
                return prefixes;
            }
        }

        // ADD VIRTUAL KEYWORD
        public virtual async Task<List<LookupDto>> GetSuffixesAsync()
        {
            List<LookupDto> suffixes = new List<LookupDto>();
            using (SqliteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                SqliteCommand cmd = new SqliteCommand("SELECT Id, Description FROM Suffixes", conn);
                using (SqliteDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        suffixes.Add(new LookupDto
                        {
                            Id = reader.GetInt32(0),
                            Description = reader.IsDBNull(1) ? null : reader.GetString(1)
                        });
                    }
                }
                return suffixes;
            }
        }

        // ADD VIRTUAL KEYWORD
        public virtual async Task UpdateContactAsync(Int32 uid, String firstName, String lastName, Int32 prefix, Int32 suffix, String? address, String? city, String? state, String? zip)
        {
            using (SqliteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                SqliteCommand cmd = new SqliteCommand(
                    "UPDATE Contacts SET FirstName = @FirstName, LastName = @LastName, PrefixId = @PrefixId, SuffixId = @SuffixId, Address = @Address, City = @City, State = @State, Zip = @Zip WHERE Uid = @Uid", conn);
                cmd.Parameters.AddWithValue("@Uid", uid);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@PrefixId", prefix);
                cmd.Parameters.AddWithValue("@SuffixId", suffix);
                cmd.Parameters.AddWithValue("@Address", (object?)address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@City", (object?)city ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@State", (object?)state ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Zip", (object?)zip ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ADD VIRTUAL KEYWORD
        public virtual async Task InsertContactAsync(String firstName, String lastName, Int32 prefix, Int32 suffix, String? address, String? city, String? state, String? zip)
        {
            using (SqliteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                // Removed Uid from INSERT as it's AUTOINCREMENT
                SqliteCommand cmd = new SqliteCommand(
                    "INSERT INTO Contacts (FirstName, LastName, PrefixId, SuffixId, Address, City, State, Zip) VALUES (@FirstName, @LastName, @PrefixId, @SuffixId, @Address, @City, @State, @Zip)", conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@PrefixId", prefix);
                cmd.Parameters.AddWithValue("@SuffixId", suffix);
                cmd.Parameters.AddWithValue("@Address", (object?)address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@City", (object?)city ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@State", (object?)state ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Zip", (object?)zip ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
