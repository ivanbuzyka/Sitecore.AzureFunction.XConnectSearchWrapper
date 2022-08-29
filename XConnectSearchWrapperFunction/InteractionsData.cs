using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitecore.Xdb.Common.Web;
using System.Collections.Generic;
using Sitecore.XConnect.Client.WebApi;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Schema;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect;

namespace XConnectSearchWrapperFunction
{
  public static class InteractionsData
  {
    [FunctionName("InteractionsData")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");

      // pass and read query string parameter if needed
      string name = req.Query["name"];

      // here how the data cann be received through request body 
      //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
      //dynamic data = JsonConvert.DeserializeObject(requestBody);
      //name = name ?? data?.name;

      var xConnectBaseUrl = System.Environment.GetEnvironmentVariable("XCONNECT_BASE_URL", EnvironmentVariableTarget.Process); //"https://fxmdemo.xconnect";

      //The client certificate path and thumbprint here
      var xConnectClientCertificateThumbprint = System.Environment.GetEnvironmentVariable("XCONNECT_CERTIFICATE_THUMBPRINT", EnvironmentVariableTarget.Process); //"https://fxmdemo.xconnect";

      // NOTE: don't want to mess up with the cerificate (find the right one, install that one on the right machine etc. etc.)? Go to the XConnect app and comment '<add key="validateCertificateThumbprint"' setting in the AppSettings.config. Of course THAT IS NOT THE GOOD IDEA FOR PRODUCTION!

      CertificateHttpClientHandlerModifierOptions options = CertificateHttpClientHandlerModifierOptions.Parse($"StoreName=My;StoreLocation=LocalMachine;FindType=FindByThumbprint;FindValue={xConnectClientCertificateThumbprint}");

      var certificateModifier = new CertificateHttpClientHandlerModifier(options);
      List<IHttpClientModifier> clientModifiers = new List<IHttpClientModifier>();
      var timeoutClientModifier = new TimeoutHttpClientModifier(new TimeSpan(0, 0, 20));
      clientModifiers.Add(timeoutClientModifier);

      // This overload takes three client end points - collection, search, and configuration
      var collectionClient = new CollectionWebApiClient(new Uri($"{xConnectBaseUrl}/odata"), clientModifiers, new[] { certificateModifier });
      var searchClient = new SearchWebApiClient(new Uri($"{xConnectBaseUrl}/odata"), clientModifiers, new[] { certificateModifier });
      var configurationClient = new ConfigurationWebApiClient(new Uri($"{xConnectBaseUrl}/configuration"), clientModifiers, new[] { certificateModifier });

      var cfg = new XConnectClientConfiguration(new XdbRuntimeModel(CollectionModel.Model), collectionClient, searchClient, configurationClient);

      //initializing configuration, that makes OData request GET https://<xconnect-hostname>/configuration/models
      await cfg.InitializeAsync();

      var result2 = new List<Interaction>();

      using (var client = new XConnectClient(cfg))
      {

        // Example 2 - Get interactions paginated (using skip and take methods)
        result2 = await PaginationExample.Run(client);
      }

      var responseMessage = $"The xConnect returned {result2.Count}";

      return new OkObjectResult(responseMessage);
    }
  }
}
