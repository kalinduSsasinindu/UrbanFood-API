using DMCW.API.Middleware;

namespace DMCW.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void ConfigureMiddlewares(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
                    options.OAuthClientId(app.ApplicationServices.GetRequiredService<IConfiguration>()["Authentication:Google:ClientId"]);
                    options.OAuthClientSecret(app.ApplicationServices.GetRequiredService<IConfiguration>()["Authentication:Google:ClientSecret"]);
                    options.OAuthUsePkce();
                    options.OAuthScopes("openid", "profile", "email");
                    options.OAuth2RedirectUrl(app.ApplicationServices.GetRequiredService<IConfiguration>()["Authentication:Google:APIRedirectURL"]);
                });
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<CustomHeadersHandlingMiddleware>();
            app.UseRouting();
            app.UseCors("AllowSpecificOrigin");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
