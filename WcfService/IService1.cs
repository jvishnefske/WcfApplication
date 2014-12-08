using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
namespace WcfService
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        DataTable GetAllContacts();

        [OperationContract]
        DataTable GetContact(int id);

        [OperationContract]
        DataTable GetSuffixes();

        [OperationContract]
        DataTable GetPrefixes();

        [OperationContract]
        void UpdateContact(Int32 uid,Person person);

        [OperationContract]
        void InsertContact(Person person);
    }

    [DataContract]
    public class Person
    {
        [DataMember]
        public String firstName;
        [DataMember]
        public int prefix;
        [DataMember]
        public String lastName;
        [DataMember]
        public Int32 suffix;
        [DataMember]
        public String address;
        [DataMember]
        public String city;
        [DataMember]
        public String state;
        [DataMember]
        public String zip;
    }    
}