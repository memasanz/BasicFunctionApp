using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using chr_mm_functionapp_01.Model;

namespace chr_mm_functionapp_01
{
    public static class Function1

    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, 
            [Queue("outqueue"),StorageAccount("AzureWebJobsStorage")] ICollector<string> msg, 
            [Blob("sample-items/name", FileAccess.Write, Connection="CONNECTIONSTRING")] TextWriter validationOutput,
            ILogger log, ExecutionContext executionContext)
        {
            var config = new ConfigurationBuilder()
                                .SetBasePath(executionContext.FunctionAppDirectory)
                                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                .AddEnvironmentVariables()
                                .Build();

            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;


            if (!string.IsNullOrEmpty(name))
            {
                // Add a message to the output collection.
                msg.Add(string.Format("Name passed to the function: {0}", name));
                validationOutput.WriteLine("hello World");
                await Save("file", "this is my message to blob", executionContext);
            }
            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

                public static async Task Save(string fileName, string message, ExecutionContext executionContext)
        {
            var config = new ConfigurationBuilder()
                                    .SetBasePath(executionContext.FunctionAppDirectory)
                                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                    .AddEnvironmentVariables()
                                    .Build();

            var blobContainer = config["BlobContainer"];
            var blobConnectionString = config["CONNECTIONSTRING"];

            var storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(blobContainer);
            var cloudBlockBlob = container.GetBlockBlobReference(fileName);

            if (await cloudBlockBlob.ExistsAsync())
            {
                //var oldContent = await cloudBlockBlob.DownloadTextAsync();
                //var newContent = oldContent + "\n" + message;
                await cloudBlockBlob.UploadTextAsync(message);
            }
            else
            {
                await cloudBlockBlob.UploadTextAsync(message);
            }
        }
        
    } //class
} //name space

  


