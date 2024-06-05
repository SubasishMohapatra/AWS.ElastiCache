using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using S3.Core;
using Huron.AWS.Common.Core;
using Polly;
using Serilog;

namespace Huron.AWS.SecretsManager.Core
{
    public class AwsSecretsManagerService : IAwsSecretsManagerService
    {
        private readonly IAwsServiceClientFactory<IAmazonSecretsManager> secretManagerClientFactory;
        private readonly ILogger logger;

        public AwsSecretsManagerService(IAwsServiceClientFactory<IAmazonSecretsManager> secretManagerClientFactory,
            AwsSecretsManagerConfigOptions awsSecretsManagerConfigOptions, ILogger logger)
        {
            this.secretManagerClientFactory = secretManagerClientFactory;
            this.logger = logger;
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            var result = await PollyRetry.ExecuteAsync<string>(async () =>
            {
                var secretManagerClient = secretManagerClientFactory.CreateClient();

                GetSecretValueRequest request = new GetSecretValueRequest
                {
                    SecretId = secretName,
                    VersionStage = SecretsManagerConstants.AWSCURRENT,
                };

                GetSecretValueResponse response;
                response = await secretManagerClient.GetSecretValueAsync(request);

                var secret = response.SecretString;

                return secret;
            },
            SecretsManagerFallbackAsync, SecretsManagerFallbackActionOnFallbackAsync, this.logger);
            return result;
        }

        private Task SecretsManagerFallbackActionOnFallbackAsync(DelegateResult<string> response, Context context)
        {
            //Console.WriteLine("About to call the fallback action. This is a good place to do some logging");
            return Task.CompletedTask;
        }

        private Task<string> SecretsManagerFallbackAsync(DelegateResult<string> responseToFailedRequest, Context context, CancellationToken cancellationToken)
        {
            var ex = responseToFailedRequest.Exception;
            logger.Error("SecretsManagerFallbackAsync error: {Error}", ex.Message);
            return Task.FromResult(ex.Message);
        }
    }

}
