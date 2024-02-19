using Pulumi;

namespace PulumiTools.AzureResources
{
    /// <summary>
    /// Base class for all all resource creator classes
    /// </summary>
    public abstract class AzureResourceCreatorBase
    {
        public string DeploymentName { get; private set; }
        public Input<string> Location { get; private set; }
        public Input<string> ResourceGroupName { get; private set; }
        public string Prefix { get; private set; }

        public AzureResourceCreatorBase(string deploymentName, Input<string> location, Input<string> resourceGroupName, string prefix)
        {
            DeploymentName = deploymentName;
            Location = location;
            ResourceGroupName = resourceGroupName;
            Prefix = prefix;
        }
    }
}
