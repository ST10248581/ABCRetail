using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models
{
    public class CustomerProfileResultModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string ProfilePhotoURL { get; set; }
    }
}
