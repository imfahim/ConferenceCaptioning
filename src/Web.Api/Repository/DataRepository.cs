using Microsoft.Data.Sqlite;
using Web.Api.Repository;

public class DataRepository : IDataRepository
{
	private readonly string _connectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

	public DataRepository(string connectionString)
	{
		//_connectionString = connectionString;
		var masterConnection = new SqliteConnection(_connectionString);
		masterConnection.Open();

		var createCommand = masterConnection.CreateCommand();
		createCommand.CommandText =
		@"
			CREATE TABLE [dbo].[StreamCreationTracker](
				[Id] [int] IDENTITY(1,1) NOT NULL,
				[StreamName] [nvarchar](100) NOT NULL,
				[CreatedOn] [datetime] NOT NULL
			) 
            ";

		createCommand.ExecuteNonQuery();
		masterConnection.Close();

	}

	//public void InsertData(string name, int age)
	//{
	//	using var connection = new SqlConnection(_connectionString);
	//	connection.Open();

	//	using (var command = new SqlCommand("INSERT INTO YourTable (Name, Age) VALUES (@Name, @Age)", connection))
	//	{
	//		command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar) { Value = name });
	//		command.Parameters.Add(new SqlParameter("@Age", SqlDbType.Int) { Value = age });

	//		command.ExecuteNonQuery();
	//	}
	//}
	public async Task GetAllCreatedStream()
	{

		using (var secondConnection = new SqliteConnection(_connectionString))
		{
			secondConnection.Open();
			var queryCommand = secondConnection.CreateCommand();
			queryCommand.CommandText =
			@"
		                  SELECT [Id]
					  ,[StreamName]
					  ,[CreatedOn]
				  FROM [dbo].[StreamCreationTracker]
		              ";
			var value = await queryCommand.ExecuteReaderAsync();
			while (value.Read())
			{
				string myreader = value.GetString(0);
			}
			Console.WriteLine(value);

			//SQLiteDataReader sqlite_datareader;
			//SQLiteCommand sqlite_cmd;
			//sqlite_cmd = conn.CreateCommand();
			//sqlite_cmd.CommandText = "SELECT * FROM SampleTable";

			//sqlite_datareader = sqlite_cmd.ExecuteReader();
			//while (sqlite_datareader.Read())
			//{
			//	string myreader = sqlite_datareader.GetString(0);
			//	Console.WriteLine(myreader);
			//}
			//conn.Close();
		}
	}

	public async Task InsertNewlyCreatedStream(string streamName)
	{
		

		using (var connection = new SqliteConnection(_connectionString))
		{
			connection.Open();

			var updateCommand = connection.CreateCommand();
			updateCommand.CommandText =
			$@"
				INSERT INTO [dbo].[StreamCreationTracker]
					   ([StreamName]
					   ,[CreatedOn])
				 VALUES
					   ({streamName}
					   ,{DateTime.UtcNow})

                ";
			await updateCommand.ExecuteNonQueryAsync();
		}
	}
}
