using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class CustomVariableSpellEffects
{
    public string key;
    public ValueTypeSpells valueType;
    public int intValue;
    public float floatValue;
    public double doubleValue;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
    public string stringValue;
    public bool boolValue;
    public StatusEffect statusEffectValue;
    public string statusEffectIdentifier;

    public object GetValue()
    {
        switch (valueType)
        {
            case ValueTypeSpells.Int:
                return intValue;
            case ValueTypeSpells.Float:
                return floatValue;
            case ValueTypeSpells.Double:
                return doubleValue;
            case ValueTypeSpells.String:
                return stringValue;
            case ValueTypeSpells.Vector2:
                return vector2Value;
            case ValueTypeSpells.Vector3:
                return vector3Value;
            case ValueTypeSpells.Boolean:
                return boolValue;
            case ValueTypeSpells.StatusEffect:
                return statusEffectValue;
            default:
                return null;
        }
    }
}

[System.Serializable]
public class SpellEffectData
{
    public string effectName;
    public List<CustomVariableSpellEffects> customVariables = new List<CustomVariableSpellEffects>();
}
public enum ValueTypeSpells
{
    Int,
    Float,
    Double,
    String,
    Vector2,
    Vector3,
    Boolean
    StatusEffect
}


[System.Serializable]
[CreateAssetMenu(fileName = "New Spell Effect", menuName = "Custom/SpellEffect")]
public class SpellEffects : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] public string _spellEffectName;
    public string spellEffectName => _spellEffectName;

    [SerializeField] public List<CustomVariableSpellEffects> _customVariablesList = new List<CustomVariableSpellEffects>();

    [SerializeField] private List<string> _customVariableKeys = new List<string>();
    [SerializeField] private List<object> _customVariableValues = new List<object>();

    public Dictionary<string, object> customVariables = new Dictionary<string, object>();

    public void AddCustomVariable(string key, object value)
    {
        CustomVariableSpellEffects customVariable = new CustomVariableSpellEffects();
        customVariable.key = key;

        if (value is StatusEffect statusEffectValue)
        {
            customVariable.valueType = ValueTypeSpells.StatusEffect;
            customVariable.statusEffectIdentifier = GenerateStatusEffectIdentifier(statusEffectValue);
            Debug.Log("we are adding the status effect");
        }
        if (value is int intValue)
        {
            customVariable.valueType = ValueTypeSpells.Int;
            customVariable.intValue = intValue;
        }
        else if (value is float floatValue)
        {
            customVariable.valueType = ValueTypeSpells.Float;
            customVariable.floatValue = floatValue;
        }
        else if (value is bool boolValue)
        {
            customVariable.valueType = ValueTypeSpells.Boolean;
            customVariable.boolValue = boolValue;
        }
        else if (value is Vector3 vector3Value)
        {
            customVariable.valueType = ValueTypeSpells.Vector3;
            customVariable.vector3Value = vector3Value;
        }
        else if (value is string stringValue)
        {
            customVariable.valueType = ValueTypeSpells.String;
            customVariable.stringValue = stringValue;
        }
        else if (value is Vector2 vector2Value)
        {
            customVariable.valueType = ValueTypeSpells.Vector2;
            customVariable.vector2Value = vector2Value;
        }
        else if (value is double doubleValue)
        {
            customVariable.valueType = ValueTypeSpells.Double;
            customVariable.doubleValue = doubleValue;
        }

        _customVariablesList.Add(customVariable);
    }

    public void RemoveCustomVariable(string key)
    {
        customVariables.Remove(key);
    }

    public void SaveCustomVariables()
    {
        string spellEffectName = _spellEffectName;
        SpellEffectData data = new SpellEffectData();
        data.effectName = spellEffectName;
        data.customVariables = _customVariablesList;

        string json = JsonUtility.ToJson(data);
        string filePath = GetDataFilePath(_spellEffectName);
        System.IO.File.WriteAllText(filePath, json);
        //Debug.Log("saved to file path: " + filePath);
    }

    public void LoadCustomVariables()
    {
        string spellEffectName = _spellEffectName;
        string filePath = GetDataFilePath(spellEffectName);

        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            SpellEffectData data = JsonUtility.FromJson<SpellEffectData>(json);
            _customVariablesList = data.customVariables;
            
          
            RebuildCustomVariablesDictionary();
            //Debug.Log("loaded from: " + filePath);
        }
    }
    public void OnBeforeSerialize()
    {
        // Before serialization, clear the temporary lists
        _customVariableKeys.Clear();
        _customVariableValues.Clear();

        // Populate the temporary lists with the current dictionary content
        foreach (var kvp in customVariables)
        {
            _customVariableKeys.Add(kvp.Key);
            _customVariableValues.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        // After deserialization, recreate the dictionary from the temporary lists
        RebuildCustomVariablesDictionary();
    }
    

    private void RebuildCustomVariablesDictionary()
    {
        customVariables.Clear();

        int count = Mathf.Min(_customVariableKeys.Count, _customVariableValues.Count);
        for (int i = 0; i < count; i++)
        {
            customVariables.Add(_customVariableKeys[i], _customVariableValues[i]);
        }
    }

    private string GetDataFilePath(string spellEffectName)
    {
        string relativePath = "JsonData/SpellEffectData/";
        string dataFolderPath = Path.Combine(Application.dataPath, relativePath);

        return Path.Combine(dataFolderPath, spellEffectName + ".json");;
    }
}
