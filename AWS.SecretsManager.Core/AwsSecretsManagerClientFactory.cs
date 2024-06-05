using Amazon;
using Amazon.SecretsManager;
using Huron.AWS.Common.Core;

namespace Huron.AWS.SecretsManager.Core
{
    public class AwsSecretsManagerClientFactory : IAwsServiceClientFactory<IAmazonSecretsManager>
    {
        private readonly string awsRegion;
        private readonly string awsAccountId;
        private readonly string awsIamAccessKey;
        private readonly string awsIamSecretKey;
        public AwsSecretsManagerClientFactory(AwsSecretsManagerConfigOptions awsSecretsManagerConfigOptions)
        {
            this.awsRegion = awsSecretsManagerConfigOptions.Region;
            this.awsAccountId = awsSecretsManagerConfigOptions.AccountId;
            this.awsIamAccessKey = awsSecretsManagerConfigOptions.IamAccessKey;
            this.awsIamSecretKey = awsSecretsManagerConfigOptions.IamSecretKey;
        }

        public IAmazonSecretsManager CreateClient(string roleArn = null)
        {
            var config = new AmazonSecretsManagerConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(this.awsRegion),
            };

            if (Environment.GetEnvironmentVariable("environment") == "local")
                return new AmazonSecretsManagerClient(awsIamAccessKey, awsIamSecretKey, config);

            return new AmazonSecretsManagerClient(config);
        }
    }
}
