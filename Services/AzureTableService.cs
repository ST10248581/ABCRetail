using Microsoft.Azure.Cosmos.Table;

namespace ABCRetail.Services
{
    public partial class AzureTableService
    {
        private readonly CloudTable _table;

        public AzureTableService(string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=abcretailadminstorage;AccountKey=Tlm4u1G7FsIkqB/wGEHkSVdDxwywfd7IefskUv/o07FEMUS81MeuzQHsh0xHzBoeF7BKeUly5rDB+AStkG+7eg==;EndpointSuffix=core.windows.net");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _table = tableClient.GetTableReference(tableName);
            _table.CreateIfNotExists();
        }


        public async Task<List<T>> GetAllEntitiesAsync<T>() where T : TableEntity, new()
        {
            var query = new TableQuery<T>();
            var entities = new List<T>();
            TableContinuationToken token = null;

            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            }

            while (token != null);

            return entities;
        }

    }

}
