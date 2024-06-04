public class OAuth2TokenService
{
    private readonly HttpClient _httpClient;
    private readonly OAuth2TokenServiceConfiguration _config;

    public OAuth2TokenService(HttpClient httpClient, OAuth2TokenServiceConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _config.TokenEndpoint);

        // Add the required headers
        request.Headers.Add("grant_type", "client_credentials");
        request.Headers.Add("client_id", _config.ClientId);
        request.Headers.Add("client_secret", _config.ClientSecret);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<OAuth2TokenResponse>(payload);

        return tokenResponse.AccessToken;
    }
}

public class OAuth2TokenResponse
{
    [JsonProperty("refresh_token_expires_in")]
    public string RefreshTokenExpiresIn { get; set; }

    [JsonProperty("api_product_list")]
    public string ApiProductList { get; set; }

    [JsonProperty("api_product_list_json")]
    public List<string> ApiProductListJson { get; set; }

    [JsonProperty("organization_name")]
    public string OrganizationName { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("issued_at")]
    public string IssuedAt { get; set; }

    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("application_name")]
    public string ApplicationName { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonProperty("expires_in")]
    public string ExpiresIn { get; set; }

    [JsonProperty("refresh_count")]
    public string RefreshCount { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }
}
