using Pulumi;
using Pulumi.AzureNative.Network.Inputs;
using Network = Pulumi.AzureNative.Network;

namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating Azure Network Interfaces
    /// </summary>
    public class NetworkInterfaceCreator : AzureResourceCreatorBase
    {
        public NetworkInterfaceCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "ni")
        {
        }

        /// <summary>
        /// Create a Network Interace associated with a Network Security Group, Subnet, and Application Security Group
        /// </summary>
        /// <param name="networkInterfaceName"></param>
        /// <param name="networkSecurityGroupId"></param>
        /// <param name="subnetId"></param>
        /// <param name="applicationSecurityGroupId"></param>
        /// <returns></returns>
        /// <remarks>Ip Allocation Method is defaulted to Dynamic</remarks>
        public Network.NetworkInterface Create(string networkInterfaceName, Input<string> networkSecurityGroupId, Input<string> subnetId, Input<string> applicationSecurityGroupId)
        {
            networkInterfaceName = $"{this.Prefix}-{networkInterfaceName}";
            var networkSecurityGroupArgs = new Network.Inputs.NetworkSecurityGroupArgs
            {
                Id = networkSecurityGroupId
            };

            var ipConfigurationArgs = new Network.Inputs.NetworkInterfaceIPConfigurationArgs
            {
                Name = networkInterfaceName,
                PrivateIPAllocationMethod = "Dynamic",
                Subnet = new Network.Inputs.SubnetArgs
                {
                    Id = subnetId
                },
                ApplicationSecurityGroups = new InputList<ApplicationSecurityGroupArgs>
                {
                    new ApplicationSecurityGroupArgs
                    {
                          Id=applicationSecurityGroupId
                    }
                }
            };

            var networkInterfaceArgs = new Network.NetworkInterfaceArgs
            {
                Location = this.Location,
                ResourceGroupName = this.ResourceGroupName,
                NetworkSecurityGroup = networkSecurityGroupArgs,
                IpConfigurations =
                {
                    ipConfigurationArgs
                }
            };
            return new Network.NetworkInterface(networkInterfaceName, networkInterfaceArgs);
        }
    }
}
