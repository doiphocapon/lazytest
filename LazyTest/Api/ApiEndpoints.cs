using LazyTest.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LazyTest.Api
{
    public static class ApiEndpoints
    {
        public static void MapApiEndpoints(this IEndpointRouteBuilder app)
        {
            var channelService = app.ServiceProvider.GetRequiredService<TestChannelService>();

            app.MapGet("/api/testitem", async (HttpContext context, ITestingServices testingServices) =>
            {
                await ApiHandlers.HandleTestItemAsync(context, testingServices);
            });
            app.MapGet("/api/testsite", async (HttpContext context, ITestingServices testingServices) =>
            {
                await ApiHandlers.HandleTestSiteAsync(context, testingServices);
            });
            app.MapGet("/api/runsiteinbackground", async (HttpContext context) =>
            {
                await ApiHandlers.HandleTestSiteInBackgroundAsync(context, channelService);
            });
          
        }
    }
}
