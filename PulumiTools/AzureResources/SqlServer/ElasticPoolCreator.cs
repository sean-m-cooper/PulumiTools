using Pulumi;
using SQL = Pulumi.AzureNative.Sql;

namespace PulumiTools.AzureResources.SqlServer
{
    /// <summary>
    /// Class for creating SQL Server Elastic Pools
    /// </summary>
    public class ElasticPoolCreator : AzureResourceCreatorBase
    {
        public Input<string> ServerName { get; private set; }
        public SQL.Inputs.SkuArgs Sku { get; private set; }
        public Input<double> MazSizeInBytes { get; private set; }
        public Input<bool> ZoneRedundant { get; private set; }
        public SQL.Inputs.ElasticPoolPerDatabaseSettingsArgs PerDatabaseSettings { get; private set; }

        /// <summary>
        /// Default contstructor for an elastic pool creator. Defaults Sku to Standard with 100 eDTUs
        /// </summary>
        /// <param name="deploymentName"></param>
        /// <param name="location"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="serverName"></param>
        public ElasticPoolCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName, Input<string> serverName) :
            this(deploymentName, location, resourceGroupName, serverName, 107374182400, false,
            new SQL.Inputs.SkuArgs
            {
                Name = "StandardPool",
                Capacity = 100,
                Size = "Standard"

            },
             new SQL.Inputs.ElasticPoolPerDatabaseSettingsArgs
             {
                 MaxCapacity = 100,
                 MinCapacity = 0
             })
        { }

        /// <summary>
        /// Full constructor for the elastic pool creator. 
        /// </summary>
        /// <param name="deploymentName"></param>
        /// <param name="location"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="serverName"></param>
        /// <param name="mazSizeInBytes">Max value is 107374182400</param>
        /// <param name="zoneRedundant"></param>
        /// <param name="sku"></param>
        /// <param name="perDatabaseSettings"></param>
        public ElasticPoolCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName, Input<string> serverName,
            double mazSizeInBytes, bool zoneRedundant, SQL.Inputs.SkuArgs sku, SQL.Inputs.ElasticPoolPerDatabaseSettingsArgs perDatabaseSettings)
            : base(deploymentName, location, resourceGroupName, "elp")
        {
            this.ServerName = serverName;
            this.Sku = sku;
            this.MazSizeInBytes = mazSizeInBytes;
            this.ZoneRedundant = zoneRedundant;
            this.PerDatabaseSettings = perDatabaseSettings;
        }

        /// <summary>
        /// Creates an elasitc pool on the server
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public SQL.ElasticPool Create(string poolName)
        {
            poolName = $"{this.Prefix}-{poolName}";
            var elasticPoolArgs = new SQL.ElasticPoolArgs
            {
                Location = this.Location,
                ResourceGroupName = this.ResourceGroupName,
                ServerName = this.ServerName,
                Sku = this.Sku,
                MaxSizeBytes = this.MazSizeInBytes,
                ZoneRedundant = this.ZoneRedundant,
                PerDatabaseSettings = this.PerDatabaseSettings
            };

            return new SQL.ElasticPool(poolName, elasticPoolArgs);
        }
    }
}
