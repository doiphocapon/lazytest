using static LazyTest.Pages.IndexModel;
using System.Net;

namespace LazyTest.Models
{
    public class DisplayModel
    {
        public string? Url { get; set; }
        public string? StatusCode { get; set; }
        public string? LastStatusCode { get; set; }
        public long? ContentLength { get; set; }
        public long? LastContentLength { get; set; }
        public int? DOMCount { get; set; }
        public int? LastDOMCount { get; set; }
        public double? ResponseTime { get; set; }
        public double? LastResponseTime { get; set; }
        public string? Color { get; set; }
        public int? OrderIndex { get; set; } = 0;

    }


    public class TestWebsiteResult
    {
        public List<TestUrl>? TestResultItem { get; set; }
        public WebSite? Site { get; set; }

    }

    public class SitemapUrl
    {
        public int? SiteId { get; set; }
        public List<string>? Urls { get; set; }
    }

    public class TestResult
    {
        public List<TestPageModel>? TestResultItem { get; set; }
        public string? SiteMapUrl { get; set; }

    }
    public class TestResultDisplayModel
    {
        public List<DisplayModel>? TestResultItem { get; set; }
        public string? SiteMapUrl { get; set; }

    }
    public record TestPageModel(string Url, HttpStatusCode? StatusCode, long? ContentLength, int? DOMCount, string? ExceptionMessage = null);

    public class LastTestItem
    {
        public long? ContentLength { get; set; }
        public int? DomCount { get; set; }
        public string StatusCode { get; set; }
        public int? HttpStatusCode { get; set; }
        public double ResponseTime { get; set; }
    }
    public class TestSiteInBackGroundModel
    {
        public int WebSiteId { get; set; }
        public bool DomCount  { get; set; }
        public bool WithImage { get; set; }
        public string StartWihtUrl { get; set; }
    }
}
