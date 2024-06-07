using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class SpellEditorWindow : EditorWindow
{
    private Spell spell;
    //private StatusEffect statusEffect;
    private SerializedObject serializedSpell;
    private SerializedProperty customFieldsProperty;
    private Vector2 scrollPosition;
    private string newSpellEffectName = "";
    private bool minimizeEffect = false;
    // Variables for managing custom variables
    private SpellEffects selectedSpellEffect;
    private string newVariableName = "";
    private object newVariableValue = null;
    private string[] variableTypes = new string[] {"Int", "Float", "Double", "Boolean", "String", "Vector2", "Vector3", "Status Effect"};
    private int selectedVariableTypeIndex = 0;
    private int selectedVariableTypeIndexCheck = 0;

    [MenuItem("Window/Spell Editor")]
    public static void ShowWindow()
    {
        GetWindow<SpellEditorWindow>("Spell Editor");
    }
    private void OnGUI()
    {
        spell = EditorGUILayout.ObjectField("Spell", spell, typeof(Spell), false) as Spell;


        // Initialize GUI styles
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

        if (spell == null)
            return;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (serializedSpell == null || serializedSpell.targetObject != spell)
        {
            serializedSpell = new SerializedObject(spell);
            customFieldsProperty = serializedSpell.FindProperty("customFields");
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        spell.spellName = (SpellName)EditorGUILayout.EnumPopup("Spell Name", spell.spellName, titleLabelStyleMedium);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        spell.spellType = (SpellType)EditorGUILayout.Popup("Spell Type", (int)spell.spellType, Enum.GetNames(typeof(SpellType)), titleLabelStyleMedium);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        spell.spellDescription = EditorGUILayout.TextArea(spell.spellDescription, GUILayout.Height(60));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        spell.spellPrefab = EditorGUILayout.ObjectField("Spell Prefab", spell.spellPrefab, typeof(GameObject), false) as GameObject;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(15);

        // GUI for adding new Spell effect
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Create New Spell Effect", titleLabelStyleLarge);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        newSpellEffectName = EditorGUILayout.TextField("Spell Effect Name:", newSpellEffectName);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Add Spell Effect") && !string.IsNullOrEmpty(newSpellEffectName))
        {
            if (spell.customFields == null)
                spell.customFields = new List<SpellEffects>();

            SpellEffects newSpellEffects = CreateSpellEffectAsset(newSpellEffectName);
            spell.customFields.Add(newSpellEffects);

            newSpellEffectName = "";

            EditorUtility.SetDirty(spell);
            serializedSpell.ApplyModifiedProperties();
        }

        EditorGUILayout.Space(10);

        if (spell.customFields != null)
        {
            foreach (var effect in spell.customFields)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if(selectedSpellEffect == effect && !minimizeEffect)
                {
                    

                    EditorGUILayout.Space(10);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Effect Name: " + effect._spellEffectName, titleLabelStyleMedium);
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
                            StatusEffect newVariableValueHolder = new StatusEffect();
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            newVariableValueHolder = EditorGUILayout.ObjectField("New Variable Value", newVariableValueHolder, typeof(StatusEffect), false) as StatusEffect;
                            newVariableValue = newVariableValueHolder;

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
                    EditorGUILayout.LabelField("Current Custom Variables", titleLabelStyleMedium);
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
                        customVariable.valueType = (ValueTypeSpells)EditorGUILayout.EnumPopup(customVariable.valueType);
                        EditorGUILayout.EndHorizontal();

                        // Display specific GUI for each value type
                       switch(customVariable.valueType)
                        {
                            case ValueTypeSpells.Int:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Int Value:", GUILayout.Width(100));
                                customVariable.intValue = EditorGUILayout.IntField(customVariable.intValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeSpells.Float:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Float Value:", GUILayout.Width(100));
                                customVariable.floatValue = EditorGUILayout.FloatField(customVariable.floatValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeSpells.Double:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Double Value:", GUILayout.Width(100));
                                customVariable.doubleValue = EditorGUILayout.DoubleField(customVariable.doubleValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeSpells.Boolean:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Boolean Value:", GUILayout.Width(100));
                                customVariable.boolValue = EditorGUILayout.Toggle(customVariable.boolValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeSpells.Vector2:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Vector2 Value:", GUILayout.Width(100));
                                customVariable.vector2Value = EditorGUILayout.Vector2Field("", customVariable.vector2Value);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeSpells.Vector3:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Vector3 Value:", GUILayout.Width(100));
                                customVariable.vector3Value = EditorGUILayout.Vector3Field("", customVariable.vector3Value);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeSpells.String:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("String Value:", GUILayout.Width(100));
                                customVariable.stringValue = EditorGUILayout.TextField(customVariable.stringValue);
                                EditorGUILayout.EndHorizontal();
                                break;
                            case ValueTypeSpells.StatusEffect:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Status Effect Value:", GUILayout.Width(100));
                                customVariable.statusEffectValue = EditorGUILayout.ObjectField(customVariable.statusEffectValue, typeof(StatusEffect), false) as StatusEffect;
                                EditorGUILayout.EndHorizontal();
                                break;
                        }

                        // Add remove button for each custom variable
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

                    // Save changes to the SpellEffects object (assuming SaveCustomVariables method saves it to EditorPrefs)
                    effect.SaveCustomVariables();
                }
                else
                {
                    EditorGUILayout.LabelField("Effect Name: " + effect._spellEffectName);

                    if (GUILayout.Button("Select Effect"))
                    {
                        selectedSpellEffect = effect;
                        minimizeEffect = false;
                        newVariableName = "";
                        newVariableValue = null;
                        LoadCustomVariables(selectedSpellEffect);
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
    private SpellEffects CreateSpellEffectAsset(string name)
    {
        SpellEffects newSpellEffects = CreateInstance<SpellEffects>();
        newSpellEffects._spellEffectName = name;

        AssetDatabase.CreateAsset(newSpellEffects, "Assets/Resources/SpellStuff/SpellEffectObjects/" + name + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newSpellEffects;
    }

    private void SaveCustomVariables()
    {
        if (spell != null && spell.customFields != null)
        {
            foreach (var effect in spell.customFields)
            {
                effect.SaveCustomVariables();
            }
        }
    }

    private void LoadCustomVariables(SpellEffects _spellEffect)
    {
        _spellEffect.LoadCustomVariables();
    }
    private void OnEnable()
    {
        if (spell != null && spell.customFields != null)
        {
            foreach (var spellEffect in spell.customFields)
            {
                LoadCustomVariables(spellEffect);
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

    private void OnEditorQuitting()
    {
        SaveCustomVariables();
    }

}
