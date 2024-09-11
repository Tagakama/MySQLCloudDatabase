using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Newtonsoft.Json;
using Object = System.Object;

namespace Data
{
	public partial class Database
	{
		
		public async UniTask<CharacterSetup> LoadCharacterData(CharacterSetup character)
		{
			using (MySqlTransaction transaction = _sqlConnection.BeginTransaction())
			{
				try
				{
					var loadData = await GetDataGeneric<CharacterSetup>(character.Name,"PlayerData",transaction);
					transaction.Commit();
					Debug.Log("Load parameters complete");
					return loadData;
				}
				catch (MySqlException ex)
				{
					Debug.LogError("Error saving character data: " + ex.Message);
					transaction.Rollback();
				}
			}

			return null;
		}
		
		public async UniTask<T> GetDataGeneric<T>(string username, string tableName, MySqlTransaction transaction, string columnName = "jsonObject")
		{
			await OpenConnectionAsync();
    
			string query = $"SELECT {columnName} FROM {tableName} WHERE username = @username";
			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection, transaction);
			cmd.Parameters.AddWithValue("@username", username);

			string playerData = null;

			try
			{
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						playerData = reader[columnName]?.ToString();
					}
				}
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error executing query: " + ex.Message);
			}

			if (!string.IsNullOrEmpty(playerData))
			{
				try
				{
					var result = JsonConvert.DeserializeObject<T>(playerData);
					return result;
				}
				catch (JsonException jsonEx)
				{
					Debug.LogError("Error deserializing JSON: " + jsonEx.Message);
				}
			}

			return default(T);
		}
		
	}
}