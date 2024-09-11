using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "SourceDataStorage", menuName = "Scriptable Objects/SourceDataStorage")]
public sealed class SourceDataStorage : ScriptableObject
{
    public CharacterSetup[] CharacterSetups
    {
        get { return _character; }
    }
    
    [SerializeField] private CharacterSetup[] _character;

    public void SetData(CharacterSetup characterSetup)
    {
        _character[0].Invetory = characterSetup.Invetory;
        _character[0].Name = characterSetup.Name;
        _character[0].Lvl = characterSetup.Lvl;
    }
    
}

[Serializable]
public sealed class CharacterSetup
{
    
    public string Name
    {
        get { return _name;}
        set { _name = value; }
    }

    public int Lvl
    {
        get { return _lvl; }
        set { _lvl = value; }
    }

    public int[] Invetory
    {
        get { return _inventory; }
        set { _inventory = value; }
    }
    
    [SerializeField] private string _name;
    [SerializeField] private int _lvl;
    [SerializeField] private int[] _inventory;
}
