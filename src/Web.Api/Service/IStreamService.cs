namespace Web.Api.Service
{
	public interface IStreamService
	{
		Task<string> CreateStream(string streamName, string? bannerColor);
	}
}
