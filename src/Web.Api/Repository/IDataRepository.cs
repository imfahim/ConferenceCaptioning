namespace Web.Api.Repository
{
	public interface IDataRepository
	{
		Task InsertNewlyCreatedStream(string streamName);
	}
}
