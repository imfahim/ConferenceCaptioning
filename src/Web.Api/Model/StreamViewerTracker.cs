using Npgsql;

namespace Web.Api.Model
{
	public class StreamViewerTracker
	{
		public int Id { get; set; }
		public int ViewCount { get; set; }
		public string Name { get; set; }
		public DateTime ViewDate { get; set; }

		public StreamViewerTracker(NpgsqlDataReader reader)
		{
			Id = (int)reader["id"];
			ViewCount = (int)reader["viewcount"];
			Name = reader["streamname"].ToString();
			ViewDate = Convert.ToDateTime(reader["viewdate"].ToString());
		}
	}
}
