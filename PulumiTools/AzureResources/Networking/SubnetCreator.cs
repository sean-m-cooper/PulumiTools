using Pulumi;
using Network = Pulumi.AzureNative.Network;

namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating an Azure Subnet
    /// </summary>
    public class SubnetCreator : AzureResourceCreatorBase
    {
        public Input<string> ParentNetworkName { get; private set; }
        public SubnetCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName, Input<string> parentNetworkName)
            : base(deploymentName, location, resourceGroupName, "sn")
        {
            this.ParentNetworkName = parentNetworkName;
        }

        /// <summary>
        /// Create a subnet with a specified address space
        /// </summary>
        /// <param name="subnetName"></param>
        /// <param name="addressSpace"></param>
        /// <param name="useSuppliedName"></param>
        /// <returns></returns>
        public Network.Subnet Create(string subnetName, string addressSpace, bool useSuppliedName = false)
        {
            if (!useSuppliedName)
            {
                subnetName = $"{this.Prefix}-{subnetName}";
            }

            var subnet1Args = CreateSubnetArgs(subnetName, addressSpace);

            return new Network.Subnet(subnetName, subnet1Args);
        }


        /// <summary>
        /// Create a subnet with a network security group
        /// </summary>
        /// <param name="subnetName">Name of the subnet</param>
        /// <param name="addressSpace">Address space in CIDR notation</param>
        /// <param name="networkSecurityGroupId">ID of the Network Security Group the subnet is to be associated with</param>
        /// <returns></returns>
        public Network.Subnet Create(string subnetName, string addressSpace, Input<string> networkSecurityGroupId, bool useSuppliedName = false)
        {
            if (!useSuppliedName)
            {
                subnetName = $"{this.Prefix}-{subnetName}";
            }

            var networkSecurityGroupArgs = new Network.Inputs.NetworkSecurityGroupArgs
            {
                Id = networkSecurityGroupId
            };

            var subnet1Args = CreateSubnetArgs(subnetName, addressSpace);
            subnet1Args.NetworkSecurityGroup = networkSecurityGroupArgs;

            return new Network.Subnet(subnetName, subnet1Args);
        }

        private Network.SubnetArgs CreateSubnetArgs(string subnetName, string addressSpace)
        {
            return new Network.SubnetArgs
            {
                SubnetName = subnetName,
                ResourceGroupName = this.ResourceGroupName,
                VirtualNetworkName = this.ParentNetworkName,
                AddressPrefix = addressSpace,
                PrivateEndpointNetworkPolicies = Network.VirtualNetworkPrivateEndpointNetworkPolicies.Disabled
            };
        }
    }
}
