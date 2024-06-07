using UnityEngine;
using System.Collections.Generic;

public enum traitNames
{
    DoubleHealth,
    DoubleMana
};
public enum TraitType
{
    Active,
    Passive
}; 
[CreateAssetMenu(fileName = "New Trait", menuName = "Custom/Trait")]
public class Trait : ScriptableObject
{
    [SerializeField] public traitNames traitName;
    [SerializeField] public List<TraitEffect> customFields = new List<TraitEffect>(); // Store TraitEffect assets directly

    [SerializeField] public TraitType traitType; 
}