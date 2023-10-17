namespace Cms.Integrations.Magento.Client;

public interface ITokenManager
{
    Task<string> GetToken();
}

public class TokenManager : ITokenManager
{
    private readonly MagentoClient _httpClient;
    private string _token;

    public TokenManager(MagentoClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetToken()
    {
        if (string.IsNullOrEmpty(_token) || IsTokenExpired())
        {
            _token = await FetchToken();
        }

        return _token;
    }

    private async Task<string> FetchToken()
    {
        var token = await _httpClient.GetToken();
        return token;
    }

    private static bool IsTokenExpired()
    {
        return false;
    }
}