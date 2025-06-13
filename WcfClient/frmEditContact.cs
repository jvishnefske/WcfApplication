using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json; // For JSON deserialization
using WcfClient.Models; // For DTOs
namespace WcfClient
{
    public partial class frmEditContact : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string BaseApiUrl = "https://localhost:7001"; // Adjust port if your API runs on a different one
        private int _contactUid; // To store the UID of the contact being edited
        public bool IsNewContact { get; private set; } // To determine if it's a new contact or existing

        // Constructor for adding a new contact
        public frmEditContact() : this(0) // Call the other constructor with a default UID
        {
            IsNewContact = true;
            this.Text = "Add New Contact";
            button1.Text = "Add"; // Use button1 for Save
        }

        // Constructor for editing an existing contact
        public frmEditContact(int uid)
        {
            InitializeComponent();
            Trace.WriteLine("editing contact.");
            _httpClient.BaseAddress = new Uri(BaseApiUrl);
            _contactUid = uid;
            IsNewContact = (uid == 0); // If UID is 0, it's a new contact

            if (!IsNewContact)
            {
                this.Text = $"Edit Contact (UID: {uid})";
                label9.Text = uid.ToString(); // Display UID in label9
                button1.Text = "Update"; // Use button1 for Save
                _ = LoadContactDetails(uid); // Load details for existing contact
            }
            else
            {
                this.Text = "Add New Contact";
                label9.Text = "New"; // Indicate new contact
                button1.Text = "Add";
            }

            _ = LoadLookups(); // Load prefixes and suffixes
        }
        ~frmEditContact() {
            Trace.WriteLine("edit contact destructor.");
        }

        private async Task LoadContactDetails(int uid)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/api/Contacts/{uid}");
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                ContactDto contact = JsonConvert.DeserializeObject<ContactDto>(jsonResponse);

                if (contact != null)
                {
                    textBox2.Text = contact.FirstName; // First Name
                    textBox3.Text = contact.LastName;  // Last Name
                    textBox4.Text = contact.Address;   // Address
                    textBox5.Text = contact.City;      // City
                    textBox6.Text = contact.State;     // State
                    textBox1.Text = contact.Zip;       // Zip

                    // Set selected values for combo boxes
                    comboBox1.SelectedValue = contact.PrefixId; // Prefix
                    comboBox2.SelectedValue = contact.SuffixId; // Suffix
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error loading contact details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadLookups()
        {
            try
            {
                // Load Prefixes
                HttpResponseMessage prefixResponse = await _httpClient.GetAsync("/api/Lookups/prefixes");
                prefixResponse.EnsureSuccessStatusCode();
                string prefixJson = await prefixResponse.Content.ReadAsStringAsync();
                List<LookupDto> prefixes = JsonConvert.DeserializeObject<List<LookupDto>>(prefixJson);
                comboBox1.DataSource = prefixes; // Use comboBox1 for prefixes
                comboBox1.DisplayMember = "Description";
                comboBox1.ValueMember = "Id";

                // Load Suffixes
                HttpResponseMessage suffixResponse = await _httpClient.GetAsync("/api/Lookups/suffixes");
                suffixResponse.EnsureSuccessStatusCode();
                string suffixJson = await suffixResponse.Content.ReadAsStringAsync();
                List<LookupDto> suffixes = JsonConvert.DeserializeObject<List<LookupDto>>(suffixJson);
                comboBox2.DataSource = suffixes; // Use comboBox2 for suffixes
                comboBox2.DisplayMember = "Description";
                comboBox2.ValueMember = "Id";
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error loading lookup data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e) // Use button1_Click for Save button
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("First Name and Last Name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PersonRequestDto person = new PersonRequestDto
            {
                FirstName = textBox2.Text, // First Name
                LastName = textBox3.Text,  // Last Name
                Prefix = (int)comboBox1.SelectedValue, // Prefix
                Suffix = (int)comboBox2.SelectedValue, // Suffix
                Address = textBox4.Text,   // Address
                City = textBox5.Text,      // City
                State = textBox6.Text,     // State
                Zip = textBox1.Text        // Zip
            };

            try
            {
                HttpResponseMessage response;
                if (IsNewContact)
                {
                    string jsonContent = JsonConvert.SerializeObject(person);
                    response = await _httpClient.PostAsync("/api/Contacts", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
                }
                else
                {
                    string jsonContent = JsonConvert.SerializeObject(person);
                    response = await _httpClient.PutAsync($"/api/Contacts/{_contactUid}", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
                }

                response.EnsureSuccessStatusCode();
                MessageBox.Show("Contact saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error saving contact: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
