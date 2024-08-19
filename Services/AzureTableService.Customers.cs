using ABCRetail.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace ABCRetail.Services
{
    public partial class AzureTableService
    {
        public async Task InsertOrMergeEntityAsync(CustomerProfile entity)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
            await _table.ExecuteAsync(insertOrMergeOperation);
        }

        public async Task<CustomerProfile> RetrieveEntityAsync(string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<CustomerProfile>(partitionKey, rowKey);
            TableResult result = await _table.ExecuteAsync(retrieveOperation);
            return result.Result as CustomerProfile;
        }

        public async Task DeleteEntityAsync(CustomerProfile entity)
        {
            TableOperation deleteOperation = TableOperation.Delete(entity);
            await _table.ExecuteAsync(deleteOperation);
        }
    }
}
