public static class OAuth2AuthenticationHelper
{
    public static void AddOAuth2Authentication(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            string authority = configuration["OAuth2:Authority"] 
                ?? throw new InvalidOperationException("OAuth2 Authority is not configured properly.");
            string audience = configuration["OAuth2:Audience"] 
                ?? throw new InvalidOperationException("OAuth2 Audience is not configured properly.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = audience;
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
