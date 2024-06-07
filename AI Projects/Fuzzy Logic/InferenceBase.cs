using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Fuzzy_Logic;
using UnityEngine;

public class InferenceBase : MonoBehaviour
{
    private Dictionary<(string, string, string), float> ruleWeights = new Dictionary<(string, string, string), float>();

    public void Awake()
    {
        InitialiseRuleWeights();
    }
    
    // this handles doing the inference part of the Fuzzification proccess, each Evaluation Function calculates the rules output which dictates how important any given input is to the action they are attempting to make.
    
    public float EvaluateAttackLocationRules(FuzzyVariable healthFuzzy, FuzzyVariable staminaFuzzy,
    FuzzyVariable groupAverageHealthFuzzy, FuzzyVariable groupAverageStaminaFuzzy,
    FuzzyVariable areaDefenseRatingFuzzy, FuzzyVariable distanceFromTargetFuzzy,
    FuzzyVariable numDiffEnemiesAlliesFuzzy, FuzzyVariable enemyFormationAccuracyFuzzy,
    FuzzyVariable allyFormationAccuracyFuzzy)
    {
        float ruleOutput = 0;

        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluateAttackLocationRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluateAttackLocationRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateAttackLocationRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateAttackLocationRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupAverageHealthFuzzy", "Low", "EvaluateAttackLocationRules") * groupAverageHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupAverageHealthFuzzy", "High", "EvaluateAttackLocationRules") * groupAverageHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupAverageStaminaFuzzy", "Low", "EvaluateAttackLocationRules") * groupAverageStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupAverageStaminaFuzzy", "High", "EvaluateAttackLocationRules") * groupAverageStaminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("areaDefenseRatingFuzzy", "Low", "EvaluateAttackLocationRules") * areaDefenseRatingFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("areaDefenseRatingFuzzy", "High", "EvaluateAttackLocationRules") * areaDefenseRatingFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("distanceFromTargetFuzzy", "Low", "EvaluateAttackLocationRules") * distanceFromTargetFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distanceFromTargetFuzzy", "High", "EvaluateAttackLocationRules") * distanceFromTargetFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateAttackLocationRules") * numDiffEnemiesAlliesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "High", "EvaluateAttackLocationRules") * numDiffEnemiesAlliesFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("enemyFormationAccuracyFuzzy", "High", "EvaluateAttackLocationRules") * enemyFormationAccuracyFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("allyFormationAccuracyFuzzy", "High", "EvaluateAttackLocationRules") * allyFormationAccuracyFuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluateDefendLocationRules(FuzzyVariable numDiffEnemiesAlliesFuzzy, FuzzyVariable healthFuzzy,
    FuzzyVariable staminaFuzzy, FuzzyVariable groupAverageHealthFuzzy, FuzzyVariable groupAverageStaminaFuzzy,
    FuzzyVariable areaDefenseRatingFuzzy)
    {
        float ruleOutput = 0.0f;

        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateDefendLocationRules") * numDiffEnemiesAlliesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "High", "EvaluateDefendLocationRules") * numDiffEnemiesAlliesFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluateDefendLocationRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluateDefendLocationRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateDefendLocationRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateDefendLocationRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupAverageHealthFuzzy", "Low", "EvaluateDefendLocationRules") * groupAverageHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupAverageHealthFuzzy", "High", "EvaluateDefendLocationRules") * groupAverageHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupAverageStaminaFuzzy", "Low", "EvaluateDefendLocationRules") * groupAverageStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupAverageStaminaFuzzy", "High", "EvaluateDefendLocationRules") * groupAverageStaminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("areaDefenseRatingFuzzy", "Low", "EvaluateDefendLocationRules") * areaDefenseRatingFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("areaDefenseRatingFuzzy", "High", "EvaluateDefendLocationRules") * areaDefenseRatingFuzzy.High; }
        );
       
        return ruleOutput;
    }
    
    public float EvaluateEliminateEnemiesRules(FuzzyVariable healthFuzzy, FuzzyVariable staminaFuzzy,
    FuzzyVariable numDiffEnemiesAlliesFuzzy, FuzzyVariable distanceFromTargetFuzzy,
    FuzzyVariable averageAllyHealthFuzzy, FuzzyVariable averageAllyStaminaFuzzy)
    {
        float ruleOutput = 0.0f;

        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateEliminateEnemiesRules") * numDiffEnemiesAlliesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "High", "EvaluateEliminateEnemiesRules") * numDiffEnemiesAlliesFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluateEliminateEnemiesRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluateEliminateEnemiesRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateEliminateEnemiesRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateEliminateEnemiesRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("distanceFromTargetFuzzy", "Low", "EvaluateEliminateEnemiesRules") * distanceFromTargetFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distanceFromTargetFuzzy", "High", "EvaluateEliminateEnemiesRules") * distanceFromTargetFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("averageAllyHealthFuzzy", "Low", "EvaluateEliminateEnemiesRules") * averageAllyHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageAllyHealthFuzzy", "High", "EvaluateEliminateEnemiesRules") * averageAllyHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("averageAllyStaminaFuzzy", "Low", "EvaluateEliminateEnemiesRules") * averageAllyStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageAllyStaminaFuzzy", "High", "EvaluateEliminateEnemiesRules") * averageAllyStaminaFuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluateExploreLocationRules(FuzzyVariable staminaFuzzy, FuzzyVariable distanceToExploreFuzzy,
    FuzzyVariable percentOfAreaScoutedFuzzy, FuzzyVariable dangerLevelFuzzy,
    FuzzyVariable distacneToFallbackPointFuzzy)
    {
        float ruleOutput = 0.0f;

        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("distanceToExploreFuzzy", "Low", "EvaluateExploreLocationRules") * distanceToExploreFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distanceToExploreFuzzy", "High", "EvaluateExploreLocationRules") * distanceToExploreFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("percentOfAreaScoutedFuzzy", "Low", "EvaluateExploreLocationRules") * percentOfAreaScoutedFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("percentOfAreaScoutedFuzzy", "High", "EvaluateExploreLocationRules") * percentOfAreaScoutedFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateExploreLocationRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateExploreLocationRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "Low", "EvaluateExploreLocationRules") * dangerLevelFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "High", "EvaluateExploreLocationRules") * dangerLevelFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("distacneToFallbackPointFuzzy", "Low", "EvaluateExploreLocationRules") * distacneToFallbackPointFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distacneToFallbackPointFuzzy", "High", "EvaluateExploreLocationRules") * distacneToFallbackPointFuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluateScoutEnemiesRules(FuzzyVariable healthFuzzy, FuzzyVariable staminaFuzzy,
    FuzzyVariable numDiffEnemiesAlliesFuzzy, FuzzyVariable dangerLevelFuzzy, FuzzyVariable distanceToExploreFuzzy,
    FuzzyVariable estimatedEnemyCountFuzzy, FuzzyVariable groupSizeFuzzy)
    {
        float ruleOutput = 0.0f;

        // Parallelize the computation of rule output
        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateScoutEnemiesRules") * numDiffEnemiesAlliesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "High", "EvaluateScoutEnemiesRules") * numDiffEnemiesAlliesFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluateScoutEnemiesRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluateScoutEnemiesRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateScoutEnemiesRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateScoutEnemiesRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "Low", "EvaluateScoutEnemiesRules") * dangerLevelFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "High", "EvaluateScoutEnemiesRules") * dangerLevelFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("distanceToExploreFuzzy", "Low", "EvaluateScoutEnemiesRules") * distanceToExploreFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distanceToExploreFuzzy", "High", "EvaluateScoutEnemiesRules") * distanceToExploreFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("estimatedEnemyCountFuzzy", "Low", "EvaluateScoutEnemiesRules") * estimatedEnemyCountFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("estimatedEnemyCountFuzzy", "High", "EvaluateScoutEnemiesRules") * estimatedEnemyCountFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupSizeFuzzy", "Low", "EvaluateScoutEnemiesRules") * groupSizeFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupSizeFuzzy", "High", "EvaluateScoutEnemiesRules") * groupSizeFuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluateSetAmbushRules(FuzzyVariable healthFuzzy, FuzzyVariable staminaFuzzy,
    FuzzyVariable groupSizeFuzzy, FuzzyVariable numDiffEnemiesAlliesFuzzy, FuzzyVariable groupAverageHealthFuzzy,
    FuzzyVariable groupAverageStaminaFuzzy, FuzzyVariable expectedEnemyCountFuzzy,
    FuzzyVariable averageHideRatingfuzzy)
    {
        float ruleOutput = 0.0f;

        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateSetAmbushRules") * numDiffEnemiesAlliesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("numDiffEnemiesAlliesFuzzy", "High", "EvaluateSetAmbushRules") * numDiffEnemiesAlliesFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluateSetAmbushRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluateSetAmbushRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateSetAmbushRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateSetAmbushRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupAverageHealthFuzzy", "Low", "EvaluateSetAmbushRules") * groupAverageHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupAverageHealthFuzzy", "High", "EvaluateSetAmbushRules") * groupAverageHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupAverageStaminaFuzzy", "Low", "EvaluateSetAmbushRules") * groupAverageStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupAverageStaminaFuzzy", "High", "EvaluateSetAmbushRules") * groupAverageStaminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("expectedEnemyCountFuzzy", "Low", "EvaluateSetAmbushRules") * expectedEnemyCountFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("expectedEnemyCountFuzzy", "High", "EvaluateSetAmbushRules") * expectedEnemyCountFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupSizeFuzzy", "Low", "EvaluateSetAmbushRules") * groupSizeFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupSizeFuzzy", "High", "EvaluateSetAmbushRules") * groupSizeFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("averageHideRatingfuzzy", "Low", "EvaluateSetAmbushRules") * averageHideRatingfuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageHideRatingfuzzy", "High", "EvaluateSetAmbushRules") * averageHideRatingfuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluateSurroundEnemiesRules(FuzzyVariable healthFuzzy, FuzzyVariable staminaFuzzy,
    FuzzyVariable groupSizeFuzzy, FuzzyVariable expectedEnemyCountFuzzy, FuzzyVariable averageAllyHealthFuzzy,
    FuzzyVariable averageAllyStaminaFuzzy, FuzzyVariable averageEnemyHealthFuzzy,
    FuzzyVariable averageEnemyStaminaFuzzy, FuzzyVariable formationAccuracyFuzzy)
    {
        float ruleOutput = 0.0f;

        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("averageAllyStaminaFuzzy", "Low", "EvaluateSurroundEnemiesRules") * averageAllyStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageAllyStaminaFuzzy", "High", "EvaluateSurroundEnemiesRules") * averageAllyStaminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluateSurroundEnemiesRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluateSurroundEnemiesRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateSurroundEnemiesRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateSurroundEnemiesRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("averageAllyHealthFuzzy", "Low", "EvaluateSurroundEnemiesRules") * averageAllyHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageAllyHealthFuzzy", "High", "EvaluateSurroundEnemiesRules") * averageAllyHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("averageEnemyHealthFuzzy", "Low", "EvaluateSurroundEnemiesRules") * averageEnemyHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageEnemyHealthFuzzy", "High", "EvaluateSurroundEnemiesRules") * averageEnemyHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("expectedEnemyCountFuzzy", "Low", "EvaluateSurroundEnemiesRules") * expectedEnemyCountFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("expectedEnemyCountFuzzy", "High", "EvaluateSurroundEnemiesRules") * expectedEnemyCountFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("groupSizeFuzzy", "Low", "EvaluateSurroundEnemiesRules") * groupSizeFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("groupSizeFuzzy", "High", "EvaluateSurroundEnemiesRules") * groupSizeFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("averageEnemyStaminaFuzzy", "Low", "EvaluateSurroundEnemiesRules") * averageEnemyStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageEnemyStaminaFuzzy", "High", "EvaluateSurroundEnemiesRules") * averageEnemyStaminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("formationAccuracyFuzzy", "High", "EvaluateSurroundEnemiesRules") * formationAccuracyFuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluateFlankEnemiesRules(FuzzyVariable distanceFromEnemyFuzzy, FuzzyVariable distanceToTravelFuzzy,
    FuzzyVariable averageStaminaFuzzy, FuzzyVariable staminaFuzzy, FuzzyVariable StaminaCostFuzzy,
    FuzzyVariable DiffEnemyiesAlliesFuzzy, FuzzyVariable dangerLevelFuzzy)
    {
        float ruleOutput = 0.0f;
        
        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("distanceFromEnemyFuzzy", "Low", "EvaluateFlankEnemiesRules") * distanceFromEnemyFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distanceFromEnemyFuzzy", "High", "EvaluateFlankEnemiesRules") * distanceFromEnemyFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("distanceToTravelFuzzy", "Low", "EvaluateFlankEnemiesRules") * distanceToTravelFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distanceToTravelFuzzy", "High", "EvaluateFlankEnemiesRules") * distanceToTravelFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateFlankEnemiesRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateFlankEnemiesRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("averageStaminaFuzzy", "Low", "EvaluateFlankEnemiesRules") * averageStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageStaminaFuzzy", "High", "EvaluateFlankEnemiesRules") * averageStaminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("StaminaCostFuzzy", "Low", "EvaluateFlankEnemiesRules") * StaminaCostFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("StaminaCostFuzzy", "High", "EvaluateFlankEnemiesRules") * StaminaCostFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("DiffEnemyiesAlliesFuzzy", "Low", "EvaluateFlankEnemiesRules") * DiffEnemyiesAlliesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("DiffEnemyiesAlliesFuzzy", "High", "EvaluateFlankEnemiesRules") * DiffEnemyiesAlliesFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "Low", "EvaluateFlankEnemiesRules") * dangerLevelFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "High", "EvaluateFlankEnemiesRules") * dangerLevelFuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluateRetreatFromEnemiesRules(FuzzyVariable healthFuzzy, FuzzyVariable staminaFuzzy,
    FuzzyVariable DiffEnemyiesAlliesFuzzy, FuzzyVariable defenseRatingFuzzy,
    FuzzyVariable alliesAverageStaminaFuzzy, FuzzyVariable alliesAverageHealthFuzzy, 
    FuzzyVariable enemiesAverageHealthFuzzy, FuzzyVariable averageHealthDiffAlliesEnemiesFuzzy,
    FuzzyVariable dangerLevelFuzzy)
    {
        float ruleOutput = 0.0f;
        
        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("defenseRatingFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * defenseRatingFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("defenseRatingFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * defenseRatingFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("alliesAverageStaminaFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * alliesAverageStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("alliesAverageStaminaFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * alliesAverageStaminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("alliesAverageHealthFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * alliesAverageHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("alliesAverageHealthFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * alliesAverageHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("DiffEnemyiesAlliesFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * DiffEnemyiesAlliesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("DiffEnemyiesAlliesFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * DiffEnemyiesAlliesFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * dangerLevelFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * dangerLevelFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("enemiesAverageHealthFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * enemiesAverageHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("enemiesAverageHealthFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * enemiesAverageHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("averageHealthDiffAlliesEnemiesFuzzy", "Low", "EvaluateRetreatFromEnemiesRules") * averageHealthDiffAlliesEnemiesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("averageHealthDiffAlliesEnemiesFuzzy", "High", "EvaluateRetreatFromEnemiesRules") * averageHealthDiffAlliesEnemiesFuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluateChargeEnemiesRules(FuzzyVariable healthFuzzy, FuzzyVariable staminaFuzzy, 
    FuzzyVariable DiffEnemyiesAlliesFuzzy, FuzzyVariable alliesAverageStaminaFuzzy,
    FuzzyVariable alliesAverageHealthFuzzy, FuzzyVariable distanceToTargetFuzzy)
    {
        float ruleOutput = 0;
        
        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluateChargeEnemiesRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluateChargeEnemiesRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("distanceToTargetFuzzy", "Low", "EvaluateChargeEnemiesRules") * distanceToTargetFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distanceToTargetFuzzy", "High", "EvaluateChargeEnemiesRules") * distanceToTargetFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateChargeEnemiesRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateChargeEnemiesRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("alliesAverageStaminaFuzzy", "Low", "EvaluateChargeEnemiesRules") * alliesAverageStaminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("alliesAverageStaminaFuzzy", "High", "EvaluateChargeEnemiesRules") * alliesAverageStaminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("alliesAverageHealthFuzzy", "Low", "EvaluateChargeEnemiesRules") * alliesAverageHealthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("alliesAverageHealthFuzzy", "High", "EvaluateChargeEnemiesRules") * alliesAverageHealthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("DiffEnemyiesAlliesFuzzy", "Low", "EvaluateChargeEnemiesRules") * DiffEnemyiesAlliesFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("DiffEnemyiesAlliesFuzzy", "High", "EvaluateChargeEnemiesRules") * DiffEnemyiesAlliesFuzzy.High; }
        );

        return ruleOutput;
    }
    
    public float EvaluatePatrolAreaRules(FuzzyVariable healthFuzzy, FuzzyVariable staminaFuzzy, 
    FuzzyVariable allyGroupSizeFuzzy, FuzzyVariable dangerLevelFuzzy, FuzzyVariable routeLengthFuzzy)
    {
        float ruleOutput = 0.0f;

        // Parallelize the computation of rule output
        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "Low", "EvaluatePatrolAreaRules") * healthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("healthFuzzy", "High", "EvaluatePatrolAreaRules") * healthFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("allyGroupSizeFuzzy", "Low", "EvaluatePatrolAreaRules") * allyGroupSizeFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("allyGroupSizeFuzzy", "High", "EvaluatePatrolAreaRules") * allyGroupSizeFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluatePatrolAreaRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluatePatrolAreaRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "Low", "EvaluatePatrolAreaRules") * dangerLevelFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "High", "EvaluatePatrolAreaRules") * dangerLevelFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("routeLengthFuzzy", "Low", "EvaluatePatrolAreaRules") * routeLengthFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("routeLengthFuzzy", "High", "EvaluatePatrolAreaRules") * routeLengthFuzzy.High; }
        );

        return ruleOutput;
    }
    public float EvaluateTravelToLocationRules(FuzzyVariable staminaFuzzy, 
        FuzzyVariable allyGroupSizeFuzzy, FuzzyVariable dangerLevelFuzzy, FuzzyVariable distanceToLocationFuzzy)
    {
        float ruleOutput = 0.0f;

        // Parallelize the computation of rule output
        Parallel.Invoke(
            () => { ruleOutput += GetRuleWeight("allyGroupSizeFuzzy", "Low", "EvaluateTravelToLocationRules") * allyGroupSizeFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("allyGroupSizeFuzzy", "High", "EvaluateTravelToLocationRules") * allyGroupSizeFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "Low", "EvaluateTravelToLocationRules") * staminaFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("staminaFuzzy", "High", "EvaluateTravelToLocationRules") * staminaFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "Low", "EvaluateTravelToLocationRules") * dangerLevelFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("dangerLevelFuzzy", "High", "EvaluateTravelToLocationRules") * dangerLevelFuzzy.High; },
            () => { ruleOutput += GetRuleWeight("distanceToLocationFuzzy", "Low", "EvaluateTravelToLocationRules") * distanceToLocationFuzzy.Low; },
            () => { ruleOutput += GetRuleWeight("distanceToLocationFuzzy", "High", "EvaluateTravelToLocationRules") * distanceToLocationFuzzy.High; }
        );
       
        return ruleOutput;
    }
    
    private float GetRuleWeight(string fuzzyVariable, string linguisticTerm, string functionName)
    {
        // Lookup rule weight efficiently using the dictionary
        if (ruleWeights.TryGetValue((fuzzyVariable, linguisticTerm, functionName), out float weight))
        {
            return weight;
        }
        return 0.0f; // Default weight for missing rules
    }
    
    // initalises each rule set for reference later.
    // hard coded values but really easy to change on the fly here.
    private void InitialiseRuleWeights()
    {
        RuleWeightAttackLocation();
        RuleWeightDefendLocation();
        RuleWeightExploreLocation();
        RuleWeightEliminateEnemies();
        RuleWeightAmbushs();
        RuleWeightPatrolArea();
        RuleWeightScoutEnemies();
        RuleWeightSurroundEnemies();
        RuleWeightFlankEnemies();
        RuleWeightRetreatFromEnemies();
        RuleWeightChargeEnemies();
        RuleWeightTravelToDestination();
    }
    
    #region Rules
    private void RuleWeightAttackLocation()
    {
        // For EvaluateAttackLocationRules
        ruleWeights[("healthFuzzy", "Low", "EvaluateAttackLocationRules")] = -0.45f;
        ruleWeights[("healthFuzzy", "High", "EvaluateAttackLocationRules")] = 0.45f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateAttackLocationRules")] = -0.35f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateAttackLocationRules")] = 0.35f;
        ruleWeights[("groupAverageHealthFuzzy", "Low", "EvaluateAttackLocationRules")] = -0.6f;
        ruleWeights[("groupAverageHealthFuzzy", "High", "EvaluateAttackLocationRules")] = 0.6f;
        ruleWeights[("groupAverageStaminaFuzzy", "Low", "EvaluateAttackLocationRules")] = -0.45f;
        ruleWeights[("groupAverageStaminaFuzzy", "High", "EvaluateAttackLocationRules")] = 0.45f;
        ruleWeights[("areaDefenseRatingFuzzy", "Low", "EvaluateAttackLocationRules")] = 0.5f;
        ruleWeights[("areaDefenseRatingFuzzy", "High", "EvaluateAttackLocationRules")] = -0.5f;
        ruleWeights[("distanceFromTargetFuzzy", "Low", "EvaluateAttackLocationRules")] = 0.25f;
        ruleWeights[("distanceFromTargetFuzzy", "High", "EvaluateAttackLocationRules")] = -0.25f;
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateAttackLocationRules")] = -0.7f;
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "High", "EvaluateAttackLocationRules")] = 0.7f;
        ruleWeights[("enemyFormationAccuracyFuzzy", "High", "EvaluateAttackLocationRules")] = -0.2f;
        ruleWeights[("allyFormationAccuracyFuzzy", "High", "EvaluateAttackLocationRules")] = 0.2f;
    }

    private void RuleWeightDefendLocation()
    {
        // For EvaluateDefendLocationRules
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateDefendLocationRules")] = -0.6f;
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "High", "EvaluateDefendLocationRules")] = 0.6f;
        ruleWeights[("healthFuzzy", "Low", "EvaluateDefendLocationRules")] = -0.35f;
        ruleWeights[("healthFuzzy", "High", "EvaluateDefendLocationRules")] = 0.35f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateDefendLocationRules")] = -0.2f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateDefendLocationRules")] = 0.2f;
        ruleWeights[("groupAverageHealthFuzzy", "Low", "EvaluateDefendLocationRules")] = -0.6f;
        ruleWeights[("groupAverageStaminaFuzzy", "High", "EvaluateDefendLocationRules")] = 0.6f;
        ruleWeights[("areaDefenseRatingFuzzy", "Low", "EvaluateDefendLocationRules")] = -0.35f;
        ruleWeights[("areaDefenseRatingFuzzy", "High", "EvaluateDefendLocationRules")] = 0.35f;
    }

    private void RuleWeightExploreLocation()
    {
        // For EvaluateExploreLocationRules
        ruleWeights[("staminaFuzzy", "Low", "EvaluateExploreLocationRules")] = -0.4f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateExploreLocationRules")] = 0.4f;
        ruleWeights[("distanceToExploreFuzzy", "Low", "EvaluateExploreLocationRules")] = 0.6f;
        ruleWeights[("distanceToExploreFuzzy", "High", "EvaluateExploreLocationRules")] = -0.6f;
        ruleWeights[("percentOfAreaScoutedFuzzy", "Low", "EvaluateExploreLocationRules")] = -0.35f;
        ruleWeights[("percentOfAreaScoutedFuzzy", "High", "EvaluateExploreLocationRules")] = 0.35f;
        ruleWeights[("dangerLevelFuzzy", "Low", "EvaluateExploreLocationRules")] = 0.35f;
        ruleWeights[("dangerLevelFuzzy", "High", "EvaluateExploreLocationRules")] = -0.35f;
        ruleWeights[("distacneToFallbackPointFuzzy", "Low", "EvaluateExploreLocationRules")] = 0.35f;
        ruleWeights[("distacneToFallbackPointFuzzy", "High", "EvaluateExploreLocationRules")] = -0.35f;
    }

    private void RuleWeightEliminateEnemies()
    {
        // For EvaluateEliminateEnemiesRules
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateEliminateEnemiesRules")] = -0.6f;
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "High", "EvaluateEliminateEnemiesRules")] = 0.6f;
        ruleWeights[("healthFuzzy", "Low", "EvaluateEliminateEnemiesRules")] = -0.35f;
        ruleWeights[("healthFuzzy", "High", "EvaluateEliminateEnemiesRules")] = 0.35f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateEliminateEnemiesRules")] = -0.3f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateEliminateEnemiesRules")] = 0.3f;
        ruleWeights[("distanceFromTargetFuzzy", "Low", "EvaluateEliminateEnemiesRules")] = 0.35f;
        ruleWeights[("distanceFromTargetFuzzy", "High", "EvaluateEliminateEnemiesRules")] = -0.35f;
        ruleWeights[("averageAllyHealthFuzzy", "Low", "EvaluateEliminateEnemiesRules")] = -0.35f;
        ruleWeights[("averageAllyHealthFuzzy", "High", "EvaluateEliminateEnemiesRules")] = 0.35f;
        ruleWeights[("averageAllyStaminaFuzzy", "Low", "EvaluateEliminateEnemiesRules")] = -0.35f;
        ruleWeights[("averageAllyStaminaFuzzy", "High", "EvaluateEliminateEnemiesRules")] = 0.35f;
    }

    private void RuleWeightAmbushs()
    {
        // for EvaluateSetAmbushRules
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateSetAmbushRules")] = -0.35f;
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "High", "EvaluateSetAmbushRules")] = 0.35f;
        ruleWeights[("healthFuzzy", "Low", "EvaluateSetAmbushRules")] = -0.25f;
        ruleWeights[("healthFuzzy", "High", "EvaluateSetAmbushRules")] = 0.25f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateSetAmbushRules")] = -0.4f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateSetAmbushRules")] = 0.4f;
        ruleWeights[("groupAverageHealthFuzzy", "Low", "EvaluateSetAmbushRules")] = -0.4f;
        ruleWeights[("groupAverageHealthFuzzy", "High", "EvaluateSetAmbushRules")] = 0.4f;
        ruleWeights[("groupAverageStaminaFuzzy", "Low", "EvaluateSetAmbushRules")] = -0.5f;
        ruleWeights[("groupAverageStaminaFuzzy", "High", "EvaluateSetAmbushRules")] = 0.5f;
        ruleWeights[("expectedEnemyCountFuzzy", "Low", "EvaluateSetAmbushRules")] = 0.6f;
        ruleWeights[("expectedEnemyCountFuzzy", "High", "EvaluateSetAmbushRules")] = -0.6f;
        ruleWeights[("groupSizeFuzzy", "Low", "EvaluateSetAmbushRules")] = -0.35f;
        ruleWeights[("groupSizeFuzzy", "High", "EvaluateSetAmbushRules")] = 0.35f;
        ruleWeights[("averageHideRatingfuzzy", "Low", "EvaluateSetAmbushRules")] = -0.45f;
        ruleWeights[("averageHideRatingfuzzy", "High", "EvaluateSetAmbushRules")] = 0.45f;
    }

    private void RuleWeightPatrolArea()
    {
        // For EvaluatePatrolAreaRules
        ruleWeights[("healthFuzzy", "Low", "EvaluatePatrolAreaRules")] = -0.25f;
        ruleWeights[("healthFuzzy", "High", "EvaluatePatrolAreaRules")] = 0.25f;
        ruleWeights[("allyGroupSizeFuzzy", "Low", "EvaluatePatrolAreaRules")] = -0.45f;
        ruleWeights[("allyGroupSizeFuzzy", "High", "EvaluatePatrolAreaRules")] = 0.45f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluatePatrolAreaRules")] = -0.45f;
        ruleWeights[("staminaFuzzy", "High", "EvaluatePatrolAreaRules")] = 0.45f;
        ruleWeights[("dangerLevelFuzzy", "Low", "EvaluatePatrolAreaRules")] = 0.5f;
        ruleWeights[("dangerLevelFuzzy", "High", "EvaluatePatrolAreaRules")] = -0.5f;
        ruleWeights[("routeLengthFuzzy", "Low", "EvaluatePatrolAreaRules")] = 0.6f;
        ruleWeights[("routeLengthFuzzy", "High", "EvaluatePatrolAreaRules")] = -0.6f;
    }
    private void RuleWeightTravelToDestination()
    {
        // For TravelToDestination rules
        ruleWeights[("allyGroupSizeFuzzy", "Low", "EvaluateTravelToLocationRules")] = -0.45f;
        ruleWeights[("allyGroupSizeFuzzy", "High", "EvaluateTravelToLocationRules")] = 0.45f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateTravelToLocationRules")] = -0.45f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateTravelToLocationRules")] = 0.45f;
        ruleWeights[("dangerLevelFuzzy", "Low", "EvaluateTravelToLocationRules")] = 0.5f;
        ruleWeights[("dangerLevelFuzzy", "High", "EvaluateTravelToLocationRules")] = -0.5f;
        ruleWeights[("distanceToLocationFuzzy", "Low", "EvaluateTravelToLocationRules")] = 0.6f;
        ruleWeights[("distanceToLocationFuzzy", "High", "EvaluateTravelToLocationRules")] = -0.6f;
    }
    private void RuleWeightScoutEnemies()
    {
        // For EvaluateScoutEnemiesRules
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "Low", "EvaluateScoutEnemiesRules")] = -0.15f;
        ruleWeights[("numDiffEnemiesAlliesFuzzy", "High", "EvaluateScoutEnemiesRules")] = 0.15f;
        ruleWeights[("healthFuzzy", "Low", "EvaluateScoutEnemiesRules")] = -0.25f;
        ruleWeights[("healthFuzzy", "High", "EvaluateScoutEnemiesRules")] = 0.25f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateScoutEnemiesRules")] = -0.4f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateScoutEnemiesRules")] = 0.4f;
        ruleWeights[("dangerLevelFuzzy", "Low", "EvaluateScoutEnemiesRules")] = 0.4f;
        ruleWeights[("dangerLevelFuzzy", "High", "EvaluateScoutEnemiesRules")] = -0.4f;
        ruleWeights[("distanceToExploreFuzzy", "Low", "EvaluateScoutEnemiesRules")] = 0.3f;
        ruleWeights[("distanceToExploreFuzzy", "High", "EvaluateScoutEnemiesRules")] = -0.3f;
        ruleWeights[("estimatedEnemyCountFuzzy", "Low", "EvaluateScoutEnemiesRules")] = 0.75f;
        ruleWeights[("estimatedEnemyCountFuzzy", "High", "EvaluateScoutEnemiesRules")] = -0.75f;
        ruleWeights[("groupSizeFuzzy", "Low", "EvaluateScoutEnemiesRules")] = 0.3f;
        ruleWeights[("groupSizeFuzzy", "High", "EvaluateScoutEnemiesRules")] = -0.3f;
    }

    private void RuleWeightSurroundEnemies()
    {
        // For EvaluateSurroundEnemiesRules
        ruleWeights[("averageAllyStaminaFuzzy", "Low", "EvaluateSurroundEnemiesRules")] = -0.4f;
        ruleWeights[("averageAllyStaminaFuzzy", "High", "EvaluateSurroundEnemiesRules")] = 0.4f;
        ruleWeights[("healthFuzzy", "Low", "EvaluateSurroundEnemiesRules")] = -0.25f;
        ruleWeights[("healthFuzzy", "High", "EvaluateSurroundEnemiesRules")] = 0.25f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateSurroundEnemiesRules")] = -0.4f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateSurroundEnemiesRules")] = 0.4f;
        ruleWeights[("averageAllyHealthFuzzy", "Low", "EvaluateSurroundEnemiesRules")] = -0.6f;
        ruleWeights[("averageAllyHealthFuzzy", "High", "EvaluateSurroundEnemiesRules")] = 0.6f;
        ruleWeights[("averageEnemyHealthFuzzy", "Low", "EvaluateSurroundEnemiesRules")] = 0.6f;
        ruleWeights[("averageEnemyHealthFuzzy", "High", "EvaluateSurroundEnemiesRules")] = -0.6f;
        ruleWeights[("expectedEnemyCountFuzzy", "Low", "EvaluateSurroundEnemiesRules")] = 0.6f;
        ruleWeights[("expectedEnemyCountFuzzy", "High", "EvaluateSurroundEnemiesRules")] = -0.6f;
        ruleWeights[("groupSizeFuzzy", "Low", "EvaluateSurroundEnemiesRules")] = -0.35f;
        ruleWeights[("groupSizeFuzzy", "High", "EvaluateSurroundEnemiesRules")] = 0.35f;
        ruleWeights[("averageEnemyStaminaFuzzy", "Low", "EvaluateSurroundEnemiesRules")] = 0.45f;
        ruleWeights[("averageEnemyStaminaFuzzy", "High", "EvaluateSurroundEnemiesRules")] = -0.45f;
        ruleWeights[("formationAccuracyFuzzy", "High", "EvaluateSurroundEnemiesRules")] = 0.15f;
    }

    private void RuleWeightFlankEnemies()
    {
        // For EvaluateFlankEnemiesRules
        ruleWeights[("distanceFromEnemyFuzzy", "Low", "EvaluateFlankEnemiesRules")] = 0.25f;
        ruleWeights[("distanceFromEnemyFuzzy", "High", "EvaluateFlankEnemiesRules")] = -0.25f;
        ruleWeights[("distanceToTravelFuzzy", "Low", "EvaluateFlankEnemiesRules")] = 0.45f;
        ruleWeights[("distanceToTravelFuzzy", "High", "EvaluateFlankEnemiesRules")] = -0.45f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateFlankEnemiesRules")] = -0.4f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateFlankEnemiesRules")] = 0.4f;
        ruleWeights[("averageStaminaFuzzy", "Low", "EvaluateFlankEnemiesRules")] = -0.6f;
        ruleWeights[("averageStaminaFuzzy", "High", "EvaluateFlankEnemiesRules")] = 0.6f;
        ruleWeights[("StaminaCostFuzzy", "Low", "EvaluateFlankEnemiesRules")] = 0.6f;
        ruleWeights[("StaminaCostFuzzy", "High", "EvaluateFlankEnemiesRules")] = -0.6f;
        ruleWeights[("DiffEnemyiesAlliesFuzzy", "Low", "EvaluateFlankEnemiesRules")] = -0.5f;
        ruleWeights[("DiffEnemyiesAlliesFuzzy", "High", "EvaluateFlankEnemiesRules")] = 0.5f;
        ruleWeights[("dangerLevelFuzzy", "Low", "EvaluateFlankEnemiesRules")] = 0.25f;
        ruleWeights[("dangerLevelFuzzy", "High", "EvaluateFlankEnemiesRules")] = -0.25f;
    }

    private void RuleWeightRetreatFromEnemies()
    {
        // For EvaluateRetreatFromEnemiesRules
        ruleWeights[("healthFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = -0.25f;
        ruleWeights[("healthFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = 0.25f;
        ruleWeights[("defenseRatingFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = -0.45f;
        ruleWeights[("defenseRatingFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = 0.45f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = -0.4f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = 0.4f;
        ruleWeights[("alliesAverageStaminaFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = -0.65f;
        ruleWeights[("alliesAverageStaminaFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = 0.45f;
        ruleWeights[("alliesAverageHealthFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = -0.7f;
        ruleWeights[("alliesAverageHealthFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = 0.7f;
        ruleWeights[("DiffEnemyiesAlliesFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = -0.5f;
        ruleWeights[("DiffEnemyiesAlliesFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = 0.5f;
        ruleWeights[("dangerLevelFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = 0.35f;
        ruleWeights[("dangerLevelFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = -0.35f;
        ruleWeights[("enemiesAverageHealthFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = 0.5f;
        ruleWeights[("enemiesAverageHealthFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = -0.5f;
        ruleWeights[("averageHealthDiffAlliesEnemiesFuzzy", "Low", "EvaluateRetreatFromEnemiesRules")] = -0.75f;
        ruleWeights[("averageHealthDiffAlliesEnemiesFuzzy", "High", "EvaluateRetreatFromEnemiesRules")] = 0.75f;
    }

    private void RuleWeightChargeEnemies()
    {
        // For EvaluateChargeEnemiesRules
        ruleWeights[("healthFuzzy", "Low", "EvaluateChargeEnemiesRules")] = -0.45f;
        ruleWeights[("healthFuzzy", "High", "EvaluateChargeEnemiesRules")] = 0.45f;
        ruleWeights[("distanceToTargetFuzzy", "Low", "EvaluateChargeEnemiesRules")] = 0.7f;
        ruleWeights[("distanceToTargetFuzzy", "High", "EvaluateChargeEnemiesRules")] = -0.7f;
        ruleWeights[("staminaFuzzy", "Low", "EvaluateChargeEnemiesRules")] = -0.5f;
        ruleWeights[("staminaFuzzy", "High", "EvaluateChargeEnemiesRules")] = 0.5f;
        ruleWeights[("alliesAverageStaminaFuzzy", "Low", "EvaluateChargeEnemiesRules")] = -0.65f;
        ruleWeights[("alliesAverageStaminaFuzzy", "High", "EvaluateChargeEnemiesRules")] = 0.65f;
        ruleWeights[("alliesAverageHealthFuzzy", "Low", "EvaluateChargeEnemiesRules")] = -0.5f;
        ruleWeights[("alliesAverageHealthFuzzy", "High", "EvaluateChargeEnemiesRules")] = 0.5f;
        ruleWeights[("DiffEnemyiesAlliesFuzzy", "Low", "EvaluateChargeEnemiesRules")] = -0.65f;
        ruleWeights[("DiffEnemyiesAlliesFuzzy", "High", "EvaluateChargeEnemiesRules")] = 0.65f;
    }
    
    #endregion
    
}
