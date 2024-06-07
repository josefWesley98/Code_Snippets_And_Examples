using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class CustomVariable
{
    public string key;
    public ValueTypeTrait valueType;
    public int intValue;
    public float floatValue;
    public double doubleValue;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
    public string stringValue;
    public bool boolValue;
    public TraitType traitType;

    public object GetValue()
    {
        switch (valueType)
        {
            case ValueTypeTrait.Int:
                return intValue;
            case ValueTypeTrait.Float:
                return floatValue;
            case ValueTypeTrait.Double:
                return doubleValue;
            case ValueTypeTrait.String:
                return stringValue;
            case ValueTypeTrait.Vector2:
                return vector2Value;
            case ValueTypeTrait.Vector3:
                return vector3Value;
            case ValueTypeTrait.Boolean:
                return boolValue;
            case ValueTypeTrait.TraitType:
                return traitType; 
            default:
                return null;
        }
    }
}

[System.Serializable]
public class TraitEffectData
{
    public string effectName;
    public List<CustomVariable> customVariables = new List<CustomVariable>();
}

public enum ValueTypeTrait
{
    Int,
    Float,
    Double,
    String,
    Vector2,
    Vector3,
    Boolean,
    TraitType
}

// Modify the TraitEffect class
[System.Serializable]
public class TraitEffect : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] public string _effectName;
    public string effectName => _effectName;
    TraitType traitType = new TraitType();
    [SerializeField] public List<CustomVariable> _customVariablesList = new List<CustomVariable>();

    [SerializeField] private List<string> _customVariableKeys = new List<string>();
    [SerializeField] private List<object> _customVariableValues = new List<object>();

    public Dictionary<string, object> customVariables = new Dictionary<string, object>();

    public void AddCustomVariable(string key, object value)
    {
        CustomVariable customVariable = new CustomVariable();
        customVariable.key = key;

       if (value is int intValue)
        {
            customVariable.valueType = ValueTypeTrait.Int;
            customVariable.intValue = intValue;
        }
        else if (value is float floatValue)
        {
            customVariable.valueType = ValueTypeTrait.Float;
            customVariable.floatValue = floatValue;
        }
        else if (value is bool boolValue)
        {
            customVariable.valueType = ValueTypeTrait.Boolean;
            customVariable.boolValue = boolValue;
        }
        else if (value is StatusEffect statusEffectValue)
        {
            customVariable.valueType = ValueTypeTrait.TraitType;
            customVariable.traitType = traitType;
        }
        else if (value is Vector3 vector3Value)
        {
            customVariable.valueType = ValueTypeTrait.Vector3;
            customVariable.vector3Value = vector3Value;
        }
        else if (value is string stringValue)
        {
            customVariable.valueType = ValueTypeTrait.String;
            customVariable.stringValue = stringValue;
        }
        else if (value is Vector2 vector2Value)
        {
            customVariable.valueType = ValueTypeTrait.Vector2;
            customVariable.vector2Value = vector2Value;
        }
        else if (value is double doubleValue)
        {
            customVariable.valueType = ValueTypeTrait.Double;
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
        string effectName = _effectName;
        TraitEffectData data = new TraitEffectData();
        data.effectName = effectName;
        data.customVariables = _customVariablesList;

        string json = JsonUtility.ToJson(data);
        string filePath = GetDataFilePath(effectName);
        System.IO.File.WriteAllText(filePath, json);
    }

    public void LoadCustomVariables()
    {
        string effectName = _effectName;
        string filePath = GetDataFilePath(effectName);

        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            TraitEffectData data = JsonUtility.FromJson<TraitEffectData>(json);

            _customVariablesList = data.customVariables;
            RebuildCustomVariablesDictionary();
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

    private string GetDataFilePath(string effectName)
    {
        string relativePath = "JsonData/TraitEffectData/";
        string dataFolderPath = Path.Combine(Application.dataPath, relativePath);
        return Path.Combine(dataFolderPath, effectName + ".json");
    }
}
