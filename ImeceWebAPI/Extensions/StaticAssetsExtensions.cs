namespace ImeceWebAPI.Extensions;

public static class StaticAssetsExtensions
{
    public static WebApplication UseImeceStaticAssets(this WebApplication app)
    {
        app.UseStaticFiles();

        // Missing /uploads/* and /assets/* must not reach FallbackPolicy (401).
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var isPublicAsset = path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase);

            if (!isPublicAsset)
            {
                await next(context);
                return;
            }

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
        });

        return app;
    }
}
