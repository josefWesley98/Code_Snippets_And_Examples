
       
        private void ActionMethod()
        {
            // handles the bulk of setting up and triggering the FuzzyLogic methods.
            orders = GetHighestPriorityOrder();
            float goalOutput = 0f;
            //float dynamicChange = 0f;
        
            //.x is health, .y is stamina
            Vector2 averages = new Vector2();
            Vector2 enemyAverages = new Vector2();
            float averageDistanceToEnemies = 0;
            float zoneDefenseRating = 0; 
            Transform nearestExplorationPoint;
            HashSet<Transform> enemiesInSight = agentStimuli.GetEnemiesInSight();
            HashSet<Transform> alliesInSight = agentStimuli.GetAlliesInSight();
            HashSet<Transform> currentGroupTransforms = new HashSet<Transform>();
            int enemiesInSightCount = agentStimuli.GetEnemiesInSight().Count;
            int alliesInSightCount = agentStimuli.GetAlliesInSight().Count;
            int allyEnemyDiff = alliesInSightCount - enemiesInSightCount;
            currentGroupTransforms = new HashSet<Transform>();
            foreach (var member in currentGroup.unitsInSquad)
            {
                currentGroupTransforms.Add(overMind.GetASpawnedAI(member));
            }
            FuzzyLogicData fuzzyData = new FuzzyLogicData()
            {
                fuzzyNum = 0.5f
            };
            idle = false;
            
            
            // gets values and checks for nulls then proccesses the data to fuzzfify it then proccess the data and pass it through the inference then its returned as the goal ouput and passed to the logic base to enact the action.
            switch (orders)
            {
                case Orders.None:
                    // this is an idle state as the AI prepare to move to the next state.
                    agentAnimationManager.RemoveOtherAnimationsAndAddAniamtion(PlayableAnimations.ShieldBraceStationary, 0.5f, true);
                    idle = true;
                    break;
                case Orders.DefendLocation:
                
                    Profiler.BeginSample("DefendLocation");
                    averages = GetAverageAllyHealthAndStamina(currentGroupTransforms);
                    nearestExplorationPoint = FindNearestExplorationPoint();
                    zoneDefenseRating = agentStimuli.GetZoneDefenseZoneRating(nearestExplorationPoint);
                
                    if (zoneDefenseRating == 0)
                    {
                        zoneDefenseRating = 0.5f;
                    }
                    if(averages.x == 0)
                    {
                        averages.x = currentHealth / 2;
                    }

                    if (averages.y == 0)
                    {
                        averages.y = currentStamina / 2;
                    }

                    if (allyEnemyDiff == 0)
                    {
                        allyEnemyDiff = 3;
                    }
                
                    goalOutput = ProccessFuzzyLogicForDefendLocation(allyEnemyDiff, currentHealth, currentStamina,
                        averages.x, averages.y, zoneDefenseRating);
                
                
                    fuzzyData.fuzzyNum = goalOutput;
                    if (activeBehaviours.ContainsKey("Defend_Location"))
                    {
                        activeBehaviours["Defend_Location"](fuzzyData);
                    }
                    Profiler.EndSample();
                    break;
                case Orders.AttackLocation:
                
                    Profiler.BeginSample("AttackLocation");
                    averages = GetAverageAllyHealthAndStamina(currentGroupTransforms);
                    nearestExplorationPoint = FindNearestExplorationPoint();
                    zoneDefenseRating = agentStimuli.GetZoneDefenseZoneRating(nearestExplorationPoint);
                    averageDistanceToEnemies = GetAverageDistanceOfGroupFromThisAI(enemiesInSight);
                
                    goalOutput = ProccessFuzzyLogicForAttackLocation(currentHealth, currentStamina, 
                        averages.x, averages.y, alliesInSightCount, zoneDefenseRating, 
                        averageDistanceToEnemies, allyEnemyDiff, 0.5f, 0.5f);
                
                    Debug.Log($"Attack location output: {goalOutput}");
                    fuzzyData.fuzzyNum = goalOutput;
                
                    if (activeBehaviours.ContainsKey("Attack_Location"))
                    {
                        activeBehaviours["Attack_Location"](fuzzyData);
                    }
                    Profiler.EndSample();
                    break;
                case Orders.EliminateEnemies:
                    
                    Profiler.BeginSample("Eliminate");
                    averages = GetAverageAllyHealthAndStamina(currentGroupTransforms);
                    averageDistanceToEnemies = GetAverageDistanceOfGroupFromThisAI(enemiesInSight);
                
                    goalOutput = ProccessFuzzyLogicForEliminateEnemies(this.currentHealth, this.currentStamina,
                        allyEnemyDiff, currentGroup.unitsInSquad.Count, averages.x, averages.y, averageDistanceToEnemies);
                    
                
                    fuzzyData.fuzzyNum = goalOutput;
                    //Eliminate_Enemies 
                    //Eliminate_Enemies
                    if (activeBehaviours.ContainsKey("Eliminate_Enemies"))
                    {
                        activeBehaviours["Eliminate_Enemies"](fuzzyData);
                    }
                    Profiler.EndSample();
                    break;
                case Orders.ExploreLocation:
                    if (rank != Rank.Private)
                    {
                        Profiler.BeginSample("ExploreLocation");
                      
                        nearestExplorationPoint = FindNearestExplorationPoint();
                        float distanceToFallBack = Vector3.Distance(transform.position, agentStimuli.GetLastAlliesPosition());
                        float maxExplorationRange = agentMovement.GetMaxiumExplorationRange();
                        float dangerLevel = agentStimuli.GetQuadrantDangerLevel(nearestExplorationPoint);
                        float percentQuadrantExplored = agentMovement.GetPercentOfMapExplored();
                    
                        goalOutput = 1.0f;
                    
                        fuzzyData.fuzzyNum = goalOutput;
                    
                        if (activeBehaviours.ContainsKey("Explore_Location"))
                        {
                            activeBehaviours["Explore_Location"](fuzzyData);
                        }
                        Profiler.EndSample();
                    }
               
                    break;
                case Orders.TravelToLocation:
                
                    Profiler.BeginSample("TravelToLocation");
                    float averageDangerLevel = 0.5f;//agentStimuli.GetAverageDangerLevelBetweenTwoPoints(quadrantsOnWay);
                    float distanceToFinalDestination = Vector3.Distance(travelToLocationPoints.First(), travelToLocationPoints.Last());
                    averages = GetAverageAllyHealthAndStamina(currentGroupTransforms);
                
                    goalOutput = ProccessFuzzyLogicForTravellingToLocation(alliesInSightCount, averages.y, averageDangerLevel, distanceToFinalDestination);
                
                    fuzzyData.fuzzyNum = goalOutput;
                    if (rank != Rank.Corporal)
                    {
                        DoTeamFormationAroundLeader(GetTravelLocation());
                    }
                    
                
                    if (activeBehaviours.ContainsKey("Move_To_Destination"))
                    {
                        activeBehaviours["Move_To_Destination"](fuzzyData);
                    }
                    Profiler.EndSample();
                    break;
                case Orders.FlankEnemies:
                    
                    Profiler.BeginSample("FlankEnemies");
                    averages = GetAverageAllyHealthAndStamina(currentGroupTransforms);
                    float dist = agentMovement.GetDistanceBetweenWaypointAndCurrentPos(transform.position);
                    goalOutput = ProccessFuzzyLogicForFlankEnemies(this.currentStamina, alliesInSightCount, GetAverageDistanceOfGroupFromThisAI(enemiesInSight), 
                        dist, averages.y, (dist * 0.1f), (alliesInSightCount - enemiesInSightCount),
                        agentCombat.GetCurrentDangerLevel());
                    Debug.Log($"Flank Enemies output: {goalOutput}");
                
                    fuzzyData.fuzzyNum = goalOutput;
                    if (activeBehaviours.ContainsKey("Flank_Enemies"))
                    {
                        activeBehaviours["Flank_Enemies"](fuzzyData);
                    }
                    //logicBase.FlankEnemiesVarients(goalOutput);
                    Profiler.EndSample();
                    break;
                case Orders.RetreatFromEnemies:
                    
                    SetOrderFinished(Orders.RetreatFromEnemies, true);
                    averages = GetAverageAllyHealthAndStamina(currentGroupTransforms);
                    enemyAverages = GetAverageAllyHealthAndStamina(enemiesInSight);
                
                    goalOutput = ProccessFuzzyLogicForRetreatFromEnemies(this.currentHealth, this.currentStamina, (alliesInSightCount - enemiesInSightCount),
                        alliesInSightCount, enemiesInSightCount, agentCombat.GetCurrentDefenseZoneRating(), averages.y, averages.x,
                        enemyAverages.x, agentCombat.GetCurrentDangerLevel());
                
                    Debug.Log($"Retreat From Enemies output: {goalOutput}");
                
                    fuzzyData.fuzzyNum = goalOutput;
                    if (activeBehaviours.ContainsKey("Retreat_From_Enemies"))
                    {
                        activeBehaviours["Retreat_From_Enemies"](fuzzyData);
                    }
                    //logicBase.RetreatFromEnemiesVarients(goalOutput);
                
                    break;
                case Orders.ChargeEnemies:
                    Profiler.BeginSample("ChargeEnemies");
                    averages = GetAverageAllyHealthAndStamina(currentGroupTransforms);
                    int allyEnemydiff = (alliesInSightCount - enemiesInSightCount);
             
                    goalOutput = ProccessFuzzyLogicForChargeEnemies(this.currentHealth, this.currentStamina, averages.x,
                        alliesInSightCount,averages.y, allyEnemydiff
                        , GetAverageDistanceOfGroupFromThisAI(enemiesInSight));
                
                    Debug.Log($"Charge Enemies output: {goalOutput}");
                
                    fuzzyData.fuzzyNum = goalOutput;
                    if (activeBehaviours.ContainsKey("Charge_Enemies"))
                    {
                        activeBehaviours["Charge_Enemies"](fuzzyData);
                    }
                    //logicBase.ChargeEnemiesVarients(goalOutput);
                    Profiler.EndSample();
                    break;
                case Orders.FormUp:
                    
                    if (agentMovement.CheckIfArrivedAtDestination() && rank != Rank.Corporal && formingUpInProgress)
                    {
                        agentAnimationManager.RemoveOtherAnimationsAndAddAniamtion(PlayableAnimations.ShieldBraceStationary, 0.35f, false);
                        TriggerSetOrdersFinishedServerRpc(Orders.FormUp, true);
                        TriggerAddOrderServerRpc(Orders.None, 0.5f, true, id);
                        SetFinishedFormingUp();
                        return;
                    }
                
                    if (formationInUse != formation)
                    {
                        SetFormingUpInProgress(false); 
                    }
                    Profiler.BeginSample("Form Up");
                    if (!formingUpInProgress)
                    {
                        doRunFormation = false;
                        runStateSet = false;
                        formationInUse = formation;
                        if (rank == Rank.Private)
                        {
                            formingUpInProgress = true;
                            return;
                        }

                        formingUpInProgress = true;
                    
                        int currentTopCandidateId = id;
                        Rank currentCandidateRank = rank;
                        if (partiedGroups.Any())
                        {
                            foreach (var pGroup in partiedGroups)
                            {
                                foreach (var agent in pGroup.unitsInSquad)
                                {
                                    Transform agentTransform = overMind.GetASpawnedAI(agent);
                                    Rank contenderRank = agentTransform.GetComponent<FuzzyLogicAgent>().GetRank();
                                
                                    if (CompareRank(currentCandidateRank, contenderRank) == RankComparison.Lower)
                                    {
                                        currentTopCandidateId = agent;
                                        currentCandidateRank = contenderRank;
                                    }

                                    if (!doRunFormation)
                                    {
                                        AgentStimuli agentStimRef = GetAIAgentFromOverMind(agent).GetComponent<AgentStimuli>();
                                        List<Transform> agentsSeenEnemies = HashSetToList(agentStimRef.GetEnemiesInSight());
                                        if (agentsSeenEnemies.Any())
                                        {
                                            doRunFormation = true;
                                        }
                                    }
                                }
                                foreach (var member in group.unitsInSquad)
                                {
                                
                                }
                            }

                            foreach (var agent in currentGroup.unitsInSquad)
                            {
                                Transform agentTransform = overMind.GetASpawnedAI(agent);
                                Rank contenderRank = agentTransform.GetComponent<FuzzyLogicAgent>().GetRank();
                                if (CompareRank(currentCandidateRank, contenderRank) == RankComparison.Lower)
                                {
                                    currentTopCandidateId = agent;
                                    currentCandidateRank = contenderRank;
                                }
                            }
                        }
                        else
                        {
                            foreach (var agent in currentGroup.unitsInSquad)
                            {
                                Transform agentTransform = overMind.GetASpawnedAI(agent);
                                if (agentTransform == null) return;
                                Rank contenderRank = agentTransform.GetComponent<FuzzyLogicAgent>().GetRank();
                                if (CompareRank(currentCandidateRank, contenderRank) == RankComparison.Lower)
                                {
                                    currentTopCandidateId = agent;
                                    currentCandidateRank = contenderRank;
                                }
                            }
                        }
                        overMind.GetASpawnedAI(currentTopCandidateId).GetComponent<FuzzyLogicAgent>().SetSelectedToInitiateToFromUp();
                        SetFormingUpInProgress(true);
                    }


                    if (selectedToInitiateToFromUp)
                    {
                        if (!runStateSet)
                        {
                            if (!partiedGroups.Any())
                            {
                                foreach (var agent in group.unitsInSquad)
                                {
                                    AgentStimuli agentStimRef = GetAIAgentFromOverMind(agent).GetComponent<AgentStimuli>();
                                    List<Transform> agentsSeenEnemies = HashSetToList(agentStimRef.GetEnemiesInSight());
                                    if (agentsSeenEnemies.Any())
                                    {
                                        doRunFormation = true;
                                        break;
                                    }
                                }
                            }
                            if (doRunFormation)
                            {
                                if (partiedGroups.Any())
                                {
                                    foreach (var pGroup in partiedGroups)
                                    {
                                        foreach (var agent in pGroup.unitsInSquad)
                                        {
                                            AgentAnimationManager animationRef = overMind.GetASpawnedAI(agent).GetComponent<AgentAnimationManager>();
                                            AgentMovement movementRef = overMind.GetASpawnedAI(agent).GetComponent<AgentMovement>();
                                            movementRef.SetMovementState(MovementType.Run);
                                            animationRef.AddAnimation(PlayableAnimations.Sprint, 0.65f, true);
                                        }
                                    }
                                    runStateSet = true;
                                }
                                else
                                {
                                    foreach (var agent in group.unitsInSquad)
                                    {
                                        AgentAnimationManager animationRef = overMind.GetASpawnedAI(agent).GetComponent<AgentAnimationManager>();
                                        AgentMovement movementRef = overMind.GetASpawnedAI(agent).GetComponent<AgentMovement>();
                                        movementRef.SetMovementState(MovementType.Run);
                                        animationRef.RemoveOtherAnimationsAndAddAniamtion(PlayableAnimations.Sprint, 0.65f, true);
                                    }

                                    runStateSet = true;
                                }
                            }
                            else
                            {
                                if (partiedGroups.Any())
                                {
                                    foreach (var pGroup in partiedGroups)
                                    {
                                        foreach (var agent in pGroup.unitsInSquad)
                                        {
                                            AgentMovement movementRef = overMind.GetASpawnedAI(agent).GetComponent<AgentMovement>();
                                            movementRef.SetMovementState(MovementType.WalkForward);
                                            agentAnimationManager.RemoveOtherAnimationsAndAddAniamtion(PlayableAnimations.WalkForwards, 0.4f, true);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var agent in group.unitsInSquad)
                                    {
                                        AgentMovement movementRef = overMind.GetASpawnedAI(agent).GetComponent<AgentMovement>();
                                        movementRef.SetMovementState(MovementType.WalkForward);
                                        agentAnimationManager.RemoveOtherAnimationsAndAddAniamtion(PlayableAnimations.WalkForwards, 0.4f, true);
                                    }
                                }
                            }
                            runStateSet = true;
                        }
                    
                        bool finishedOrganising = true;
                        if (partiedGroups.Any())
                        {
                            foreach (var pGroup in partiedGroups)
                            {
                                foreach (var agent in pGroup.unitsInSquad)
                                {
                                    FuzzyLogicAgent agentRef = overMind.GetASpawnedAI(agent).GetComponent<FuzzyLogicAgent>();
                                    if (agentRef.GetHighestPriorityOrder() != Orders.Rest ||
                                        agentRef.GetHighestPriorityOrder() != Orders.EliminateEnemies)
                                    {
                                        finishedOrganising = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var agent in group.unitsInSquad)
                            {
                                FuzzyLogicAgent agentRef = overMind.GetASpawnedAI(agent).GetComponent<FuzzyLogicAgent>();
                                if (agentRef.GetHighestPriorityOrder() != Orders.Rest ||
                                    agentRef.GetHighestPriorityOrder() != Orders.EliminateEnemies)
                                {
                                    finishedOrganising = false;
                                }
                            
                            }
                        }

                        if (finishedOrganising)
                        {
                            Debug.Log($"Group In Position");
                            agentAnimationManager.RemoveOtherAnimationsAndAddAniamtion(PlayableAnimations.ShieldBraceStationary, 0.35f, false);
                            corpReadyForOrders = true;
                            SetFinishedFormingUp();
                        }
                    
                        switch (formation)
                        {
                            case Formation.LinesAndRows:
                                FindFormUpPosition1();
                                FormLinesAndRows(formUpPosition1, 3, 5, 2.0f,2.5f, null, null);
                                break;
                            case Formation.Box:
                                FindFormUpPosition1();
                                FormBox(formUpPosition1, 15.0f, 1.5f, null, null);
                                break;
                            case Formation.Circle:
                                FindFormUpPosition1();
                                FormCircle(formUpPosition1, 15.0f, 1.5f, null, null, 0);
                                break;
                            case Formation.TwoGroups:
                                FindFormUpPosition1();
                                FindFormUpPosition2();
                                FormTwoGroups(formUpPosition1, formUpPosition2, secondGroupFormation, 10, 5, 1.5f, 5.0f, 15.0f);
                                break;
                            default:
                                break;
                        }
                   
                    }

                
                    Profiler.EndSample();
                
                    break;
                case Orders.PatrolArea:
                    Profiler.BeginSample("PatrolArea");
                    goalOutput = ProccessFuzzyLogicForPatrolArea(this.currentHealth, this.currentStamina, alliesInSightCount, agentCombat.GetCurrentDangerLevel(),
                        Vector3.Distance(transform.position, patrolPoints[GetCurrentPatrolPoint()]));
                
                    Debug.Log($"Patrol Area output: {goalOutput}");
                
                    fuzzyData.fuzzyNum = goalOutput;
                    if (activeBehaviours.ContainsKey("Patrol_Area"))
                    {
                        activeBehaviours["Patrol_Area"](fuzzyData);
                    }
                    Profiler.EndSample();
                    break;
                case Orders.Rest:
                    if (Time.time - lastRestUpdate >= 3.0f)
                    {
                        RegenStamina(3.0f);
                        lastRestUpdate = Time.time;
                    }
                    agentMovement.SetMovementState(MovementType.None);
                    break;
                case Orders.RunForHelp:
                    agentMovement.ReturnToHomeLocation();
                
                    break;
            }
            Profiler.EndSample();
        }

  
        #region MainLogic
    
        // this is all the main logic where we handle the fuzzification, by doing parrellel multithreading.
        // each of these functions are larges the same other than the variables being used and the functions used to fuzzify.
        private float ProccessFuzzyLogicForDefendLocation(int diffEnemiesAliies, float health, float stamina, 
            float averageAllyHealth, float averageAllyStamina, float areaDefenseRating)
        {
            FuzzyVariable numDiffEnemiesAlliesFuzzy = null;
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable groupAverageHealthFuzzy = null;
            FuzzyVariable groupAverageStaminaFuzzy = null;
            FuzzyVariable areaDefenseRatingFuzzy = null;

            Parallel.Invoke(
                () => { numDiffEnemiesAlliesFuzzy = fuzzificationBase.FuzzifyDiffEnemiesAllies(diffEnemiesAliies); },
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { groupAverageHealthFuzzy = fuzzificationBase.FuzzifyHealth(averageAllyHealth); },
                () => { groupAverageStaminaFuzzy = fuzzificationBase.FuzzifyStamina(averageAllyStamina); },
                () => { areaDefenseRatingFuzzy = fuzzificationBase.FuzzifyDefenseRating(areaDefenseRating); }
            );
            float output = inferenceBase.EvaluateDefendLocationRules(numDiffEnemiesAlliesFuzzy, healthFuzzy, staminaFuzzy,
                groupAverageHealthFuzzy, groupAverageStaminaFuzzy, areaDefenseRatingFuzzy);

            return defuzzificationBase.Defuzzify(output);
        }
    
        private float ProccessFuzzyLogicForAttackLocation(float health, float stamina, float averageAllyHealth,
            float averageAllyStamina, int allyGroupSize, float defenseRatingOfZone, float distanceToTarget,
            int diffEnemiesAliies, float enemiesInFormation, float alliesInFormation)
        {
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable groupAverageHealthFuzzy = null;
            FuzzyVariable groupAverageStaminaFuzzy = null;
            FuzzyVariable areaDefenseRatingFuzzy = null;
            FuzzyVariable distanceFromTargetFuzzy = null;
            FuzzyVariable numDiffEnemiesAlliesFuzzy = null;
            FuzzyVariable enemyFormationAccuracyFuzzy = null;
            FuzzyVariable allyFormationAccuracyFuzzy = null;

            Parallel.Invoke(
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { groupAverageHealthFuzzy = fuzzificationBase.FuzzifyHealth(averageAllyHealth); },
                () => { groupAverageStaminaFuzzy = fuzzificationBase.FuzzifyStamina(averageAllyStamina); },
                () => { areaDefenseRatingFuzzy = fuzzificationBase.FuzzifyDefenseRating(defenseRatingOfZone); },
                () => { distanceFromTargetFuzzy = fuzzificationBase.FuzzifyPosition(distanceToTarget); },
                () => { numDiffEnemiesAlliesFuzzy = fuzzificationBase.FuzzifyDiffEnemiesAllies(diffEnemiesAliies); },
                () => { enemyFormationAccuracyFuzzy = fuzzificationBase.FuzzifyFormationAccuracy(enemiesInFormation); },
                () => { allyFormationAccuracyFuzzy = fuzzificationBase.FuzzifyFormationAccuracy(alliesInFormation); }
            );

            float output = inferenceBase.EvaluateAttackLocationRules(healthFuzzy, staminaFuzzy, groupAverageHealthFuzzy,
                groupAverageStaminaFuzzy, areaDefenseRatingFuzzy, distanceFromTargetFuzzy, numDiffEnemiesAlliesFuzzy,
                enemyFormationAccuracyFuzzy, allyFormationAccuracyFuzzy);

            return defuzzificationBase.DefuzzifyNew(output);
        }
    
        private float ProccessFuzzyLogicForEliminateEnemies(float health, float stamina, int diffEnemiesAllies,
            int allyGroupSize, float averageAllyHealth, float averageAllyStamina, float distanceToEnemy)
        {
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable numDiffEnemiesAlliesFuzzy = null;
            FuzzyVariable distanceFromTargetFuzzy = null;
            FuzzyVariable averageAllyHealthFuzzy = null;
            FuzzyVariable averageAllyStaminaFuzzy = null;

            Parallel.Invoke(
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { numDiffEnemiesAlliesFuzzy = fuzzificationBase.FuzzifyDiffEnemiesAllies(diffEnemiesAllies); },
                () => { distanceFromTargetFuzzy = fuzzificationBase.FuzzifyPosition(distanceToEnemy); },
                () => { averageAllyHealthFuzzy = fuzzificationBase.FuzzifyHealth(averageAllyHealth); },
                () => { averageAllyStaminaFuzzy = fuzzificationBase.FuzzifyStamina(averageAllyStamina); }
            );

            float output = inferenceBase.EvaluateEliminateEnemiesRules(healthFuzzy, staminaFuzzy, numDiffEnemiesAlliesFuzzy,
                distanceFromTargetFuzzy, averageAllyHealthFuzzy, averageAllyStaminaFuzzy);
    
            return defuzzificationBase.DefuzzifyNew(output);
        }
    
        private float ProccessFuzzyLogicForExploreLocation(float stamina, float distanceToExplore, float dangerLevel,
            float percentOfAreaScouted, float distanceToFallbackPoint)
        {
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable distanceToExploreFuzzy = null;
            FuzzyVariable percentOfAreaScoutedFuzzy = null;
            FuzzyVariable dangerLevelFuzzy = null;
            FuzzyVariable distacneToFallbackPointFuzzy = null;

            Parallel.Invoke(
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { distanceToExploreFuzzy = fuzzificationBase.FuzzifyPosition(distanceToExplore); },
                () => { percentOfAreaScoutedFuzzy = fuzzificationBase.FuzzifyScoutedAreaPercent(percentOfAreaScouted); },
                () => { dangerLevelFuzzy = fuzzificationBase.FuzzifyDangerLevel(dangerLevel); },
                () => { distacneToFallbackPointFuzzy = fuzzificationBase.FuzzifyPosition(distanceToFallbackPoint); }
            );

            float output = inferenceBase.EvaluateExploreLocationRules(staminaFuzzy, distanceToExploreFuzzy, percentOfAreaScoutedFuzzy,
                dangerLevelFuzzy, distacneToFallbackPointFuzzy);

            return defuzzificationBase.DefuzzifyNew(output);
        }
    
        private float ProccessFuzzyLogicForScoutEnemies(int allyGroupSize, int diffEnemiesAliies,
            int estimatedEnemyCount, float stamina, float health, float dangerLevel, float distanceToExplore)
        {
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable numDiffEnemiesAlliesFuzzy = null;
            FuzzyVariable dangerLevelFuzzy = null;
            FuzzyVariable distanceToExploreFuzzy = null;
            FuzzyVariable estimatedEnemyCountFuzzy = null;
            FuzzyVariable groupSizeFuzzy = null;

            Parallel.Invoke(
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { numDiffEnemiesAlliesFuzzy = fuzzificationBase.FuzzifyDiffEnemiesAllies(diffEnemiesAliies); },
                () => { dangerLevelFuzzy = fuzzificationBase.FuzzifyDangerLevel(dangerLevel); },
                () => { distanceToExploreFuzzy = fuzzificationBase.FuzzifyPosition(distanceToExplore); },
                () => { estimatedEnemyCountFuzzy = fuzzificationBase.FuzzifyNumEnemies(estimatedEnemyCount); },
                () => { groupSizeFuzzy = fuzzificationBase.FuzzifyNumAllies(allyGroupSize); }
            );

            float output = inferenceBase.EvaluateScoutEnemiesRules(healthFuzzy, staminaFuzzy, numDiffEnemiesAlliesFuzzy,
                dangerLevelFuzzy, distanceToExploreFuzzy, estimatedEnemyCountFuzzy, groupSizeFuzzy);
        
            return defuzzificationBase.DefuzzifyNew(output);
        }
    
        private float ProccessFuzzyLogicForTravellingToLocation(int allyGroupSize,
            float stamina, float dangerLevel, float distanceToLocation)
        {
            FuzzyVariable distanceToLocationFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable dangerLevelFuzzy = null;
            FuzzyVariable groupSizeFuzzy = null;

            Parallel.Invoke(
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { dangerLevelFuzzy = fuzzificationBase.FuzzifyDangerLevel(dangerLevel); },
                () => { distanceToLocationFuzzy = fuzzificationBase.FuzzifyDistanceToTravel(distanceToLocation); },
                () => { groupSizeFuzzy = fuzzificationBase.FuzzifyNumAllies(allyGroupSize); }
            );
      
        
            float output = inferenceBase.EvaluateTravelToLocationRules(staminaFuzzy, groupSizeFuzzy, dangerLevelFuzzy,
                distanceToLocationFuzzy);
       
            return defuzzificationBase.Defuzzify(output);
        }

        private float ProccessFuzzyLogicForSetAmbush(float health, float stamina, int expectedEnemyCount,
            int allyGroupSize, float averageHideRating, int diffEnemiesAliies,
            float groupAverageHealth, float groupAverageStamina)
        {
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable groupSizeFuzzy = null;
            FuzzyVariable numDiffEnemiesAlliesFuzzy = null;
            FuzzyVariable groupAverageHealthFuzzy = null;
            FuzzyVariable groupAverageStaminaFuzzy = null;
            FuzzyVariable expectedEnemyCountFuzzy = null;
            FuzzyVariable averageHideRatingfuzzy = null;

            Parallel.Invoke(
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { groupSizeFuzzy = fuzzificationBase.FuzzifyNumAllies(allyGroupSize); },
                () => { numDiffEnemiesAlliesFuzzy = fuzzificationBase.FuzzifyDiffEnemiesAllies(diffEnemiesAliies); },
                () => { groupAverageHealthFuzzy = fuzzificationBase.FuzzifyHealth(groupAverageHealth); },
                () => { groupAverageStaminaFuzzy = fuzzificationBase.FuzzifyStamina(groupAverageStamina); },
                () => { expectedEnemyCountFuzzy = fuzzificationBase.FuzzifyNumEnemies(expectedEnemyCount); },
                () => { averageHideRatingfuzzy = fuzzificationBase.FuzzifyHideRating(averageHideRating); }
            );

            float output = inferenceBase.EvaluateSetAmbushRules(healthFuzzy, staminaFuzzy, groupSizeFuzzy, numDiffEnemiesAlliesFuzzy,
                groupAverageHealthFuzzy, groupAverageStaminaFuzzy, expectedEnemyCountFuzzy, averageHideRatingfuzzy);

            return defuzzificationBase.DefuzzifyNew(output);
        }
    
        private float ProccessFuzzyLogicForFlankEnemies(float stamina, int allyGroupSize,
            float distanceFromEnemy, float distanceToTravelToFlank, float averageStamina, float staminaCost,
            int diffEnemiesAliies, float dangerLevel)
        {
            FuzzyVariable distanceFromEnemyFuzzy = fuzzificationBase.FuzzifyPosition(distanceFromEnemy);
            FuzzyVariable distanceToTravelFuzzy = fuzzificationBase.FuzzifyDistanceToTravel(distanceToTravelToFlank);
            FuzzyVariable averageStaminaFuzzy = fuzzificationBase.FuzzifyStamina(averageStamina);
            FuzzyVariable staminaFuzzy =  fuzzificationBase.FuzzifyStamina(stamina);
            FuzzyVariable StaminaCostFuzzy = fuzzificationBase.FuzzifyStaminaCost(staminaCost);
            FuzzyVariable DiffEnemyiesAlliesFuzzy = fuzzificationBase.FuzzifyDiffEnemiesAllies(diffEnemiesAliies);
            FuzzyVariable dangerLevelFuzzy = fuzzificationBase.FuzzifyDangerLevel(dangerLevel);

            float output = inferenceBase.EvaluateFlankEnemiesRules(distanceFromEnemyFuzzy,
                distanceToTravelFuzzy, averageStaminaFuzzy, staminaFuzzy, StaminaCostFuzzy, DiffEnemyiesAlliesFuzzy,
                dangerLevelFuzzy);

            return defuzzificationBase.DefuzzifyNew(output);
        }
    
        private float ProccessFuzzyLogicForSurroundEnemies(float health, float stamina, int allyGroupSize,
            int enemyGroupSize, float averageAllyHealth, float averageEnemyHealth, float averageAllyStamina,
            float averageEnemyStamina, float formationAccuracy)
        {
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable groupSizeFuzzy = null;
            FuzzyVariable expectedEnemyCountFuzzy = null;
            FuzzyVariable averageAllyHealthFuzzy = null;
            FuzzyVariable averageAllyStaminaFuzzy = null;
            FuzzyVariable averageEnemyHealthFuzzy = null;
            FuzzyVariable averageEnemyStaminaFuzzy = null;
            FuzzyVariable formationAccuracyFuzzy = null;

            Parallel.Invoke(
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { groupSizeFuzzy = fuzzificationBase.FuzzifyNumAllies(allyGroupSize); },
                () => { expectedEnemyCountFuzzy = fuzzificationBase.FuzzifyNumEnemies(enemyGroupSize); },
                () => { averageAllyHealthFuzzy = fuzzificationBase.FuzzifyHealth(averageAllyHealth); },
                () => { averageAllyStaminaFuzzy = fuzzificationBase.FuzzifyStamina(averageAllyStamina); },
                () => { averageEnemyHealthFuzzy = fuzzificationBase.FuzzifyHealth(averageEnemyHealth); },
                () => { averageEnemyStaminaFuzzy = fuzzificationBase.FuzzifyStamina(averageEnemyStamina); },
                () => { formationAccuracyFuzzy = fuzzificationBase.FuzzifyFormationAccuracy(formationAccuracy); }
            );

            float output = inferenceBase.EvaluateSurroundEnemiesRules(healthFuzzy, staminaFuzzy, groupSizeFuzzy, expectedEnemyCountFuzzy,
                averageAllyHealthFuzzy, averageAllyStaminaFuzzy, averageEnemyHealthFuzzy, averageEnemyStaminaFuzzy,
                formationAccuracyFuzzy);

            return defuzzificationBase.DefuzzifyNew(output);
        }
    
        private float ProccessFuzzyLogicForRetreatFromEnemies(float health, float stamina, int diffEnemiesAliies,
            int alliesGroupSize, int enemiesGroupSize, float defenseRating,
            float alliesAverageStamina, float alliesAverageHealth, float enemiesAverageHealth, float dangerLevel)
        {
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable DiffEnemyiesAlliesFuzzy = null;
            FuzzyVariable defenseRatingFuzzy = null;
            FuzzyVariable alliesAverageStaminaFuzzy = null;
            FuzzyVariable alliesAverageHealthFuzzy = null;
            FuzzyVariable enemiesAverageHealthFuzzy = null;
            FuzzyVariable averageHealthDiffAlliesEnemiesFuzzy = null;
            FuzzyVariable dangerLevelFuzzy = null;

            Parallel.Invoke(
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { DiffEnemyiesAlliesFuzzy = fuzzificationBase.FuzzifyDiffEnemiesAllies(diffEnemiesAliies); },
                () => { defenseRatingFuzzy = fuzzificationBase.FuzzifyDefenseRating(defenseRating); },
                () => { alliesAverageStaminaFuzzy = fuzzificationBase.FuzzifyStamina(alliesAverageStamina); },
                () => { alliesAverageHealthFuzzy = fuzzificationBase.FuzzifyHealth(alliesAverageHealth); },
                () => { enemiesAverageHealthFuzzy = fuzzificationBase.FuzzifyHealth(enemiesAverageHealth); },
                () => { averageHealthDiffAlliesEnemiesFuzzy = fuzzificationBase.FuzztifyAverageHealthDiff(alliesAverageHealth - enemiesAverageHealth); },
                () => { dangerLevelFuzzy = fuzzificationBase.FuzzifyDangerLevel(dangerLevel); }
            );

            float output = inferenceBase.EvaluateRetreatFromEnemiesRules(healthFuzzy, staminaFuzzy,
                DiffEnemyiesAlliesFuzzy, defenseRatingFuzzy,
                alliesAverageStaminaFuzzy, alliesAverageHealthFuzzy, enemiesAverageHealthFuzzy,
                averageHealthDiffAlliesEnemiesFuzzy, dangerLevelFuzzy);

            return defuzzificationBase.DefuzzifyNew(output);
        }

        private float ProccessFuzzyLogicForChargeEnemies(float health, float stamina, float alliesAverageHealth,
            int alliesGroupSize, float alliesAverageStamina, int diffEnemiesAllies, float distanceToTarget)
        {
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable DiffEnemyiesAlliesFuzzy = null;
            FuzzyVariable alliesAverageStaminaFuzzy = null;
            FuzzyVariable alliesAverageHealthFuzzy = null;
            FuzzyVariable distanceToTargetFuzzy = null;

            Parallel.Invoke(
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { DiffEnemyiesAlliesFuzzy = fuzzificationBase.FuzzifyDiffEnemiesAllies(diffEnemiesAllies); },
                () => { alliesAverageStaminaFuzzy = fuzzificationBase.FuzzifyStamina(alliesAverageStamina); },
                () => { alliesAverageHealthFuzzy = fuzzificationBase.FuzzifyHealth(alliesAverageHealth); },
                () => { distanceToTargetFuzzy = fuzzificationBase.FuzzifyPosition(distanceToTarget); }
            );

            float output = inferenceBase.EvaluateChargeEnemiesRules(healthFuzzy, staminaFuzzy,
                DiffEnemyiesAlliesFuzzy, alliesAverageStaminaFuzzy,
                alliesAverageHealthFuzzy, distanceToTargetFuzzy);

            return defuzzificationBase.DefuzzifyNew(output);
        }
        private float ProccessFuzzyLogicForPatrolArea(float health, float stamina, int allyGroupSize,
            float dangerLevel, float routeLength)
        {
            FuzzyVariable healthFuzzy = null;
            FuzzyVariable staminaFuzzy = null;
            FuzzyVariable allyGroupSizeFuzzy = null;
            FuzzyVariable dangerLevelFuzzy = null;
            FuzzyVariable routeLengthFuzzy = null;

            Parallel.Invoke(
                () => { healthFuzzy = fuzzificationBase.FuzzifyHealth(health); },
                () => { staminaFuzzy = fuzzificationBase.FuzzifyStamina(stamina); },
                () => { allyGroupSizeFuzzy = fuzzificationBase.FuzzifyNumAllies(allyGroupSize); },
                () => { dangerLevelFuzzy = fuzzificationBase.FuzzifyDangerLevel(dangerLevel); },
                () => { routeLengthFuzzy = fuzzificationBase.FuzzifyPosition(routeLength); }
            );

            float output = inferenceBase.EvaluatePatrolAreaRules(

                healthFuzzy, staminaFuzzy,
                allyGroupSizeFuzzy, dangerLevelFuzzy, routeLengthFuzzy);

            return defuzzificationBase.DefuzzifyNew(output);
        }
    
        #endregion
