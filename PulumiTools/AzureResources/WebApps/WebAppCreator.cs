using Pulumi;
using Web = Pulumi.AzureNative.Web;

namespace PulumiTools.AzureResources.WebApps
{
    /// <summary>
    /// Class for creating an Azure Web App
    /// </summary>
    public class WebAppCreator : AzureResourceCreatorBase
    {
        public WebAppCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "appsvc")
        {
        }

        /// <summary>
        /// Create a web application with a database connection
        /// </summary>
        /// <param name="webAppName"></param>
        /// <param name="appServicePlanId"></param>
        /// <param name="codeBlobUrl"></param>
        /// <param name="sqlServerName"></param>
        /// <param name="databaseName"></param>
        /// <param name="sqlUserName"></param>
        /// <param name="sqlPassword"></param>
        /// <returns></returns>
        public Web.WebApp CreateWithDbConnection(string webAppName, Input<string> appServicePlanId, Input<string> codeBlobUrl, Input<string> sqlServerName, Input<string> databaseName, string sqlUserName, string sqlPassword)
        {
            webAppName = $"{this.Prefix}-{webAppName}";
            var appSettingsArgs = new InputList<Web.Inputs.NameValuePairArgs>
                {
                    new Web.Inputs.NameValuePairArgs { Name = "ApplicationInsightsAgent_EXTENSION_VERSION", Value = "~2" },
                    new Web.Inputs.NameValuePairArgs { Name = "WEBSITE_RUN_FROM_PACKAGE", Value = codeBlobUrl }
                };
            var sqlConnectionArgs = new Web.Inputs.ConnStringInfoArgs
            {
                Name = "db",
                ConnectionString = Output.Tuple(sqlServerName, databaseName).Apply(i => $"Server=tcp:{i.Item1}.database.windows.net;initial catalog={i.Item2};user ID ={sqlUserName}; password={sqlPassword}; Min Pool Size=0; Max Pool Size=30; Persist Security Info=true;"),
                Type = Web.ConnectionStringType.SQLAzure
            };

            InputList<Web.Inputs.ConnStringInfoArgs> inputList = new InputList<Web.Inputs.ConnStringInfoArgs>
                    {
                        sqlConnectionArgs
                    };
            Web.Inputs.SiteConfigArgs siteConfigArgs = new Web.Inputs.SiteConfigArgs
            {
                AppSettings = appSettingsArgs,
                ConnectionStrings = inputList,
                FtpsState = Web.FtpsState.FtpsOnly,
                HttpLoggingEnabled = true,
                MinTlsVersion = Web.SupportedTlsVersions.SupportedTlsVersions_1_2
            };

            var managedServiceIdentityArgs = new Web.Inputs.ManagedServiceIdentityArgs
            {
                Type = Web.ManagedServiceIdentityType.SystemAssigned,
            };

            Web.WebAppArgs args = new Web.WebAppArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                ServerFarmId = appServicePlanId,
                SiteConfig = siteConfigArgs,
                HttpsOnly = true,
                Identity = managedServiceIdentityArgs
            };
            return new Web.WebApp(webAppName, args);
        }
    }
}
