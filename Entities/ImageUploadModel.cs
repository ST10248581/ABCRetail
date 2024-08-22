using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Entities
{
    public class ImageUploadModel
    {
        [Required]
        public IFormFile ImageFile { get; set; }
    }
}
