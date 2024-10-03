using AngleSharp;
using System.Xml.Linq;
using LazyTest.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using LazyTest.Constants;



namespace LazyTest.Services
{
    public class TestingServices : ITestingServices
    {
        private readonly string _userAgent;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TestingServices> _logger;
        private readonly LazyTestContext _dbContext;

        public TestingServices(IHttpClientFactory httpClientFactory, LazyTestContext dbContext, ILogger<TestingServices> logger)
        {
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
            _logger = logger;
            _userAgent = DefaultConstants.DefaultBrowserAgent;
        }

        public async Task<List<WebSite>> GetTestSitesAsync()
        {
            return await _dbContext.TestSite.ToListAsync<WebSite>();
        }

        public async Task<SitemapUrl> GetWebsiteUrls(string sitemapUrl, string? websiteGuid = null, bool withImage = false)
        {

            List<string?> urls = null;
            var SiteMapUrl = sitemapUrl;
            try
            {
                urls = await GetUrlsAsync(SiteMapUrl, withImage);
            }
            catch
            { }

            if (urls == null)
            {
                SiteMapUrl = await GetSitemapUrlFromRobotsTxt(SiteMapUrl);
                urls = await GetUrlsAsync(SiteMapUrl, withImage);
            }

            if (urls == null)
            {

                throw new Exception(($"this {SiteMapUrl} is not an sitemap format"));

            }
            var site = await CreateTestSite(SiteMapUrl, websiteGuid, urls.Count);


            return new SitemapUrl
            {
                SiteId = site.WebsiteId,
                Urls = urls

            };
        }

        public async Task<List<string>?> GetUrlsAsync(string siteMapUrl, bool withImage = false)
        {
            try
            {

          
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/json");
            var response = await client.GetAsync(siteMapUrl);
            var contentType = response?.Content?.Headers.ContentType.MediaType;
            if (contentType == "application/xml" || contentType == "text/xml")
            {
                var xmlContent = await response.Content.ReadAsStringAsync();
                return await ReadSiteMapAsync(xmlContent, withImage);
            }
            return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e,$"There is an error with sitemap:{siteMapUrl} ");
                return null;
            }
        }

        public async Task<string?> GetSitemapUrlFromRobotsTxt(string domain)
        {

            var robotsUrl = $"{domain}/robots.txt";
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            var response = await client.GetStringAsync(robotsUrl);

            var lines = response.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (line.StartsWith("Sitemap:", StringComparison.OrdinalIgnoreCase))
                {
                    return line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                }
            }

            return null;
        }

        public async Task TestAndSaveSiteStartWithThisURL(TestSiteInBackGroundModel model)
        {
            var testSite = await GetTestSiteAsync(model.WebSiteId);
            if (testSite == null)
            {
                return;
            }
            var urls = await GetUrlsAsync(testSite.SitemapUrl, model.WithImage);
            if (urls == null)
            {
                return;
            }
            bool ignore = true;
            await UpdateIsTesting(testSite.WebsiteId, true);


            foreach (var url in urls)
            {
                if (url == model.StartWihtUrl)
                {
                    ignore = false;
                }

                if (ignore == false)
                {
                    await TestAndSaveThisURL(url, model.WebSiteId, model.DomCount);

                }
            }
            await UpdateIsTesting(testSite.WebsiteId, false);

        }
        public async Task<TestUrl?> TestAndSaveThisURL(string url, int websiteId, bool withdomcount = false)
        {
            try
            {
                var testUrl = await TestThisURL(url, websiteId, withdomcount);
                if (testUrl != null)
                {
                    await SaveURL(testUrl);
                    await IncreaseTestedUrlNumber(websiteId);
                    var lastTestSite = await GetLastTest(websiteId);
                    var lastTestItem = await GetLastItem(websiteId, url);
                    if (lastTestItem != null)
                    {
                        testUrl.LastTest = new LastTestItem
                        {
                            ContentLength = lastTestItem.ContentLength,
                            HttpStatusCode = lastTestItem.HttpStatusCode,
                            DomCount = lastTestItem.DomCount,
                            ResponseTime = lastTestItem.ResponseTime,
                            StatusCode = lastTestItem.StatusCode

                        };
                    }
                }

                return testUrl;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"There is an error with url:{url} ");
                return null;
            }
           

        }
        private async Task<TestUrl?> TestThisURL(string url, int websiteId, bool withdomcount = false)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/json");
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var response = await client.GetAsync(url);
                stopwatch.Stop();
                int? domCount = 0;
                if (withdomcount)
                {
                    domCount = await DOMCaculateAsync(url);
                }

                var httpStatusCode = (int)response.StatusCode;

                return new TestUrl
                {
                    Url = url,
                    StatusCode = response.StatusCode.ToString(),
                    HttpStatusCode = httpStatusCode,
                    ContentLength = response.Content.Headers?.ContentLength,
                    DomCount = domCount,
                    WebsiteId = websiteId,
                    ResponseTime = stopwatch.Elapsed.TotalMilliseconds


                };
            }
            catch (Exception ex)
            {
                return new TestUrl
                {
                    Url = url,
                    StatusCode = $"Exception:{ex.Message}",
                    HttpStatusCode = null,
                    ContentLength = null,
                    DomCount = 0,
                    WebsiteId = websiteId,
                    ResponseTime = 0


                };

            }
        }
        public async Task<int?> DOMCaculateAsync(string url)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(url);
            return document.All.Count();
        }

        public async Task<WebSite?> GetTestSiteAsync(int id)
        {
            return await _dbContext.TestSite.FirstOrDefaultAsync(m => m.WebsiteId == id);
        }
        public async Task<List<TestUrl>?> GetResultItems(int id)
        {

            return await _dbContext.TestResult.Where(m => m.WebsiteId == id).ToListAsync();

        }

        public async Task<WebSite?> GetLastTest(int? currentTestID = null)
        {

            if (currentTestID != null)
            {
                var currentTest = await _dbContext.TestSite.FindAsync(currentTestID);
                if (currentTest != null)
                {
                    if (!string.IsNullOrEmpty(currentTest.WebsiteGuid))
                    {
                        return await _dbContext.TestSite.OrderByDescending(x => x.Created).FirstOrDefaultAsync(x => x.WebsiteId < currentTestID && x.WebsiteGuid == currentTest.WebsiteGuid);
                    }
                    else
                    {
                        return await _dbContext.TestSite.OrderByDescending(x => x.Created).FirstOrDefaultAsync(x => x.WebsiteId < currentTestID && x.SitemapUrl == currentTest.SitemapUrl);
                    }
                }

            }
            return await _dbContext.TestSite.OrderByDescending(x => x.Created).FirstOrDefaultAsync();

        }
        public async Task<TestResultDisplayModel?> BuildDisplayModel(WebSite? currentTest, WebSite? lastTest)
        {
            if (currentTest == null)
            {
                return null;
            }
            var currentTestResult = await this.GetResultItems(currentTest.WebsiteId);
            if (lastTest == null)
            {
                return BuildDisplayModel(currentTest.SitemapUrl, currentTestResult, null);
            }
            var lastTestResult = await this.GetResultItems(lastTest.WebsiteId);

            return BuildDisplayModel(currentTest.SitemapUrl, currentTestResult, lastTestResult);
        }

        public TestResultDisplayModel? BuildDisplayModel(string siteMapUrl, List<TestUrl>? currentTestResult, List<TestUrl>? lastTestResult)
        {
            List<DisplayModel>? displayModelItem = new List<DisplayModel>();
            var defaultColor = "green";
            if (currentTestResult == null)
            {
                return null;
            }
            currentTestResult = currentTestResult.Where(x => x is not null).OrderBy(x => x.HttpStatusCode.HasValue).ThenByDescending(x => x.ContentLength).ToList();
            if (lastTestResult != null)
            {
                lastTestResult = lastTestResult.Where(x => x is not null).OrderBy(x => x.HttpStatusCode.HasValue).ThenByDescending(x => x.ContentLength).ToList();

                if (currentTestResult?.Any() ?? false)
                {
                    foreach (var lastTestItem in lastTestResult)
                    {

                        displayModelItem.Add(new DisplayModel { Url = lastTestItem.Url, StatusCode = null, LastStatusCode = lastTestItem.StatusCode, ContentLength = null, LastContentLength = lastTestItem.ContentLength, DOMCount = null, LastDOMCount = lastTestItem.DomCount, Color = defaultColor, OrderIndex = 0 });

                    }

                }
            }
            foreach (var currentTestItem in currentTestResult)
            {
                int? orderIndex = 0;
                var color = defaultColor;
                var displayItem = displayModelItem.FirstOrDefault(u => u.Url.Equals(currentTestItem.Url));
                if (displayItem != null)
                {
                    displayItem.StatusCode = currentTestItem.StatusCode;
                    displayItem.LastStatusCode = currentTestItem.LastTest?.StatusCode;
                    displayItem.ContentLength = currentTestItem.ContentLength;
                    displayItem.LastContentLength = currentTestItem.LastTest?.ContentLength;
                    displayItem.DOMCount = currentTestItem.DomCount;
                    displayItem.LastDOMCount = currentTestItem.LastTest?.DomCount;
                    displayItem.ResponseTime = currentTestItem.ResponseTime;
                    displayItem.LastResponseTime = currentTestItem.LastTest?.ResponseTime;


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



                }
                else
                {
                    displayModelItem.Add(new DisplayModel { Url = currentTestItem.Url, StatusCode = currentTestItem.StatusCode, LastStatusCode = null,ResponseTime = currentTestItem.ResponseTime,LastResponseTime = null, ContentLength = currentTestItem.ContentLength, LastContentLength = null, DOMCount = currentTestItem.DomCount, LastDOMCount = null, Color = color, OrderIndex = 0 });
                }
            }

            return new TestResultDisplayModel
            {
                TestResultItem = displayModelItem.OrderByDescending(x => x.OrderIndex).ToList(),
                SiteMapUrl = siteMapUrl
            };
        }

        private async Task<List<string>?> ReadSiteMapAsync(string xmlContent, bool withImage = false)
        {
            var xdoc = XDocument.Parse(xmlContent);
            if (xdoc.Root == null)
            {
                throw new Exception(($"Invalid XML content"));
            }
            var ns = xdoc.Root.Name.Namespace;
            List<string> Urls = new List<string>();
            foreach (var urlElement in xdoc.Descendants(ns + "sitemap"))
            {
                var locElement = urlElement.Element(ns + "loc");
                if (locElement != null)
                {
                    var suburls = await GetUrlsAsync(locElement.Value);
                    if (suburls != null)
                    {
                        Urls.AddRange(suburls);
                    }
                }
            }
            foreach (var urlElement in xdoc.Descendants(ns + "url"))
            {
                var locElement = urlElement.Element(ns + "loc");
                if (locElement != null)
                {
                    Urls.Add(locElement.Value);
                    if (withImage)
                    {

                        var imageNs = xdoc.Root.GetNamespaceOfPrefix("image");
                        var imageElements = urlElement.Elements(imageNs + "image");
                        if (imageElements?.Any() ?? false)
                        {
                            foreach (var imageElement in imageElements)
                            {
                                var imageLocElement = imageElement.Element(imageNs + "loc");
                                if (imageLocElement != null)
                                {
                                    Urls.Add(imageLocElement.Value);
                                }
                            }
                        }
                    }
                }
            }
            return Urls;

        }

        private async Task<TestUrl?> GetLastItem(int id, string url)
        {
            return await _dbContext.TestResult.FirstOrDefaultAsync(x => x.WebsiteId == id && x.Url == url);
        }


        private async Task SaveURL(TestUrl testUrl)
        {
            var created = await _dbContext.AddAsync<TestUrl>(testUrl);
            await _dbContext.SaveChangesAsync();

        }
        private async Task<WebSite> CreateTestSite(string URL, string? WebsiteGuid = null, int Totalurls = 0)
        {
            var created = await _dbContext.AddAsync<WebSite>(new WebSite { SitemapUrl = URL, WebsiteGuid = WebsiteGuid, Created = DateTime.UtcNow, TotalUrls = Totalurls });
            await _dbContext.SaveChangesAsync();
            return created.Entity;
        }
        public async Task<WebSite> UpdateSiteTestedUrls(int siteId, int testedUrls)
        {
            var website = await _dbContext.FindAsync<WebSite>(siteId);
            website.TestedUrls = testedUrls;
            _dbContext.Entry(website).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return website;
        }
        public async Task DeleteTest(int siteId)
        {
            var webSite = await _dbContext.FindAsync<WebSite>(siteId);
            if (webSite == null)
            {
                Console.WriteLine("WebSite not found.");
                return;
            }
            var testItem = _dbContext.TestResult.Where(x => x.WebsiteId == siteId);
            _dbContext.TestSite.Remove(webSite);
            _dbContext.TestResult.RemoveRange(testItem);
            await _dbContext.SaveChangesAsync();
        }
        private async Task<WebSite?> UpdateIsTesting(int siteId, bool isTesting)
        {
            var website = await _dbContext.FindAsync<WebSite>(siteId);
            if (website == null)
            {
                return null;
            }
            website.IsTesting = isTesting;
            _dbContext.Entry(website).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return website;
        }
        public async Task<WebSite> IncreaseTestedUrlNumber(int siteId)
        {
            var website = await _dbContext.FindAsync<WebSite>(siteId);
            website.TestedUrls = website.TestedUrls+1;
            _dbContext.Entry(website).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return website;
        }
    }
}

