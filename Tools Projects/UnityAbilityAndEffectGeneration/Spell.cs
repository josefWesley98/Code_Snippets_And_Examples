using UnityEngine;
using System.Collections.Generic;

public enum SpellName
{
    FireBall,
    LighteningBolt,
    Warp,
}

[CreateAssetMenu(fileName = "New Spell", menuName = "Custom/Spell")]
public class Spell : ScriptableObject
{
    [SerializeField] public SpellName spellName;
    [TextArea]
    [SerializeField] public string spellDescription;
    [SerializeField] public GameObject spellPrefab;
    [SerializeField] public SpellType spellType;
    [SerializeField] public List<SpellEffects> customFields = new List<SpellEffects>();
}
