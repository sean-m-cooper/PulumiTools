using Pulumi;
using Network = Pulumi.AzureNative.Network;

namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating an Azure Firewall
    /// </summary>
    public class FirewallCreator : AzureResourceCreatorBase
    {
        public FirewallCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "fw")
        {
        }

        /// <summary>
        /// Create an Azure Firewall with  the specified subnet and pulic ip address
        /// </summary>
        /// <param name="name"></param>
        /// <param name="subnetId"></param>
        /// <param name="publicIpAddressId"></param>
        /// <returns></returns>
        public Network.AzureFirewall Create(string name, Input<string> subnetId, Input<string> publicIpAddressId)
        {
            string firewallName = $"{this.Prefix}-{name}";
            var azureFirewallSkuArgs = new Network.Inputs.AzureFirewallSkuArgs
            {
                Name = Network.AzureFirewallSkuName.AZFW_VNet,
                Tier = Network.AzureFirewallSkuTier.Standard
            };

            var subnetResourceArgs = new Network.Inputs.SubResourceArgs
            {
                Id = subnetId
            };

            var azureFirewallIPConfigurationArgs = new Network.Inputs.AzureFirewallIPConfigurationArgs
            {
                Subnet = subnetResourceArgs
            };

            return new Network.AzureFirewall(firewallName, new Network.AzureFirewallArgs
            {
                Sku = azureFirewallSkuArgs,
                ThreatIntelMode = Network.AzureFirewallThreatIntelMode.Deny,
                ResourceGroupName = this.ResourceGroupName,
                Location = this.Location,
                ManagementIpConfiguration = azureFirewallIPConfigurationArgs,
                IpConfigurations = {
                    new Network.Inputs.AzureFirewallIPConfigurationArgs
                     {
                        Name=$"{firewallName}ipconfig",
                        PublicIPAddress = new Network.Inputs.SubResourceArgs
                        {
                            Id= publicIpAddressId
                        },
                        Subnet = subnetResourceArgs,
                     }
                }
            });
        }
    }
}
