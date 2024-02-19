using Newtonsoft.Json;
namespace Silverwork.PulumiOps
{
    public class InfrastructureConfig
    {
        [JsonRequired]
        public string AdminPassword { get; set; }

        [JsonRequired]
        public string AdminUsername { get; set; }

        [JsonRequired]
        public string BastionAddressSpace { get; set; }

        [JsonRequired]
        public int BotVmCount { get; set; }

        public int DbRetentionDays { get; set; } = 30;

        [JsonRequired]
        public string FirewallSubnetAddressSpace { get; set; }

        [JsonRequired]
        public string Location { get; set; }

        [JsonRequired]
        public string PortalLocation { get; set; }

        [JsonRequired]
        public string DeploymentSuffix { get; set; }

        [JsonRequired]
        public int ServiceVmCount { get; set; }

        [JsonRequired]
        public string SqlAdminGroupId { get; set; }

        [JsonRequired]
        public string SqlAdminGroupName { get; set; }

        [JsonRequired]
        public string SqlPassword { get; set; }

        [JsonRequired]
        public string SqlUsername { get; set; }

        [JsonRequired]
        public string Subnet1AddressSpace { get; set; }

        [JsonRequired]
        public bool UseBotImage { get; set; }

        [JsonRequired]
        public string V2SubnetAddressSpace { get; set; }

        [JsonRequired]
        public string VmBotImageName { get; set; }

        [JsonRequired]
        public string VmImageGallery { get; set; }

        [JsonRequired]
        public string VmImageResourceGroup { get; set; }
        [JsonRequired]
        public string VNetAddressSpace { get; set; }
    }
}
