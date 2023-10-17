namespace Cms.Integrations.Magento.Client;

public static class MagentoClientFactory
{
    public static void ConfigureHttpClient(
        HttpClient httpClient, string token)
    {
        ConfigureHttpClientCore(httpClient);

        var headers = httpClient.DefaultRequestHeaders;
        headers.Add("Authorization", $"Bearer {token}");
    }

    private static void ConfigureHttpClientCore(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
    }
}