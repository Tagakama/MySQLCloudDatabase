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
			connectionString = "Server=0.0.0.0;Database=test;User ID=test;Password=password;Pooling=true;";
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