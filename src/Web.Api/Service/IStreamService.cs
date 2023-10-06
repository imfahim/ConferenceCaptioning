using Web.Api.Model.Request;

namespace Web.Api.Service
{
	public interface IStreamService
	{
		Task<string> CreateStream(RequestModel postData);
	}
}
