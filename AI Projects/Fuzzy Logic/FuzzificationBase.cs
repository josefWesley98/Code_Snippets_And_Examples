using System.Collections;
using System.Collections.Generic;
using Fuzzy_Logic;
using UnityEngine;

public class FuzzificationBase : MonoBehaviour
{
    
    // this class is my membership function which is used in the Inference proccess to get the final defuzzified Value
    // it also containts our fuzzification methods for each value we pass into the Inference Base.
    
    public float MembershipFunction(float value, float low, float medium, float high)
    {
        float lowValue = Mathf.Max(0, 1 - Mathf.Abs((value - low) / low));
        float mediumValue = Mathf.Max(0, 1 - Mathf.Abs((value - medium) / (medium - low)));
        float highValue = Mathf.Max(0, 1 - Mathf.Abs((value - high) / (high - medium)));

        return Mathf.Max(lowValue, mediumValue, highValue);
    }
    
    public FuzzyVariable FuzzifyPosition(float distance)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(distance, 0, 5, 10),
            Medium = MembershipFunction(distance, 3, 10, 15),
            High = MembershipFunction(distance, 8, 15, 20)
        };

        return fuzzyVariable;
    }
    
    public FuzzyVariable FuzzifyStaminaCost(float cost)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(cost, 0, 5, 15),
            Medium = MembershipFunction(cost, 8, 15, 20),
            High = MembershipFunction(cost, 14, 22, 30)
        };

        return fuzzyVariable;
    }

    public FuzzyVariable FuzztifyAverageHealthDiff(float difference)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(difference, -30, -15, 0),
            Medium = MembershipFunction(difference, -10, 0, 10),
            High = MembershipFunction(difference, 5, 15, 30)
        };

        return fuzzyVariable;
    }

    public FuzzyVariable FuzzifyDistanceToTravel(float distance)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(distance, 0, 15, 25),
            Medium = MembershipFunction(distance, 12.5f, 35, 55),
            High = MembershipFunction(distance, 55, 75, 100)
        };
        
        return fuzzyVariable;
    }
    
    public FuzzyVariable FuzzifyDefenseRating(float rating)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(rating, 0f, 0.15f, 0.25f),
            Medium = MembershipFunction(rating, 0.2f, 0.45f, 0.65f),
            High = MembershipFunction(rating, 0.7f, 0.85f, 1.0f)
        };

        return fuzzyVariable;
    }
    
    public FuzzyVariable FuzzifyDangerLevel(float level)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(level, 0f, 0.15f, 0.25f),
            Medium = MembershipFunction(level, 0.2f, 0.45f, 0.65f),
            High = MembershipFunction(level, 0.7f, 0.85f, 1.0f)
        };

        return fuzzyVariable;
    }
    
    public FuzzyVariable FuzzifyHideRating(float rating)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(rating, 0f, 0.15f, 0.25f),
            Medium = MembershipFunction(rating, 0.2f, 0.45f, 0.65f),
            High = MembershipFunction(rating, 0.7f, 0.85f, 1.0f)
        };

        return fuzzyVariable;
    }
    
    public FuzzyVariable FuzzifyScoutedAreaPercent(float percent)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(percent, 0f, 15f, 35f),
            Medium = MembershipFunction(percent, 17.5f, 45f, 65f),
            High = MembershipFunction(percent, 70f, 85f, 100f)
        };

        return fuzzyVariable;
    }
    
    public FuzzyVariable FuzzifyFormationAccuracy(float accuracy)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(accuracy, 0f, 0.15f, 0.25f),
            Medium = MembershipFunction(accuracy, 0.2f, 0.45f, 0.65f),
            High = MembershipFunction(accuracy, 0.7f, 0.85f, 1.0f)
        };

        return fuzzyVariable;
    }

    public FuzzyVariable FuzzifyNumEnemies(int numEnemies)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(numEnemies, 0, 1, 3),
            Medium = MembershipFunction(numEnemies, 2, 5, 8),
            High = MembershipFunction(numEnemies, 7, 10, 15)
        };
        return fuzzyVariable;
    }

    public FuzzyVariable FuzzifyNumAllies(int numAllies)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(numAllies, 0, 1, 3),
            Medium = MembershipFunction(numAllies, 2, 5, 8),
            High = MembershipFunction(numAllies, 7, 10, 15)
        };
        return fuzzyVariable;
    }

    public FuzzyVariable FuzzifyDiffEnemiesAllies(int diffEnemiesAllies)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(diffEnemiesAllies, -5, -2, 0),
            Medium = MembershipFunction(diffEnemiesAllies, -1, 0, 1),
            High = MembershipFunction(diffEnemiesAllies, 0, 2, 5)
        };
        return fuzzyVariable;
    }

    public FuzzyVariable FuzzifyHealth(float health)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(health, 0, 30, 50),
            Medium = MembershipFunction(health, 40, 60, 80),
            High = MembershipFunction(health, 70, 90, 100)
        };

        return fuzzyVariable;
    }

    public FuzzyVariable FuzzifyStamina(float stamina)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(stamina, 0, 30, 50),
            Medium = MembershipFunction(stamina, 40, 60, 80),
            High = MembershipFunction(stamina, 70, 90, 100)
        };

        return fuzzyVariable;
    }
    
    public FuzzyVariable FuzzifyMoral(float moral)
    {
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(moral, 0, 30, 50),
            Medium = MembershipFunction(moral, 40, 60, 80),
            High = MembershipFunction(moral, 70, 90, 100)
        };

        return fuzzyVariable;
    }
    
    public FuzzyVariable FuzzifyInCombat(bool incombat)
    {
        // this is not getting used till later now it all got to complicated for first feasability demo
        float value = 0f;
        value = incombat ? 1f : 0f;
        FuzzyVariable fuzzyVariable = new FuzzyVariable
        {
            Low = MembershipFunction(value, 0, 0, 1),
            Medium = MembershipFunction(value, 0, 1, 1),
            High = MembershipFunction(value, 1, 1, 1)
        };

        return fuzzyVariable;
    }
}
