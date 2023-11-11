using Web.Api.Model.Request;

namespace Web.Api.Service
{
	public interface IStreamService
	{
		Task<string> CreateStream(RequestModel postData);
		Task AddViewerOnStream(string streamName);
		Task SendAnalytics();
	}
}
