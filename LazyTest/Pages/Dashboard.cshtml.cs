using LazyTest.Models;
using LazyTest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LazyTest.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ITestingServices _testingServices;

        public DashboardModel(ITestingServices testingServices)
        {
            _testingServices = testingServices;
        }
        public bool NeedPageRefresh { get; set; }
        public List<WebSite> TestSites { get; set; }

        public async Task OnGetAsync()
        {
            TestSites =  await _testingServices.GetTestSitesAsync();
            foreach (var site in TestSites)
            {

                if (site.TestedUrls == 0 || site.IsTesting)
                {
                    if (site.IsTestRunning)
                    {
                        NeedPageRefresh = true;
                    }

                }
            }
            if (TestSites.Any())
                TestSites.Reverse();
           
        }
        public IActionResult OnGetDeleteSite(int id)
        {
           
            _testingServices.DeleteTest(id);
            return RedirectToPage();
        }
    }
}
