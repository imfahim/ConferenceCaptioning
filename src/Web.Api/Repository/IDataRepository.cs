using Web.Api.Model;

namespace Web.Api.Repository
{
	public interface IDataRepository
	{
		Task<List<StreamCreationTracker>> GetLastDayCreatedStream();
		Task<List<StreamViewerTracker>> GetLastDayViewedStream();
		Task InsertNewlyCreatedStream(string streamName);
		Task<StreamViewerTracker?> GetViewerOnStream(string streamName);
		Task AddViewerOnStream(string streamName);

	}
}
