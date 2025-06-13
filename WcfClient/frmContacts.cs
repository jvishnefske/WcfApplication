using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Diagnostics;
using System.Net.Http; // Added for HttpClient
using Newtonsoft.Json; // Added for JSON deserialization
using WcfClient.Models; // Added to use the new Contact model

namespace WcfClient
{
    public partial class frmContacts : Form
    {
        
        DataTable contacts;
        private readonly HttpClient _httpClient; // Declare HttpClient

        public frmContacts()
        {
            var traceFile = new System.Diagnostics.TextWriterTraceListener("tracelog.txt");
            
            Trace.Listeners.Add(traceFile);

            System.Diagnostics.Trace.WriteLine("itializing contacts form.");

            System.Diagnostics.Debug.WriteLine("Debug output from contacts form.");
            
            InitializeComponent();
            contacts = new DataTable();
            dataGridView1.DataSource = contacts;

            _httpClient = new HttpClient(); // Initialize HttpClient
            _httpClient.BaseAddress = new Uri("http://localhost:5000/"); // Set base address for your new Web API
        }

        ~frmContacts() {
            Trace.WriteLine("contact destructor.");
            _httpClient.Dispose(); // Dispose HttpClient when the form is closed
        }

        // Changed to async Task to allow await calls
        async Task updateContacts()
        {
            lblStatus.Text = "Attempting connection...";
            try
            {
                Trace.WriteLine("refreshing contacts.");

                // Make an HTTP GET request to your new Web API endpoint
                HttpResponseMessage response = await _httpClient.GetAsync("api/contacts");
                response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code

                string jsonResponse = await response.Content.ReadAsStringAsync();
                
                // Deserialize the JSON response into a list of Contact objects
                List<Contact> contactList = JsonConvert.DeserializeObject<List<Contact>>(jsonResponse);

                // Convert the list of contacts to a DataTable for display
                contacts = Contact.ToDataTable(contactList);
                
                lblStatus.Text = "complete";
                dataGridView1.DataSource = contacts;
                
            }
            catch (HttpRequestException httpEx)
            {
                lblStatus.Text = $"HTTP Error: {httpEx.Message}";
                Trace.TraceError($"HttpRequestException in updateContacts: {httpEx.Message}");
            }
            catch (JsonSerializationException jsonEx)
            {
                lblStatus.Text = $"JSON Error: {jsonEx.Message}";
                Trace.TraceError($"JsonSerializationException in updateContacts: {jsonEx.Message}");
            }
            catch (Exception e)
            {
                lblStatus.Text = $"General Error: {e.Message}";
                Trace.TraceError($"General Exception in updateContacts: {e.ToString()}");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // This event handler seems to be incorrectly wired to updateContacts
            // If it's meant to refresh, it should call updateContacts()
        }

        // Changed to async void for event handler
        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            await updateContacts();
        }

        // Changed to async void for event handler
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await updateContacts();
        }
    }
}
