using Pulumi;
using Resources = Pulumi.AzureNative.Resources;

namespace PulumiTools.AzureResources.ResourceGroup
{
    /// <summary>
    /// Class for creating Azure Resource Groups
    /// </summary>
    public class ResourceGroupCreator : AzureResourceCreatorBase
    {
        private string resourceGroupName;
        public ResourceGroupCreator(string deploymentName, Input<string> location, string resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "rg")
        {
            this.resourceGroupName = $"{this.Prefix}-{resourceGroupName}";
        }

        /// <summary>
        /// Creates a resource group
        /// </summary>
        /// <returns></returns>
        public Resources.ResourceGroup Create()
        {
            var resourceGroupArgs = new Resources.ResourceGroupArgs
            {
                Location = this.Location,
                ResourceGroupName = this.resourceGroupName
            };
            return new Resources.ResourceGroup(resourceGroupName, resourceGroupArgs);
        }
    }
}
