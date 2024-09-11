using System;
using TMPro;
using UnityEngine;

namespace Data
{
    [Serializable]    
    public class Global : MonoBehaviour
    {
        public TextMeshProUGUI _debugLog;

        private Database _database;
        private GenerateData _generateData;
        private SourceDataStorage _sourceDataStorage;
        private CharacterSetup _characterSetup;
        private Character _randomCharacter;
        
        public void Start()
        {
            InitilizeFields();
        }

        public void SaveCharacterData()
        {
            _database.SaveCharacterData(_characterSetup);
        }

        public async void LoadCharacterData()
        {
          _characterSetup = await _database.LoadCharacterData(_characterSetup);
          SendLog(_characterSetup);
        }

        public void InitializeCharacter()
        {
            _randomCharacter = _generateData.GetRandomCharacter();
            _characterSetup.Name = _randomCharacter.RandomName;
            _characterSetup.Inventory = _randomCharacter.RandomInventory;
            _characterSetup.Lvl = _randomCharacter.RandomLvl;
            SendLog(_characterSetup);
        }

        private void InitilizeFields()
        {
            _database = GetComponent<Database>();
            _generateData = GetComponent<GenerateData>();
            _characterSetup = new CharacterSetup();
            _randomCharacter = new Character();
        }

        private void SendLog(CharacterSetup character)
        {

            _debugLog.text = $"Name: {character.Name} \n" +
                             $"Inventory: {string.Join(", ",character.Inventory)} \n" +
                             $"Lvl: {character.Lvl}";
        }

    }
}
