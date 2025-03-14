namespace DMCW.API.Middleware
{
    public class CustomHeadersHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomHeadersHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("ClientId", out var clientId))
            {
                context.Items["ClientId"] = clientId.ToString();
            }

            

            if (context.Request.Headers.TryGetValue("UserId", out var userId))
            {
                context.Items["UserId"] = userId.ToString();
            }
            await _next(context);
        }
    }
}
