using LazyTest.Models;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace LazyTest.Services
{
    public class TestChannelReaderService : BackgroundService
    {
        private readonly TestChannelService _channelService;
       private readonly ITestingServices _testingServices;

        public TestChannelReaderService(TestChannelService channelService, ITestingServices testingServices)
        {
            _channelService = channelService;
           
            _testingServices = testingServices;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
	
            await foreach (var testUrl in _channelService.Reader.ReadAllAsync(stoppingToken))
			{

				await _testingServices.TestAndSaveSiteStartWithThisURL(testUrl);
			}
		}
    }
}
