using Pulumi;
using Web = Pulumi.AzureNative.Web;

namespace PulumiTools.AzureResources.WebApps
{
    /// <summary>
    /// Class for creating an Azure Application Service Plan
    /// </summary>
    public class AppServicePlanCreator : AzureResourceCreatorBase
    {
        public AppServicePlanCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName) 
            : base(deploymentName, location, resourceGroupName,"asp")
        {
        }

        /// <summary>
        /// Creates a B1 Basic app service plan
        /// </summary>
        /// <param name="appServicePlanName"></param>
        /// <returns></returns>
        public Web.AppServicePlan CreateBasicAppPlan(string appServicePlanName)
        {
            appServicePlanName = $"{this.Prefix}-{appServicePlanName}";
            return Create(appServicePlanName, "B1", "Basic");
        }

        /// <summary>
        /// Create an app service plan
        /// </summary>
        /// <param name="appServicePlanName"></param>
        /// <param name="skuName">Name of the sku to use</param>
        /// <param name="skuTier">Tier of the sku to use</param>
        /// <returns></returns>
        public Web.AppServicePlan Create(string appServicePlanName, string skuName, string skuTier)
        {
            var skuArgs = new Web.Inputs.SkuDescriptionArgs
            {
                Name = skuName,
                Tier = skuTier,
            };
            Web.AppServicePlanArgs args = new Web.AppServicePlanArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                Kind = "App",
                Sku = skuArgs,
            };
            return new Web.AppServicePlan(appServicePlanName, args);
        }
    }
}
