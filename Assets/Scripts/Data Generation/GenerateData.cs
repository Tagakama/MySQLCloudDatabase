using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GenerateData : MonoBehaviour
{
    
    [SerializeField] string[] _nameList = new string[]{"Jorje","Abram","Carno","Dolch","Munhem"};
    [SerializeField] private int _maxInventoryLen = 10;

    
    private int _minInventoryLen = 1;
   
    
    public Character GetRandomCharacter()
    {
        string name = _nameList[Random.Range(0, _nameList.Length)];
        int lvl = Random.Range(1, 100);
        int[] inventory = GetRandomInventory();
        
        Character randomCharacter = new Character()
        {
            RandomName = name,
            RandomLvl = lvl,
            RandomInventory = inventory
        };
        
        return randomCharacter;
    }

    private int[] GetRandomInventory()
    {
        int[] _randomInventory = new int[_maxInventoryLen];
        
        Debug.Log(_randomInventory.Length);
        int inventoryLen = Random.Range(_minInventoryLen, _maxInventoryLen);
        for (int i = 0; i < inventoryLen; i++)
        {
            _randomInventory[i] = Random.Range(1, 99);
        }
            return _randomInventory;
    }
}

public struct Character
{
    public string RandomName;
    public int RandomLvl;
    public int[] RandomInventory;
}
