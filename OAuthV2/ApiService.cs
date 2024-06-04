public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly OAuth2TokenService _tokenService;

    public ApiService(HttpClient httpClient, OAuth2TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<string> GetProtectedResourceAsync(string apiUrl)
    {
        var accessToken = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
