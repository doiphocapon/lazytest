using LazyTest.Models;

namespace LazyTest.Services
{
    public interface ITestingServices
    {
        Task<TestResultDisplayModel?> BuildDisplayModel(WebSite? currentTest, WebSite? lastTest);
        TestResultDisplayModel? BuildDisplayModel(string siteMapUrl, List<TestUrl>? currentTestResult, List<TestUrl>? lastTestResult);
        Task<int?> DOMCaculateAsync(string url);
        Task<WebSite?> GetLastTest(int? currentTestID = null);
        Task<List<TestUrl>?> GetResultItems(int id);
        Task<string?> GetSitemapUrlFromRobotsTxt(string domain);
        Task<WebSite?> GetTestSiteAsync(int id);

        Task<List<WebSite>> GetTestSitesAsync();
        Task<List<string>?> GetUrlsAsync(string url, bool withImage = false);
        Task<SitemapUrl> GetWebsiteUrls(string sitemapUrl, string? websiteGuid = null, bool withImage = false);
        Task<TestUrl?> TestAndSaveThisURL(string url, int websiteId, bool withdomcount = false);
       
        Task<WebSite> UpdateSiteTestedUrls(int siteId, int testedUrls);
        Task TestAndSaveSiteStartWithThisURL(TestSiteInBackGroundModel model);
        Task DeleteTest(int siteId);
    }
}