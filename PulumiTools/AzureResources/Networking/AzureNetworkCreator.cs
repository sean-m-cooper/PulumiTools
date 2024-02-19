using Pulumi;
using Network = Pulumi.AzureNative.Network;
namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating Virtual Networks
    /// </summary>
    public class AzureNetworkCreator : AzureResourceCreatorBase
    {
        public AzureNetworkCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "vn")
        {

        }

        /// <summary>
        /// Creates a virtual network with a given name and address space
        /// </summary>
        /// <param name="addressSpace">CIDR notation. Defaults to 10.0.0/16</param>
        /// <param name="networkName">Virtual network name. Defaults to "vn-[DeploymentName]</param>
        /// <returns></returns>
        public Network.VirtualNetwork Create(string addressSpace, string networkName = "")
        {
            if (string.IsNullOrEmpty(networkName))
            {
                networkName = $"{this.Prefix}-{DeploymentName}";
            }
            else
            {
                networkName = $"{this.Prefix}-{networkName}";
            }

            var addressSpaceArgs = new Network.Inputs.AddressSpaceArgs
            {
                AddressPrefixes = { addressSpace }
            };

            var virtualNetworkArgs = new Network.VirtualNetworkArgs
            {
                VirtualNetworkName = networkName,
                ResourceGroupName = this.ResourceGroupName,
                AddressSpace = addressSpaceArgs,
                Location = this.Location
            };

            return new Network.VirtualNetwork(networkName, virtualNetworkArgs);
        }
    }
}
