using System.Net;

namespace Huron.AWS.SecretsManager.Core
{
    public interface IAwsSecretsManagerService
    {
        Task<string> GetSecretAsync(string secretName);
    }
}