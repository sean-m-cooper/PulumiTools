using Pulumi;
using Insights = Pulumi.AzureNative.Insights;

namespace PulumiTools.AzureResources.WebApps
{
    /// <summary>
    /// Class for creating Azure App Insights
    /// </summary>
    public class AppInsightsCreator : AzureResourceCreatorBase
    {
        public AppInsightsCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "ai")
        {
        }

        /// <summary>
        /// Creates an instance of an application insights component for a web app
        /// </summary>
        /// <param name="appInsightsName"></param>
        /// <returns></returns>
        public Insights.Component CreateWebInsights(string appInsightsName, Input<string> targetResourcename)
        {
            return Create(appInsightsName, targetResourcename, "web", Insights.ApplicationType.Web);
        }

        /// <summary>
        /// Create App Insights for a virtual machine
        /// </summary>
        /// <param name="appInsightsName"></param>
        /// <param name="targetResourcename"></param>
        /// <returns></returns>
        public Insights.Component CreateVmInsights(string appInsightsName, Input<string> targetResourcename)
        {
            return Create(appInsightsName, targetResourcename, "virtualMachine", Insights.ApplicationType.Other);
        }

        /// <summary>
        /// Creates an instance of an application insights component
        /// </summary>
        /// <param name="appInsightsName"></param>
        /// <param name="appKind">The kind of application that this component refers to, used to customize UI. This value is a freeform string, values should typically be one of the following: web, ios, other, store, java, phone.</param>
        /// <param name="appType"></param>
        /// <returns></returns>
        public Insights.Component Create(string appInsightsName, Input<string> targetResourcename, string appKind, Insights.ApplicationType appType)
        {
            appInsightsName = $"{this.Prefix}-{appInsightsName}";
            return new Insights.Component(appInsightsName, new Insights.ComponentArgs
            {
                ResourceName = targetResourcename,
                ResourceGroupName = this.ResourceGroupName,
                Kind = appKind,
                ApplicationType = appType,
            });
        }
    }
}
