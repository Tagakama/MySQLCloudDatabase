using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Data
{
	public partial class Database
	{
		
		public async UniTask SaveCharacterData(CharacterSetup modelToSave)
		{
			Debug.Log("Opened connections");
			await OpenConnectionAsync();
			Debug.Log("Connection open");

			using (MySqlTransaction transaction = _sqlConnection.BeginTransaction())
			{
				try
				{
						var saveData = new
						{
							Name = modelToSave.Name,
							Lvl = modelToSave.Lvl,
							Invetory = modelToSave.Invetory,
						};

						string jsonObject = JsonConvert.SerializeObject(saveData);
						await InsertOrUpdateAllAsync(jsonObject, modelToSave.Name, transaction);

					transaction.Commit();
					Debug.Log("Save parameters complete");
				}
				catch (Exception ex)
				{
					Debug.LogError("Error saving character data: " + ex.Message);
					transaction.Rollback();
				}
			}
		}
		
		public async UniTask InsertOrUpdateAllAsync(string jsonObject, string Username, MySqlTransaction transaction)
		{
					string query = $@"
		        INSERT INTO PlayerData (Username, jsonObject)
		        VALUES (@username, @jsonObject)
		        ON DUPLICATE KEY UPDATE 
		            jsonObject = VALUES(jsonObject);";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection, transaction);
			cmd.Parameters.AddWithValue("@username", Username);
			cmd.Parameters.AddWithValue("@jsonObject", jsonObject);

			try
			{
				int rowsAffected = await cmd.ExecuteNonQueryAsync();
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error executing query: " + ex.Message);
			}
		}
		
	}
}