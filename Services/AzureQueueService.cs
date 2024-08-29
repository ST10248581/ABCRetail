using System.Text.Json;
using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;

namespace ABCRetail.Services
{
    public class AzureQueueService
    {
        private readonly QueueClient _queueClient;

        public AzureQueueService(string queueName)
        {
            _queueClient = new QueueClient("DefaultEndpointsProtocol=https;AccountName=abcretailadminstorage;AccountKey=Tlm4u1G7FsIkqB/wGEHkSVdDxwywfd7IefskUv/o07FEMUS81MeuzQHsh0xHzBoeF7BKeUly5rDB+AStkG+7eg==;EndpointSuffix=core.windows.net", queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(string message)
        {
            if (_queueClient.Exists())
            {
                await _queueClient.SendMessageAsync(message);
            }
        }

        public async Task<string> ReceiveMessageAsync()
        {
            if (_queueClient.Exists())
            {
                QueueMessage[] retrievedMessage = await _queueClient.ReceiveMessagesAsync(1);

                if (retrievedMessage.Length > 0)
                {
                    string message = retrievedMessage[0].MessageText;

                    await _queueClient.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);

                    return message;
                }
            }

            return null;
        }
    }
}
