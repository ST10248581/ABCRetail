using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Table;

namespace ABCRetail.Entities
{
    public class CustomerProfile : TableEntity
    {
        public CustomerProfile(string customerId, string lastName)
        {
            this.PartitionKey = customerId;
            this.RowKey = lastName;
        }

        public CustomerProfile() { }


        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
