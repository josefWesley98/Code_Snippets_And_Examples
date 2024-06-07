using UnityEngine;
using System.Collections.Generic;

public enum StatusEffectName
{
    Slowed,
    Stunned,
    KnockedDown
};
public enum StatusType
{
    Buff,
    Debuff
};

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Custom/StatusEffect")]
public class StatusEffect : ScriptableObject
{
    [SerializeField] public StatusEffectName statusEffectName;
    [SerializeField] public StatusType type;
    [TextArea]
    [SerializeField] public string StatusEffectDescription;
    [SerializeField] public List<StatusEffectConditions> customFields = new List<StatusEffectConditions>();
    // prefab for visual effect?

}
