using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class StatusEffectEditor : EditorWindow
{
    private StatusEffect statusEffect;
    private SerializedObject serializedStatusEffect;
    private SerializedProperty customFieldsProperty;

    // Variables for adding new Trait Effect
    private string newEffectName = "";
    private float newEffectDamageAmount = 0f;
    private float newEffectDuration = 0f;
    private Vector2 scrollPosition;
    private bool minimizeEffect = false;
    // Variables for managing custom variables
    private StatusEffectConditions selectedEffect;
    private string newVariableName = "";
    private object newVariableValue = null;
    private string[] variableTypes = new string[] { "Int", "Float", "Double", "Boolean", "String", "Vector2", "Vector3", "Status Type" };
    private int selectedVariableTypeIndex = 0;
    private int selectedVariableTypeIndexCheck = 0;

    [MenuItem("Window/Status Effect Editor")]
    public static void ShowWindow()
    {
        GetWindow<StatusEffectEditor>("Status Effect Editor");
    }
    private void OnGUI()
    {
        statusEffect = EditorGUILayout.ObjectField("Status Effect", statusEffect, typeof(StatusEffect), false) as StatusEffect;


        GUIStyle titleLabelStyleLarge = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            fontStyle = FontStyle.BoldAndItalic
        };
        GUIStyle titleLabelStyleMedium = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            fontStyle = FontStyle.BoldAndItalic
        };
        GUIStyle titleLabelStyleSmall = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            fontStyle = FontStyle.BoldAndItalic
        };


        if (statusEffect == null)
            return;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (serializedStatusEffect == null || serializedStatusEffect.targetObject != statusEffect)
        {
            serializedStatusEffect = new SerializedObject(statusEffect);
            customFieldsProperty = serializedStatusEffect.FindProperty("customFields");
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        statusEffect.statusEffectName = (StatusEffectName)EditorGUILayout.EnumPopup("Status Effect Name: ", statusEffect.statusEffectName, titleLabelStyleMedium);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        statusEffect.type = (StatusType)EditorGUILayout.Popup("Status Effect Type", (int) statusEffect.type, Enum.GetNames(typeof(StatusType)), titleLabelStyleMedium);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        // GUI for adding new Trait effect
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Create New Status Effect Condition", titleLabelStyleSmall);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        newEffectName = EditorGUILayout.TextField("Condition Name:", newEffectName);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Add Status Effect Condition") && !string.IsNullOrEmpty(newEffectName))
        {
            if (statusEffect.customFields == null)
                statusEffect.customFields = new List<StatusEffectConditions>();

            StatusEffectConditions newStatusEffect = CreateTraitEffectAsset(newEffectName);
            statusEffect.customFields.Add(newStatusEffect);

            newEffectName = "";

            EditorUtility.SetDirty(statusEffect);
            serializedStatusEffect.ApplyModifiedProperties();
        }

        EditorGUILayout.Space(10);

        if (statusEffect.customFields != null)
        {
            foreach (var effect in statusEffect.customFields)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (selectedEffect == effect && !minimizeEffect) 
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Condition Name: " + effect.statusEffectName, titleLabelStyleMedium);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(10);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Create New Variable", titleLabelStyleSmall);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    newVariableName = EditorGUILayout.TextField("New Variable Name", newVariableName);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    selectedVariableTypeIndex = EditorGUILayout.Popup("Variable Type", selectedVariableTypeIndex, variableTypes);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);

                    if (selectedVariableTypeIndexCheck != selectedVariableTypeIndex)
                    {
                        newVariableValue = null;
                    }
                    switch(selectedVariableTypeIndex)
                    {
                        case 0:
                            if (newVariableValue == null)
                            {
                                newVariableValue = 0;
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValue = EditorGUILayout.IntField("New Variable Value", (int)newVariableValue);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            break;
                        case 1:
                            if (newVariableValue == null)
                            {
                                newVariableValue = 0f;
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValue = EditorGUILayout.FloatField("New Variable Value", (float)newVariableValue);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            break;
                        case 2:
                        if (newVariableValue == null)
                            {
                                newVariableValue = 0;
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValue = EditorGUILayout.DoubleField("New Variable Value", (double)newVariableValue);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            break;
                        case 3: 
                            if (newVariableValue == null)
                            {
                                newVariableValue = false;
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValue = EditorGUILayout.Toggle("New Variable Value", (bool)newVariableValue);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            break;
                        case 4:
                            if (newVariableValue == null)
                            {
                                newVariableValue = "";
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValue = EditorGUILayout.TextField("New Variable Value", (string)newVariableValue);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            break;
                        case 5:
                            if (newVariableValue == null)
                            {
                                newVariableValue = new Vector2(0,0);
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValue = EditorGUILayout.Vector2Field("New Variable Value", (Vector2)newVariableValue);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            break;
                        case 6:
                            if (newVariableValue == null)
                            {
                                newVariableValue = Vector3.zero;
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValue = EditorGUILayout.Vector3Field("New Variable Value", (Vector3)newVariableValue);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            break;
                        case 7:
                            if (newVariableValue == null)
                            {
                                newVariableValue = StatusEffectMisc.Passive;
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValue = EditorGUILayout.EnumPopup("New Variable Value", (StatusEffectMisc)newVariableValue);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            break;
                    }

                    if (GUILayout.Button("Add Custom Variable") && !string.IsNullOrEmpty(newVariableName))
                    {
                        if (!effect.customVariables.ContainsKey(newVariableName))
                        {
                            effect.AddCustomVariable(newVariableName, newVariableValue);
                            newVariableName = "";
                            newVariableValue = null;
                        }
                    }

                    selectedVariableTypeIndexCheck = selectedVariableTypeIndex;
                    EditorGUILayout.Space(10);


                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField("Current Custom Variables",titleLabelStyleMedium);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(10);

                    // Display existing custom variables
                    for (int j = 0; j < effect._customVariablesList.Count; j++)
                    {
                        var customVariable = effect._customVariablesList[j];
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Key:", GUILayout.Width(100));
                        customVariable.key = EditorGUILayout.TextField(customVariable.key);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Value Type:", GUILayout.Width(100));
                        customVariable.valueType = (ValueTypeStatusEffect)EditorGUILayout.EnumPopup(customVariable.valueType);
                        EditorGUILayout.EndHorizontal();

                        switch(customVariable.valueType)
                        {
                            case ValueTypeStatusEffect.Int:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Int Value:", GUILayout.Width(100));
                                customVariable.intValue = EditorGUILayout.IntField(customVariable.intValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeStatusEffect.Float:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Float Value:", GUILayout.Width(100));
                                customVariable.floatValue = EditorGUILayout.FloatField(customVariable.floatValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeStatusEffect.Double:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Double Value:", GUILayout.Width(100));
                                customVariable.doubleValue = EditorGUILayout.DoubleField(customVariable.doubleValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeStatusEffect.Boolean:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Boolean Value:", GUILayout.Width(100));
                                customVariable.boolValue = EditorGUILayout.Toggle(customVariable.boolValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeStatusEffect.Vector2:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Vector2 Value:", GUILayout.Width(100));
                                customVariable.vector2Value = EditorGUILayout.Vector2Field("", customVariable.vector2Value);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeStatusEffect.Vector3:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Vector3 Value:", GUILayout.Width(100));
                                customVariable.vector3Value = EditorGUILayout.Vector3Field("", customVariable.vector3Value);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeStatusEffect.String:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("String Value:", GUILayout.Width(100));
                                customVariable.stringValue = EditorGUILayout.TextField(customVariable.stringValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeStatusEffect.passiveOrActive:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Status Type:", GUILayout.Width(100));
                                customVariable.activeOrPassiveValue = (StatusEffectMisc)EditorGUILayout.EnumPopup(customVariable.activeOrPassiveValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                        }
                       

                        if (GUILayout.Button("Remove"))
                        {
                            effect._customVariablesList.RemoveAt(j);
                            j--; // Decrease the loop counter as we removed an element
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }

                    if (GUILayout.Button("Minimize"))
                    {
                        minimizeEffect = true;
                    }

                    EditorGUILayout.Space(10);

                    // Save changes to the TraitEffect object (assuming SaveCustomVariables method saves it to EditorPrefs)
                    effect.SaveCustomVariables();
                }
                else
                {
                    EditorGUILayout.LabelField("Effect Name: " + effect.statusEffectName);

                    if (GUILayout.Button("Select Effect"))
                    {
                        selectedEffect = effect;
                        minimizeEffect = false;
                        newVariableName = "";
                        newVariableValue = null;
                        LoadCustomVariables(selectedEffect);
                    }
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.Space(20);
        EditorGUILayout.EndScrollView();
    }
    

    // Helper method to create a new TraitEffect asset and save it as an asset
    private StatusEffectConditions CreateTraitEffectAsset(string name)
    {
        StatusEffectConditions newStatusEffect = CreateInstance<StatusEffectConditions>();
        newStatusEffect._statusEffectName = name;

        AssetDatabase.CreateAsset(newStatusEffect, "Assets/Resources/StatusEffectStuff/StatusEffectObjects/" + name + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newStatusEffect;
    }

    private void SaveCustomVariables()
    {
        if (statusEffect != null && statusEffect.customFields != null)
        {
            foreach (var effect in statusEffect.customFields)
            {
                effect.SaveCustomVariables(); 
        }
    }

    private void LoadCustomVariables(StatusEffectConditions effect)
    {
        effect.LoadCustomVariables();
    }
    private void OnEnable()
    {
        if (statusEffect != null && statusEffect.customFields != null)
        {
            foreach (var effect in statusEffect.customFields)
            {
                LoadCustomVariables(effect);
            }
        }
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.quitting += OnEditorQuitting;
    }

    private void OnDisable()
    {
        SaveCustomVariables();
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.quitting -= OnEditorQuitting;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            SaveCustomVariables();
        }
    }

    // Callback when the editor is being closed
    private void OnEditorQuitting()
    {
        SaveCustomVariables();
    }
}
