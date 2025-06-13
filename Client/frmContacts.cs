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
using System.Net.Http.Json;
using Client.Models; // Now refers to ContactDto, PersonRequestDto, LookupDto

namespace Client
{
    public partial class frmContacts : Form
    {
        
        DataTable contacts;

        public frmContacts()
        {
            var traceFile = new System.Diagnostics.TextWriterTraceListener("tracelog.txt");
            
            Trace.Listeners.Add(traceFile);

            Trace.WriteLine("Initializing contacts form.");

            System.Diagnostics.Debug.WriteLine("Debug output from contacts form.");
            
            InitializeComponent();
            contacts = new DataTable();
            dataGridView1.DataSource = contacts;
        }

        // Changed to async Task to allow await calls
        async Task updateContacts()
        {
            lblStatus.Text = "Attempting connection...";
            try
            {
                Trace.WriteLine("Refreshing contacts.");

                // Use ApiClient.Client and GetFromJsonAsync
                List<ContactDto>? contactList = await ApiClient.Client.GetFromJsonAsync<List<ContactDto>>("api/contacts");
                
                if (contactList != null)
                {
                    contacts = ContactDto.ToDataTable(contactList);
                    lblStatus.Text = "Complete";
                    dataGridView1.DataSource = contacts;
                }
                else
                {
                    lblStatus.Text = "No contacts found or deserialization failed.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                lblStatus.Text = $"HTTP Error: {httpEx.Message}";
                Trace.TraceError($"HttpRequestException in updateContacts: {httpEx.Message}");
                MessageBox.Show($"Error communicating with the API: {httpEx.Message}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                lblStatus.Text = $"General Error: {e.Message}";
                Trace.TraceError($"General Exception in updateContacts: {e.ToString()}");
                MessageBox.Show($"An unexpected error occurred: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
