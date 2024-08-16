using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models
{
    public class CustomerProfileRequest
    {
      
        
            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, Phone]
            public string Phone { get; set; }

            [Required]
            public string Address { get; set; }

            public IFormFile ProfilePhoto { get; set; }
        }
    }



