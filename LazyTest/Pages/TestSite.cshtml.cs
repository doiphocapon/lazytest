using LazyTest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LazyTest.Models;
using System.Text.Json;
using LazyTest.Constants;
using System.Net;
namespace LazyTest.Pages
{
    public class TestSiteModel : PageModel
    {
        private readonly ITestingServices _testingServices;

        public TestSiteModel(ITestingServices testingServices)
        {
            _testingServices = testingServices;
        }
        public string ChartDataJson { get; set; }
        public WebSite TestSite { get; set; }
        public TestResultDisplayModel? TestResultDisplayData { get; set; }
        public TestResultDisplayModel? HighlightData { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            TestSite = await _testingServices.GetTestSiteAsync(id);
            if (TestSite==null)
            {
                return NotFound();
            }
            var lastTestSite = await _testingServices.GetLastTest(id);
            var result= await _testingServices.GetResultItems(id);
            var lastTest = await _testingServices.GetLastTest(id);
            TestResultDisplayData= await _testingServices.BuildDisplayModel(TestSite, lastTestSite);
            HighlightData= GetHighlightData();

            if (result == null)
            {
                return NotFound();
            }
           var pieChartData = result.OrderBy(x=>x.HttpStatusCode).GroupBy(x => x.StatusCode).Select(x => new { StatusCode = !string.IsNullOrEmpty(x.Key)? x.Key : "Unknown", Count = x.Count() });
          
           var pieChardLabels = pieChartData.Select(x => x.StatusCode).ToArray();
           var pieChardValues = pieChartData.Select(x => x.Count).ToArray();
           var pieChardColors = result.OrderBy(x => x.HttpStatusCode).GroupBy(x => x.HttpStatusCode).Select(x => ColorByResponseCodeHelper.GetColorNameByCode(x.Key).ToLower()).ToArray();
        var unScanUrls=TestSite.TotalUrls-TestSite.TestedUrls;
        if (unScanUrls > 0)
        {
            pieChardLabels = pieChardLabels.Append("Un scanned Urls").ToArray();
            pieChardValues = pieChardValues.Append(unScanUrls).ToArray();
            pieChardColors = pieChardColors.Append("grey").ToArray();
        }

        var chartData = new
            {
                labels = pieChardLabels,
                datasets = new[]
              {
                    new
                    {   backgroundColor =pieChardColors,
                        data = pieChardValues
                    }
                }
            };
            ChartDataJson = JsonSerializer.Serialize(chartData);

           

            return Page();
        }

        private TestResultDisplayModel? GetHighlightData()
        {
            if (TestResultDisplayData == null)
            {
                return null;
            }
            var highlightData = new TestResultDisplayModel
            {
                SiteMapUrl = TestResultDisplayData.SiteMapUrl,
                TestResultItem = TestResultDisplayData.TestResultItem.Where(x => (x.StatusCode != HttpStatusCode.OK.ToString() && x.StatusCode is not null) || x.ContentLength ==0 || Over10PercentDifference(x.ContentLength,x.LastContentLength)==true ).ToList()
            };
            return highlightData;
        }
        public static bool Over10PercentDifference(long? num1, long? num2)
    {
        if (num1==null || num2 == null ||  num2 == 0 || num1==0 )
        {
            return false;
        }
        // Calculate the absolute difference
        long difference = Math.Abs((long)num1 - (long)num2);

        // Calculate 10% of the larger number
        long largerNumber = Math.Max((long)num1, (long)num2);
        double tenPercent = largerNumber * 0.10;

        // Compare the difference to 10% of the larger number
        return difference > tenPercent;
    }



    }
}
