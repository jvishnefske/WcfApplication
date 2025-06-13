using System;
using System.Data;
using System.Collections.Generic;

namespace WcfClient.Models
{
    public class Contact
    {
        public int Uid { get; set; }
        public string Prefix { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        // Helper method to convert a list of Contacts to a DataTable
        public static DataTable ToDataTable(List<Contact> contacts)
        {
            DataTable dt = new DataTable();

            if (contacts == null || contacts.Count == 0)
            {
                // Define columns even if no data, to match original DataTable structure
                dt.Columns.Add("uid", typeof(int));
                dt.Columns.Add("prefix", typeof(string));
                dt.Columns.Add("first_name", typeof(string));
                dt.Columns.Add("last_name", typeof(string));
                dt.Columns.Add("suffix", typeof(string));
                dt.Columns.Add("address", typeof(string));
                dt.Columns.Add("city", typeof(string));
                dt.Columns.Add("state", typeof(string));
                dt.Columns.Add("zip", typeof(string));
                return dt;
            }

            // Create columns based on the first contact's properties
            dt.Columns.Add("uid", typeof(int));
            dt.Columns.Add("pre.prefix", typeof(string)); // Matches original SQL alias
            dt.Columns.Add("first_name", typeof(string));
            dt.Columns.Add("last_name", typeof(string));
            dt.Columns.Add("suffix", typeof(string));
            dt.Columns.Add("address", typeof(string));
            dt.Columns.Add("city", typeof(string));
            dt.Columns.Add("state", typeof(string));
            dt.Columns.Add("zip", typeof(string));

            foreach (var contact in contacts)
            {
                dt.Rows.Add(
                    contact.Uid,
                    contact.Prefix,
                    contact.FirstName,
                    contact.LastName,
                    contact.Suffix,
                    contact.Address,
                    contact.City,
                    contact.State,
                    contact.Zip
                );
            }
            return dt;
        }
    }
}
