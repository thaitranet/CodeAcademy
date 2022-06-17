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
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Linq;

namespace CodeAcademy
{
    public class SecretFunction
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationRefresher _configurationRefresher;

        public SecretFunction(IConfiguration configuration, IConfigurationRefresherProvider refresherProvider)
        {
            _configuration = configuration;
            _configurationRefresher = refresherProvider.Refreshers.First();
        }


        [FunctionName("Secret")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            await _configurationRefresher.TryRefreshAsync();

            var secret = _configuration["SecretConfiguration"];
            var nonsecret = _configuration["NonSecretConfiguration"];

            return new OkObjectResult($"{nonsecret} {secret}");
        }
    }
}
