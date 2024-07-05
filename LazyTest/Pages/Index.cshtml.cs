using AngleSharp;
using AngleSharp.Io;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LazyTest.Pages
{
    public class IndexModel : PageModel
    {
        private string userAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;


        [BindProperty]
        public string? SiteMapUrl { get; set; }

        [BindProperty]
        public IFormFile TestFile { get; set; }

        public string? Message { get; set; }



        public TestResult? TestResultData { get; set; }
        public TestResultDisplayModel? TestResultDisplayData { get; set; }
        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public void OnGetAsync()
        {
            /*
            // Retrieve the JSON data from TempData if it exists
            var testResultData = TempData["ResultData"] as string;
            if (!string.IsNullOrWhiteSpace(testResultData))
            {
                TestResultData = JsonSerializer.Deserialize<TestResult>(testResultData);
                SiteMapUrl = TestResultData.SiteMapUrl;
            }
            */
        }
        public async Task<IActionResult> OnPostAsync()
        {/*
            if (!ModelState.IsValid)
            {
                return Page();
            }*/

            
            if (TestFile != null || TestFile?.Length > 0)
            {
                using var stream = TestFile.OpenReadStream();
                using var reader = new StreamReader(stream);

                var fileContents = await reader.ReadToEndAsync();
                TestResultData = JsonSerializer.Deserialize<TestResult>(fileContents);
                TestResultDisplayData = BuildDisplayModel(null, TestResultData);
                SiteMapUrl = TestResultData.SiteMapUrl;
                var jsonData = JsonSerializer.Serialize(TestResultData);
                // Save the JSON data to TempData
                TempData["ResultData"] = jsonData;

            }
            else if (!string.IsNullOrEmpty(SiteMapUrl))
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("user-agent", userAgent);
                try
                {
                    var response = await client.GetAsync(SiteMapUrl);
                    response.EnsureSuccessStatusCode();

                    var xmlContent = await response.Content.ReadAsStringAsync();
                    var resultItems = await ParseSitemapAsync(xmlContent);
                    TestResultData = new TestResult
                    {
                        SiteMapUrl = SiteMapUrl,
                        TestResultItem = resultItems
                    };
                    var LastTestData = GetLastData();
                    TestResultDisplayData = BuildDisplayModel(LastTestData, TestResultData);

                    var jsonData = JsonSerializer.Serialize(TestResultData);
                    // Save the JSON data to TempData
                    TempData["ResultData"] = jsonData;

                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., logging)
                    Message = ($"Error fetching sitemap: {ex.Message}");
                }
            }





            return Page();
        }
        public IActionResult OnGetSaveTest()
        {
            TestResultData = GetLastData();
            var fileName = $"Test-{GetDomainFromUrl(TestResultData?.SiteMapUrl ?? string.Empty)}-{DateTime.Now.ToString("yyyyMMdd-hhmm")}.json";
            var jsonContent = JsonSerializer.Serialize(TestResultData);
            var fileContent = new System.Text.UTF8Encoding().GetBytes(jsonContent);
            return File(fileContent, "application/json", fileName);
        }
        private async Task<List<TestPageModel>?> ParseSitemapAsync(string xmlContent)
        {

            try
            {
                var xdoc = XDocument.Parse(xmlContent);
                var ns = xdoc.Root.Name.Namespace;
                List<TestPageModel> testResult = new List<TestPageModel>();
                foreach (var urlElement in xdoc.Descendants(ns + "url"))
                {
                    var locElement = urlElement.Element(ns + "loc");
                    if (locElement != null)
                    {
                        //test url
                        var testUrl = await TestURL(locElement.Value);
                        testResult.Add(testUrl);
                    }
                }
                return testResult;
            }
            catch (Exception ex)
            {
                // Handle XML parsing exceptions
                Message = ($"Error fetching sitemap: {ex.Message}");
                return null;
            }
        }
        public async Task<TestPageModel> TestURL(string url)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/json");
                client.DefaultRequestHeaders.Add("user-agent", userAgent);

                var response = await client.GetAsync(url);
                var domCount = await DOMCaculateAsync(url);

                return new(url, response.StatusCode, response.Content.Headers?.ContentLength, domCount);
            }
            catch (Exception ex)
            {
                return new(url, null, null, null, ex.Message);
            }


        }
        public async Task<int?> DOMCaculateAsync(string url)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(url);
            return document.All.Count();
        }
        private static string GetDomainFromUrl(string url)
        {
            try
            {
                // Parse the URL
                Uri uri = new Uri(url);

                // Get the host name (domain)
                string domain = uri.Host;

                return domain;
            }
            catch (UriFormatException)
            {
                // Handle invalid URI format
                return null;
            }
        }

        private TestResult? GetLastData()
        {
            var testResultData = TempData["ResultData"] as string;
            if (!string.IsNullOrWhiteSpace(testResultData))
            {
                return  JsonSerializer.Deserialize<TestResult>(testResultData);
            }
            return null;
        }

        private TestResultDisplayModel? BuildDisplayModel(TestResult? LastTestResult, TestResult? currentTestResul)
        {
            List<DisplayModel>? displayModelItem = new List<DisplayModel>();
            string siteMapUrl = string.Empty;
            var defaultColor = "green";
         

            if (LastTestResult?.TestResultItem?.Any() ?? false)
            {
                foreach (var lastTestItem in LastTestResult.TestResultItem)
                {

                    displayModelItem.Add(new DisplayModel { Url = lastTestItem.Url, StatusCode= null, LastStatusCode= lastTestItem.StatusCode, ContentLength= null, LastContentLength= lastTestItem.ContentLength, DOMCount= null, LastDOMCount= lastTestItem.DOMCount, ExceptionMessage=lastTestItem.ExceptionMessage ,Color=defaultColor,OrderIndex=0});

                }
                 siteMapUrl = LastTestResult?.SiteMapUrl;
            }
            foreach (var currentTestItem in currentTestResul.TestResultItem)
            {
                int? orderIndex = 0;
                string color = defaultColor;
                var displayItem = displayModelItem.FirstOrDefault(u => u.Url.Equals(currentTestItem.Url));
                if (displayItem != null)
                {
                    displayItem.StatusCode = currentTestItem.StatusCode;
                    displayItem.ContentLength = currentTestItem.ContentLength;
                    displayItem.DOMCount = currentTestItem.DOMCount;
                    displayItem.ExceptionMessage = currentTestItem.ExceptionMessage;

                    if (displayItem.LastStatusCode != displayItem.StatusCode)
                    {
                        color = "red";
                        orderIndex++;
                    }
                    if (displayItem.ContentLength != displayItem.LastContentLength)
                    {
                        color = "red";
                        orderIndex++;
                    }
                    if (displayItem.DOMCount != displayItem.LastDOMCount)
                    {
                        color = "red";
                        orderIndex++;
                    }
                    displayItem.Color = color;
                    displayItem.OrderIndex = orderIndex;

                }
                else
                {
                    displayModelItem.Add(new DisplayModel{ Url= currentTestItem.Url, StatusCode= currentTestItem.StatusCode, LastStatusCode=null, ContentLength= currentTestItem.ContentLength, LastContentLength= null, DOMCount= currentTestItem.DOMCount, LastDOMCount= null, ExceptionMessage= currentTestItem.ExceptionMessage, Color = color, OrderIndex = 0 });
                }
                siteMapUrl = currentTestResul?.SiteMapUrl;
            }

            return new TestResultDisplayModel
            {
                TestResultItem = displayModelItem.OrderByDescending(x=>x.OrderIndex).ToList(),
                SiteMapUrl = siteMapUrl
            };
        }

    }

    public record TestPageModel(string Url, HttpStatusCode? StatusCode, long? ContentLength, int? DOMCount, string? ExceptionMessage = null);

    public class DisplayModel
    {
        public string? Url { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public HttpStatusCode? LastStatusCode { get; set; }
        public long? ContentLength { get; set; }
        public long? LastContentLength { get; set; }
        public int? DOMCount { get; set; }
        public int? LastDOMCount { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? Color { get; set; }
        public int? OrderIndex { get; set; } = 0;
    
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

}