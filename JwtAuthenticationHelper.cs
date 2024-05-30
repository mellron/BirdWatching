using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Util
{
    public static class JwtAuthenticationHelper
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            string secretKey = configuration["JwtAuth:SecretKey"] 
                ?? throw new InvalidOperationException("JWT Secret Key is not configured properly.");

            try
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    });
            }
            catch (Exception ex)
            {
                Util.Logger.Information(ex.Message);
            }
        }
    }
}
