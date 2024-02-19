using Pulumi;
using SQL = Pulumi.AzureNative.Sql;

namespace PulumiTools.AzureResources.SqlServer
{
    /// <summary>
    /// Class for creating Azure SQL Databases
    /// </summary>
    public class SqlDatabaseCreator : AzureResourceCreatorBase
    {

        public Input<string> ServerName { get; private set; }
        public Input<string> ElasticPoolId { get; private set; }

        public SqlDatabaseCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName, Input<string> serverName, Input<string> elasticPoolId)
            : base(deploymentName, location, resourceGroupName, "sqldb")
        {
            this.ServerName = serverName;
            this.ElasticPoolId = elasticPoolId;
        }

        /// <summary>
        /// Create a SQL database
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public SQL.Database Create(string databaseName)
        {
            databaseName = $"{this.Prefix}-{databaseName}";
            var args = new SQL.DatabaseArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                ServerName = this.ServerName,
                Location = this.Location,
                ElasticPoolId = this.ElasticPoolId,
            };
            return new SQL.Database(databaseName, args);
        }

        /// <summary>
        /// Create a SQL database with a backup policy
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="retentionDays"></param>
        /// <returns></returns>
        public SQL.Database CreateWithBackupPolicy(string databaseName, int retentionDays = 30)
        {
            var sqlDatabase = Create(databaseName);
            _ = CreateDefaultBackupRetentionPolicy(databaseName, sqlDatabase.Name, retentionDays);
            return sqlDatabase;
        }

        /// <summary>
        /// Create a default backup policy for a database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="databaseName"></param>
        /// <param name="retentionDays"></param>
        /// <returns></returns>
        public SQL.BackupShortTermRetentionPolicy CreateDefaultBackupRetentionPolicy(string name, Input<string> databaseName, int retentionDays)
        {
            name = $"{this.Prefix}-{name}";
            var args = new SQL.BackupShortTermRetentionPolicyArgs
            {
                DatabaseName = databaseName,
                PolicyName = "default",
                ResourceGroupName = this.ResourceGroupName,
                RetentionDays = retentionDays,
                ServerName = this.ServerName,
            };

            return new SQL.BackupShortTermRetentionPolicy(name, args);
        }
    }
}
