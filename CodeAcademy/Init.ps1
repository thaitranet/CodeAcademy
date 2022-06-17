param ($azureAppConfigurationConnectionString)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AzureAppConfiguration" $azureAppConfigurationConnectionString