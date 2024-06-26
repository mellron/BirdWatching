public static class OAuth2AuthenticationHelper
{
    public static void AddOAuth2Authentication(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            string tokenEndpoint = configuration["OAuth2:TokenEndpoint"] 
                ?? throw new InvalidOperationException("OAuth2 Token Endpoint is not configured properly.");
            string clientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID") 
                ?? throw new InvalidOperationException("OAuth2 Client ID is not configured properly.");
            string clientSecret = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_SECRET") 
                ?? throw new InvalidOperationException("OAuth2 Client Secret is not configured properly.");

            services.AddHttpClient<OAuth2TokenService>();
            services.AddSingleton(new OAuth2TokenServiceConfiguration
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                TokenEndpoint = tokenEndpoint
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";

                            var errorDetails = new
                            {
                                context.Exception.Message,
                                StackTrace = context.Exception.StackTrace,
                                context.Exception.Source
                            };

                            var result = JsonConvert.SerializeObject(new { message = "Authentication failed", errorDetails });

                            return context.Response.WriteAsync(result);
                        }
                    };
                });
        }
        catch (Exception ex)
        {
            // Log the exception here or handle it as needed
            Console.WriteLine($"Error configuring OAuth2 authentication: {ex.Message}");
        }
    }
}

public class OAuth2TokenServiceConfiguration
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string TokenEndpoint { get; set; }
}

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
        var clientCredentials = Encoding.ASCII.GetBytes($"{_config.ClientId}:{_config.ClientSecret}");
        var request = new HttpRequestMessage(HttpMethod.Post, _config.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            })
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(clientCredentials));

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<OAuth2TokenResponse>(payload);

        return tokenResponse.AccessToken;
    }
}

public class OAuth2TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}
