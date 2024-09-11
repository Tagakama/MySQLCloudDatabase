using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Converters;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using Object = System.Object;

namespace Data
{
	public partial class Database
	{

		//MySQL Cloud
		
		public async UniTask<PlayerModel> CharacterLoadAsync(string characterName, string worldName)
		{
			characters character = await asyncConnection.FindAsync<characters>(characterName);

			if (character != null)
			{
				PlayerModel playerModel = new PlayerModel(character)
				{
					Inventories = await asyncConnection.QueryAsync<character_inventory>("SELECT * FROM character_inventory WHERE character=?", characterName)
				};

				if (character.sleepingBagID.IsNullOrEmpty() == false)
				{
					List<posedobjects> rowPosedRows =
						await asyncConnection.QueryAsync<posedobjects>("SELECT * FROM posedobjects WHERE uniqueId=? AND worldname=?", character.sleepingBagID, worldName);

					if (rowPosedRows.Any())
					{
						posedobjects rowPosed = rowPosedRows.First();
						playerModel.respawnPosition = new Vector3(rowPosed.x, rowPosed.y, rowPosed.z);

						Vector3 rotationVector = new Vector3(rowPosed.xrotation, rowPosed.yrotation, rowPosed.zrotation);
						playerModel.respawnRotation = Quaternion.Euler(rotationVector);
					}
				}

				// Загрузка выбранного слота хотбара
				playerModel.hotbarSelection = await asyncConnection.FindAsync<character_hotbar_selection>(characterName);

				playerModel.Skills = await asyncConnection.FindAsync<character_skills>(characterName);
				playerModel.Perks = await asyncConnection.FindAsync<character_perks>(characterName);

				character_appearance characterAppearance = await asyncConnection.FindAsync<character_appearance>(characterName);

				playerModel.AddAppearance(characterAppearance);

				return playerModel;
			}

			return null;
		}

		//MySQL Cloud
		public async UniTask<PlayerModel> CharacterDataLoadAsync(string characterName)
		{
			if (await GetDataGeneric<characters>(characterName, "Characters") is { } character)
			{
				PlayerModel playerModel = new PlayerModel(character)
				{
					Inventories = await GetDataGeneric<List<character_inventory>>(characterName, "CharacterData", "Inventories"),
					hotbarSelection = await GetDataGeneric<character_hotbar_selection>(characterName, "CharacterData", "HotbarSelection"),
					Skills = await GetDataGeneric<character_skills>(characterName, "CharacterData", "Skills"),
					Perks = await GetDataGeneric<character_perks>(characterName, "CharacterData", "Perks")
				};

				character_appearance characterAppearance = await GetDataGeneric<character_appearance>(characterName, "CharacterData", "Appearances");

				playerModel.AddAppearance(characterAppearance);

				return playerModel;
			}

			Debug.Log("Character is null");
			return null;
		}

		public async UniTask<PlayerModel> LoadUserDataAsync(string characterName, string worldName)
		{
			characters character = await asyncConnection.FindAsync<characters>(characterName);

			if (character != null)
			{
				PlayerModel playerModel = new PlayerModel(character)
				{
					Inventories = await asyncConnection.QueryAsync<character_inventory>("SELECT * FROM character_inventory WHERE character=?", characterName)
				};

				// Загрузка выбранного слота хотбара
				playerModel.hotbarSelection = await asyncConnection.FindAsync<character_hotbar_selection>(characterName);

				playerModel.Skills = await asyncConnection.FindAsync<character_skills>(characterName);
				playerModel.Perks = await asyncConnection.FindAsync<character_perks>(characterName);

				character_appearance characterAppearance = await asyncConnection.FindAsync<character_appearance>(characterName);

				playerModel.AddAppearance(characterAppearance);

				return playerModel;
			}

			return null;
		}


		//MySQL Cloud
		public async UniTask<T> GetDataGeneric<T>(string username, string tableName, string columnName = "jsonObject")
		{
			await OpenConnectionAsync();

			// Динамический запрос с указанием конкретного столбца
			string query = $"SELECT {columnName} FROM {tableName} WHERE username = @username";
			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			cmd.Parameters.AddWithValue("@username", username);

			string playerData = null;

			try
			{
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						// Получаем данные из конкретного столбца
						playerData = reader[columnName]?.ToString();
					}
				}
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error executing query: " + ex.Message);
			}

			// Если данные существуют, десериализуем их, иначе возвращаем значение по умолчанию для типа T
			return !string.IsNullOrEmpty(playerData)
				? JsonConvert.DeserializeObject<T>(playerData)
				: default(T);
		}

		public async UniTask<List<string>> CheckPlayerData()
		{
			List<accounts> accountsList = await asyncConnection.QueryAsync<accounts>("SELECT * FROM accounts ORDER BY created DESC");

			if (accountsList.Any())
			{
				var latestAccount = accountsList.First();

				List<string> latestUserData = new List<string>();
				latestUserData.Add(latestAccount.name);
				latestUserData.Add(latestAccount.password);

				return latestUserData;
			}

			return null;
		}

		public async UniTask<List<string>> GetUserData(string username, string password)
		{
			await OpenConnectionAsync();
			List<string> userData = new List<string>();
			string query = "SELECT username, password FROM LoginTable WHERE username = @username AND password = @password";

			MySqlCommand cmd = new MySqlCommand(query, _sqlConnection);
			cmd.Parameters.AddWithValue("@username", username);
			cmd.Parameters.AddWithValue("@password", password);

			try
			{
				MySqlDataReader dataReader = cmd.ExecuteReader();
				if (await dataReader.ReadAsync())
				{
					userData.Add(dataReader["username"].ToString());
					userData.Add(dataReader["password"].ToString());
				}

				dataReader.Close();
			}
			catch (MySqlException ex)
			{
				Debug.LogError("Error: " + ex.Message);
			}


			return userData;
		}
	}
}