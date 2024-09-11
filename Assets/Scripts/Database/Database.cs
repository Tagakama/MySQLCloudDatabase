using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SQLite;
using UnityEngine;
using PUSHKA.MySQL;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Data
{
	public partial class Database : MonoBehaviour
	{
		public static Database singleton;

		public string databaseFile = "Database.sqlite";

		public SQLiteAsyncConnection asyncConnection;

		private SqlDataBase _sqlDataBase;

		private MySqlConnection _sqlConnection;

		private string connectionString;

		protected void Start()
		{

			SqlDataBase.logState = SqlDataBase.LogState.Enabled;
			connectionString = "Server=185.255.133.194;Database=test;User ID=user_cli;Password=passworD123!;Pooling=true;";
			OpenConnectionAsync();

			Connect();
		}

		private void OnDestroy()
		{
			asyncConnection?.CloseAsync();
			asyncConnection = null;
			CloseConnection();
			Debug.Log("DataBase Destroyed");
		}

		private void OpenConnection()
		{
			using (_sqlConnection = new MySqlConnection(connectionString))
			{
				try
				{
					_sqlConnection.Open();
					Debug.Log("Подключение к базе данных успешно!");


					/*
					// Пример выполнения запроса
					string query = "SELECT * FROM users";
					MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
					MySqlDataReader dataReader = cmd.ExecuteReader();

					while (dataReader.Read())
					{
						Debug.Log("ID: " + dataReader["id"] + ", Username: " + dataReader["username"]);
					}

					dataReader.Close();
					*/
				}
				catch (Exception ex)
				{
					Debug.LogError("Ошибка подключения к базе данных: " + ex.Message);
				}
			}
		}

		void CloseConnection()
		{
			if (_sqlConnection != null && _sqlConnection.State == System.Data.ConnectionState.Open)
			{
				_sqlConnection.Close();
				_sqlConnection = null;
				Debug.Log("Connection closed.");
			}
		}
		
		public async void Connect()
		{
#if UNITY_EDITOR
			string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, databaseFile);
#elif UNITY_ANDROID
            string path = Path.Combine(Application.persistentDataPath, databaseFile);
#elif UNITY_IOS
            string path = Path.Combine(Application.persistentDataPath, databaseFile);
#else
            string path = Path.Combine(Application.dataPath, databaseFile);
#endif

			// open connection
			// note: automatically creates database file if not created yet
			if (asyncConnection == null)
			{
				asyncConnection = await CreateAsyncConnection(path);
			}

			if (singleton == null)
			{
				singleton = this;
			}

			Debug.Log("connected to database");
		}

		async UniTask CloseConnectionAsync()
		{
			if (_sqlConnection != null && _sqlConnection.State == System.Data.ConnectionState.Open)
			{
				await _sqlConnection.CloseAsync();
				_sqlConnection = null;
				Debug.Log("Connection closed.");
			}
		}

		public async UniTask<bool> TableExists(string tableName)
		{
			await OpenConnectionAsync();
			bool exists = false;
			string query = $"SHOW TABLES LIKE '{tableName}'";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			try
			{
				var dataReader = await cmd.ExecuteReaderAsync();
				exists = dataReader.HasRows;
				await dataReader.CloseAsync();
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error: " + ex.Message);
			}


			return exists;
		}

		public void DeleteTable(string tableName)
		{
			OpenConnectionAsync();
			string query = $"DROP TABLE {tableName}";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			try
			{
				cmd.ExecuteNonQuery();
				Debug.Log("Table deleted successfully.");
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error: " + ex.Message);
			}
		}

		public async Task<bool> IsRecordExists(string username, string password)
		{
			await OpenConnectionAsync();
			bool recordExists = false;
			string query = "SELECT COUNT(*) FROM LoginTable WHERE username = @UserName AND password = @Password";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			cmd.Parameters.AddWithValue("@username", username);
			cmd.Parameters.AddWithValue("@password", password);

			try
			{
				recordExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error: " + ex.Message);
			}


			return recordExists;
		}

		public async UniTask<bool> DoesUserExistAsync(string username, string tableName)
		{
			await OpenConnectionAsync();

			string query = $"SELECT 1 FROM {tableName} WHERE username = @username LIMIT 1";
			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			cmd.Parameters.AddWithValue("@username", username);

			bool userExists = false;

			try
			{
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						userExists = true;
					}
				}
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error executing query: " + ex.Message);
			}

			return userExists;
		}

		async UniTask OpenConnectionAsync()
		{
			if (_sqlConnection == null || _sqlConnection.State == ConnectionState.Closed)
			{
				_sqlConnection = new MySqlConnection(connectionString);

				try
				{
					await _sqlConnection.OpenAsync();
					Debug.Log("Successfully connected to database.");
				}
				catch (MySqlException ex)
				{
					Debug.LogError("Error opening connection: " + ex.Message);
				}
			}
		}

		public async UniTask CreateRecord(string username, string password)
		{
			await OpenConnectionAsync();
			string query = "INSERT INTO LoginTable (username, password) VALUES (@username, @password)";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			cmd.Parameters.AddWithValue("@username", username);
			cmd.Parameters.AddWithValue("@password", password);

			try
			{
				int rowsAffected = await cmd.ExecuteNonQueryAsync();
				Debug.Log("Record created successfully. Rows affected: " + rowsAffected);
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error: " + ex.Message);
			}
		}


		public async UniTask CreateLoginTable(string tableName)
		{
			await OpenConnectionAsync();
			string query = $"CREATE TABLE {tableName} (ID INT AUTO_INCREMENT PRIMARY KEY, UserName VARCHAR(100), Password VARCHAR(100))";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			try
			{
				await cmd.ExecuteNonQueryAsync();
				Debug.Log("Table created successfully.");
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error: " + ex.Message);
			}
		}

		public async UniTask CreateDataTable(string tableName)
		{
			await OpenConnectionAsync();

			string query = $@"CREATE TABLE `{tableName}` (
			Username VARCHAR(255) NOT NULL,
				jsonObject JSON,
			PRIMARY KEY (Username)
				);";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			try
			{
				await cmd.ExecuteNonQueryAsync();
				Debug.Log("Table created successfully.");
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error: " + ex.Message);
			}
		}

		public async UniTask ClearRowContent(string username, string tableName)
		{
			await OpenConnectionAsync();
			string query = $@"
        UPDATE `{tableName}`
        SET jsonObject = '{""}'
        WHERE Username = @username;";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			cmd.Parameters.AddWithValue("@username", username);

			try
			{
				int rowsAffected = await cmd.ExecuteNonQueryAsync();
				Debug.Log($"Rows affected: {rowsAffected}");
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error executing query: " + ex.Message);
			}
		}

		// close connection when Unity closes to prevent locking
		void OnApplicationQuit()
		{
			// TODO проверка на null и обработка
			asyncConnection?.CloseAsync();
		}

		async Task<SQLiteAsyncConnection> CreateAsyncConnection(string path)
		{
			SQLiteAsyncConnection conn = new SQLiteAsyncConnection(path);

			return conn;
		}
	}
}