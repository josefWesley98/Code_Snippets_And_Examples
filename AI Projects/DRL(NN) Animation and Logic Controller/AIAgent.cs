using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class AIAgent : Agent
{
    #region Initalise Variables.
        #region Overall AI Settings
        [Header("Settings")]
        [SerializeField] private bool useLocationalDamage = true;
        [SerializeField] private bool useRatingSystem = true;
        [SerializeField] private bool randomiseStartingRatings = true;
        [SerializeField] private bool fightAsTeam = false;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask allyLayer;
        #endregion

        #region Agent Stats
        [Header("Agent Tags")]
        [SerializeField] private string weaponTag;
        [SerializeField] private string shieldTag;
        [Header("Agent Health")]
        [SerializeField] private float currentHealth = 300.0f;
        [SerializeField] private float maxHealth = 300.0f;
        [SerializeField] private float defaultMaxHealth = 300.0f;
        [Header("Agent Stamina")]
        [SerializeField] private float currentStamina = 300.0f;
        [SerializeField] private float maxStamina = 300.0f;
        [SerializeField] private float defaultMaxStamina = 300.0f;
        [SerializeField] private float staminaRegeneration  = 1.0f;
        [SerializeField] private float staminaRegenerationDefault  = 1.0f;
        [SerializeField] private float staminaCostReduction = 1.0f;

        [Header("Agent Speed")]
        [SerializeField] private float currentSpeed = 3.0f;
        [SerializeField] private float maxSpeed = 3.0f;
        [SerializeField] private float defaultMaxSpeed = 3.0f;
        [SerializeField] private float currentAnimationSpeed = 1.0f;
        [SerializeField] private float defaultMaxAnimationSpeed = 1.0f;
        [Header("Agent Damage and Attack Cooldown")]
        [SerializeField] private float damage = 100.0f;
        [SerializeField] private float damageModifier = 1.0f;
        //[SerializeField] private float defaultDamageModifier = 1.0f;
        [SerializeField] private float attackCooldown = 1.0f;
        [Header("Agent Detection")]
        [SerializeField] private float detectionRadius = 50.0f;
        private Transform agentShieldCentrePoint;
        private Transform agentSpearPoint;
        private float exhaustionModifier = 1.0f;
        private bool staminaBelowHalf = false;
        private bool staminaBelowQuarter = false;
        private Vector3 closestEnemiesDistance;
        private float prevTargetDistance;
        private float prevTargetAngle;
        private bool isAttacking = false;
        private bool movingAwayFromTarget = false;
        private bool movingCloserToTarget = false;
        private float prevHealth = 0f;
        private bool notMovingForward = false;
        private bool notMovingDirectionally = false;
        private bool notRotating = false;
        private float defaultStaminaCostReduction = 1.0f;
        private GameObject target =  null;
        private bool targetAlive = false;
        private GameObject allyOne;
        private GameObject allyTwo;

        #endregion

        #region Ratings And Ranks
        private char overallRating = 'F';
        private int overallRank = 1;
        [SerializeField] private string Overall;
        private char strengthRating = 'F';
        private int strengthRank = 1;
        [SerializeField] private string Strength;
        private char agilityRating = 'F';
        private int agilityRank = 1;
        [SerializeField] private string Agility;
        private char durabilityRating = 'F';
        private int durabilityRank = 1;
        [SerializeField] private string Durability;
        private char enduranceRating = 'F';
        private int enduranceRank = 1;
        [SerializeField] private string Endurance;
        private char intelligenceRating = 'F';
        private int intelligenceRank = 1;
        [SerializeField] private string Intelligence;
        #endregion
        
        #region Attack Setup Variables
        // attack stuff.
        private bool doAttack = false;
        private bool canSee = false;
        private bool isInRange = false;
        private bool swingOnCooldown = false;
        private bool swingMissed = false;
        private bool swinging = false;
        private bool hitEnemy = false;
        private bool finishedSwing = false;
        private bool attack = false;
        private float attackCooldownTimer = 0.0f;
        private bool startAttack = true;
        #endregion

        #region Spear Attack Variables
        // spear positioning stuff.
        private Vector3 spearTargetDestination = Vector3.zero;
        private float spearInterpolationSpeed = 2.0f;
        private float spearInterpolation = 0.0f;
        private bool pullSpearBack = false;
        int highestXRotSpear = 0;
        int highestYRotSpear = 0;
        int highestZRotSpear = 0;
        float highestXRotFloatSpear = 0;
        float highestYRotFloatSpear = 0;
        float highestZRotFloatSpear = 0;   
        private float highestXFloatPosSpear = 0.0f;
        private float highestYFloatPosSpear = 0.0f;
        private float highestZFloatPosSpear = 0.0f;
        #endregion

        #region Shield Movement Variables
        // shield positioning stuff.
        private Vector3 shieldTargetDestination = Vector3.zero;
        private float shieldInterpolation = 0.0f;
        private float shieldInterpolationSpeed = 1.25f;
        private bool defend = false;
        #endregion
    
        #region AI Punishments
        // rewards and punishments.
        private float punishNotAttackingTime = 10f;
        private float lastAttackTime = 0.0f;
        private float timeSinceLastAttack = 0.0f;
        #endregion

        #region Movement
        // movement stuff.
        private bool isMoving = false;
        private float moveX = 0.0f;
        private float moveZ = 0.0f;
        private float rotation = 0.0f;
        #endregion
        
        #region Enemies Setup
        [Header("Enemies")]
        private GameObject[] enemies = new GameObject[5];
        private Transform[] enemiesSpearPoint = new Transform[5];
        private Transform[] enemiesShieldPoint = new Transform[5];
        private AIAgent[] enemiesAI = new AIAgent[5];

        private float[] enemiesDistance = new float[5];
        private float[] enemiesAngle = new float[5];
        private float[] enemiesHealth = new float[5];

        private float[] prevEnemiesDistance = new float[5];
        private float[] prevEnemiesAngle = new float[5];
        private float[] prevEnemiesHealth = new float[5];
        private bool inTheSweetSpotEnemy = false;
        private bool toCloseToEnemy = false;
        private bool toFarAwayFromEnemy = false;
        private float targetDistance = 0.0f;
        private float targetAngle = 0.0f;
        private float targetHealth = 0.0f;
        private bool inTargetAngleSweetSpot = false;
        private bool lookingCloserToTarget = false;
        private bool lookingAwayFromTarget = false;

        private Vector3[] enemiesTargetDirAngle = new Vector3[5];
        #endregion

        #region Allies Setup
        [Header("Allies")]
        private GameObject[] allies = new GameObject[5];
        private Transform[] alliesSpearPoint = new Transform[5];
        private Transform[] alliesShieldPoint = new Transform[5];
        private AIAgent[]  alliesAI = new AIAgent[5];

        private float[] alliesDistance = new float[5];
        private float[] alliesAngle = new float[5];
        private float[] alliesHealth = new float[5];

        private float[] prevAlliesDistance = new float[5];
        private float[] prevAlliesAngle = new float[5];
        private float[] prevAlliesHealth = new float[5];
        private Vector3[] alliesTargetDirAngle = new Vector3[5];
        private bool inTheSweetSpotAllyOneX = false;
        private bool toCloseToAllyOneX = false;
        private bool toFarAwayFromAllyOneX = false;
        private bool inTheSweetSpotAllyOneZ = false;
        private bool toCloseToAllyOneZ = false;
        private bool toFarAwayFromAllyOneZ = false;
        private bool inTheSweetSpotAllyTwoX = false;
        private bool toCloseToAllyTwoX = false;
        private bool toFarAwayFromAllyTwoX = false;
        private bool inTheSweetSpotAllyTwoZ = false;
        private bool toCloseToAllyTwoZ = false;
        private bool toFarAwayFromAllyTwoZ = false;
        #endregion

        #region Animation Rigging
        [Header("Rigging")]
        [TextArea]
        [SerializeField] private string userNoteRigging = "";
        [Header("Right Arm Rigging (Spear Arm)")]
        [SerializeField] private bool useYourOwnDefaultPositionRightRig = false;
        [SerializeField] private Vector3 defaultRightArmTargetPos;
        [SerializeField] private bool useYourOwnDefaultRotationRightRig = false;
        [SerializeField] private Vector3 defaultRightArmTargetRot;
        [SerializeField] private Rig RightArmRig;
        [SerializeField] private Transform rightArmRigTarget;
        [SerializeField] private Transform rightArmRigHint;

        [Header("Left Arm Rigging (Shield Arm)")]
        [SerializeField] private bool useYourOwnDefaultPositionLeftRig = false;
        [SerializeField] private Vector3 defaultLeftArmTargetPos;
        [SerializeField] private bool useYourOwnDefaultRotationLeftRig = false;
        [SerializeField] private Vector3 defaultLeftArmTargetRot;
        [SerializeField] private Rig LeftArmRig;
        [SerializeField] private Transform leftArmRigTarget;
        [SerializeField] private Transform leftArmRigHint;
        #endregion

        #region Developers Tools
    [Header("Utility/Development Components")]
    [SerializeField] private bool randomSpawnOnXAxis = true;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private UIManager UI;
    private Animator animator;
    private Camera cam;
    private Material winMaterial;
    private Material loseMaterial;
    private MeshRenderer floorMeshRenderer;
    private bool hitCheck = false;
    private bool lookingAtTarget = false;
    private float currentDmgMod = 0.0f;
    private GameObject[] enemiesThatHitMe = new GameObject[10];
    private List<Transform> childs  = new List<Transform>();
    #endregion
    #endregion
    
    #region Initial Setup AI
    private void Awake()
    {
        // base setup for targets for the rigs. only applies these if the user doesn't add their own.
        if(!useYourOwnDefaultPositionLeftRig)
        {
            defaultLeftArmTargetPos = new Vector3(0f,1.2f,0.4f);
        }
        if(!useYourOwnDefaultRotationLeftRig)
        {
            defaultLeftArmTargetRot = new Vector3(0f,90f,100f);
        }
        if(!useYourOwnDefaultPositionRightRig)
        {
            defaultRightArmTargetPos = new Vector3(0.4f,1f,0f);
        }
        if(!useYourOwnDefaultRotationRightRig)
        {
            defaultRightArmTargetRot = new Vector3(130f,75f,0f);
        }

        animator = GetComponent<Animator>();

        // if using the rating system within the project do these functions.
        if(useRatingSystem)
        {
            if(randomiseStartingRatings)
            {
                RandomiseStartingRatings();
            }
            RecalculateStats();
            UpdateDisplayRanks();
            calcAverages();
        }
        FindEveryChild(gameObject.transform);
        
        for (int i = 0; i < childs.Count; i++)
        {
            FindEveryChild(childs[i]);
        }
        for(int i = 0; i < childs.Count; i++)
        {
            if(childs[i].tag == weaponTag)
            {
                agentSpearPoint = childs[i].transform;
                if(agentSpearPoint.transform.gameObject.GetComponent<Weapon>() != null)
                {
                    // setup for the weapon script so it has access to all the variables it needs.
                    agentSpearPoint.transform.gameObject.GetComponent<Weapon>().SetAI(this);
                    agentSpearPoint.transform.gameObject.GetComponent<Weapon>().SetTeamLayer(allyLayer);
                    agentSpearPoint.transform.gameObject.GetComponent<Weapon>().SetEnemyLayer(enemyLayer);
                }
                 
            }
            if(childs[i].tag == shieldTag)
            {
                agentShieldCentrePoint = childs[i].transform;
                if(agentShieldCentrePoint.transform.gameObject.GetComponent<Shield>() != null)
                {
                    // same thing as the weapon but for the shield.
                    agentShieldCentrePoint.transform.gameObject.GetComponent<Shield>().SetAI(this);
                    agentShieldCentrePoint.transform.gameObject.GetComponent<Shield>().SetTeamLayer(allyLayer);
                    agentShieldCentrePoint.transform.gameObject.GetComponent<Shield>().SetEnemyLayer(enemyLayer);
                }
            }
            if(childs[i].tag == "MainCamera")
            {
                if(childs[i].transform.gameObject.GetComponent<Camera>() != null)
                {
                    // grabs the camera on the AI that works with the ray cast hit system.
                    cam = childs[i].transform.gameObject.GetComponent<Camera>();
                }
            }
        }
    }
    public void FindEveryChild(Transform parent)
    {
        // gathers every child element of the this gameObject.
        int count = parent.childCount;
        for (int i = 0; i < count; i++)
        {
            childs.Add(parent.GetChild(i));
        }
    }
    private void RandomiseStartingRatings()
    {
        // randomises all the starting values.
        strengthRating = GetRating();
        strengthRank = GetRank();

        agilityRating = GetRating();
        agilityRank = GetRank();

        enduranceRating = GetRating();
        enduranceRank = GetRank();

        durabilityRating = GetRating();
        durabilityRank = GetRank();

        intelligenceRating = GetRating();
        intelligenceRank = GetRank();  
    }
    private void UpdateDisplayRanks()
    {
        // just an ease of life setup so i could easily see the ranks of each AI Agent.
        Overall = overallRating.ToString() + " " + overallRank.ToString();
        Strength = strengthRating.ToString() + " " + strengthRank.ToString();
        Agility = agilityRating.ToString() + " " + agilityRank.ToString();
        Endurance = enduranceRating.ToString() + " " + enduranceRank.ToString(); 
        Durability = durabilityRating.ToString() + " " + durabilityRank.ToString();
        Intelligence = intelligenceRating.ToString() + " " + intelligenceRank.ToString();
    }
    private void calcAverages()
    {
        // code to calculate the average of all the ranks and ratings.
        overallRating = GetAverageRating(strengthRating, agilityRating, enduranceRating, durabilityRating, intelligenceRating);
        overallRank = (strengthRank + agilityRank + enduranceRank + durabilityRank + intelligenceRank) / 5;
    }
    private char GetAverageRating(char strRating, char agiRating, char endRating, char durRating, char intRating)
    {
        // A working but probably not amazing implementation of sorting the chars into number value to find the average rating.
        char[] storage = new char[5]{strRating, agiRating, endRating, durRating, intRating};
        int[] average = new int[5];

        char rating = 'F';

        for(int i = 0; i < 5; i++)
        {
            if(storage[i] == 'F')
            {
                average[i] = 1;
            }
            if(storage[i] == 'E')
            {
                average[i] = 2;
            }
            if(storage[i] == 'D')
            {
                average[i] = 3;
            }
            if(storage[i] == 'C')
            {
                average[i] = 4;
            }
            if(storage[i] == 'B')
            {
                average[i] = 5;
            }
            if(storage[i] == 'A')
            {
                average[i] = 6;
            }
            if(storage[i] == 'S')
            {
                average[i] = 7;
            }
        }
        float final = ((float)average[0] + (float)average[1] + (float)average[2] + (float)average[3] + (float)average[4]) / 5;
        
        if(final > 0 && final <= 1)
        {
            rating = 'F';
        }
        else if(final > 1 && final <= 2)
        {
            rating = 'E';
        }
        else if(final > 2 && final <= 3)
        {
            rating = 'D';
        }
        else if(final > 3 && final <= 4)
        {
            rating = 'C';
        }
        else if(final > 4 && final <= 5)
        {
            rating = 'B';
        }
        else if(final > 5 && final <= 6)
        {
            rating = 'A';
        }
        else if(final > 6 && final <= 7)
        {
            rating = 'S';
        }
        else
        {
            Debug.Log("something went wrong.");
        }
            
        return rating;
    }
    private char GetRating()
    {
        // rand rating assigning. with a scale for probability as i wanted the majority to train in a middle ground.
        char rating = 'F';

        float rand = Random.Range(0f, 100f);

        if(rand < 40)
        {
            rating = 'F';
        }
        if(rand >= 40f && rand < 75f)
        {
            rating = 'E';
        }
        else if(rand >= 75f && rand < 85f)
        {
            rating = 'D';
        }
        else if(rand >= 85f && rand < 92f)
        {
            rating = 'C';
        }
        else if(rand >= 92f && rand < 97)
        {
            rating = 'B';
        }
        else if(rand >= 97f && rand < 99.5)
        {
            rating = 'A';
        }
        else if(rand >= 99.5f && rand < 100)
        {
            rating = 'S';
        }

        return rating;
    }
    private int GetRank()
    {
        // simple returning of rank which is just a number between 1 and 100.
        int rand = Random.Range(1, 100);
        return rand;
    }
    private void RecalculateStats()
    {
        // these are all the modifiers that the characters ratings and ranks effect to add variance to the stats so the AI can work regardless of what move speed ext, you give them and just added some fun to the testing process.
        // strength increasing damage.
        damageModifier = ((strengthRank * 0.25f) * GetMultiplierForRating(strengthRating)) / 10f;
        
        // agility increasing anim speed and movement speed
        float moveSpeedIncrease = ((agilityRank * 0.5f) * GetMultiplierForRating(agilityRating)) /100f;
        float animSpeedIncrease = moveSpeedIncrease * 0.75f;
        animSpeedIncrease = defaultMaxAnimationSpeed * animSpeedIncrease;
        moveSpeedIncrease = (defaultMaxSpeed * moveSpeedIncrease)*1.5f;
        maxSpeed = defaultMaxSpeed  + moveSpeedIncrease;
        spearInterpolationSpeed = 2.0f + moveSpeedIncrease;
        shieldInterpolationSpeed = 1.25f + moveSpeedIncrease;
        currentAnimationSpeed = defaultMaxAnimationSpeed + animSpeedIncrease;
        //durability increasing max health.
        float maxHealthIncrease = ((durabilityRank * 0.5f) * GetMultiplierForRating(durabilityRating)) /100f;
        maxHealthIncrease = defaultMaxHealth * maxHealthIncrease;
        maxHealth = defaultMaxHealth + maxHealthIncrease;

        // endurance increasing max stamina.
        float maxStaminaIncrease = ((enduranceRank * 0.5f) * GetMultiplierForRating(enduranceRating)) /100f;
        maxStaminaIncrease = defaultMaxStamina * maxStaminaIncrease;
        staminaRegeneration = staminaRegenerationDefault * (maxStaminaIncrease / maxStamina);
        maxStamina = defaultMaxStamina + maxStaminaIncrease;
        
        float staminaReductionRating = GetMultiplierForRating(enduranceRating) / 10f;
        float staminaReductionRank = enduranceRank / 100f;
        float combined = staminaReductionRank * staminaReductionRating;
        staminaCostReduction = defaultStaminaCostReduction - combined;

    }
    private float GetMultiplierForRating(char rating)
    {
        float multiplier = 0f;

        switch(rating)
        {
            case 'F':
                multiplier = 1f;
                break;
            case 'E':
                multiplier = 1.5f;
                break;
            case 'D':
                multiplier = 2.0f;
                break;
            case 'C':
                multiplier = 2.5f;
                break;
            case 'B':
                multiplier = 3.25f;
                break;
            case 'A':
                multiplier = 4.5f;
                break;
            case 'S':
                multiplier = 6.0f;
                break;
        }
        return multiplier;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;      
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    public override void OnEpisodeBegin()
    {
        maxHealth = defaultMaxHealth;
        maxStamina = defaultMaxStamina;
        currentAnimationSpeed = defaultMaxAnimationSpeed;
        damageModifier = 1.0f;
        exhaustionModifier = 1.0f;
        if(useRatingSystem)
        {
            if(randomiseStartingRatings)
            {
                RandomiseStartingRatings();
            }
            RecalculateStats();
            UpdateDisplayRanks();
            calcAverages();
        }
        
        inTheSweetSpotEnemy = false;
        toCloseToEnemy = false;
        target =  null;
        inTheSweetSpotAllyOneX = false;
        toCloseToAllyOneX = false;
        toFarAwayFromAllyOneX = false;
         inTheSweetSpotAllyOneZ = false;
        toCloseToAllyOneZ = false;
        toFarAwayFromAllyOneZ = false;

        inTheSweetSpotAllyTwoX = false;
        toCloseToAllyTwoX = false;
        toFarAwayFromAllyTwoX = false;
         inTheSweetSpotAllyTwoZ = false;
        toCloseToAllyTwoZ = false;
        toFarAwayFromAllyTwoZ = false;

        isAttacking = false;
        
        movingAwayFromTarget = false;
        movingCloserToTarget = false;

        moveX = 0.0f;
        moveZ = 0.0f;
        rotation = 0.0f;
        attackCooldownTimer =  0.0f;   
        hitCheck = false;
        spearInterpolation = 0.0f;
        shieldInterpolation = 0.0f;
        attackCooldown = 1.0f;
        timeSinceLastAttack = 0f;
        targetDistance = 0.0f;
        targetAngle = 0.0f;
        targetHealth = 0.0f;
        lastAttackTime = float.NegativeInfinity;
        prevTargetDistance = 0f;
        prevTargetAngle = 0f;

        isInRange = false;
        doAttack = false;
        hitEnemy = false;
        swingMissed = false;
        finishedSwing = false;
        attack = false;
        defend = false;
        swingOnCooldown = false;
        pullSpearBack = false;
        startAttack = true;
        if(randomSpawnOnXAxis)
        {
            spawnPoint.localPosition = new Vector3(Random.Range(-10f, 7.5f), spawnPoint.localPosition.y, spawnPoint.localPosition.z);
        }

        transform.localPosition = spawnPoint.localPosition;
        transform.localRotation = spawnPoint.transform.localRotation;
        spearTargetDestination = Vector3.zero;
        
        currentStamina = maxStamina;     
        currentHealth = maxHealth;

        // reset nearby enemies.
        for(int i = 0; i < enemies.Length; i++)
        {
            enemies[i] = null;
            enemiesAI[i] = null;

            enemiesSpearPoint[i] = null;
            enemiesShieldPoint[i] = null;

            enemiesDistance[i] = 200f;
            enemiesAngle[i] = 200f;
            enemiesHealth[i] = 200f;
        }

        // reset nearby enemies.
        for(int i = 0; i < allies.Length; i++)
        {
            allies[i] = null;
            alliesAI[i] = null;
            
            alliesShieldPoint[i] = null;
            alliesSpearPoint[i] = null;
            
            alliesDistance[i] = 200f;
            alliesAngle[i] = 200f;
            alliesHealth[i] = 200f;
        }
    }
    #endregion

    #region ML Agents Functions
    public override void CollectObservations(VectorSensor sensor)
    {
        
        // Enemy Observations.
        for(int i = 0; i < enemies.Length; i++)
        {
            // checks all the positions of enemies compared to itself, as well as the direction they are facing.
            // also checks where their weapons and shields are.
            if(enemies[i] != null)
            {
                sensor.AddObservation(enemiesDistance[i] - prevEnemiesDistance[i]); 
                sensor.AddObservation(enemiesAngle[i] - prevEnemiesAngle[i]);
                sensor.AddObservation(enemiesHealth[i] - prevEnemiesHealth[i]);

                sensor.AddObservation(enemies[i].transform.position);
                sensor.AddObservation(enemies[i].transform.rotation);

                sensor.AddObservation(enemiesSpearPoint[i].transform.position); 
                sensor.AddObservation(enemiesSpearPoint[i].transform.rotation); 
                sensor.AddObservation(enemiesShieldPoint[i].transform.position); 
            }
        }// 20 * 5 = 100

        // Ally Observations.
        // same as enemies but allies.
        for(int i = 0; i < allies.Length; i++)
        {
            if(allies[i] != null)
            {
                sensor.AddObservation(alliesDistance[i] - prevAlliesDistance[i]);
                sensor.AddObservation(alliesAngle[i] - prevAlliesAngle[i]);
                sensor.AddObservation(alliesHealth[i] - prevAlliesHealth[i]);

                sensor.AddObservation(allies[i].transform.position);
                sensor.AddObservation(allies[i].transform.rotation);

                sensor.AddObservation(alliesSpearPoint[i].transform.position); 
                sensor.AddObservation(alliesSpearPoint[i].transform.rotation); 
                sensor.AddObservation(alliesShieldPoint[i].transform.position);
            }
        }// 200

        // Personal Observations.
        // this is an observations of all relevant variables internally withing this AI Agent.
        sensor.AddObservation(currentHealth - prevHealth);// 201
        sensor.AddObservation(transform.position);// 204
        sensor.AddObservation(transform.rotation);//208
        sensor.AddObservation(agentSpearPoint.position);//211
        sensor.AddObservation(agentSpearPoint.rotation);//215
        sensor.AddObservation(agentShieldCentrePoint.position);//218
        sensor.AddObservation(doAttack);//219
        sensor.AddObservation(canSee);//220
        sensor.AddObservation(isInRange);//221
        sensor.AddObservation(moveX);//222
        sensor.AddObservation(moveZ);//223
        sensor.AddObservation(rotation);//224
        sensor.AddObservation(damage);//225
        sensor.AddObservation(currentStamina);//226
        sensor.AddObservation(currentSpeed);//227
        sensor.AddObservation(attack);//228
        sensor.AddObservation(defend);//229
        sensor.AddObservation(rightArmRigTarget.transform.localPosition);//232
        sensor.AddObservation(rightArmRigTarget.localRotation);//236
        sensor.AddObservation(leftArmRigTarget.transform.localPosition);//239
        sensor.AddObservation(staminaRegeneration);//240
        sensor.AddObservation(staminaBelowHalf);//241
        sensor.AddObservation(staminaBelowQuarter);//242
        sensor.AddObservation(lookingAtTarget);//243
        sensor.AddObservation(currentDmgMod);//244
        sensor.AddObservation(isAttacking); //245
        // enemy position detection
  

        //ally one
        // variables specific to the Two primary allies, this massively improved the average score and positioning of the AI.
        if(allyOne != null)
        {
            sensor.AddObservation(allyOne.transform.position);//248
            sensor.AddObservation(allyOne.transform.rotation);//252

            sensor.AddObservation(Mathf.Abs(allyOne.transform.position.x - transform.position.x)); //253
            sensor.AddObservation(Mathf.Abs(allyOne.transform.position.z - transform.position.z)); //254

            sensor.AddObservation(inTheSweetSpotAllyOneX);//255
            sensor.AddObservation(toCloseToAllyOneX);//256
            sensor.AddObservation(toFarAwayFromAllyOneX);//257
            sensor.AddObservation(inTheSweetSpotAllyOneZ);//258
            sensor.AddObservation(toCloseToAllyOneZ);//259
            sensor.AddObservation(toFarAwayFromAllyOneZ);//260

        }
        //ally two
        if(allyTwo != null)
        {
            sensor.AddObservation(allyTwo.transform.position);//263
            sensor.AddObservation(allyTwo.transform.rotation);//267

            sensor.AddObservation(Mathf.Abs(allyTwo.transform.position.x - transform.position.x)); //268
            sensor.AddObservation(Mathf.Abs(allyTwo.transform.position.z - transform.position.z)); //269

            sensor.AddObservation(inTheSweetSpotAllyTwoX);//270
            sensor.AddObservation(toCloseToAllyTwoX);//271
            sensor.AddObservation(toFarAwayFromAllyTwoX);//272
            sensor.AddObservation(inTheSweetSpotAllyTwoZ);//273
            sensor.AddObservation(toCloseToAllyTwoZ);//274
            sensor.AddObservation(toFarAwayFromAllyTwoZ);//275
        }
        // same as the allies but enemy target, again increase the score dramatically. as well as rate of training.
        // target.
        if(target != null)
        {
            sensor.AddObservation(target.transform.position);//278
            sensor.AddObservation(target.transform.rotation);//284

            sensor.AddObservation(targetDistance - prevTargetDistance);//285
            sensor.AddObservation(targetAngle - prevTargetAngle);//286

            sensor.AddObservation(inTheSweetSpotEnemy); //287
            sensor.AddObservation(toCloseToEnemy);  //288
            sensor.AddObservation(movingAwayFromTarget); //289
            sensor.AddObservation(movingCloserToTarget); //290
            sensor.AddObservation(targetHealth);//291
            sensor.AddObservation(inTargetAngleSweetSpot);//292
            sensor.AddObservation(lookingCloserToTarget);//293
            sensor.AddObservation(lookingAwayFromTarget);//294
            // says 292 is correct. so using that but may change.
        }
        Debug.Log("beep boop 123");
    }
    private void OnTriggerEnter(Collider other)
    {
        // handles being in spear range.
        if(other.gameObject.GetComponent<AIAgent>() != null)
        {
            if(other.gameObject.GetComponent<AIAgent>().GetEnemyLayerMask() == allyLayer)
            {
                isInRange = true;
                
            }
            else
            {
                isInRange = false;
            }
        }
        else if(other.gameObject.GetComponent<CollisionSpot>()!= null)
        {
            if(other.gameObject.GetComponent<CollisionSpot>().GetEnemyLayer() == allyLayer)
            {
                isInRange = true;
            
            }
            else
            {
                isInRange = false;
            }
        }
        else 
        {
            isInRange = false;
        }
       
    }
    private void OnCollisionEnter(Collision collisionInfo)
    {
        // just stops them from continually running into walls, really easy fix for just running aimlessly.
        if(collisionInfo.gameObject.tag == "wall")
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("beep boop 321");
        timeSinceLastAttack = Time.time - lastAttackTime;
        // if not currently swinging generate the position at which the AI will attack, this takes a lot of learning for it to get the hang of,
        // ive seen some success in using this when doing more than 3 million steps, estimated 25 million steps to make genuinely good use of it.
        if(!swinging)
        {
            //spear 
            // thrust 0.65 - 0.1 on X
            float ZPosSpear = actions.ContinuousActions[0];

            ZPosSpear += 0.9f;
            if(ZPosSpear > 0.65f)
            {
                ZPosSpear = 0.65f;
            }
            
            highestXFloatPosSpear = ZPosSpear;

            // +/- 25 on y. left and right.
            // +/- 10 on x for up and down.
            // +/- 10 on z for altering up and down.

            highestXRotSpear = actions.DiscreteActions[0]; // 50
            highestYRotSpear = actions.DiscreteActions[1]; //20
            highestZRotSpear = actions.DiscreteActions[2]; //20

            float highestXRotFloatSpear = (float)highestXRotSpear;
            float highestYRotFloatSpear = (float)highestYRotSpear;
            float highestZRotFloatSpear = (float)highestZRotSpear;

            // calc
            highestXRotFloatSpear -= 25;
            highestYRotFloatSpear -= 10;
            highestZRotFloatSpear -= 10;
        }

        //choice
        // the choice between attacking and defending.
        int choice = actions.DiscreteActions[3]; // 2 

        //AI
        //movement
        // choice on move X is between moving Left or right  or staying completely still.
        moveX = actions.DiscreteActions[4]; // 3
        // same as X but forward and backwards.
        moveZ = actions.DiscreteActions[5]; // 3
        // finally same thing but rotating left or right or not at all.
        rotation = actions.DiscreteActions[6]; // 3
        
        if(target != null)
        {
            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.localRotation = targetRotation;
          
        }
        // Shield
        // 45 - 95. 0-45 + 45 /100
        // Up down 0.95 – 0.45 on X
        // Left right +/- 0.15 on Z-Axis
        // divide by 100.

        // this positions the shield to specific locations on the AI's body so they can learn to block incoming blows.
        // note worked surprising well generally seeing 15-20% increase in blocks over the course of 1 million steps from the first kill at about 50000 steps.
        // think this could be down to the lack of inclusion of rotation of the shield.

        int highestXPosShield = actions.DiscreteActions[7];//45
        int highestZPosShield = actions.DiscreteActions[8];//30

        float highestXFloatPosShield = (float)highestXPosShield;
        float highestZFloatPosShield = (float)highestZPosShield;

        highestXFloatPosShield -= 15;
        highestZFloatPosShield -= 15;

        //calc
        highestXFloatPosShield = highestXFloatPosShield / 100f;
        highestZFloatPosShield = highestZFloatPosShield / 100f;
        
        int moveSpeedMultiplier = actions.DiscreteActions[9]; //15
        moveSpeedMultiplier += 1;
        float msMultiplier = (float)moveSpeedMultiplier;
        if(msMultiplier > maxSpeed)
        {
            msMultiplier = maxSpeed;
        }

        notMovingForward = false;
        notMovingDirectionally = false;
        notRotating = false;

        // handles the actual movement and rotation of the AI.
        switch(moveZ)
        {
            //forward
            case 0:
                currentStamina -= (0.075f * msMultiplier) * staminaCostReduction;
                if(targetDistance < 1.5f)
                {
                    
                }
                else
                {
                    transform.Translate(Vector3.forward * Time.deltaTime * (msMultiplier * exhaustionModifier));
                }
                break;

            //backward
            case 1:
                currentStamina -= (0.075f * msMultiplier) * staminaCostReduction;
                transform.Translate(Vector3.back * Time.deltaTime * ((msMultiplier * 0.66f) * exhaustionModifier));
                break;

            // no forward and backward movement.
            case 2:
                notMovingForward = true;
               
                break;
        }
        switch(moveX)
        {
            //left
            case 0:
                currentStamina -= (0.075f * msMultiplier) * staminaCostReduction;
                transform.Translate(Vector3.left * Time.deltaTime * ((msMultiplier * 0.66f) * exhaustionModifier));
                break;

            //right
            case 1:
                currentStamina -= (0.075f * msMultiplier) * staminaCostReduction;
                transform.Translate(Vector3.right * Time.deltaTime * ((msMultiplier * 0.66f)) * exhaustionModifier);
                break;

            //no action.
            case 2:
                notMovingDirectionally = true;
                break;
               
        }
        // switch(rotation)
        // {
        //     //left
        //     case 0:
        //         currentStamina -= 0.1f * staminaCostReduction;
        //         transform.Rotate(Vector3.up, -30f * Time.deltaTime);
        //         break;

        //     //right
        //     case 1:
        //         currentStamina -= 0.1f * staminaCostReduction;
        //         transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        //         break;

        //     //no rotation.
        //     case 2:
        //         notRotating = true;
        //         currentStamina += 0.1f;
        //         break;
        // }

        switch(choice)
        {
            case 0:
                attack = true;
                defend = false;
                break;
            case 1:
                defend = true;
                attack = false;
                break;
        }
        
        if(notMovingForward && notMovingDirectionally)
        {
            currentStamina += staminaRegeneration;
        }
        
        // when swinging this is the process it uses.
        if(swinging)
        {
            LeftArmRig.weight = 0.0f;
            RightArmRig.weight = 1.0f;
            
            if(startAttack)
            {
                // the inital setup for the AI to strike assigning the rotation of the blow as well as setting the end point of the attack based on the previous input.
                lastAttackTime = Time.time;
                spearTargetDestination = new Vector3(defaultLeftArmTargetPos.x + highestXFloatPosSpear, defaultLeftArmTargetPos.y, defaultLeftArmTargetPos.z);
                rightArmRigTarget.localRotation = Quaternion.Euler(highestXRotFloatSpear, highestYRotFloatSpear, highestZRotFloatSpear).normalized;
                startAttack = false;
                Debug.Log("only happens once per attack.");
            }
            // this creates the effect of pulling the spear forward and back when thrusting.
            if(!pullSpearBack)
            {
                spearInterpolation += Time.deltaTime * (spearInterpolationSpeed * exhaustionModifier); // default was 2.0f.
            }
            if(pullSpearBack)
            {
                spearInterpolation -= Time.deltaTime * (spearInterpolationSpeed * exhaustionModifier);
            }

            // I found lerping each value independently for some reason produced much better results. needs more investigation.
            float newX = Mathf.Lerp(defaultLeftArmTargetPos.x, spearTargetDestination.x, spearInterpolation);
            float newY = Mathf.Lerp(defaultLeftArmTargetPos.y, spearTargetDestination.y, spearInterpolation);
            float newZ = Mathf.Lerp(defaultLeftArmTargetPos.z, spearTargetDestination.z, spearInterpolation);

            // lerps that local position of the right arms target for the Rig.
            rightArmRigTarget.localPosition = new Vector3(newX,newY,newZ).normalized;
           

            if(spearInterpolation >= 1.0f && !pullSpearBack && !finishedSwing)
            {
                pullSpearBack = true;

            }
            if(spearInterpolation <= 0.0f && pullSpearBack && !finishedSwing)
            {
                finishedSwing = true;
                pullSpearBack = false;
            }

            if(finishedSwing)
            {
                // resets base values to a neutral position for the spear and shield.
                rightArmRigTarget.localPosition = defaultRightArmTargetPos;
                rightArmRigTarget.localRotation = Quaternion.Euler(defaultRightArmTargetRot);
                leftArmRigTarget.localPosition = defaultLeftArmTargetPos;
                isAttacking = false;
                swinging = false;
                finishedSwing = false;
                startAttack = true;
                doAttack = false;
                swingOnCooldown = true;
                
                currentStamina -= ((maxStamina / 50f) * staminaCostReduction);
                spearInterpolation = 0.0f;
                RightArmRig.weight = 0.0f;

                if(!hitCheck)
                {
                    swingMissed = true;
                }
                else
                {
                    hitCheck = false;    
                }
            }
        }
        else
        {
            LeftArmRig.weight = 1.0f;
            RightArmRig.weight = 0.0f;
        }

        if(defend)
        {
            // similar to  the attack function but runs constantly moving the shield when the "choice" variable is = 1. causes the shield to lerp to the target location.
            shieldTargetDestination = new Vector3(defaultLeftArmTargetPos.x + highestXFloatPosShield, defaultLeftArmTargetPos.y, defaultLeftArmTargetPos.z + highestZFloatPosShield);
            shieldInterpolation += Time.deltaTime * (shieldInterpolationSpeed * exhaustionModifier); // default was 1.25f.
            
            float newX = Mathf.Lerp(defaultLeftArmTargetPos.x, shieldTargetDestination.x, shieldInterpolation);
            float newY = Mathf.Lerp(defaultLeftArmTargetPos.y, shieldTargetDestination.y, shieldInterpolation);
            float newZ = Mathf.Lerp(defaultLeftArmTargetPos.z, shieldTargetDestination.z, shieldInterpolation);

            leftArmRigTarget.localPosition = new Vector3(newX,newY,newZ).normalized;
        }

        if(shieldInterpolation >= 1.0f)
        {
            shieldInterpolation = 0.0f;
            currentStamina -=  ((maxStamina / 75f) * staminaCostReduction);
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
        RaycastHit hit;
        
        // this is a method i used to make sure the AI is directally looking at their target so they know they can attack.
        if(Physics.Raycast(ray, out hit)){

            if(hit.rigidbody.gameObject.GetComponent<CollisionSpot>() != null)
            {
                if(hit.rigidbody.gameObject.GetComponent<CollisionSpot>().GetEnemyLayer() == allyLayer)
                {
                    canSee = true;
                    if(hit.rigidbody.gameObject == enemies[0])
                    {
                        lookingAtTarget = true;
                    }
                    else
                    {
                        lookingAtTarget = false;
                    }
                }
                else
                {
                    canSee = false;  
                    lookingAtTarget = false;
                }
            }
        }

        currentStamina += staminaRegeneration;
    }
    public void IncreaseKillCount()
    {
        // UI call to get a scoreboard so i can keep track  of kills for each team, mainly for keeping myself invested haha.
        if(gameObject.layer == 8)
        {
            UI.IncreaseKillCountTeamOne();
        }
        if(gameObject.layer == 9)
        {
            UI.IncreaseKillCountTeamTwo();
        }
    }
    #endregion

    #region Update Functions
    private void FixedUpdate()
    {
        // all my updates being done within fixed update as its the time frame the ML agents training in my console runs at so when exiting training they dont have major bugs.
        FindNearbyEnemies();
        FindNearbyAllies();

        if(fightAsTeam)
        {
            TwoAlliesSetup();
            AlliesAliveCheck();
        }

        TargetAliveCheck();
        StaminaCheck();
        CheckDeath();
        CheckSwingOnCooldown();
        CheckHitOrMiss();
        DoAttacking();
        DoDistanceChecks();
        DoRewardsAndPunishments();
    }
    private void TargetAliveCheck()
    {
        // checks if target is alive.
        if(target != null)
        {
            if(target.GetComponent<AIAgent>().GetCurrentHealth() <= 0)
            {
                targetAlive = false;
                
            }
            else 
            {
                targetAlive = true;
            }
        }
    }
    private void AlliesAliveCheck()
    {
        // checks if allies are alive.
        if(allyOne != null)
        {
            if(allyOne.GetComponent<AIAgent>().GetCurrentHealth() <= 0)
            {
                allyOne = null;
            }
        }
        if(allyTwo != null)
        {
            if(allyTwo.GetComponent<AIAgent>().GetCurrentHealth() <= 0)
            {
                allyTwo = null;
            }
        }
    }
    private void TwoAlliesSetup()
    {
        // just a setup to find ally 1  and ally 2 that should position themselves around each other causing them all to stay fairly close to each other.
        if(allies[0] != null && allyOne == null)
        {
            if(allyTwo == null)
            {
                allyOne = allies[0];
            }
            else if(allyTwo != null)
            {
                if(allyTwo != allies[0])
                {
                    allyOne = allies[0];
                }
                else
                {
                    if(allies[1] != null)
                    {
                        allyOne = allies[1];
                    }
                    else
                    {
                        allyOne = null;
                    }
                }
            }
        }
        if(allies[1] != null && allyTwo == null)
        {
            if(allyOne != null)
            {
                if(allyOne != allies[1])
                {
                    allyTwo = allies[1];
                }
                else
                {
                    if(allies[2] != null)
                    {
                        allyTwo = allies[2];
                    }
                    else
                    {
                        allyTwo = null;
                    }
                }
            }
            else
            {
                allyTwo = allies[1];
            }
        }
    }
    private void DoRewardsAndPunishments()
    {
        // regards killing the target/a targets death.
        if(!targetAlive && target != null)
        {
            AddReward(1f);
            target = null;
        }
        // a slight punishment if the AI decides to bottom out the stamina, makes sure it must be performing a task worth spending the stamina on rather than sprinting constantly for no reason.
        if(currentStamina <= maxStamina /2)
        {
            AddReward(-0.0001f);
        }
        // rewards having a positive stamina trying to push for them to have better stamina management.
        if(currentStamina > maxStamina /2)
        {
            AddReward(+0.0001f);
        }
        // if the AI does not make an attack within X seconds they will receive a punishment to encourage them to not just linger.
        if(Time.time - lastAttackTime >= punishNotAttackingTime)
        {
            // Punish the AI for not attacking in the last 10 seconds
            AddReward(-0.025f);
            lastAttackTime = Time.time;
        } 
        // if the AI is not looking towards the target the have then they are punished.
        if(!lookingAtTarget && target != null)
        {
            AddReward(-0.0001f);
        }
        // rewards the AI for being within attack range of the target or be punished for not doing that.
        if(isInRange)
        {
            AddReward(0.005f);
        }
        else
        {
            AddReward(-0.0075f);
        }
        // if the AI isnt continually moving while striking the target they will be rewarded, over the long term hoping this will make them only move if it is to avoid a strike as for best overall score.
        if(notMovingForward)
        {
            if(swinging)
            {
                AddReward(+0.001f);
            }
        }
        // same as last one but left and right rather than back and forward.
        if(notMovingDirectionally)
        {
            if(swinging)
            {
                AddReward(+0.001f);
            }
        }
        // an attempt to stop the AI from spinning while attacking the target.
        // if(notRotating)
        // {
        //     if(canSee)
        //     {
        //         AddReward(+0.005f);
        //         if(isInRange)
        //         {
        //             AddReward(+0.005f);
        //         }
        //     }
        //     if(swinging)
        //     {
        //         AddReward(+0.005f);
        //     }
        //     if(lookingAtTarget)
        //     {
        //         AddReward(+0.005f);
        //     }
        // }
        // // if they are rotating and but are in the line of sight of the target then they get punished as to try and keep them facing the target.
        // if(!notRotating)
        // {
        //     if(canSee)
        //     {
        //         AddReward(-0.02f);
        //         if(isInRange)
        //         {
        //             AddReward(-0.02f);
        //         }
        //     }
        //     if(swinging)
        //     {
        //         AddReward(-0.05f);
        //     }
        //     if(lookingAtTarget)
        //     {
        //         AddReward(-0.05f);
        //     }
        // }
        // punish the AI for failing to hit the target the tried to strike in an attempt to make them Aim their attacks.
        if(swingMissed)
        {
            swingMissed = false;
            AddReward(-0.025f);
        }
        
        // if they can attack and are in rage doattack is true but the attack variable is attached to a discrete branch that chooses between 0 and 1, 
        // so they can choose if they wish to attack. if they choose not to attack when given the chance they are punished a small amount so they will only not attack when it will cause them more loss of score.
        if(doAttack && !attack && !swingOnCooldown)
        {
            AddReward(-0.0002f);
        }
        // this entire region is just incentive to move towards your target, or stay near your allies. or to rotate to face your target.
        #region positioning Reward/Punishment
        // enemy
        if(inTheSweetSpotEnemy)
        {
            AddReward(+0.01f);
        }
       
        if(toCloseToEnemy)
        {
            AddReward(-0.001f);
        }
       
        if(movingCloserToTarget)
        {
            AddReward(+0.05f);
        }
        
        if(movingAwayFromTarget)
        {
            AddReward(-0.01f);
        }
        // if(inTargetAngleSweetSpot)
        // {
        //      AddReward(+0.01f);
        // }
     
        // if(lookingCloserToTarget)
        // {
        //     AddReward(+0.001f);
        // }
        // if(lookingAwayFromTarget)
        // {
        //     AddReward(-0.005f);
        // }
        // ally one
        if(inTheSweetSpotAllyTwoX)
        {
            AddReward(+0.005f);
        }
      
        if(toCloseToAllyTwoX)
        {
            AddReward(-0.005f);
        }
        
        if(toFarAwayFromAllyTwoX)
        {
            AddReward(-0.005f);
        }
        
        if(inTheSweetSpotAllyTwoZ)
        {
            AddReward(+0.005f);
        }
       
        if(toCloseToAllyTwoZ)
        {
             AddReward(-0.005f);
        }
       
        if(toFarAwayFromAllyTwoZ)
        {
            AddReward(-0.005f);
        }
        
        //ally two
        if(inTheSweetSpotAllyTwoX)
        {
            AddReward(+0.005f);
        }
       
        if(toCloseToAllyTwoX)
        {
            AddReward(-0.005f);
        }
        
        if(toFarAwayFromAllyTwoX)
        {
            AddReward(-0.005f);
        }
        
        if(inTheSweetSpotAllyTwoZ)
        {
            AddReward(+0.001f);
        }
       
        if(toCloseToAllyTwoZ)
        {
             AddReward(-0.005f);
        }
       
        if(toFarAwayFromAllyTwoZ)
        {
            AddReward(-0.005f);
        }
        #endregion

    }
    private void StaminaCheck()
    {
        // how stamina effects the player.
        if(currentStamina < 0)
        {
            currentStamina = 0;
        }
        if(currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
        if(currentStamina < maxStamina / 2f)
        {
            staminaBelowHalf = true;
            exhaustionModifier = 0.75f;
        }
        else
        {
            staminaBelowHalf = false;
            exhaustionModifier = 1.0f;
        }
        if(currentStamina < maxStamina / 4f)
        {
            staminaBelowQuarter = true;
            exhaustionModifier = 0.5f;
        }
        else if(!staminaBelowHalf && !staminaBelowQuarter)
        {
            staminaBelowQuarter = false;
            exhaustionModifier = 1.0f;
        }

        animator.speed = currentAnimationSpeed * exhaustionModifier;
    }
    private void FindNearbyAllies()
    {
        // detects nearby allies, sorts them nearest to farthest and removes duplicates.

        Collider[] currentlyDetectedAllies = Physics.OverlapSphere(transform.position, detectionRadius, allyLayer);
        HashSet<GameObject> uniqueAllies = new HashSet<GameObject>();

        if (currentlyDetectedAllies.Length != 0)
        {
            foreach (Collider allyCollider in currentlyDetectedAllies)
            {
                if (allyCollider.gameObject != gameObject && !uniqueAllies.Contains(allyCollider.gameObject))
                {
                    uniqueAllies.Add(allyCollider.gameObject);
                }
            }

            // sort allies by distance
            GameObject[] sortedAllies = uniqueAllies.OrderBy(ally => Vector3.Distance(transform.position, ally.transform.position)).ToArray();

            int i = 0;
            foreach (GameObject sortedAlly in sortedAllies)
            {
                if (i < allies.Length)
                {
                    allies[i] = sortedAlly;
                    alliesAI[i] = sortedAlly.GetComponent<AIAgent>();
                    alliesShieldPoint[i] = alliesAI[i].GetShieldPos();
                    alliesSpearPoint[i] = alliesAI[i].GetSpearPos();
                    i++;
                }
                else
                {
                    break;
                }
            }

            // set remaining allies to null
            for (; i < allies.Length; i++)
            {
                allies[i] = null;
                alliesAI[i] = null;
                alliesShieldPoint[i] = null;
                alliesSpearPoint[i] = null;
            }
        }
        else
        {
            // no allies nearby, set all allies to null
            for (int i = 0; i < allies.Length; i++)
            {
                allies[i] = null;
                alliesAI[i] = null;
                alliesShieldPoint[i] = null;
                alliesSpearPoint[i] = null;
            }
        }
        
    }
    private void FindNearbyEnemies()
    {
        // same things as allies but for enemies with the addition of assigning a target if its = to null.
        Collider[] currentlyDetectedEnemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        List<GameObject> uniqueEnemies = new List<GameObject>();

        if (currentlyDetectedEnemies.Length != 0)
        {
            foreach (Collider enemyCollider in currentlyDetectedEnemies)
            {
                if (enemyCollider.gameObject != gameObject && !uniqueEnemies.Contains(enemyCollider.gameObject))
                {
                    uniqueEnemies.Add(enemyCollider.gameObject);
                }
            }

            uniqueEnemies.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position).CompareTo(Vector3.Distance(transform.position, b.transform.position)));

            int i = 0;
            foreach (GameObject uniqueEnemy in uniqueEnemies)
            {
                if (i < enemies.Length)
                {
                    enemies[i] = uniqueEnemy;
                    enemiesAI[i] = uniqueEnemy.GetComponent<AIAgent>();
                    enemiesSpearPoint[i] = enemiesAI[i].GetSpearPos();
                    enemiesShieldPoint[i] = enemiesAI[i].GetShieldPos();
                    i++;
                }
                else
                {
                    break;
                }
            }

            // set remaining enemies to null
            for (; i < enemies.Length; i++)
            {
                enemies[i] = null;
                enemiesAI[i] = null;
                enemiesSpearPoint[i] = null;
                enemiesShieldPoint[i] = null;
            }
        }
        else
        {
            // no enemies nearby, set all enemies to null
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i] = null;
                enemiesAI[i] = null;
                enemiesSpearPoint[i] = null;
                enemiesShieldPoint[i] = null;
            }
        }

        if(target == null || !targetAlive)
        {
            if(enemies[0] != null)
            {
                target = enemies[0];
                targetAlive = true;
            }
        }
    }
    private void CheckHitOrMiss()
    {
        // tracks hits and misses for the AI.
        if(hitEnemy)
        {
            Debug.Log("hit");
            hitCheck = true;
            hitEnemy = false;
        }
       
    }
    private void DoAttacking()
    {
        // checks to see if they can throw and attack, and controls running either walk animation or attack animation.
        if(isInRange && !swingOnCooldown)
        {
            // do swing prep
            doAttack = true;

            if(doAttack && attack && !isAttacking && !swinging)
            {
                DamageCalculation();
                swinging = true;
                isAttacking = true;
                animator.SetBool("IsAttacking", true);
            }
        }
        else
        {
            RightArmRig.weight = 0.0f;
            animator.SetBool("IsAttacking", false);
        }
    }
    private void CheckSwingOnCooldown()
    {
        // runs a cooldown between attacks so they don't trust endlessly with the spear.
        if(swingOnCooldown)
        {
            attackCooldownTimer += Time.deltaTime;
            if(attackCooldownTimer >= attackCooldown)
            {
                swingOnCooldown = false;
                attackCooldownTimer = 0.0f;
            }
        }
    }

    private void CheckDeath()
    {
        // handles the death of this AI, basically just rewards the AI for killing or assisting on its death, as well as removing it as a target incase it gets missed during time skip in the above function.
        if(currentHealth <= 0.0f)
        {
            Debug.Log("confirmed kill");
            AddReward(-2.5f);
            for(int i = 0; i < enemiesThatHitMe.Length; i++)
            {
                if(enemiesThatHitMe[i] != null)
                {
                    if(enemiesThatHitMe[i].GetComponent<AIAgent>().GetTarget() == this)
                    {
                        SetTargetToNull();
                        enemiesThatHitMe[i].GetComponent<AIAgent>().AddReward(+1f);
                        enemiesThatHitMe[i].GetComponent<AIAgent>().SetTargetIsDead(true); 
                    }
                    else
                    {
                        enemiesThatHitMe[i].GetComponent<AIAgent>().AddReward(+0.25f);
                    }
                }
                enemiesThatHitMe[i] = null;
            }
            EndEpisode();
        }
    }
    private void DoDistanceChecks()
    {
        // this checks all the distance values to enable variables that detects true or false on booleans for the rewards and punishments. (handles all enemies allies, the target and the two allies that the position via.)
        if(target !=  null)
        {
            targetDistance = Vector3.Distance(transform.position, target.transform.position);
            Vector3 targetDirAngle = target.transform.position - transform.position;
            targetAngle = Vector3.Angle(transform.forward, targetDirAngle);
            targetHealth = target.GetComponent<AIAgent>().GetCurrentHealth();
        }
        else
        {
            targetDistance = 1000f;
            targetAngle = 1000f;
            targetHealth = 1000f;
        }

        if(target != null)
        {
            
            if(targetDistance < prevTargetDistance)
            {
                if(targetDistance < 1.5f)
                {
                    toCloseToEnemy = true;
                    inTheSweetSpotEnemy = false;
                    movingAwayFromTarget = false;
                    movingCloserToTarget = false;
                }
                else if(targetDistance >= 2 && targetDistance <= 5f)
                {
                    toCloseToEnemy = false;
                    inTheSweetSpotEnemy = true;
                    movingAwayFromTarget = false;
                    movingCloserToTarget = false;
                }
                else
                {
                    toCloseToEnemy = false;
                    inTheSweetSpotEnemy = false;
                    movingAwayFromTarget = false;
                    movingCloserToTarget = true;
                }
                
            }
            if(targetDistance >= prevTargetDistance)
            {
                toCloseToEnemy = false;
                inTheSweetSpotEnemy = false;
                movingAwayFromTarget = true;
                movingCloserToTarget = false;
            }
            if(targetAngle <= 20f && targetAngle >= -20f)
            {
                inTargetAngleSweetSpot = true;
            }
            else
            {
                inTargetAngleSweetSpot = false;
            }

            if(targetAngle <= prevTargetAngle && targetAngle > -0.001f)
            {
                inTargetAngleSweetSpot = false;
                lookingCloserToTarget = true;
                lookingAwayFromTarget = false;
            }
            if(targetAngle >= prevTargetAngle && targetAngle < 0.001f)
            {
                inTargetAngleSweetSpot = false;
                lookingCloserToTarget = true;
                lookingAwayFromTarget = false;
            }
            if(targetAngle > prevTargetAngle && !inTargetAngleSweetSpot && targetAngle > -0.001f)
            {
                inTargetAngleSweetSpot = false;
                lookingCloserToTarget = false;
                lookingAwayFromTarget = true;
            }
            if(targetAngle < prevTargetAngle && !inTargetAngleSweetSpot && targetAngle < 0.001f)
            {
                inTargetAngleSweetSpot = false;
                lookingCloserToTarget = false;
                lookingAwayFromTarget = true;
            }
          
        }
        // Checking previous positions and directions.
        for(int i = 0; i < enemies.Length; i++)
        {
            
            if(enemies[i] != null)
            {
                enemiesDistance[i] = Vector3.Distance(transform.position, enemies[i].transform.position);
                enemiesTargetDirAngle[i] = enemies[i].transform.position - transform.position;
                enemiesAngle[i] = Vector3.Angle(transform.forward, enemiesTargetDirAngle[i]);
                enemiesHealth[i] = enemies[i].GetComponent<AIAgent>().GetCurrentHealth();
            }
            else
            {
                enemiesDistance[i] = 1000f;
                enemiesAngle[i] = 1000f;
                enemiesHealth[i] = 1000f;
            }
        }

        for(int i = 0; i < allies.Length; i++)
        {
            if(allies[i] != null)
            {
                alliesDistance[i] = Vector3.Distance(transform.position, allies[i].transform.position);
                alliesTargetDirAngle[i] = allies[i].transform.position - transform.position;
                alliesAngle[i] = Vector3.Angle(transform.forward, alliesTargetDirAngle[i]);
                alliesHealth[i] = allies[i].GetComponent<AIAgent>().GetCurrentHealth();
            }
            else
            {
                alliesDistance[i] = 1000f;
                alliesAngle[i] = 1000f;
                alliesHealth[i] = 1000f;
            }  
        }

        if(allyOne != null)
        {
            // check closeness on X
          
            if(fightAsTeam)
            {
                if(Mathf.Abs(allyOne.transform.position.x - transform.position.x) > 1.5f && Mathf.Abs(allyOne.transform.position.x - transform.position.x) <= 5f 
                    || Mathf.Abs(allyOne.transform.position.x - transform.position.x) < -1.5f && Mathf.Abs(allyOne.transform.position.x - transform.position.x) >= -5f)
                {
                    inTheSweetSpotAllyOneX = true;
                    toCloseToAllyOneX = false;
                    toFarAwayFromAllyOneX = false;
                }
                else
                {
                    
                    if(!toCloseToAllyOneX)
                    {
                        toFarAwayFromAllyOneX = true;
                    }
                    else
                    {
                        toFarAwayFromAllyOneX = false;
                    }
                    inTheSweetSpotAllyOneX = false;
                    toCloseToAllyOneX = false;
                }
            }
            if(Mathf.Abs(allyOne.transform.position.x - transform.position.x) <= 1.5f && Mathf.Abs(allyOne.transform.position.x - transform.position.x) >= -1.5f)
            {
                inTheSweetSpotAllyOneX = false;
                toCloseToAllyOneX = true;
                toFarAwayFromAllyOneX = false;
            }
            // check closeness of enemies on Z.
            
            if(fightAsTeam)
            {
                if(Mathf.Abs(allyOne.transform.position.z - transform.position.z) > 1.5f && Mathf.Abs(allyOne.transform.position.z - transform.position.z) <= 5f
                    || Mathf.Abs(allyOne.transform.position.z - transform.position.z) < -1.5f && Mathf.Abs(allyOne.transform.position.z - transform.position.z) >= -5f)
                {
                    inTheSweetSpotAllyOneZ = true;
                    toCloseToAllyOneZ = false;
                    toFarAwayFromAllyOneZ = false;
                }
                else
                {
                    if(!toCloseToAllyOneZ)
                    {
                        toFarAwayFromAllyOneZ = true;
                    }
                    else
                    {
                        toFarAwayFromAllyOneZ = false;
                    }
                    inTheSweetSpotAllyOneZ = false;
                    toCloseToAllyOneZ = false;
                }
            }
            if(Mathf.Abs(allyOne.transform.position.x - transform.position.x) <= 1.5f && Mathf.Abs(allyOne.transform.position.x - transform.position.x) >= -1.5f)
            {
                inTheSweetSpotAllyOneZ = false;
                toCloseToAllyOneZ = true;
                toFarAwayFromAllyOneZ = false;
            }

        } 
        if(allyTwo != null)
        {

            if(fightAsTeam)
            {
                if(Mathf.Abs(allyTwo.transform.position.x - transform.position.x) > 1.5f && Mathf.Abs(allyTwo.transform.position.x - transform.position.x) <= 5f
                    || Mathf.Abs(allyTwo.transform.position.x - transform.position.x) < -1.5f && Mathf.Abs(allyTwo.transform.position.x - transform.position.x) >= -5f)
                {
                    inTheSweetSpotAllyTwoX = true;
                    toCloseToAllyTwoX = false;
                    toFarAwayFromAllyTwoX = false;
                }
                else
                {
                    if(!toCloseToAllyTwoX)
                    {
                        toFarAwayFromAllyTwoX = true;
                    }
                    else
                    {
                        toFarAwayFromAllyTwoX = false;
                    }
                    inTheSweetSpotAllyTwoX = false;
                    toCloseToAllyTwoX = false;
                }
            }

            if(Mathf.Abs(allyTwo.transform.position.x - transform.position.x) <= 1.5f && Mathf.Abs(allyTwo.transform.position.x - transform.position.x) >= -1.5f)
            {
                inTheSweetSpotAllyTwoX = false;
                toCloseToAllyTwoX = true;
                toFarAwayFromAllyTwoX = false;
            }
            // check closeness of enemies on Z.

            if(fightAsTeam)
            {
                if(Mathf.Abs(allyTwo.transform.position.z - transform.position.z) > 1.5f && Mathf.Abs(allyTwo.transform.position.z - transform.position.z) <= 5f
                    || Mathf.Abs(allyTwo.transform.position.z - transform.position.z) < -1.5f && Mathf.Abs(allyTwo.transform.position.z - transform.position.z) >= -5f)
                {
                    inTheSweetSpotAllyTwoZ = true;
                    toCloseToAllyTwoZ = false;
                    toFarAwayFromAllyTwoZ = false;
                }
                else
                {
                    if(!toCloseToAllyTwoZ)
                    {
                        toFarAwayFromAllyTwoZ = true;
                    }
                    else
                    {
                        toFarAwayFromAllyTwoZ = false;
                    }
                    inTheSweetSpotAllyTwoZ = false;
                    toCloseToAllyTwoZ = false;
                }
            }
            
            if(Mathf.Abs(allyTwo.transform.position.z - transform.position.z) <= 1.5f && Mathf.Abs(allyTwo.transform.position.z - transform.position.z) >= -1.5f)
            {
                inTheSweetSpotAllyTwoZ = false;
                toCloseToAllyTwoZ = true;
                toFarAwayFromAllyTwoZ = false;
            }
        }

        prevTargetDistance = targetDistance;
        prevTargetAngle = targetAngle;
    }
    #endregion

    #region Getters and Setters
    // Getters and Setters.

    // honestly just going leave this comment, lots of getters and setters to setup all the variables on these agents.
    public void SetGameObjectThatHit(GameObject enemy)
    {
        for(int i = 0; i < enemiesThatHitMe.Length; i++)
        {
            if(enemiesThatHitMe[i] == null)
            {
                enemiesThatHitMe[i] = enemy;
                break;
            }
        }
    }
    public void SetDamageMod(float _mod)
    {
        currentDmgMod = _mod;
    }
    public float GetDamage()
    {
        return damage;
    }
    public void SetDamage(float _damage)
    {
        damage = _damage;
    }
    public float DamageCalculation()
    {
        damage = 100f;

        if(useRatingSystem)
        {
            float bonusDamage = damage * damageModifier;
            damage += damageModifier;
        }

        return damage;
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    public float GetCurrentStamina()
    {
        return currentStamina;
    }
    public bool GetSwinging()
    {
        return swinging;
    }
    public void SetSwingMiss(bool miss)
    {
        swingMissed = miss;
    }
    public Transform GetSpearPos()
    {
        return agentSpearPoint;
    }
    public Transform GetShieldPos()
    {
        return agentShieldCentrePoint;
    }
    public LayerMask GetEnemyLayerMask()
    {
        return enemyLayer;
    }
    public LayerMask GetAllyLayerMask()
    {
        return allyLayer;
    }
    public string GetWeaponTag()
    {
        return weaponTag;
    }
    public string GetShieldTag()
    {
        return shieldTag;
    }
    public void SetWeaponInterpolate(float _change)
    {
        spearInterpolation = 1.0f;
    }
    public void DealDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
    }
    public void SetSwingHit(bool hit)
    {
        hitEnemy = hit;
    }
    public void SetTargetToNull()
    {
        target =  null;
    }
    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }
    public GameObject GetTarget()
    {
        return target;
    }
    public void SetTargetIsDead(bool isDead)
    {
        targetAlive = false;
    }
    #endregion
}
