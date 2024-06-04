public static class OAuth2AuthenticationHelper
{
    public static void AddOAuth2Authentication(this IServiceCollection services)
    {
        try
        {
            string authority = Environment.GetEnvironmentVariable("OAUTH2_AUTHORITY") 
                ?? throw new InvalidOperationException("OAuth2 Authority is not configured properly.");
            string clientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID") 
                ?? throw new InvalidOperationException("OAuth2 Client ID is not configured properly.");
            string clientSecret = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_SECRET") 
                ?? throw new InvalidOperationException("OAuth2 Client Secret is not configured properly.");
            string scope = Environment.GetEnvironmentVariable("OAUTH2_SCOPE") 
                ?? throw new InvalidOperationException("OAuth2 Scope is not configured properly.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = clientId;
                    options.RequireHttpsMetadata = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
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
