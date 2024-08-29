using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models
{
    public class ProductInformationResultModel
    {
        public string ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public string ProfilePhotoURL { get; set; }
    }
}
