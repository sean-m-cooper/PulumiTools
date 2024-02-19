using Pulumi;
using Pulumi.AzureNative.Network;
using Network = Pulumi.AzureNative.Network;


namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating an Azure Route Table
    /// </summary>
    public class RouteTableCreator : AzureResourceCreatorBase
    {
        public RouteTableCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "rt")
        {
        }

        /// <summary>
        /// Create an Azure Route Table
        /// </summary>
        /// <returns></returns>
        public Network.RouteTable Create()
        {
            var name = $"{Prefix}-{DeploymentName}";
            RouteTableArgs args = new RouteTableArgs
            {
                Location = this.Location,
                ResourceGroupName = this.ResourceGroupName,
            };
            return new Network.RouteTable(name, args);
        }
    }
}
