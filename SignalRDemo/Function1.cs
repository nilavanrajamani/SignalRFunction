using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SignalRDemo
{
    public static class Function1
    {
        private static HttpClient httpClient = new HttpClient();
        private static string Etag = string.Empty;
        private static string StarCount = "0";

        [FunctionName("index")]
        public static IActionResult GetHomePage([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ExecutionContext context)
        {
            var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
            return new ContentResult
            {
                Content = File.ReadAllText(path),
                ContentType = "text/html",
            };
        }

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "MissionSignalR")] SignalRConnectionInfo connectionInfo)
        {            
            return connectionInfo;
        }

        [FunctionName("GroupAction")]
        public static async Task<IActionResult> GroupAction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "signalr/groups/action")] HttpRequest req,
            [SignalR(HubName = "MissionSignalR")] ICollector<SignalRGroupAction> signalRGroupActions)
        {
            string requestBody = string.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            
            signalRGroupActions.Add(
                new SignalRGroupAction
                {                    
                    ConnectionId = req.Headers["ConnectionId"].ToString(),
                    GroupName = req.Headers["GroupName"].ToString(),                    
                    Action = 0
        });

            return new OkObjectResult(new { message = string.Format($"connection id  - {req.Headers["ConnectionId"].ToString()} {0} group successfully", "added form") });
        }

        //[FunctionName("broadcast")]
        //public static async Task Broadcast([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer,
        //        [SignalR(HubName = "MissionSignalR")] IAsyncCollector<SignalRMessage> signalRMessages)
        //{

        //    var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/azure/azure-signalr");
        //    request.Headers.UserAgent.ParseAdd("Serverless");
        //    request.Headers.Add("If-None-Match", Etag);
        //    var response = await httpClient.SendAsync(request);
        //    if (response.Headers.Contains("Etag"))
        //    {
        //        Etag = response.Headers.GetValues("Etag").First();
        //    }
        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        var result = JsonConvert.DeserializeObject<GitResult>(await response.Content.ReadAsStringAsync());
        //        StarCount = result.StarCount;
        //    }

        //    await signalRMessages.AddAsync(
        //        new SignalRMessage
        //        {
        //            Target = "newMessage",
        //            Arguments = new[] { $"Current star count of https://github.com/Azure/azure-signalr is: {StarCount}" }
        //        });
        //}

        private class GitResult
        {
            [JsonRequired]
            [JsonProperty("stargazers_count")]
            public string StarCount { get; set; }
        }
    }
}
