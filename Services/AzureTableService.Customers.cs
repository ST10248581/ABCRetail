using System.Collections.Concurrent;
using ABCRetail.Entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;

namespace ABCRetail.Services
{
    public partial class AzureTableService
    {
        public async Task InsertOrMergeEntityAsync(CustomerProfile entity)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
            await _table.ExecuteAsync(insertOrMergeOperation);
        }

        public async Task<CustomerProfile> RetrieveCustomerEntityAsync(string partitionKey, string rowKey)
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
