namespace ElastiCache.Client.Caching
{
    public class Employee
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string Name { get; set; }
        public string Dept { get; set; }
        public Address Address { get; set; }

        public Employee() { }

    }

    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public int PinCode { get; set; }

        public Address()
        {

        }
    }
}

