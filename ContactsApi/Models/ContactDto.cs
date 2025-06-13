namespace ContactsApi.Models
{
    public class ContactDto
    {
        public int Uid { get; set; }
        public int PrefixId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int SuffixId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
