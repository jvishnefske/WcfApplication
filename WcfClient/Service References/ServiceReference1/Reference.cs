﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfClient.ServiceReference1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IService1")]
    public interface IService1 {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetAllContacts", ReplyAction="http://tempuri.org/IService1/GetAllContactsResponse")]
        System.Data.DataTable GetAllContacts();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetAllContacts", ReplyAction="http://tempuri.org/IService1/GetAllContactsResponse")]
        System.Threading.Tasks.Task<System.Data.DataTable> GetAllContactsAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetContact", ReplyAction="http://tempuri.org/IService1/GetContactResponse")]
        System.Data.DataTable GetContact(int id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetContact", ReplyAction="http://tempuri.org/IService1/GetContactResponse")]
        System.Threading.Tasks.Task<System.Data.DataTable> GetContactAsync(int id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetSuffixes", ReplyAction="http://tempuri.org/IService1/GetSuffixesResponse")]
        System.Data.DataTable GetSuffixes();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetSuffixes", ReplyAction="http://tempuri.org/IService1/GetSuffixesResponse")]
        System.Threading.Tasks.Task<System.Data.DataTable> GetSuffixesAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetPrefixes", ReplyAction="http://tempuri.org/IService1/GetPrefixesResponse")]
        System.Data.DataTable GetPrefixes();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetPrefixes", ReplyAction="http://tempuri.org/IService1/GetPrefixesResponse")]
        System.Threading.Tasks.Task<System.Data.DataTable> GetPrefixesAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/UpdateContact", ReplyAction="http://tempuri.org/IService1/UpdateContactResponse")]
        void UpdateContact(int uid, WcfService.Person person);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/UpdateContact", ReplyAction="http://tempuri.org/IService1/UpdateContactResponse")]
        System.Threading.Tasks.Task UpdateContactAsync(int uid, WcfService.Person person);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/InsertContact", ReplyAction="http://tempuri.org/IService1/InsertContactResponse")]
        void InsertContact(WcfService.Person person);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/InsertContact", ReplyAction="http://tempuri.org/IService1/InsertContactResponse")]
        System.Threading.Tasks.Task InsertContactAsync(WcfService.Person person);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IService1Channel : WcfClient.ServiceReference1.IService1, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class Service1Client : System.ServiceModel.ClientBase<WcfClient.ServiceReference1.IService1>, WcfClient.ServiceReference1.IService1 {
        
        public Service1Client() {
        }
        
        public Service1Client(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public Service1Client(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Service1Client(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Service1Client(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Data.DataTable GetAllContacts() {
            return base.Channel.GetAllContacts();
        }
        
        public System.Threading.Tasks.Task<System.Data.DataTable> GetAllContactsAsync() {
            return base.Channel.GetAllContactsAsync();
        }
        
        public System.Data.DataTable GetContact(int id) {
            return base.Channel.GetContact(id);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataTable> GetContactAsync(int id) {
            return base.Channel.GetContactAsync(id);
        }
        
        public System.Data.DataTable GetSuffixes() {
            return base.Channel.GetSuffixes();
        }
        
        public System.Threading.Tasks.Task<System.Data.DataTable> GetSuffixesAsync() {
            return base.Channel.GetSuffixesAsync();
        }
        
        public System.Data.DataTable GetPrefixes() {
            return base.Channel.GetPrefixes();
        }
        
        public System.Threading.Tasks.Task<System.Data.DataTable> GetPrefixesAsync() {
            return base.Channel.GetPrefixesAsync();
        }
        
        public void UpdateContact(int uid, WcfService.Person person) {
            base.Channel.UpdateContact(uid, person);
        }
        
        public System.Threading.Tasks.Task UpdateContactAsync(int uid, WcfService.Person person) {
            return base.Channel.UpdateContactAsync(uid, person);
        }
        
        public void InsertContact(WcfService.Person person) {
            base.Channel.InsertContact(person);
        }
        
        public System.Threading.Tasks.Task InsertContactAsync(WcfService.Person person) {
            return base.Channel.InsertContactAsync(person);
        }
    }
}
