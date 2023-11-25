using Npgsql;
using Web.Api.Model;
using Web.Api.Repository;

public class DataRepository : IDataRepository
{
	private readonly string _connectionString = "Server=localhost;Port=5432;Database=FirstProto;Username=postgres;Password=1234;";

	public DataRepository()
	{
		var masterConnection = new NpgsqlConnection(_connectionString);
		masterConnection.Open();

		var createCommand = masterConnection.CreateCommand();
		createCommand.CommandText =
		@"
			CREATE TABLE if not exists StreamViewerTracker(
			Id SERIAL PRIMARY KEY, 
			StreamName VARCHAR (200) NOT NULL, 
			ViewCount int NOT NULL,
			ViewDate TIMESTAMPTZ NOT NULL
			);
			CREATE TABLE if not exists StreamCreationTracker(
			Id SERIAL PRIMARY KEY, 
			StreamName VARCHAR (200) NOT NULL, 
			CreatedOn TIMESTAMPTZ NOT NULL
			)
            ";

		createCommand.ExecuteNonQuery();
		masterConnection.Close();

	}

	public async Task<List<StreamCreationTracker>> GetLastDayCreatedStream()
	{

		using (var secondConnection = new NpgsqlConnection(_connectionString))
		{
			secondConnection.Open();
			var dateString = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
			var commandText =
			@"SELECT * FROM StreamCreationTracker WHERE DATE(CreatedOn) = '" + dateString + "'";
			var streamCreationTrackers = new List<StreamCreationTracker>();

			await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, secondConnection))
			{
				cmd.Parameters.AddWithValue("date", DateTime.Now.AddDays(-1).Date);

				await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						StreamCreationTracker tracker = new StreamCreationTracker(reader);
						streamCreationTrackers.Add(tracker);
					}
				}
			}
			secondConnection.Close();
			return streamCreationTrackers;
		}
	}

	public async Task<List<StreamViewerTracker>> GetLastDayViewedStream()
	{

		using (var secondConnection = new NpgsqlConnection(_connectionString))
		{
			secondConnection.Open();
			var dateString = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
			var commandText =
			@"SELECT * FROM StreamViewerTracker WHERE DATE(viewdate) = '" + dateString + "'";
			var streamViewerTracker = new List<StreamViewerTracker>();

			await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, secondConnection))
			{
				cmd.Parameters.AddWithValue("date", DateTime.Now.AddDays(-1).Date);

				await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						StreamViewerTracker tracker = new StreamViewerTracker(reader);
						streamViewerTracker.Add(tracker);
					}
				}
			}
			secondConnection.Close();
			return streamViewerTracker;
		}
	}

	public async Task InsertNewlyCreatedStream(string streamName)
	{
		using (var secondConnection = new NpgsqlConnection(_connectionString))
		{
			secondConnection.Open();
			string commandText = $"INSERT INTO StreamCreationTracker (StreamName, CreatedOn) VALUES (@name, @date)";
			await using var cmd = new NpgsqlCommand(commandText, secondConnection);
			cmd.Parameters.AddWithValue("name", streamName);
			cmd.Parameters.AddWithValue("date", DateTime.Now.Date);

			await cmd.ExecuteNonQueryAsync();
			secondConnection.Close();
		}		
	}

	public async Task<StreamViewerTracker?> GetViewerOnStream(string streamName)
	{
		using (var secondConnection = new NpgsqlConnection(_connectionString))
		{
			secondConnection.Open();
			var dateString = DateTime.Now.ToString("yyyy-MM-dd");

			string commandText = $"SELECT * FROM StreamViewerTracker WHERE streamname = @name and DATE(viewdate) = '"+ dateString + "' limit 1";

			await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, secondConnection))
			{
				cmd.Parameters.AddWithValue("name", streamName);
				await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						StreamViewerTracker viewTracker = new StreamViewerTracker(reader);
						secondConnection.Close();
						return viewTracker;
					}
				}
					
			}

			secondConnection.Close();
			return null;
		}
	}

	public async Task AddViewerOnStream(string streamName)
	{
		var existingCount = await GetViewerOnStream(streamName);
		if(existingCount == null)
		{
			using (var secondConnection = new NpgsqlConnection(_connectionString))
			{
				secondConnection.Open();
				string commandText = $"INSERT INTO StreamViewerTracker (streamname, viewcount, viewdate) VALUES (@name, @count, @date)";
				await using var cmd = new NpgsqlCommand(commandText, secondConnection);
				cmd.Parameters.AddWithValue("name", streamName);
				cmd.Parameters.AddWithValue("count", 1);
				cmd.Parameters.AddWithValue("date", DateTime.Now);
				await cmd.ExecuteNonQueryAsync();
				secondConnection.Close();
			}
		}
		else
		{
			using (var secondConnection = new NpgsqlConnection(_connectionString))
			{
				secondConnection.Open();
				var commandText = $@"UPDATE StreamViewerTracker SET viewcount = @count WHERE id = @id";

				await using (var cmd = new NpgsqlCommand(commandText, secondConnection))
				{
					cmd.Parameters.AddWithValue("id", existingCount.Id);
					cmd.Parameters.AddWithValue("count", existingCount.ViewCount + 1);
					await cmd.ExecuteNonQueryAsync();
				}
				secondConnection.Close();
			}

		}
	}
}
