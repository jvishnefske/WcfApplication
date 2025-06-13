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
using System.Net.Http.Json; // Add this for GetFromJsonAsync, PostAsJsonAsync, PutAsJsonAsync
using Newtonsoft.Json; // For JSON deserialization
using WcfClient.Models; // For DTOs
namespace WcfClient
{
    public partial class frmEditContact : Form
    {
        // Remove the private HttpClient field as we will use ApiClient.Client
        // private readonly HttpClient _httpClient = new HttpClient();
        // Remove BaseApiUrl as it's now in ApiClient
        // private const string BaseApiUrl = "https://localhost:7001"; 
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
            Trace.WriteLine("Editing contact.");
            // Remove _httpClient.BaseAddress assignment
            // _httpClient.BaseAddress = new Uri(BaseApiUrl);
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
        // Remove the destructor as HttpClient is no longer managed by the form
        // ~frmEditContact() {
        //     Trace.WriteLine("edit contact destructor.");
        // }

        private async Task LoadContactDetails(int uid)
        {
            try
            {
                // Use ApiClient.Client and GetFromJsonAsync
                ContactDto? contact = await ApiClient.Client.GetFromJsonAsync<ContactDto>($"api/Contacts/{uid}");

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
                else
                {
                    MessageBox.Show("Contact not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error loading contact details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private async Task LoadLookups()
        {
            try
            {
                // Load Prefixes
                // Use ApiClient.Client and GetFromJsonAsync
                List<LookupDto>? prefixes = await ApiClient.Client.GetFromJsonAsync<List<LookupDto>>("api/Lookups/prefixes");
                
                if (prefixes != null)
                {
                    comboBox1.DataSource = prefixes; // Use comboBox1 for prefixes
                    comboBox1.DisplayMember = "Description";
                    comboBox1.ValueMember = "Id";
                }

                // Load Suffixes
                // Use ApiClient.Client and GetFromJsonAsync
                List<LookupDto>? suffixes = await ApiClient.Client.GetFromJsonAsync<List<LookupDto>>("api/Lookups/suffixes");
                
                if (suffixes != null)
                {
                    comboBox2.DataSource = suffixes; // Use comboBox2 for suffixes
                    comboBox2.DisplayMember = "Description";
                    comboBox2.ValueMember = "Id";
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error loading lookup data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred loading lookups: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Ensure SelectedValue is not null before casting
            if (comboBox1.SelectedValue == null || comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Please select a Prefix and Suffix.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PersonRequestDto person = new PersonRequestDto
            {
                FirstName = textBox2.Text, // First Name
                LastName = textBox3.Text,  // Last Name
                PrefixId = (int)comboBox1.SelectedValue, // Use PrefixId as per PersonRequestDto
                SuffixId = (int)comboBox2.SelectedValue, // Use SuffixId as per PersonRequestDto
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
                    // Use PostAsJsonAsync
                    response = await ApiClient.Client.PostAsJsonAsync("api/Contacts", person);
                }
                else
                {
                    // Use PutAsJsonAsync
                    response = await ApiClient.Client.PutAsJsonAsync($"api/Contacts/{_contactUid}", person);
                }

                response.EnsureSuccessStatusCode();
                MessageBox.Show("Contact saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (HttpRequestException ex)
            {
                string errorContent = "No additional error details.";
                if (ex.StatusCode.HasValue)
                {
                    // Attempt to read the response content for more details on HTTP errors
                    errorContent = await ex.HttpResponseMessage.Content.ReadAsStringAsync();
                }
                MessageBox.Show($"Error saving contact: {ex.Message}\nDetails: {errorContent}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
