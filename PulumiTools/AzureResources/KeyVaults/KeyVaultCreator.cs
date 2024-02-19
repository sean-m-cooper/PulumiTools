using Pulumi;
using System;
using KeyVault = Pulumi.AzureNative.KeyVault;

namespace PulumiTools.AzureResources.KeyVaults
{
    /// <summary>
    /// Class for creating and interacting with key vaults
    /// </summary>
    public class KeyVaultCreator : AzureResourceCreatorBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deploymentName"></param>
        /// <param name="location"></param>
        /// <param name="resourceGroupName"></param>
        public KeyVaultCreator(string deploymentName, Input<string> location, Input<string> resourceGroupName)
            : base(deploymentName, location, resourceGroupName, "kv")
        {
        }

        /// <summary>
        /// Creates a keyvault with the name [prefix]-[deployment name]
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="vaultPrefix">Defaults to "kv"</param>
        /// <returns></returns>
        public KeyVault.Vault Create(string tenantId, string objectId, string vaultPrefix = "")
        {
            string vaultName = string.IsNullOrEmpty(vaultPrefix) ? $"{this.Prefix}-{this.DeploymentName}" : $"{this.Prefix}-{vaultPrefix}-{this.DeploymentName}";
            var args = new KeyVault.VaultArgs
            {
                Location = this.Location,
                ResourceGroupName = this.ResourceGroupName,
                Properties = new KeyVault.Inputs.VaultPropertiesArgs
                {
                    EnabledForDiskEncryption = true,
                    TenantId = tenantId,
                    Sku = new KeyVault.Inputs.SkuArgs
                    {
                        Name = KeyVault.SkuName.Standard,
                        Family = KeyVault.SkuFamily.A
                    },
                    AccessPolicies = new KeyVault.Inputs.AccessPolicyEntryArgs
                    {
                        TenantId = tenantId,
                        ObjectId = objectId,
                        Permissions = new KeyVault.Inputs.PermissionsArgs
                        {
                            Secrets = { KeyVault.SecretPermissions.All },
                            Certificates = { KeyVault.CertificatePermissions.All },
                            Keys = { KeyVault.KeyPermissions.All },
                            Storage = { KeyVault.StoragePermissions.All }
                        }
                    }
                },
            };
            return new KeyVault.Vault(vaultName, args);
        }

        /// <summary>
        /// Create a key in the specified key vault
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="keyVaultName"></param>
        /// <returns></returns>
        public KeyVault.Key CreateKey(string keyName, Input<string> keyVaultName)
        {
            var args = new KeyVault.KeyArgs
            {
                KeyName = keyName,
                ResourceGroupName = this.ResourceGroupName,
                VaultName = keyVaultName,
                Properties = new KeyVault.Inputs.KeyPropertiesArgs
                {
                    KeySize = 2048,
                    Kty = KeyVault.JsonWebKeyType.RSA,
                }
            };
            return new KeyVault.Key(keyName, args);
        }

        /// <summary>
        /// Create an encryption secrete in the specified key vault
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="keyVaultName"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public KeyVault.Secret CreateEncryptionSecret(string secretName, Input<string> keyVaultName, Resource dependency = null)
        {
            var args = CreateArgs(secretName, keyVaultName);
            return CreateSecret(secretName, args, dependency);
        }

        /// <summary>
        /// Create a disk encryption secret in a specified key vault
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="keyVaultName"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public KeyVault.Secret CreateDiskEncryptionSecret(string secretName, Input<string> keyVaultName, Resource dependency = null)
        {
            var args = CreateArgs(secretName, keyVaultName);
            args.Tags.Add("DiskEncryptionKeyFileName", $"{secretName}");
            args.Tags.Add("DiskEncryptionKeyEncryptionAlgorithm", "RSA");
            return CreateSecret(secretName, args, dependency);
        }

        private KeyVault.Secret CreateSecret(string secretName, KeyVault.SecretArgs args, Resource dependency = null)
        {
            var opts = new CustomResourceOptions
            {
                DependsOn = { dependency }
            };
            return new KeyVault.Secret(secretName, args, opts);
        }

        private KeyVault.SecretArgs CreateArgs(string secretName, Input<string> keyVaultName)
        {
            return new KeyVault.SecretArgs
            {
                ResourceGroupName = this.ResourceGroupName,
                VaultName = keyVaultName,
                SecretName = secretName,
                Properties = new KeyVault.Inputs.SecretPropertiesArgs
                {
                    Value = Guid.NewGuid().ToString(),
                },
            };
        }

    }
}
