using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//namespaces for SQL 
//using System.ServiceModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Tracing;
using System.Diagnostics;
namespace WcfService
{
    public class Utilities
    {

        public static System.Diagnostics.Tracing.EventSource tracingSource;
        static Utilities() {
            
            //System.Diagnostics.Tracing.w
            //tracingSource = new EventSource();
            //tracingSource.IsEnabled = true;
           
        }
        const String connectionString = 
            "server=BAKER\\SQLEXPRESS;"+
                "Database=personnel;"+
                "User Id=sa;"+
                "Password=7529;";
        private static SqlConnection getConnection(){
            return new SqlConnection(connectionString);
        } 
        public static DataTable getContact(int uid){
            using(SqlConnection conn = getConnection() ){
                
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText=
                    @"select uid,prefixid,first_name,last_name,suffixid,address,city,state,zip
                    from personnel.dbo.employees 
                    where uid = @uid;";
                command.Parameters.AddWithValue("@uid",uid);
                                
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);
                return dataset.Tables[0];
            }
        }
        public static DataTable getAllContacts() {
            Trace.TraceInformation("getAllContacts: attempting to get connection.");
            //return getContact(1);
            using (SqlConnection conn = getConnection()){
                conn.Open();
                SqlCommand command = conn.CreateCommand();
                command.CommandText = 
                    @"SELECT TOP 100 [uid]
     [pre.prefix]
      ,[first_name]
      ,[last_name]
     ,[suffix]
      ,[address]
      ,[city]
      ,[state]
      ,[zip]
  FROM [personnel].[dbo].employees as emp
  left outer join personnel.dbo.prefixes as pre on emp.prefixid = pre.prefixid
  left outer join personnel.dbo.suffixes as suf on emp.suffixid = suf.suffixid;";
                    
                SqlDataAdapter adapter = new SqlDataAdapter();

                adapter.SelectCommand = command;
                //new SqlCommand(query, conn);
                DataTable datatable=new DataTable();

                adapter.Fill(datatable);
                Trace.TraceInformation("GetAllContacts: returning dataTable of size " + datatable.Rows);

                
                return datatable;
            }
             
            /*using (SqlConnection conn = getConnection())
            {

                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText =
                    @"
SELECT TOP 100 [uid]
     [pre.prefix]
      ,[first_name]
      ,[last_name]
     ,[suffix]
      ,[address]
      ,[city]
      ,[state]
      ,[zip]
  FROM [personnel].[dbo].employees as emp
  left outer join personnel.dbo.prefixes as pre on emp.prefixid = pre.prefixid
  left outer join personnel.dbo.suffixes as suf on emp.suffixid = suf.suffixid;
";
                    /*@"select uid,prefixid,first_name,last_name,suffixid,address,city,state,zip
                    from personnel.dbo.employees 
                    where uid = @uid;";
                    */
            /*   
            command.Parameters.AddWithValue("@uid", 1);

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);
                return dataset.Tables[0];
            }*/
            //return new DataTable();
            
        }
        public static DataTable getPrefixes() {
            using (SqlConnection conn = getConnection())
            {
                
                const String query =
                    "select  * from personnel.prefixes";
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand(query, conn);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);
                return dataset.Tables[0];
            }
        }
        public static DataTable getSuffixes()
        {
            using (SqlConnection conn = getConnection())
            {
                const String query =
                    "select  * from personnel.suffixes";
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand(query, conn);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);
                return dataset.Tables[0];
            }
        }
        public static void updateContact(Int32 uid,String firstName,String lastName,Int32 prefix,Int32 suffix,String address,String city,String state,String zip){
            using (SqlConnection conn = getConnection())
            {
                const String query =
                    "select  * from personnel.suffixes";
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.UpdateCommand = new SqlCommand(query, conn);
                DataSet dataset = new DataSet();
            }
        }
        public static void insertContact(String firstName,String lastName,Int32 prefix,Int32 suffix,String address,String city,String state,String zip){
            using (SqlConnection conn = getConnection())
            {
                const String query =
                    "select  * from personnel.suffixes";
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.UpdateCommand = new SqlCommand(query, conn);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);

            }
        }
        
    }
}

