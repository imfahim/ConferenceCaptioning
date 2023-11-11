using Npgsql;

namespace Web.Api.Model
{
	public class StreamCreationTracker
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime CreatedOn { get; set; }

		public StreamCreationTracker (NpgsqlDataReader reader)
		{
			Id = (int)reader["id"];
			Name = reader["streamname"].ToString();
			CreatedOn = Convert.ToDateTime(reader["createdon"].ToString());
		}
	}

}
