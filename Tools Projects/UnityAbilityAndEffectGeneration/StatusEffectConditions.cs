using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum StatusEffectMisc
{
    Active,
    Passive
};

[System.Serializable]
public class CustomVariableStatusEffects
{
    public string key;
    public ValueTypeStatusEffect valueType;
    public int intValue;
    public float floatValue;
    public double doubleValue;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
    public string stringValue;
    public bool boolValue;
    public StatusEffectMisc activeOrPassiveValue;
    public object GetValue()
    {
        switch (valueType)
        {
            case ValueTypeStatusEffect.Int:
                return intValue;
            case ValueTypeStatusEffect.Float:
                return floatValue;
            case ValueTypeStatusEffect.Double:
                return doubleValue;
            case ValueTypeStatusEffect.String:
                return stringValue;
            case ValueTypeStatusEffect.Vector2:
                return vector2Value;
            case ValueTypeStatusEffect.Vector3:
                return vector3Value;
            case ValueTypeStatusEffect.Boolean:
                return boolValue;
            case ValueTypeStatusEffect.passiveOrActive:
                return activeOrPassiveValue;
            default:
                return null;
        }
    }
}

public enum ValueTypeStatusEffect
{
    Int,
    Float,
    Double,
    String,
    Vector2,
    Vector3,
    Boolean,
    passiveOrActive

}
[System.Serializable]
public class StatusEffectData
{
    public string effectName;
    public List<CustomVariableStatusEffects> customVariables = new List<CustomVariableStatusEffects>();
}

[System.Serializable]
public class StatusEffectConditions : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] public string _statusEffectName;
    public string statusEffectName => _statusEffectName;
    StatusEffectMisc activeOrPassiveValue = new StatusEffectMisc();
    [SerializeField] public List<CustomVariableStatusEffects> _customVariablesList = new List<CustomVariableStatusEffects>();

    [SerializeField] private List<string> _customVariableKeys = new List<string>();
    [SerializeField] private List<object> _customVariableValues = new List<object>();

    public Dictionary<string, object> customVariables = new Dictionary<string, object>();

    public void AddCustomVariable(string key, object value)
    {
        CustomVariableStatusEffects customVariable = new CustomVariableStatusEffects();
        customVariable.key = key;

        if (value is int intValue)
        {
            customVariable.valueType = ValueTypeStatusEffect.Int;
            customVariable.intValue = intValue;
        }
        else if (value is float floatValue)
        {
            customVariable.valueType = ValueTypeStatusEffect.Float;
            customVariable.floatValue = floatValue;
        }
        else if (value is bool boolValue)
        {
            customVariable.valueType = ValueTypeStatusEffect.Boolean;
            customVariable.boolValue = boolValue;
        }
        else if (value is StatusEffect statusEffectValue)
        {
            customVariable.valueType = ValueTypeStatusEffect.passiveOrActive;
            customVariable.activeOrPassiveValue = activeOrPassiveValue;
        }
        else if (value is Vector3 vector3Value)
        {
            customVariable.valueType = ValueTypeStatusEffect.Vector3;
            customVariable.vector3Value = vector3Value;
        }
        else if (value is string stringValue)
        {
            customVariable.valueType = ValueTypeStatusEffect.String;
            customVariable.stringValue = stringValue;
        }
        else if (value is Vector2 vector2Value)
        {
            customVariable.valueType = ValueTypeStatusEffect.Vector2;
            customVariable.vector2Value = vector2Value;
        }
        else if (value is double doubleValue)
        {
            customVariable.valueType = ValueTypeStatusEffect.Double;
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
        string statusEffectName = _statusEffectName;
        StatusEffectData data = new StatusEffectData();
        data.effectName = statusEffectName;
        data.customVariables = _customVariablesList;

        string json = JsonUtility.ToJson(data);
        string filePath = GetDataFilePath(statusEffectName);
        System.IO.File.WriteAllText(filePath, json);
        //Debug.Log("saved to file path: " + filePath);
    }

    public void LoadCustomVariables()
    {
        string statusEffectName = _statusEffectName;
        string filePath = GetDataFilePath(statusEffectName);

        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            StatusEffectData data = JsonUtility.FromJson<StatusEffectData>(json);

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

    private string GetDataFilePath(string statusEffectName)
    {
        string relativePath = ""JsonData/StatusEffectData/";
        string dataFolderPath = Path.Combine(Application.dataPath, relativePath);
        return Path.Combine(dataFolderPath, statusEffectName + ".json");
    }
}
