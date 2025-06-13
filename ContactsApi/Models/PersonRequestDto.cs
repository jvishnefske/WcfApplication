namespace ContactsApi.Models
{
    public class PersonRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Prefix { get; set; }
        public int Suffix { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
