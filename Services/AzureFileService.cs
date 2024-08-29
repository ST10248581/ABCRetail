using System.Text;
using Azure.Storage.Files.Shares;

namespace ABCRetail.Services
{
    public class AzureFileService
    {
        private readonly string _connectionString;
        private readonly string _fileShareName;

        public AzureFileService(string fileShareName)
        {
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=abcretailadminstorage;AccountKey=Tlm4u1G7FsIkqB/wGEHkSVdDxwywfd7IefskUv/o07FEMUS81MeuzQHsh0xHzBoeF7BKeUly5rDB+AStkG+7eg==;EndpointSuffix=core.windows.net";
            _fileShareName = fileShareName;
        }

        public async Task UploadFileAsync(string fileName, Stream fileStream)
        {
            var shareClient = new ShareClient(_connectionString, _fileShareName);
            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);

            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var shareClient = new ShareClient(_connectionString, _fileShareName);
            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);

            var downloadResponse = await fileClient.DownloadAsync();
            return downloadResponse.Value.Content;
        }

        public async Task AppendLogAsync(string logFileName, string logMessage)
        {
            var shareClient = new ShareClient(_connectionString, _fileShareName);
            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(logFileName);

            if (await fileClient.ExistsAsync())
            {
                // Download existing file content
                var downloadResponse = await fileClient.DownloadAsync();
                var existingContent = string.Empty;

                using (var stream = new MemoryStream())
                {
                    await downloadResponse.Value.Content.CopyToAsync(stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        existingContent = await reader.ReadToEndAsync();
                    }
                }

                // Append the new log message
                var updatedContent = existingContent + Environment.NewLine + logMessage;
                var updatedContentBytes = Encoding.UTF8.GetBytes(updatedContent);

                // Upload the updated content
                using (var stream = new MemoryStream(updatedContentBytes))
                {
                    await fileClient.CreateAsync(stream.Length);
                    stream.Position = 0;
                    await fileClient.UploadAsync(stream);
                }
            }
            else
            {
                // Create new file and write log message
                var newContent = logMessage;
                var newContentBytes = Encoding.UTF8.GetBytes(newContent);

                using (var stream = new MemoryStream(newContentBytes))
                {
                    await fileClient.CreateAsync(stream.Length);
                    stream.Position = 0;
                    await fileClient.UploadAsync(stream);
                }
            }
        }


        public string GetConnection()
        {
            return _connectionString; ;
        }

        public string GetFileShareName()
        {
            return _fileShareName;
        }
    }
}
