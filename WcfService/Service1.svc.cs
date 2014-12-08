using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.Diagnostics;
//using WcfClient;

namespace WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        TextWriterTraceListener listner;
        Service1(){
            String tracelog = "c:\tracefile.txt";
            listner = new TextWriterTraceListener(tracelog);
            //listner.TraceOutputOptions
        }
        public DataTable GetAllContacts() {
            return Utilities.getAllContacts();
            
        }
        public DataTable GetContact(int uid){
            return Utilities.getContact(uid);
        }
        public DataTable GetPrefixes(){
            return Utilities.getPrefixes();
        }
        public DataTable GetSuffixes(){
            return Utilities.getSuffixes();
        }
        //public void UpdateContact(Int32 uid, String firstName,String lastName,Int32 prefix,Int32 suffix,String address,String city,String state,String zip) {
        public void UpdateContact(Int32 uid,Person person){
            Utilities.updateContact(uid,person.firstName,person.lastName,person.prefix,person.suffix,person.address,person.city,person.state,person.zip);
        }
        //public void InsertContact(String firstName,String lastName,Int32 prefix,Int32 suffix,String address,String city,String state,String zip){
        public void InsertContact(Person person){
            Utilities.insertContact(person.firstName,person.lastName,person.prefix,person.suffix,person.address,person.city,person.state,person.zip);
        }
    }
}
