using AngleSharp.Io;
using LazyTest.Models;
using LazyTest.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

public static class ApiHandlers
{
    public static async Task HandleTestItemAsync(HttpContext context, ITestingServices testingServices)
    {
        var itemurl = (string?)context.Request.Query["itemurl"];
        _ = int.TryParse((string?)context.Request.Query["siteid"], out var siteid);
        var domcountString = (string?)context.Request.Query["domcount"];
        var domcount = bool.TryParse(domcountString, out var domcountValue) && domcountValue;
        if (string.IsNullOrEmpty(itemurl))
        {
            await context.Response.WriteAsync("URL parameter is missing");
            return;
        }
        if (siteid == 0)
        {
            await context.Response.WriteAsync("Site ID parameter is missing");
            return;
        }
        var results = await testingServices.TestAndSaveThisURL(itemurl, siteid, domcount);

        if (results == null)
        {
            await context.Response.WriteAsync("No results found");
            return;
        }
        var displayModel = new DisplayModel {
            StatusCode = results.StatusCode,
            Url = results.Url,
            DOMCount = results.DomCount,
            ContentLength = results.ContentLength,
            ResponseTime = results.ResponseTime,
            LastStatusCode = results.LastTest.StatusCode,
            LastContentLength = results.LastTest.ContentLength,
            LastDOMCount = results.LastTest.DomCount,
            LastResponseTime = results.LastTest.ResponseTime,


        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(displayModel));
    }
    public static async Task HandleTestSiteAsync(HttpContext context, ITestingServices testingServices)
    {
        var sitemapUrl = (string?)context.Request.Query["sitemapurl"];

        var token = (string?)context.Request.Query["token"];
        var withImageString = (string?)context.Request.Query["image"];
        var withImage = bool.TryParse(withImageString, out var withImageValue) && withImageValue;
        if (string.IsNullOrEmpty(sitemapUrl))
        {
            await context.Response.WriteAsync("URL parameter is missing");
            return;
        }

        var results = await testingServices.GetWebsiteUrls(sitemapUrl, token, withImage);

        if (results == null)
        {
            await context.Response.WriteAsync("No results found");
            return;
        }
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(results));
    }
    public static async Task HandleTestSiteInBackgroundAsync(HttpContext context,  TestChannelService testChannelService)
    {
        _ = int.TryParse((string?)context.Request.Query["siteid"], out var siteid);
        var itemurl = (string?)context.Request.Query["itemurl"];
        var domcountString = (string?)context.Request.Query["domcount"];
        var domcount = bool.TryParse(domcountString, out var domcountValue) && domcountValue;
        var withImageString = (string?)context.Request.Query["image"];
        var withImage = bool.TryParse(withImageString, out var withImageValue) && withImageValue;
        await testChannelService.Writer.WriteAsync(new TestSiteInBackGroundModel { WebSiteId=siteid,StartWihtUrl = itemurl,WithImage=withImage, DomCount= domcount });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("OK");
    }
  
}
