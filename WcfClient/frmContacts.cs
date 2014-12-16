using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Diagnostics;
//using System.Diagnostics.
//using System.Data;
namespace WcfClient
{
    public partial class frmContacts : Form
    {
        
        DataTable contacts;
        public frmContacts()
        {
            var traceFile = new System.Diagnostics.TextWriterTraceListener("tracelog.txt");
            
            Trace.Listeners.Add(traceFile);

            System.Diagnostics.Trace.WriteLine("itializing contacts form.");

            System.Diagnostics.Debug.WriteLine("Debug output from contacts form.");
            //traceFile.Close();

            //Form traceForm = new frmTraceLog();
            //traceForm.Show();
            //contacts = new DataTable("","<container><stuff>content</stuff></containier>");

            /*Uri baseAddress = new Uri("http://0.0.0.0:8000/ClientData/");
            //Uri baseAddress = new Uri("http://localhost:51599/");
            ServiceHost selfHost = new ServiceHost(typeof(WcfService.Service1), baseAddress);
            try
            {
                selfHost.AddServiceEndpoint(typeof(WcfService.IService1),
                new WSHttpBinding(), "ContactData");
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                selfHost.Description.Behaviors.Add(smb);
                selfHost.Open();
                Console.WriteLine("Service Started.");
            }
            catch (Exception e) {
                Console.WriteLine("danger! : "+e.ToString());
                throw e;
            }
            */
            InitializeComponent();
            contacts = new DataTable();
            dataGridView1.DataSource = contacts;

        }
        ~frmContacts() {
            Trace.WriteLine("contact destructor.");
        }
        void updateContacts(){
            lblStatus.Text = "Attempting connection...";
                    using (ServiceReference1.Service1Client client =
            new ServiceReference1.Service1Client())

            {

                try
                {
                    Trace.WriteLine("refreshing contacts.");

                    contacts = client.GetAllContacts();
                    //contacts = client.GetContact(1);
                    lblStatus.Text = "complete";
                    dataGridView1.DataSource = contacts;
                    
                }
                catch (Exception e)
                {
                    if (((Exception)e) == null) lblStatus.Text = "null exception";
                    else
                        lblStatus.Text = e.ToString();
                       
                }
            }
                    
        
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            updateContacts();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            updateContacts();
        }
    }
}
