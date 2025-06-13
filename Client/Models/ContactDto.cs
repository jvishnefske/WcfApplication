using System.Data; // Needed for ToDataTable helper
using System.Collections.Generic;
using System.Linq; // Needed for Any()

namespace WcfClient.Models
{
    public class ContactDto
    {
        public int Uid { get; set; }
        public int PrefixId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int SuffixId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        // Helper method to convert a list of ContactDto to a DataTable for DataGridView
        public static DataTable ToDataTable(List<ContactDto> contacts)
        {
            DataTable dt = new DataTable();

            if (contacts == null || !contacts.Any())
            {
                // Define columns even if no data
                dt.Columns.Add("Uid", typeof(int));
                dt.Columns.Add("PrefixId", typeof(int));
                dt.Columns.Add("FirstName", typeof(string));
                dt.Columns.Add("LastName", typeof(string));
                dt.Columns.Add("SuffixId", typeof(int));
                dt.Columns.Add("Address", typeof(string));
                dt.Columns.Add("City", typeof(string));
                dt.Columns.Add("State", typeof(string));
                dt.Columns.Add("Zip", typeof(string));
                return dt;
            }

            // Create columns based on ContactDto properties
            foreach (var prop in typeof(ContactDto).GetProperties())
            {
                dt.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Populate rows
            foreach (var contact in contacts)
            {
                DataRow row = dt.NewRow();
                foreach (var prop in typeof(ContactDto).GetProperties())
                {
                    row[prop.Name] = prop.GetValue(contact) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
