using ABCRetail.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace ABCRetail.Services
{
    public partial class AzureTableService
    {
        public async Task InsertOrMergeEntityAsync(Product entity)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
            await _table.ExecuteAsync(insertOrMergeOperation);
        }

        public async Task<Product> RetrieveProductEntityAsync(string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Product>(partitionKey, rowKey);
            TableResult result = await _table.ExecuteAsync(retrieveOperation);
            return result.Result as Product;
        }

        public async Task DeleteEntityAsync(Product entity)
        {
            TableOperation deleteOperation = TableOperation.Delete(entity);
            await _table.ExecuteAsync(deleteOperation);
        }
    }
}
