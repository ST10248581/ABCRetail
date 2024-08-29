using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Table;

namespace ABCRetail.Entities
{
    public class Product : TableEntity
    {
        public Product(string productId, string productName) 
        { 
            this.PartitionKey = productId;
            this.RowKey = productName;
        }

        public Product() { }

        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }

    }
}
