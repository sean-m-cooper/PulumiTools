using Pulumi;
using Pulumi.AzureNative.Network;
using System.Collections.Generic;
using System.Linq;
using Network = Pulumi.AzureNative.Network;

namespace PulumiTools.AzureResources.Networking
{
    /// <summary>
    /// Class for creating Azure Network Security Groups
    /// </summary>
    public class NetworkSecurityGroupCreator : AzureResourceCreatorBase
    {
        public NetworkSecurityGroupCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "nsg")
        {
        }

        /// <summary>
        /// Create a Network Security Group with a set of Security rules
        /// </summary>
        /// <param name="networkSecurityGroupName"></param>
        /// <param name="securityRules"></param>
        /// <returns></returns>
        public NetworkSecurityGroup Create(string networkSecurityGroupName, Network.Inputs.SecurityRuleArgs[] securityRules)
        {
            networkSecurityGroupName = $"{this.Prefix}-{networkSecurityGroupName}";
            var networkSecurityGroupArgs = new Network.NetworkSecurityGroupArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                SecurityRules = securityRules,
            };

            return new NetworkSecurityGroup(networkSecurityGroupName, networkSecurityGroupArgs);
        }

        /// <summary>
        /// Creates the default security rules to allow inbound HTTP, LoadBalancer traffic, and Bastion traffic
        /// </summary>
        /// <param name="applicationSecGroupIds"></param>
        /// <returns></returns>
        public static Network.Inputs.SecurityRuleArgs[] GenerateDefaultInboundBotSecurityRules(string bastionAddressCIDR, List<Input<string>> applicationSecGroupIds)
        {
            InputList<Network.Inputs.ApplicationSecurityGroupArgs> appSecGrpList = applicationSecGroupIds.Select(i => new Network.Inputs.ApplicationSecurityGroupArgs
            {
                Id = i
            }).ToArray();

            var allowHttpInbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowHttpInbound",
                Priority = 100,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "*",
                DestinationApplicationSecurityGroups = appSecGrpList,
                DestinationPortRanges = { "80", "443" },
            };

            var allowLoadBalancer = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowLoadBalancerInbound",
                Priority = 110,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "AzureLoadBalancer",
                DestinationPortRanges = { "80", "443" },
                DestinationApplicationSecurityGroups = appSecGrpList,
            };

            var allowBastionSshRdp = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowSshRdpFromBastion",
                Priority = 120,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = bastionAddressCIDR,
                DestinationPortRanges = { "3389", "22" },
                DestinationApplicationSecurityGroups = appSecGrpList,
            };

            var denyAllInbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "DenyAllInbound",
                Priority = 1000,
                Direction = "Inbound",
                Access = "Deny",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "*",
                DestinationPortRange = "*",
                DestinationAddressPrefix = "*",
            };

            return new Network.Inputs.SecurityRuleArgs[] { allowHttpInbound, allowLoadBalancer, allowBastionSshRdp, denyAllInbound };
        }

        /// <summary>
        /// Create the default security rules for the web portal
        /// </summary>
        /// <param name="applicationSecGroupIds"></param>
        /// <returns></returns>
        public static Network.Inputs.SecurityRuleArgs[] GenerateDefaultWebSecurityRules(List<Input<string>> applicationSecGroupIds)
        {
            InputList<Network.Inputs.ApplicationSecurityGroupArgs> appSecGrpList = applicationSecGroupIds.Select(i => new Network.Inputs.ApplicationSecurityGroupArgs
            {
                Id = i
            }).ToArray();

            var allowHttpInbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowHttpInbound",
                Priority = 100,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "*",
                DestinationApplicationSecurityGroups = appSecGrpList,
                DestinationPortRanges = { "80", "443" },
            };
            var allowLoadBalancer = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowLoadBalancerInbound",
                Priority = 110,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                DestinationApplicationSecurityGroups = appSecGrpList,
                SourceAddressPrefix = "AzureLoadBalancer",
                DestinationPortRanges = { "80", "443" },

            };
            return new Network.Inputs.SecurityRuleArgs[] { allowHttpInbound, allowLoadBalancer };
        }

        /// <summary>
        /// Create the default security rules for the sql server
        /// </summary>
        /// <param name="applicationSecGroupIds"></param>
        /// <returns></returns>
        public static Network.Inputs.SecurityRuleArgs[] GenerateDefaultSQLInboundSecurityRules(List<Input<string>> applicationSecGroupIds)
        {
            InputList<Network.Inputs.ApplicationSecurityGroupArgs> appSecGrpList = applicationSecGroupIds.Select(i => new Network.Inputs.ApplicationSecurityGroupArgs
            {
                Id = i
            }).ToArray();

            var allow1443Inbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "Allow1443Inbound",
                Priority = 100,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "*",
                DestinationApplicationSecurityGroups = appSecGrpList,
                DestinationPortRanges = { "1443" },
            };

            var allowSQLBrowserInbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowSQLBrowserInbound",
                Priority = 110,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "UDP",
                SourcePortRange = "*",
                SourceAddressPrefix = "*",
                DestinationApplicationSecurityGroups = appSecGrpList,
                DestinationPortRanges = { "1434" },
            };
            var allowLoadBalancer = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowLoadBalancerInbound",
                Priority = 120,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                DestinationApplicationSecurityGroups = appSecGrpList,
                SourceAddressPrefix = "AzureLoadBalancer",
                DestinationPortRanges = { "1443" },

            };

            var denyAllInbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "DenyAllInbound",
                Priority = 1000,
                Direction = "Inbound",
                Access = "Deny",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "*",
                DestinationPortRange="*",
                DestinationApplicationSecurityGroups = appSecGrpList,
            };

            return new Network.Inputs.SecurityRuleArgs[] { allow1443Inbound, allowSQLBrowserInbound, allowLoadBalancer, denyAllInbound };
        }

        /// <summary>
        /// Create the default security rules for an Azure Bastion Service
        /// </summary>
        /// <returns></returns>
        /// <remarks>SC: Rules are specified by Microsoft and should not be changed.</remarks>
        public static Network.Inputs.SecurityRuleArgs[] GenerateDefaultBastionSecurityRules()
        {

            #region Inbound Rules
            var allowHttps = new Network.Inputs.SecurityRuleArgs
            {

                Name = "AllowHttpsInbound",
                Priority = 100,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "Internet",
                DestinationPortRange = "443",
                DestinationAddressPrefix = "*"
            };

            var allowGatewayManager = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowGatewayManagerInbound",
                Priority = 110,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "GatewayManager",
                DestinationPortRanges = { "443" },
                DestinationAddressPrefix = "*"
            };

            var allowLoadBalancer = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowLoadBalancerInbound",
                Priority = 120,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "AzureLoadBalancer",
                DestinationPortRanges = { "443" },
                DestinationAddressPrefix = "*"
            };

            var allowBastionCommunication = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowBastionCommunication",
                Priority = 130,
                Direction = "Inbound",
                Access = "Allow",
                Protocol = "*",
                SourcePortRange = "*",
                SourceAddressPrefix = "VirtualNetwork",
                DestinationPortRanges = { "8080", "5701" },
                DestinationAddressPrefix = "VirtualNetwork"
            };

            var denyAllInbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "DenyAllInboud",
                Priority = 1000,
                Direction = "Inbound",
                Access = "Deny",
                Protocol = "TCP",
                SourcePortRange = "*",
                SourceAddressPrefix = "*",
                DestinationPortRange = "*",
                DestinationAddressPrefix = "*",
            };
            #endregion

            #region Outbound rules
            var allowSshRdpOutbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowSshRdpOutbound",
                Priority = 100,
                Direction = "Outbound",
                Access = "Allow",
                Protocol = "*",
                SourceAddressPrefix = "*",
                SourcePortRange = "*",
                DestinationAddressPrefix = "VirtualNetwork",
                DestinationPortRanges = { "22", "3389" }
            };

            var allowAzureCloudOutbound = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowAzureCloudOutbound",
                Priority = 110,
                Direction = "Outbound",
                Access = "Allow",
                Protocol = "TCP",
                SourceAddressPrefix = "*",
                SourcePortRange = "*",
                DestinationAddressPrefix = "AzureCloud",
                DestinationPortRange = "443"
            };

            var allowBastionCommunicationOut = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowBastionCommunicationOut",
                Priority = 120,
                Direction = "Outbound",
                Access = "Allow",
                Protocol = "*",
                SourceAddressPrefix = "VirtualNetwork",
                SourcePortRange = "*",
                DestinationAddressPrefix = "VirtualNetwork",
                DestinationPortRanges = { "8080", "5701" }
            };

            var getSessionInformation = new Network.Inputs.SecurityRuleArgs
            {
                Name = "AllowGetSessionInformation",
                Priority = 130,
                Direction = "Outbound",
                Access = "Allow",
                Protocol = "*",
                SourceAddressPrefix = "*",
                SourcePortRange = "*",
                DestinationAddressPrefix = "Internet",
                DestinationPortRange = "80"
            };
            #endregion

            return new Network.Inputs.SecurityRuleArgs[] { allowHttps, allowGatewayManager, allowLoadBalancer, allowBastionCommunication, denyAllInbound, allowSshRdpOutbound, allowAzureCloudOutbound, allowBastionCommunicationOut, getSessionInformation };

        }
    }
}
