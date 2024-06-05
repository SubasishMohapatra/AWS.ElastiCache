using Huron.AWS.Common.Core;
using Microsoft.Extensions.Configuration;

namespace Huron.AWS.SecretsManager.Core
{
    public class AwsSecretsManagerConfigOptions:AwsConfigOptions
    {
        public AwsSecretsManagerConfigOptions(IConfiguration configuration)
        {
            configuration.GetSection("AWSConfigOptions").Bind(this);
            BindNonNullValues(configuration.GetSection("AWSConfigOptions:SecretsManager"));
        }
    }
}
