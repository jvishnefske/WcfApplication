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
using WcfClient.Models; // Now refers to ContactDto, PersonRequestDto, LookupDto

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
            _httpClient.BaseAddress = new Uri("https://localhost:7001/"); // Set base address for your new Web API (ASP.NET Core default HTTPS port)
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
                
                // Deserialize the JSON response into a list of ContactDto objects
                List<ContactDto> contactList = JsonConvert.DeserializeObject<List<ContactDto>>(jsonResponse);
                
                // Convert the list of contacts to a DataTable for display
                contacts = ContactDto.ToDataTable(contactList);
                
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
            // If it's meant to refresh, it should call updateContacts() - No change needed, just a comment.
        }

        // Changed to async void for event handler
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) // Revert to non-async, this event is not for refresh
        {
            // This event is typically for handling clicks on cell content, not for refreshing the entire grid.
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Get the UID from the clicked row
                if (dataGridView1.Rows[e.RowIndex].Cells["uid"].Value != null)
                {
                    int contactUid = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["uid"].Value);
                    
                    // Open the edit form
                    using (frmEditContact editForm = new frmEditContact(contactUid))
                    {
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            // If the edit form was saved successfully, refresh the contacts list
                            // This call is now handled by the button click or form load, not cell content click
                            // await updateContacts(); // Removed as per plan, if refresh is desired here, uncomment and make method async
                        }
                    }
                }
            }
        }

        // Changed to async void for event handler
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await updateContacts();
        }

        private async void btnAddContact_Click(object sender, EventArgs e)
        {
            using (frmEditContact addForm = new frmEditContact())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    await updateContacts();
                }
            }
        }
    }
}
