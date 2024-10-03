using LazyTest.Models;
using System.Threading.Channels;
namespace LazyTest.Services
{
  

    public class TestChannelService
    {
        private readonly Channel<TestSiteInBackGroundModel> _channel;
        public TestChannelService()
        {
            _channel = Channel.CreateUnbounded<TestSiteInBackGroundModel>(); 
        }

        public ChannelWriter<TestSiteInBackGroundModel> Writer => _channel.Writer;
        public ChannelReader<TestSiteInBackGroundModel> Reader => _channel.Reader;
    }
     
}
