

private void Start()
{
    //calculate stats
    CalculateFrequency();
    CalculateAverages();

    // sorting.
    SortAugmentsForUI();
    SortTraitsForUI();
    CheckTraitComposition();
    PruneTraitCompositionResults();
    CalculateAveragesForHeroAugments();
    CalculateAveragesForNormalAugmentsList();
    GenerateListOfAllItems();

    // new encoding.
    encodedTraitList = EncodeTraitNames();
    encodedItemSet = EncodeItemSet();
    encodedNormalAugmentAveragePlacements = EncodeNormAugAvgPlacement();
    encodedHeroAugmentAveragePlacements = EncodeHeroAugAvgPlacement();
    encodedTraitCompResults = EncodeTraitCompResults();

    // setting up the final composition Data.
    SetupCompositionData();
}


    public void ReceiveMatchData(DataForAgent agentData)// done 
    {
        // reads in the match data for storage as whole.
        if (matches.ContainsKey(agentData.match_id))
        {
            matches[agentData.match_id].Add(agentData);
        }
        else
        {
            List<DataForAgent> matchPlayers = new List<DataForAgent> { agentData };
            matches.Add(agentData.match_id, matchPlayers);
        }
        
        if (agentData != null)
        {
            dataForAgentList.Add(agentData);
            totalMatches = dataForAgentList.Count;
        }

    }
    private void SortAugmentsForUI()// done 
    {
        // this was developed to add a UI element so i could actually use this AI functionally.
        heroAugmentsList.Add("Let AI Choose");
        normalAugmentsList.Add("Let AI Choose");
        championNames.Add("Let AI Choose");

        // there was an issue with some of the data discrepancies in how i entered the data compared to riots API 
        // so i had to remove a spaces and ' this symbol from all names so they would be equal to riots for references.
        foreach(var trait in dataTraitList)
        {
            foreach(var unit in trait.units)
            {

                if(!championNames.Contains(unit.name.Replace(" ", string.Empty)))
                {
                    championNames.Add(unit.name.Replace(" ", string.Empty));
                }
                else if(!championNames.Contains(unit.name.Replace("'", string.Empty)))
                {
                    championNames.Add(unit.name.Replace("'", string.Empty));
                }

            }
        }
        // sorted all the augments based on if they are normal augments or hero augments by identifying
        // names of characters within the names of augments that only occur in hero augments.
        foreach (var match in matches.Values)
        {
            foreach (var player in match)
            {
                foreach(string augment in player.augments)
                {
                    bool containsUnitNames = false;

                    foreach (string name in championNames)
                    {
                        if(augment.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            containsUnitNames = true;
                            break;
                        }
                    }
                    
                    if (!containsUnitNames)
                    {
                        if (!normalAugmentsList.Contains(augment))
                        {
                            normalAugmentsList.Add(augment);
                        }
                    }
                    if(containsUnitNames)
                    {
                        if (!heroAugmentsList.Contains(augment))
                        {
                            heroAugmentsList.Add(augment);
                        }
                    }
                }
            }
        }

        //Debug.Log("Augment Sorting Complete");
    }
    public void ReceiveTraitData(TraitDataSet traitData)// done 
    {
        // reads in my hand written data set that was reference material for later data sorting.
        if (traitList.ContainsKey(traitData.id))
        {
            traitList[traitData.id].Add(traitData);
        }
        else
        {
            List<TraitDataSet> traitHolder = new List<TraitDataSet> { traitData };
            traitList.Add(traitData.id, traitHolder);
        }
        if(traitData != null)
        {
            dataTraitList.Add(traitData);
            totalTraitList = dataTraitList.Count;
        }
       // Debug.Log("Trait Data Received");
    }
    private void SortTraitsForUI()// done 
    {
        // adds a list of traits for the UI to use as list elements.
        listOfTraits.Add("Let AI Choose");
        foreach(var trait in dataTraitList)
        {
            if(!listOfTraits.Contains(trait.name))
            {
                listOfTraits.Add(trait.name);
            }
        }
         //Debug.Log("Trait Sorting Complete");
    }
    private void SetupCompositionData()// done 
    {
        // goes through all the matches and players in those games
        // stores each team composition played by each player in each game and their placement
        // in the match it was played to use as reference material for judging the AI's choice of composition
        foreach (var match in matches.Values)
        {
            foreach (var player in match)
            {
                string playerId = player.puuid;
                int _placement = player.placement;

                foreach (var unit in player.units)
                {
                    // for some reason the all the names of characters were missing from the Data pulled from the riot API.
                    // but there Id's were not missing, and in my reference file i created i had both names and ID's so i 
                    // searched for matching Id's in my reference file then pulled the name from the unit with the same ID.
                    // having the name was important later for check against trait conditions being fulfilled.
                    string unitName = "";
                    string unitCharacterId = unit.character_id;
                    foreach (TraitDataSet trait in dataTraitList)
                    {
                        foreach (var traitUnit in trait.units)
                        {
                            if(traitUnit.id == unitCharacterId)
                            {
                                unitName = traitUnit.name;
                                break;
                            }
                        }
                        if(unitName != "")
                        {
                            break;
                        }
                    }
                    // storing the data for later use in an object of a class declared below.
                    CompositionData compositionData = new CompositionData
                    {
                        placement = _placement,
                        characterId = unitCharacterId,
                        name = unitName
                    };

                    // Check if the player's composition already exists in the dictionary
                    if (compositions.ContainsKey(playerId))
                    {
                        compositions[playerId].Add(compositionData);
                    }
                    // if not makes a new entry.
                    else
                    {
                        List<CompositionData> playerCompositions = new List<CompositionData> { compositionData };
                        compositions.Add(playerId, playerCompositions);
                    }
                }
            }
        }
    }
    private void CalculateFrequency() // done.
    {
        Dictionary<string, string> idsAndNames = new Dictionary<string, string>();
        // basic loops that go over all data for the AI, checks at what frequency each condition occurs and just adds one to the value recording it.
        foreach(DataForAgent data in dataForAgentList)
        {
            foreach(Trait trait in data.traits)
            {
                if (traitFrequency.ContainsKey(trait.name))
                    traitFrequency[trait.name]++;
                else
                    traitFrequency[trait.name] = 1;
            }

            foreach (Units unit in data.units)
            {
                if(!idsAndNames.ContainsKey(unit.name) && !idsAndNames.ContainsValue(unit.character_id))
                {
                    idsAndNames.Add(unit.name, unit.character_id);
                }

                if (unitFrequency.ContainsKey(unit.character_id))
                {
                    unitFrequency[unit.character_id]++;
                }
                else
                {
                    unitFrequency[unit.character_id] = 1;
                }

                string unitItemId = unit.character_id + "-" + string.Join(",", unit.itemNames);

                if (unitItemPlacements.ContainsKey(unitItemId))
                {
                    unitItemPlacements[unitItemId].Add(data.placement);
                }
                else
                {
                    unitItemPlacements[unitItemId] = new List<int>() { data.placement };
                }
            }
            
            
            foreach (string augment in data.augments)
            {
                if (augmentFrequency.ContainsKey(augment))
                    augmentFrequency[augment]++;
                else
                    augmentFrequency[augment] = 1;
            }

            totalDamageToPlayers += data.total_damage_to_players;
            totalPlayersEliminated += data.players_eliminated;
            totalGoldLeft += data.gold_left;
            
        }
       
        // makes a record of each time a unit was played with specific items and what the placement was, so we have a record to reference against 
        // for judging the AI's choice of items and what the likely placement would be given.
        foreach (var kvp in unitItemPlacements)
        {
            string unitItemId = kvp.Key;
            List<int> placements = kvp.Value;

            string[] splitUnitItem = unitItemId.Split('-');
            string unit = splitUnitItem[0];
            string items = splitUnitItem[1];
            int frequency = placements.Count;

            string unitName = "";

            // same issues as previously mentioned in terms of the name not being recorded in the Riot API files.
            foreach (TraitDataSet trait in dataTraitList)
            {
                foreach (var traitUnit in trait.units)
                {
                    if(traitUnit.id == unit)
                    {
                        unitName = traitUnit.name;
                        break;
                    }
                }
                if(unitName != "")
                {
                    break;
                }
            }
            // prunes the data so there out an insane amount of cases where something only happened once or twice.
            // makes the data we are comparing against more reliable if it was done on multiple occasions it is way more likely to be good data.
            if (frequency >= 5 && !string.IsNullOrEmpty(items))
            {
                float averagePlacement = CalculateAveragePlacement(placements);

                string itemSet = unit + " - " + items;

                UnitItemBreakdown newUnitItemBreakdown = new UnitItemBreakdown(unit, items, averagePlacement, frequency, unitName);

                if (!itemSetList.ContainsKey(itemSet))
                {
                    itemSetList[itemSet] = newUnitItemBreakdown;
                }
            }
        }

        // a previous thought on trying to gather information about how players spent their economy 
        // float averageDamageToPlayers = (float)totalDamageToPlayers / totalMatches;
        // float averagePlayersEliminated = (float)totalPlayersEliminated / totalMatches;
        // float averageGoldLeft = (float)totalGoldLeft / totalMatches;

        // Debug.Log("Frequencies Calculations Complete");
    }
    public float CalculateAveragePlacement(List<int> placements) // done 
    {
        // basic calculation for integers averaging. 
        // helped reduce duplication of code.
        if (placements.Count == 0)
        {
            return 0f;
        }

        int sum = placements.Sum();
        float averagePlacement = (float)sum / placements.Count;
        return averagePlacement;
    }
    public float CalculateAveragePlacement(List<float> placements)// done 
    {
        // same as last but for floats.
        if (placements.Count == 0)
        {
            return 0f;
        }

        float sum = placements.Sum();
        float averagePlacement = sum / placements.Count;
        return averagePlacement;
    }
    private void PruneTraitCompositionResults() // done 
    {
        // just removes team compositions that only occurred a very limited amount of times
        List<string> keysToRemove = new List<string>();

        foreach (var kvp in traitCompositionResults)
        {
            TraitCompositionResult result = kvp.Value;

            if (result.frequency < 4)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (string key in keysToRemove)
        {
            traitCompositionResults.Remove(key);
        }
    }
    private void CalculateAverages() // done 
    {
        // storing the average placements of augments for later reference in teaching the AI.
        foreach(DataForAgent data in dataForAgentList)
        {
            foreach (string augment in data.augments)
            {
                string augments = augment;

                if (!augmentPlacements.ContainsKey(augment))
                {
                    augmentPlacements[augment] = new List<int>();
                }
                augmentPlacements[augment].Add(data.placement);
            }
        }
    }
    private void CalculateAveragesForNormalAugmentsList() // done 
    {

        // setup of same as the hero augment average placements but changes the values to store only normal augments
        Dictionary<string, List<int>> normalAugPlacements = new Dictionary<string, List<int>>();

        // setup the dictionary with empty placement lists for each normal augment
        foreach (string augment in normalAugmentsList)
        {
            normalAugPlacements[augment] = new List<int>();
        }

        // Collect the placements for each player based on normal augments
        foreach (var match in matches.Values)
        {
            foreach (var player in match)
            {
                foreach (string augment in normalAugmentsList)
                {
                    if (player.augments.Contains(augment))
                    {
                        normalAugPlacements[augment].Add(player.placement);
                    }
                }
            }
        }

        // Calculate the average placement for each normal augment
        foreach (var kvp in normalAugPlacements)
        {
            string augment = kvp.Key;
            List<int> placements = kvp.Value;
            float averagePlacement = CalculateAveragePlacement(placements);
            normalAugmentAveragePlacements[augment] = averagePlacement;
        }
    }
    private void CalculateAveragesForHeroAugments()// done 
    {

        // a break down and storage of augments and storing their placements in each game they played, then averaged for final use by the AI
        // to calculate its performance in terms of its recommendations.
        Dictionary<string, List<int>> heroAugPlacements = new Dictionary<string, List<int>>();

        // setup the dictionary with empty placement lists for each hero augment
        foreach (string augment in heroAugmentsList)
        {
            heroAugPlacements[augment] = new List<int>();
        }

        // Collect the placements for each player based on hero augments
        foreach (var match in matches.Values)
        {
            foreach (var player in match)
            {
                foreach (string augment in heroAugmentsList)
                {
                    if (player.augments.Contains(augment))
                    {
                        heroAugPlacements[augment].Add(player.placement);
                    }
                }
            }
        }

        // Calculate the average placement for each hero augment
        foreach (var kvp in heroAugPlacements)
        {
            string augment = kvp.Key;
            List<int> placements = kvp.Value;
            float averagePlacement = CalculateAveragePlacement(placements);
            heroAugmentAveragePlacements[augment] = averagePlacement;
        }
    }
    private void CheckTraitComposition()// done 
    {
        // looks complicated but breaks down the trait frequencies lineups, and placements of said traits so i could use it refer more deeply to
        // this really data really helped me breakdown the choices for rewards or punishments when regarding AI's choice in traits.
        // not gonna break the code down further here as its a similar rinse and repeat of previous nested for each loops but for traits rather than units or augments.
        foreach (var match in matches.Values)
        {
            foreach (var player in match)
            {
                List<string> playerUnits = new List<string>();

                foreach (var unit in player.units)
                {
                    playerUnits.Add(unit.character_id);
                }

                foreach (TraitDataSet trait in dataTraitList)
                {
                    List<string> unitsWithTrait = new List<string>();

                    foreach (var unit in trait.units)
                    {
                        unitsWithTrait.AddRange(playerUnits.FindAll(u => u == unit.id));
                    }

                    foreach (int traitActiveNum in trait.trait_active_nums)
                    {
                        if (unitsWithTrait.Count >= traitActiveNum)
                        {
                            List<string> unitNames = unitsWithTrait.Select(u => u).ToList();
                            string lineupKey = GenerateLineupKey(player.puuid, trait.name, traitActiveNum);

                            if (traitCompositionResults.ContainsKey(lineupKey))
                            {
                                TraitCompositionResult existingResult = traitCompositionResults[lineupKey];
                                existingResult.frequency++;
                                existingResult.placement.Add(player.placement);
                            }
                            else
                            {
                                TraitCompositionResult newResult = new TraitCompositionResult
                                {
                                    traitName = trait.name,
                                    traitActiveNum = traitActiveNum,
                                    placement = new List<int>() { player.placement },
                                    units = unitNames
                                };

                                traitCompositionResults.Add(lineupKey, newResult);
                            }
                        }
                    }
                }
            }
        }
    }
    private void GenerateListOfAllItems()// done 
    {
        // creates a list of items for the AI to choose from from the carry.
        foreach (var match in matches.Values)
        {
            foreach (var player in match)
            {
                foreach(var unit in player.units)
                {
                    foreach (string item in unit.itemNames)
                    {
                        // just looks for every item it does'nt already contain until all are added.
                        if(!listOfAllItems.Contains(item))
                        {
                            listOfAllItems.Add(item);
                        }
                    }
                }
            }
        }
        // these are special items that require special circumstances and i did'nt have time to factor them in.
        string[] wordsToRemove = { "Emblem", "Radiant", "Ornn", "Spatula", "GenAE", "ForceOfNature" };
        RemoveItemsWithWords(listOfAllItems, wordsToRemove);     
    }
    private void RemoveItemsWithWords(List<string> list, string[] words) // done 
    {
        // quick little way to remove and words from a list i dont want included, thought it might be something i re use in the so i made it a function
        list.RemoveAll(item => words.Any(word => item.Contains(word)));
    }
    private string GenerateLineupKey(string playerId, string traitName, int traitActiveNum)// done 
    {
        // generates the lineup key that becomes the key for the trait composition results.
        return playerId + "_" + traitName + "_" + traitActiveNum.ToString();
    }

    #endregion
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        List<float> averages = new List<float>();
        DoSetupForTeamGeneration();
        displayTestingResults++;

        ///////////////////////////
        //     Info Counts       // 
        //       66 units        //
        //      28 traits        //
        //  172 normal augments  //
        //   113 hero augments   //
        //      45 items         //
        ///////////////////////////
        // Decides which of the inputs will be randomised.
        int randomNumber = UnityEngine.Random.Range(0, 8);

        #region Main Loop    

        // generates the Carry unit choice.
        if(doRandomChamp && randomNumber != 0)
        {
            int var = 0;
            var = actions.DiscreteActions[0];
            if(var != 66)
            {
                var += 1;
            }
            choiceChamp = championNames[var];
        }
        else if(doRandomChamp && randomNumber == 0)
        {
            int rand = UnityEngine.Random.Range(0, 66);
            if(rand != 66)
            {
                rand += 1;
            }
            choiceChamp = championNames[rand];
            Debug.Log("did rand");
        }

        //generates the item choice.
        int itemVar1 = actions.DiscreteActions[17];
        int itemVar2 = actions.DiscreteActions[18];
        int itemVar3 = actions.DiscreteActions[19];
        carryItems[0] = listOfAllItems[itemVar1];
        carryItems[1] = listOfAllItems[itemVar2];
        carryItems[2] = listOfAllItems[itemVar3];

        // generates the first augment choice.
        if(doRandomAugmentOne && randomNumber != 1)
        {
            int var = 0;
            var = actions.DiscreteActions[1];
            if(var != 172)
            {
                var += 1;
            }
            choiceAugOne = normalAugmentsList[var];
            //Debug.Log("aug 1: " + choiceAugOne);
        }
        else if(doRandomAugmentOne && randomNumber == 1)
        {
            int rand = UnityEngine.Random.Range(0, 172);
            if(rand != 172)
            {
                rand += 1;
            }
            choiceAugOne = normalAugmentsList[rand];
            //Debug.Log("did rand" + randomNumber);
        }
        
        // generates second augment choice.
        if(doRandomAugmentTwo && randomNumber != 2)
        {
            int var = 0;
            var = actions.DiscreteActions[2];
            if(var != 172)
            {
                var += 1;
            }
            choiceAugTwo = normalAugmentsList[var];
            //Debug.Log("aug 2: " + choiceAugTwo);
        }
        else if(doRandomAugmentTwo && randomNumber == 2)
        {
            int rand = UnityEngine.Random.Range(0, 172);
            if(rand != 172)
            {
                rand += 1;
            }
            choiceAugTwo = normalAugmentsList[rand];
            //Debug.Log("did rand" + randomNumber);
        }
        //generates the hero augment choice.
        if(doRandomChosenHeroAugment && randomNumber != 3)
        {
            int var = 0;
            var = actions.DiscreteActions[3];
            if(var != 113)
            {
                var += 1;
            }
            choiceHeroAug = heroAugmentsList[var];
            //Debug.Log("hero aug: " + choiceHeroAug);
        }
        else  if(doRandomChosenHeroAugment && randomNumber == 3)
        {
            int rand = UnityEngine.Random.Range(0, 113);
            if(rand != 113)
            {
                rand += 1;
            }
            choiceHeroAug = heroAugmentsList[rand];
            //Debug.Log("did rand" + randomNumber);
        }
        // generates the first primary trait choice.
        if(doRandomTraitOne && randomNumber != 4)
        {
            int var = 0;
            var = actions.DiscreteActions[4];
            if(var != 28)
            {
                var += 1;
            }
            choiceTraitOne = listOfTraits[var];
        // Debug.Log(choiceTraitOne);
        }
        else if(doRandomTraitOne && randomNumber == 4)
        {
            int rand = UnityEngine.Random.Range(0, 28);
            if(rand != 28)
            {
                rand += 1;
            }
            choiceTraitOne = listOfTraits[rand];
            Debug.Log("did rand" + randomNumber);
        }
        // generates the second primary trait choice.
        if(doRandomTraitTwo && randomNumber != 5)
        {
            int var = 0;
            var = actions.DiscreteActions[5];
            if(var != 28)
            {
                var += 1;
            }
            choiceTraitTwo = listOfTraits[var];
        //  Debug.Log(choiceTraitTwo);
        }
        else if(doRandomTraitTwo && randomNumber == 5)
        {
            int rand = UnityEngine.Random.Range(0, 28);
            if(rand != 28)
            {
                rand += 1;
            }
            choiceTraitTwo = listOfTraits[rand];
            //Debug.Log("did rand" + randomNumber);
        }
        // generates the third primary trait choice.
        if(doRandomTraitThree && randomNumber != 6)
        {
            int var = 0;
            var = actions.DiscreteActions[6];
            if(var != 28)
            {
                var += 1;
            }
            choiceTraitThree = listOfTraits[var];
        // Debug.Log(choiceTraitThree);
        }
        else if(doRandomTraitThree && randomNumber == 6)
        {
            int rand = UnityEngine.Random.Range(0, 28);
            if(rand != 28)
            {
                rand += 1;
            }
            choiceTraitThree = listOfTraits[rand];
            //Debug.Log("did rand" + randomNumber);
        }

        // generates team size.
        if(doRandomTeamSize && randomNumber != 7)
        {
            int holder = 0;
            holder = actions.DiscreteActions[7];
            if(holder != 5)
            {
                holder += 1;
            }
            choiceTeamSize = holder;
        }
        else if(doRandomTeamSize && randomNumber == 7)
        {
            int rand = UnityEngine.Random.Range(0, 5);
            if(rand != 5)
            {
                rand += 1;
            }
            choiceTeamSize = rand;
            //Debug.Log("did rand" + randomNumber);
        }

        // generates team unit composition.
        int loopSize = choiceTeamSize -1;
        for(int i = 0; i < loopSize; i++)
        {
            int posInActions = 8 + i;
            int var = 0;
            var = actions.DiscreteActions[posInActions];
            if(var != 66)
            {
                var += 1;
            }
            teamCompSuggestions.Add(championNames[var]);
            //Debug.Log("Additional Recommended unit " + i + " : " + championNames[var]);
        }

        // generates a list of the choices made thats then encoded and passed to the AI so it can review the decisions its made vs the outcome.
        AIChoices.Add(choiceChamp);
        AIChoices.Add(choiceAugOne);
        AIChoices.Add(choiceAugTwo);
        AIChoices.Add(choiceHeroAug);
        AIChoices.Add(choiceTraitOne);
        AIChoices.Add(choiceTraitTwo);
        AIChoices.Add(choiceTraitThree);
        foreach(string select in teamCompSuggestions)
        {
            AIChoices.Add(select);
        }
        encodedChoices = EncodeTraitAIChoices();

        // a break down into the console for test purposes.
        Debug.Log("Final team breakdown: ");
        Debug.Log("Carry: " + choiceChamp);
        for(int i = 0; i < carryItems.Length; i++)
        {
            Debug.Log("Carry's " + i + " : " + carryItems[i]);
        }
        Debug.Log("Additional Recommended units :");
        foreach(string name in teamCompSuggestions)
        {
            Debug.Log(name);
        }

        // adds the average success of the AI's decisions to a list that output every 1000 iterations and averaged to see how the AI is improving.
        averages.Add(DetermineLikelyPlacementCarry()); 
        averages.Add(DetermineLikelyPlacementTeam());
        averages.Add(DetermineLikelyPlacementHeroAugment());
        averages.Add(DetermineLikelyPlacementAugmentOne());
        averages.Add(DetermineLikelyPlacementAugmentTwo());
        Debug.Log("Activated Traits: " + CheckTraitActivation());

        float finalAvg = 0;
        foreach(float average in averages)
        {
            finalAvg += average;
        }
        finalAvg /= averages.Count;
        totalAverageForTesting.Add(finalAvg);
        Debug.Log(finalAvg);
        Debug.Log("----------------------------");
        ResetVariables();
        #endregion
      
    }

    #region AI Features
    private void ResetVariables()
    {
        // resets thins after each iteration.
        choiceChamp = "";
        choiceAugOne = "";
        choiceAugTwo = "";
        choiceHeroAug = "";
        choiceTraitOne = "";
        choiceTraitTwo = "";
        choiceTraitThree = "";
        carryItems = new string[3];
        choiceTeamSize = 0;
    }
    private int CheckTraitActivation()
    {
        Dictionary<string, int> activeTraitsAndCount = new Dictionary<string, int>();
        Dictionary<string, List<int>> traitAndNumToActive = new Dictionary<string, List<int>>();
        List<string> chosenUnitTraits = new List<string>();
        List<string> rewardTraits = new List<string>();

        // records the traits and the amount of units itll take to activate it for each unit on the team.
        int activateTraits = 0;
        foreach(var trait in traitList)
        {
            string key = trait.Key;
            List<TraitDataSet> allTraits = trait.Value;
            foreach(var traitInfo in allTraits)
            {
                foreach(var unitsInTrait in traitInfo.units)
                {
                    if(unitsInTrait.name == chosenChamp)
                    {
                        chosenUnitTraits.Add(traitInfo.id);
                        activeTraitsAndCount.Add(traitInfo.id, 1);
                        traitAndNumToActive.Add(traitInfo.id, traitInfo.trait_active_nums);
                        break;
                    }
                }    
            }
        }

        // loops through the traits and checks against the trait list dictionary to see if enough units are in the team to activate the trait. 
        foreach (string unit in teamCompSuggestions)
        {
            foreach(var trait in traitList)
            {
                string key = trait.Key;
                List<TraitDataSet> allTraits = trait.Value;
                foreach(var traitInfo in allTraits)
                {
                    foreach(var unitsInTrait in traitInfo.units)
                    {
                        if(unitsInTrait.name == unit)
                        {
                            if(activeTraitsAndCount.ContainsKey(traitInfo.id))
                            {
                                activeTraitsAndCount[traitInfo.id]++;
                                break;
                            }
                            else
                            {
                                activeTraitsAndCount.Add(traitInfo.id, 1);
                                traitAndNumToActive.Add(traitInfo.id, traitInfo.trait_active_nums);
                                break;
                            }
                        }
                    }    
                }
            }
        }

        // this loops rewards the AI based on the trait they activated, if it was a any of the primary traits, or the trait of the carry unit then they gain extra rewards.
        foreach(var activeTrait in activeTraitsAndCount)
        {
            string name = activeTrait.Key;
            int count = activeTrait.Value;

            foreach(var canActiveCheck in traitAndNumToActive)
            {
                string activeCountName = activeTrait.Key;
                List<int> activeCount = canActiveCheck.Value;

                if(name == activeCountName)
                {
                    foreach(int value in activeCount)
                    {
                        if(count >= value)
                        {
                            if(!rewardTraits.Contains(name))
                            {
                                rewardTraits.Add(name);
                                bool lesserReward = true;
                                foreach(string chosenCheck in chosenUnitTraits)
                                {
                                    if(name == chosenCheck)
                                    {
                                        lesserReward = false;
                                        AddReward(+1.5f);
                                        //Debug.Log("activated a Carry Augment");
                                        activateTraits++;
                                        break;   
                                    }
                                }
                                if(lesserReward)
                                {
                                    AddReward(+0.5f);

                                    activateTraits++;
                                   // Debug.Log("activated a other unit Trait.");
                                    break;
                                }
                                if(name == choiceTraitOne || name == choiceTraitTwo || name == choiceTraitThree)
                                {
                                    AddReward(+0.5f);
                                    activateTraits++;
                                   // Debug.Log("activated a desired trait");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        averageForTrait.Add(activateTraits);
        return activateTraits;
    }
    private float DetermineLikelyPlacementCarry()
     {     

        // loops through all the unit item combinations to find how other units with the same items performed on average
        // if they isn't any record of that combinations its automatically given an average of 6th place.
        List<float> averagePlacement = new List<float>();

        foreach (var kvp in itemSetList)
        {
            //string championName = kvp.Value.name;
            string itemsString = kvp.Value.items;
            
            // Split the items string into an array
            string[] itemsArray = itemsString.Split(',');
            
            if(kvp.Value.name == choiceChamp)
            {
                for(int i = 0; i < itemsArray.Length; i++)
                {
                    if(itemsArray[i] == carryItems[i])
                    {
                        if(kvp.Value.averagePlacement <= 8f && kvp.Value.averagePlacement > 7f)
                        {
                            AddReward(-1.0f);
                            averagePlacement.Add(kvp.Value.averagePlacement);
                        }
                        else if(kvp.Value.averagePlacement <= 7f && kvp.Value.averagePlacement > 6f)
                        {
                            AddReward(-0.75f);
                            averagePlacement.Add(kvp.Value.averagePlacement);
                        }
                        else if(kvp.Value.averagePlacement <= 6f && kvp.Value.averagePlacement > 5f)
                        {
                            AddReward(-0.5f);
                            averagePlacement.Add(kvp.Value.averagePlacement);
                        }
                        else if(kvp.Value.averagePlacement <= 5f && kvp.Value.averagePlacement > 4f)
                        {
                            AddReward(-0.25f);
                            averagePlacement.Add(kvp.Value.averagePlacement);
                        }
                        else if(kvp.Value.averagePlacement <= 4f && kvp.Value.averagePlacement > 3f)
                        {
                            AddReward(+0.25f);
                            averagePlacement.Add(kvp.Value.averagePlacement);
                        }
                        else if(kvp.Value.averagePlacement <= 3f && kvp.Value.averagePlacement > 2f)
                        {
                            AddReward(+0.5f);
                            averagePlacement.Add(kvp.Value.averagePlacement);
                        }
                        else if(kvp.Value.averagePlacement <= 2f && kvp.Value.averagePlacement > 1f)
                        {
                            AddReward(+0.75f);
                            averagePlacement.Add(kvp.Value.averagePlacement);
                        }
                        else
                        {
                            AddReward(+1.0f);
                            averagePlacement.Add(kvp.Value.averagePlacement);
                        }
                    }  
                }
                break;
            }
        }
        float returnVal = CalculateAveragePlacement(averagePlacement);
        if(returnVal == 0)
        {
            returnVal = 6;
        }
        averageForCarry.Add(returnVal);
        return returnVal;
     }
    private float DetermineLikelyPlacementAugmentOne()
    {
        float averagePlacement = 0f;

        // this checks through all the placements of the chosen augment and returns an average placement for that augment.
        if (normalAugmentAveragePlacements.ContainsKey(choiceAugOne))
        {
            averagePlacement = normalAugmentAveragePlacements[choiceAugOne];
        }


        if(averagePlacement != 0f)
        {
            if(averagePlacement == 8)
            {
                AddReward(-2f);
                // 8th
            }
            else if(averagePlacement < 8 && averagePlacement > 7)
            {
                AddReward(-1.5f);
                // 7th punishment
            }
            else if(averagePlacement <= 7 && averagePlacement > 6)
            {
                AddReward(-1.25f);
                // 6th
            }
            else if(averagePlacement <= 6 && averagePlacement > 5)
            {
                AddReward(-1f);
                //5th
            }
            else if(averagePlacement <= 5 && averagePlacement > 4)
            {
                AddReward(+0.25f);
                //4th
            }
            else if(averagePlacement <= 4 && averagePlacement > 3)
            {
                AddReward(+1f);
                //3rd
            }
            else if(averagePlacement <= 3 && averagePlacement > 2)
            {
                AddReward(+1.5f);
                //2nd
            }
            else if(averagePlacement <= 2 && averagePlacement > 1)
            {
                AddReward(+2f);
                //somewhere between first and second
            }
            else if(averagePlacement == 1)
            {
                AddReward(+3f);
                // flat first. big reward.
            }
        }
        averageForAugOne.Add(averagePlacement);
        return averagePlacement;
    }
    private float DetermineLikelyPlacementAugmentTwo()
    {

        // this is the same as the first augment so i wont repeat commenting it

        float averagePlacement = 0f;

        if (normalAugmentAveragePlacements.ContainsKey(choiceAugTwo))
        {
            averagePlacement = normalAugmentAveragePlacements[choiceAugTwo];
        }

        if(averagePlacement != 0f)
        {
            if(averagePlacement == 8)
            {
                AddReward(-2f);
                // 8th
            }
            else if(averagePlacement < 8 && averagePlacement > 7)
            {
                AddReward(-1.5f);
                // 7th punishment
            }
            else if(averagePlacement <= 7 && averagePlacement > 6)
            {
                AddReward(-1.25f);
                // 6th
            }
            else if(averagePlacement <= 6 && averagePlacement > 5)
            {
                AddReward(-1f);
                //5th
            }
            else if(averagePlacement <= 5 && averagePlacement > 4)
            {
                AddReward(+0.25f);
                //4th
            }
            else if(averagePlacement <= 4 && averagePlacement > 3)
            {
                AddReward(+1f);
                //3rd
            }
            else if(averagePlacement <= 3 && averagePlacement > 2)
            {
                AddReward(+1.5f);
                //2nd
            }
            else if(averagePlacement <= 2 && averagePlacement > 1)
            {
                AddReward(+2f);
                //somewhere between first and second
            }
            else if(averagePlacement == 1)
            {
                AddReward(+3f);
                // flat first. big reward.
            }
        }

        averageForAugTwo.Add(averagePlacement);
        return averagePlacement;
    }
    private float DetermineLikelyPlacementHeroAugment()
     {

        // again very similar to aug 1 and aug 2 but hero augment instead.
        float averagePlacement = 0f;

        if (heroAugmentAveragePlacements.ContainsKey(choiceHeroAug))
        {
            averagePlacement = heroAugmentAveragePlacements[choiceHeroAug];
            //Debug.Log("average placement for hero augment: " + averagePlacement);
        }
        else
        {
            //Debug.Log("something went wrong with hero aug");
        }
        if(averagePlacement != 0f)
        {
            if(averagePlacement == 8)
            {
                AddReward(-2f);
                // 8th
            }
            else if(averagePlacement < 8 && averagePlacement > 7)
            {
                AddReward(-1.5f);
                // 7th punishment
            }
            else if(averagePlacement <= 7 && averagePlacement > 6)
            {
                AddReward(-1.25f);
                // 6th
            }
            else if(averagePlacement <= 6 && averagePlacement > 5)
            {
                AddReward(-1f);
                //5th
            }
            else if(averagePlacement <= 5 && averagePlacement > 4)
            {
                AddReward(+0.25f);
                //4th
            }
            else if(averagePlacement <= 4 && averagePlacement > 3)
            {
                AddReward(+1f);
                //3rd
            }
            else if(averagePlacement <= 3 && averagePlacement > 2)
            {
                AddReward(+1.5f);
                //2nd
            }
            else if(averagePlacement <= 2 && averagePlacement > 1)
            {
                AddReward(+2f);
                //somewhere between first and second
            }
            else if(averagePlacement == 1)
            {
                AddReward(+3f);
                // flat first. big reward.
            }
        }
        averageForHeroAug.Add(averagePlacement);
        return averagePlacement;
     }
    private float DetermineLikelyPlacementTeam()
    {
        // punish the AI for adding a copy of the carry to the team causing duplicates.
        foreach (string champ in teamCompSuggestions)
        {
            if (champ == choiceChamp)
            {
                AddReward(-0.5f);
            }
        }
        // also do check to see if the same champ has been added twice to the team.

        float averagePlacement = 0f;
        
        List<float> placementsOfCloseMatch = new List<float>();
        List<float> placementsOfTotalMatch = new List<float>();
       
        
        int matchCount = 0;
        bool doOnce = true;

        // go through all the players in the team and check against other instances where that composition has been played.
        // the check is a bit more forgiving and will find the averages of any game that has at least 4 matching units if there isn't a absolute match but the reward is lessened.
        foreach(var player in compositions)
        {
            List<CompositionData> compositionData = player.Value;

            foreach(var unit in compositionData)
            {
                foreach (string name in teamCompSuggestions)
                {
                    if(name == unit.name)
                    {
                        matchCount++;
                    }
                }
                if(matchCount >= 4 && doOnce)
                {
                    placementsOfCloseMatch.Add(unit.placement);
                    doOnce = false;
                }
                if(matchCount >= choiceTeamSize)
                {
                    placementsOfTotalMatch.Add(unit.placement);
                    break;
                }
            }
        }  


        // the rewards for finding matching teams.
         

        if (placementsOfTotalMatch.Count >= 3)
        {
            averagePlacement = CalculateAveragePlacement(placementsOfTotalMatch);
            averageForTeam.Add(averagePlacement);
            AddReward(+1.0f);
        }
        else if(placementsOfCloseMatch.Count >= 5)
        {
            averagePlacement = CalculateAveragePlacement(placementsOfCloseMatch);
            averageForTeam.Add(averagePlacement);
            AddReward(+0.5f);
        }
        else
        {
            averageForTeam.Add(6);
            AddReward(-1f);
            return 6;
        }
        
        // the reward for the estimated placement of the composition.

        if(averagePlacement != 0f)
        {
            if(averagePlacement == 8)
            {
                AddReward(-2f);
                // 8th
            }
            else if(averagePlacement < 8 && averagePlacement > 7)
            {
                AddReward(-1.5f);
                // 7th punishment
            }
            else if(averagePlacement <= 7 && averagePlacement > 6)
            {
                AddReward(-1.25f);
                // 6th
            }
            else if(averagePlacement <= 6 && averagePlacement > 5)
            {
                AddReward(-1f);
                //5th
            }
            else if(averagePlacement <= 5 && averagePlacement > 4)
            {
                AddReward(+0.25f);
                //4th
            }
            else if(averagePlacement <= 4 && averagePlacement > 3)
            {
                AddReward(+1f);
                //3rd
            }
            else if(averagePlacement <= 3 && averagePlacement > 2)
            {
                AddReward(+1.5f);
                //2nd
            }
            else if(averagePlacement <= 2 && averagePlacement > 1)
            {
                AddReward(+2f);
                //somewhere between first and second
            }
            else if(averagePlacement == 1)
            {
                AddReward(+3f);
                // flat first. big reward.
            }
        }
         
        return averagePlacement;
    }
    private void DoSetupForTeamGeneration()
    {
        // setups UI elements 
        champHolder = UI.GetChosenChampion();
        
        chosenChamp = champHolder;

        //Debug.Log("chosenChamp test: " + chosenChamp);
         augment1Holder = UI.GetChosenAugmentOne();
        chosenAugmentOne = augment1Holder;
        //Debug.Log("chosenAugmentOne: " + chosenAugmentOne);
         augment2Holder = UI.GetChosenAugmentTwo();
        chosenAugmentTwo = augment2Holder;
        //Debug.Log("chosenAugmentTwo: " + chosenAugmentTwo);
         heroAugHolder = UI.GetChosenHeroAugment();
        chosenHeroAugment = heroAugHolder;
        //Debug.Log("chosenHeroAugment: " + chosenHeroAugment);
         trait1Holder = UI.GetChosenTraitOne();
        chosenTraitOne = trait1Holder;
        //Debug.Log("chosenTraitOne: " + chosenTraitOne);
         trait2Holder = UI.GetChosenTraitTwo();
        chosenTraitTwo = trait2Holder;
        //Debug.Log("chosenTraitTwo: " + chosenTraitTwo);
         trait3Holder = UI.GetChosenTraitThree();
        chosenTraitThree = trait3Holder;
        //Debug.Log("chosenTraitThree: " + chosenTraitThree);
        int teamSizeHolder =  UI.GetChosenTeamSize();
        chosenTeamSize = teamSizeHolder;
        //Debug.Log("chosenTeamSize: " + chosenTeamSize);
        teamCompSuggestions.Clear();

        if(chosenChamp == "Let AI Choose")
        {
            doRandomChamp = true;
        }
        else
        {
            doRandomChamp = false;   
        }
        if(chosenAugmentOne == "Let AI Choose")
        {
            doRandomAugmentOne = true;
        }
        else
        {
            doRandomAugmentOne = false;
        }
        if(chosenAugmentTwo == "Let AI Choose")
        {
            doRandomAugmentTwo = true;           
        }
        else
        {
            doRandomAugmentTwo = false;
        }
        if(chosenHeroAugment == "Let AI Choose")
        {
            doRandomChosenHeroAugment = true;
        }
        else
        {
            doRandomChosenHeroAugment = false;
        }
        if(chosenTraitOne == "Let AI Choose")
        {
            doRandomTraitOne = true;
        }
        else
        {
            doRandomTraitOne = false;
        }
        if(chosenTraitTwo == "Let AI Choose")
        {
            doRandomTraitTwo = true;
        }
        else
        {
            doRandomTraitTwo = false;
        }
        if(chosenTraitThree == "Let AI Choose")
        {
            doRandomTraitThree = true;
        }
        else
        {
            doRandomTraitThree = false;
        }
        if(chosenTeamSize == 0)
        {
            doRandomTeamSize = true;
        }
        else
        {
            doRandomTeamSize = false;
        }
    }
    #endregion
    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var match in matches.Values)
        {
            foreach (var player in match)
            {
                sensor.AddObservation(player.traitEncoding);
                sensor.AddObservation(player.unitEncoding);
                sensor.AddObservation(player.augmentEncoding);
                
                sensor.AddObservation(player.gold_left);
                sensor.AddObservation(player.last_round);
                sensor.AddObservation(player.level);
                sensor.AddObservation(player.placement);
                sensor.AddObservation(player.total_damage_to_players);
                sensor.AddObservation(player.players_eliminated);
            }
        }

        foreach (KeyValuePair<string, float[]> data in encodedTraitList)
        {
            float[] encodedData = data.Value;

            foreach (float value in encodedData)
            {
                sensor.AddObservation(value);
            }
        }

        foreach (KeyValuePair<string, float[]> data in encodedItemSet)
        {
            float[] encodedData = data.Value;

            foreach (float value in encodedData)
            {
                sensor.AddObservation(value);
            }
        }

        foreach (KeyValuePair<string, float[]> data in encodedNormalAugmentAveragePlacements)
        {
            float[] encodedData = data.Value;

            foreach (float value in encodedData)
            {
                sensor.AddObservation(value);
            }
        }

        foreach (KeyValuePair<string, float[]> data in encodedHeroAugmentAveragePlacements)
        {
            float[] encodedData = data.Value;

            foreach (float value in encodedData)
            {
                sensor.AddObservation(value);
            }
        }

        foreach (KeyValuePair<string, float[]> data in encodedTraitCompResults)
        {
            float[] encodedData = data.Value;

            foreach (float value in encodedData)
            {
                sensor.AddObservation(value);
            }
        }
        foreach (KeyValuePair<string, float[]> data in encodedChoices)
        {
            float[] encodedData = data.Value;

            foreach (float value in encodedData)
            {
                sensor.AddObservation(value);
            }
        }
        encodedChoices.Clear();
    }

    #region Encoding
    
    // these just setup for the encoding to happen with each container, and returns the encoded information.
    private Dictionary<string, float[]> EncodeNormAugAvgPlacement()
    {
        var uniqueAugmentNames = normalAugmentAveragePlacements.Keys.ToArray();
        Dictionary<string, float[]> encodedAugments = OneHotEncoding(normalAugmentAveragePlacements, uniqueAugmentNames, uniqueAugmentNames.Length);
        return encodedAugments;
    }
    private Dictionary<string, float[]> EncodeTraitNames()
    {
        var uniqueTraitNames = traitList.Keys.ToArray();
        Dictionary<string, float[]> encodedTraits = OneHotEncoding(traitList, uniqueTraitNames, uniqueTraitNames.Length);
        return encodedTraits;
    }
    private Dictionary<string, float[]> EncodeItemSet()
    {
        string[] uniqueCategories = itemSetList.Keys.ToArray();
        Dictionary<string, float[]> encodedData = OneHotEncoding(itemSetList, uniqueCategories, uniqueCategories.Length);
        
        return encodedData;
    }
    private Dictionary<string, float[]> EncodeHeroAugAvgPlacement()
    {
        var uniqueAugmentNames = heroAugmentAveragePlacements.Keys.ToArray();
        Dictionary<string, float[]> encodedAugments = OneHotEncoding(heroAugmentAveragePlacements, uniqueAugmentNames, uniqueAugmentNames.Length);
        return encodedAugments;
    }
    private Dictionary<string, float[]> EncodeTraitCompResults()
    {
        var uniqueAugmentNames = traitCompositionResults.Keys.ToArray();
        Dictionary<string, float[]> encodedAugments = OneHotEncoding(traitCompositionResults, uniqueAugmentNames, uniqueAugmentNames.Length);
        return encodedAugments;
    }
    private Dictionary<string, float[]> EncodeTraitAIChoices()
    {
        string[] uniqueChoices = AIChoices.Distinct().ToArray();
        Dictionary<string, float[]> encodedAugments = OneHotEncoding(AIChoices, uniqueChoices, uniqueChoices.Length);
        AIChoices.Clear();
        return encodedAugments;
    }
    
    // these are overloaded functions of the same thing to account for a change in input. I will comment just the first one.
    private Dictionary<string, float[]> OneHotEncoding(Dictionary<string, TraitCompositionResult> categories, string[] uniqueCategories, int maxLength)
    {
        Dictionary<string, float[]> encoding = new Dictionary<string, float[]>();

        // go through each entry in the dictionary to encode it all.
        foreach (var kvp in categories)
        {
            string category = kvp.Key;
            // the value being the important data we want to encode.
            TraitCompositionResult result = kvp.Value;

            float[] categoryEncoding = new float[maxLength];

            // this index is then used to set the corresponding value in the categoryEncoding array to 1. 
            // this performs one-hot encoding, where only the index corresponding to the category being encoded is set to 1.
            // while all other indices remain 0.
            int index = Array.IndexOf(uniqueCategories, category);
            if (index >= 0)
            {
                categoryEncoding[index] = 1f;
            }

            encoding.Add(category, categoryEncoding);
        }

        return encoding;
    }
    private Dictionary<string, float[]> OneHotEncoding(List<string> categories, string[] uniqueCategories, int maxLength)
    {
        Dictionary<string, float[]> encoding = new Dictionary<string, float[]>();

        foreach (string category in categories)
        {
            float[] categoryEncoding = new float[maxLength];

            int index = Array.IndexOf(uniqueCategories, category);
            if (index >= 0)
            {
                categoryEncoding[index] = 1f;
            }
            if(!encoding.ContainsKey(category))
            {
                encoding.Add(category, categoryEncoding);
            }
        }

        return encoding;
    }
    // norm and hero aug placement encoding.
    private Dictionary<string, float[]> OneHotEncoding(Dictionary<string, float> categories, string[] uniqueCategories, int maxLength)
    {
        Dictionary<string, float[]> encoding = new Dictionary<string, float[]>();

        foreach (var kvp in categories)
        {
            string category = kvp.Key;
            float[] categoryEncoding = new float[maxLength];

            int index = Array.IndexOf(uniqueCategories, category);
            if (index >= 0)
            {
                categoryEncoding[index] = 1f;
            }

            encoding.Add(category, categoryEncoding);
        }

        return encoding;
    }
    
    // unit item breakdown encoding.
    private Dictionary<string, float[]> OneHotEncoding(Dictionary<string, UnitItemBreakdown> categories, string[] uniqueCategories, int maxLength)
    {
        Dictionary<string, float[]> encoding = new Dictionary<string, float[]>();

        foreach (var category in categories)
        {
            string categoryName = category.Key;
            UnitItemBreakdown categoryData = category.Value;

            float[] categoryEncoding = new float[maxLength];

            int index = Array.IndexOf(uniqueCategories, categoryName);
            if (index >= 0)
            {
                categoryEncoding[index] = 1f;
            }

            encoding.Add(categoryName, categoryEncoding);
        }

        return encoding;
    }
    // trait encoding.
    private Dictionary<string, float[]> OneHotEncoding(Dictionary<string, List<TraitDataSet>> categories, string[] uniqueCategories, int maxLength)
    {
        Dictionary<string, float[]> encoding = new Dictionary<string, float[]>();

        foreach (var kvp in categories)
        {
            string category = kvp.Key;
            float[] categoryEncoding = new float[maxLength];

            int index = Array.IndexOf(uniqueCategories, category);
            if (index >= 0)
            {
                categoryEncoding[index] = 1f;
            }

            encoding.Add(category, categoryEncoding);
        }

        return encoding;
    }
    
    #endregion

    #region Debugging
    // everything in here was just functionality testing , to make sure i was getting the outputs i expected.
    private int CalculateObservationCount()
    {
        int observationCount = 0;

        foreach (KeyValuePair<string, float[]> kvp in encodedTraitCompResults)
        {
            float[] encodedData = kvp.Value;
            observationCount += encodedData.Length;
        }

        return observationCount;
    }
    private void DebugFrequencies()
    {
        var sortedTraits = traitFrequency.OrderByDescending(pair => pair.Value);
        foreach (var trait in sortedTraits)
        {
            Debug.Log("Trait: " + trait.Key + ", Frequency: " + trait.Value);
        }

        var sortedUnits = unitFrequency.OrderByDescending(pair => pair.Value);
        foreach (var unit in sortedUnits)
        {
            Debug.Log("Unit: " + unit.Key + ", Frequency: " + unit.Value);
        }

        var sortedAugments = augmentFrequency.OrderByDescending(pair => pair.Value);
        foreach (var augment in sortedAugments)
        {
            Debug.Log("Augment: " + augment.Key + ", Frequency: " + augment.Value);
        }
    }
    private void DebugTraitData()
    {
        foreach(var trait in dataTraitList)
        {
            Debug.Log(trait.id);
            Debug.Log(trait.name);
            Debug.Log(trait.num_of_units);
            Debug.Log("-----------------------");
            foreach(var traitActive in trait.trait_active_nums)
            {
                Debug.Log("     trait active count: " + traitActive);
            }

            foreach(var unit in trait.units)
            {
                Debug.Log("     Unit Id: " + unit.id);
                Debug.Log("     Unit name: " + unit.name);
                Debug.Log("     Unit tier: " + unit.tier);
                Debug.Log("     -----------------------");
            }
            Debug.Log("-----------------------");
        }
    }
    public void DebugObservationCount()
    {
        int numMatches = matches.Count;
        int numPlayers = matches.Values.Sum(match => match.Count);
        int observationsPerPlayer = GetObservationsPerPlayer();

        int totalObservations = numPlayers * observationsPerPlayer;
        Debug.Log("observations : " + totalObservations);

    }
    private int CountObservations()
    {
        int observationCount = 0;

        foreach (KeyValuePair<string, float[]> kvp in encodedTraitList)
        {
            float[] encodedData = kvp.Value;
            observationCount += encodedData.Length;
        }

        foreach (KeyValuePair<string, float[]> kvp in encodedItemSet)
        {
            float[] encodedData = kvp.Value;
            observationCount += encodedData.Length;
        }

        foreach (KeyValuePair<string, float[]> kvp in encodedNormalAugmentAveragePlacements)
        {
            float[] encodedData = kvp.Value;
            observationCount += encodedData.Length;
        }

        foreach (KeyValuePair<string, float[]> kvp in encodedHeroAugmentAveragePlacements)
        {
            float[] encodedData = kvp.Value;
            observationCount += encodedData.Length;
        }

        return observationCount;
    }
    private int GetObservationsPerPlayer()
    {
        int numObservationsPerPlayer = 0;

        int maxTraitEncodingLength = matches.SelectMany(match => match.Value).Max(player => player.traitEncoding.Length);
        int maxUnitEncodingLength = matches.SelectMany(match => match.Value).Max(player => player.unitEncoding.Length);
        int maxAugmentEncodingLength = matches.SelectMany(match => match.Value).Max(player => player.augmentEncoding.Length);

        numObservationsPerPlayer += maxTraitEncodingLength;
        numObservationsPerPlayer += maxUnitEncodingLength;
        numObservationsPerPlayer += maxAugmentEncodingLength;

        numObservationsPerPlayer += 6;

        return numObservationsPerPlayer;
    }
    private void DebugMatchData()
    {
        Debug.Log("Number of matches: " + matches.Count);
        foreach (var match in matches)
        {
            Debug.Log("Match ID: " + match.Key);
            Debug.Log("Number of players: " + match.Value.Count);
            
            foreach (var player in match.Value)
            {
                Debug.Log("Player ID: " + player.puuid);
            }
        }
    }
    private void DebugTheAugmentsData()
    {
        Debug.Log("NORMAL AUGMENTS");
        foreach(string augment in normalAugmentsList)
        {
            Debug.Log(augment);
        }
        Debug.Log("-------------------");

        Debug.Log("HERO AUGMENTS");
        foreach(string augment in heroAugmentsList)
        {
            Debug.Log(augment);
        }
        Debug.Log("-------------------");
    }
    #endregion

    #region Getters and Setters
    public List<string> GetHeroAugments()
    {
        return heroAugmentsList;
    }
    public List<string> GetNormalAugmentsList()
    {
        return normalAugmentsList;
    }
    public List<string> GetChampionNames()
    {
        return championNames;
    }
    public List<string> GetListOfTraits()
    {
        return listOfTraits;
    }
    #endregion


#region classes for data storage
[System.Serializable]
public class UnitItemBreakdown
{
    public string name;
    public string unit;
    public string items;
    public float averagePlacement;
    public int frequency;

    public UnitItemBreakdown(string unit, string items, float averagePlacement, int frequency, string name)
    {
        this.name = name;
        this.unit = unit;
        this.items = items;
        this.averagePlacement = averagePlacement;
        this.frequency = frequency;
    }
}

[System.Serializable]
public class TraitCompositionResult
{
    public string traitName;
    public int traitActiveNum;
    public List<int> placement;
    public List<string> units;
    public int frequency = 1;
}

[System.Serializable]
public class CompositionData
{
    public int placement { get; set; }
    public string characterId { get; set; }
    public string name { get; set; }
}
#endregion