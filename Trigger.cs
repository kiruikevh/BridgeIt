using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//Receiving data from an http trigger and inserting it into a Cosmos DB container and
//then receive the data from the Cosmos DB container using a Cosmos DB trigger. The Cosmos DB trigger will be triggered when there is a change in the container.
//The Cosmos DB trigger will receive the changed data and log it or do something meaningful with it.
namespace FunctionApp
{
    public class Trigger
    {
        private readonly ILogger<Trigger> _logger;
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;
        public Trigger(ILogger<Trigger> logger)
        {
            _logger = logger;
            _cosmosClient = new CosmosClient("");
            _database = _cosmosClient.GetDatabase("");
            _container = _database.GetContainer("");
        }
        [Function("HttpTrigger")]
        public async Task<IActionResult> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger function processed a request.");
            var document = await req.ReadFromJsonAsync<MyDocument>();
            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));

            return new OkObjectResult($"Document inserted with ID: {document.Id}");
        }
        [Function("CosmosDBTrigger")]
        public void RunCosmosDBTrigger(
            [CosmosDBTrigger(
                databaseName: "",
                containerName: "",
                Connection = "",
                LeaseContainerName = "",
                CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation($"Documents modified: {input.Count}");
                _logger.LogInformation($"First document Id: {input[0].Id}");
            }
        }
    }

    public class MyDocument
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int Number { get; set; }
        public string PartitionKey { get; set; }
    }
}
