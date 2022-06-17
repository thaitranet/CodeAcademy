using Azure.Identity;
using CodeAcademy;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace CodeAcademy
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAzureAppConfiguration();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var builtConfig = builder.ConfigurationBuilder.Build();
            var appConfigurationConnectionString = builtConfig.GetConnectionString("AzureAppConfiguration");

            if (!string.IsNullOrEmpty(appConfigurationConnectionString))
            {
                var appConfigurationLabelFilter = builtConfig.GetConnectionStringOrSetting("AzureAppConfigurationLabelFilter");
                // using AAC, either local dev or deployed
                builder.ConfigurationBuilder
                        .SetBasePath(Environment.CurrentDirectory)
                        .AddAzureAppConfiguration(opts =>
                            opts.Connect(appConfigurationConnectionString)
                                .ConfigureRefresh(refresh =>
                                {
                                    refresh.Register("Sentinel", refreshAll: true);
                                           //.SetCacheExpiration(new TimeSpan(0, 5, 0));
                                })
                                .ConfigureKeyVault(kv =>
                                {
                                    kv.SetCredential(new DefaultAzureCredential());
                                })
                                .Select(KeyFilter.Any, LabelFilter.Null)
                                .Select(KeyFilter.Any, appConfigurationLabelFilter)
                        )
                        .AddJsonFile("local.settings.json", true)
                        .AddEnvironmentVariables()
                    .Build();
            }
            else
            {
                // local dev no AAC
                builder.ConfigurationBuilder
                   .SetBasePath(Environment.CurrentDirectory)
                   .AddJsonFile("local.settings.json", true)
                   .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                   .AddEnvironmentVariables()
                   .Build();
            }
        }
    }
}