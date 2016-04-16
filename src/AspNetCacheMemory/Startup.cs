namespace AspNetCacheMemory
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCaching();
        }

        public void Configure(IApplicationBuilder app, IMemoryCache cache)
        {
            app.UseIISPlatformHandler();

            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("ASP.NET Core:  Memory Cache");
                await next();
            });

            app.Map(new PathString("/set"), branch =>
            {
                branch.Run(async context =>
                {
                    foreach (var value in context.Request.Query)
                    {
                        cache.Set(value.Key, value.Value.First());
                    }

                    await context.Response.WriteAsync("<br>values set");
                });
            });

            app.Map(new PathString("/get"), branch =>
            {
                branch.Run(async context =>
                {
                    var keys = context.Request.Query["key"];

                    foreach (var key in keys)
                    {
                        var value = cache.Get(key) ?? "(not found)";
                        await context.Response.WriteAsync($"<br>{key} :: {value}");
                    }
                });
            });

            app.Map(new PathString("/del"), branch =>
            {
                branch.Run(async context =>
                {
                    var keys = context.Request.Query["key"];

                    foreach (var key in keys)
                    {
                        cache.Remove(key);
                    }

                    await context.Response.WriteAsync("<br>values removed");
                });
            });

            app.Map(new PathString("/perf"), branch =>
            {
                branch.Run(async context =>
                {
                    var key = Guid.NewGuid();
                    var value = Guid.NewGuid();

                    cache.Set(key, value);

                    Guid cacheValue;

                    var reps = 1000000;

                    var sw = Stopwatch.StartNew();

                    for (var i = 0; i < reps; i++)
                    {
                        cacheValue = cache.Get<Guid>(key);
                    }

                    sw.Stop();

                    await
                        context.Response.WriteAsync(
                            $"<br>average time to retrieve value: {sw.ElapsedTicks/(decimal) reps} ticks");
                });
            });
        }
    }
}