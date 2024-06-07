
    void Awake()
    {
        jsonString = new string[jsonFile.Length];
        agentDataList = new List<DataForAgent>();
        traitDataList = new List<TraitJsonDataWrapper>();
        jsonRefString = jsonUnitRefFile.text;

        SetupReferenceMaterial();
        SetupTFTMaterial();
    }

    private void SetupTFTMaterial()
    {
        for (int i = 0; i < jsonFile.Length; i++)
        {
            // deserialize the json object of the file and fill all the match data 
            jsonString[i] = jsonFile[i].text;
            Match[] matches = JsonConvert.DeserializeObject<Match[]>(jsonString[i]);
            
            // loops through all the matches and players in each match to gather the match data I want to pass to the Agent
            foreach (Match match in matches)
            {
                foreach (string participantId in match.metadata.participants)
                {
                    Participant participant = FindParticipantById(participantId, match.info.participants);

                    if (participant != null)
                    {
                        DataForAgent agentData = new DataForAgent
                        {
                            match_id = match.metadata.match_id,
                            game_length = match.info.game_length,
                            augments = participant.augments,
                            gold_left = participant.gold_left,
                            last_round = participant.last_round,
                            level = participant.level,
                            placement = participant.placement,
                            traits = participant.traits,
                            units = participant.units,
                            total_damage_to_players = participant.total_damage_to_players,
                            players_eliminated = participant.players_eliminated,
                            puuid = participant.puuid,
                        };
                        // performs some of the encoding here for the traits, units and augments.
                        agentData.PerformOneHotEncoding();
                        agentDataList.Add(agentData);

                        // Pass the agentData to the Agent class
                        agent.ReceiveMatchData(agentData);
                    }
                }
            }
        }
    }

    private void SetupReferenceMaterial()
    {
        // this method was impmented later to add my hand written reference file which helped increase the refining of information to give the AI to learn form.
        TraitJsonDataWrapper traitJsonDataWrapper = JsonConvert.DeserializeObject<TraitJsonDataWrapper>(jsonRefString);

        if (traitJsonDataWrapper != null && traitJsonDataWrapper.tft_trait != null)
        {
            foreach (var kvp in traitJsonDataWrapper.tft_trait)
            {
                string traitId = kvp.Key;
                TraitDataSet traitData = kvp.Value;

                //traitDataList.Add(traitData);
                agent.ReceiveTraitData(traitData);
            }
        }
        else
        {
            Debug.Log("Failed at SetupReferenceMaterial().");
        }
    }

    private Participant FindParticipantById(string participantId, Participant[] participants)
    {
        foreach (Participant participant in participants)
        {
            if (participant.puuid == participantId)
            {
                return participant;
            }
        }

        return null;
    }


#region Classes For Storing Objects From Json Data
    [System.Serializable]
    public class DataForAgent
    {
        public string match_id;
        public float game_length;
        public int gold_left;
        public int last_round;
        public int level;
        public int placement;
        public int total_damage_to_players;
        public int players_eliminated;
        public string puuid;
        public string[] augments;
        public Trait[] traits;
        public Units[] units;
        public float[] traitEncoding;
        public float[] unitEncoding;
        public float[] augmentEncoding;
        private int TraitMax = 19;
        private int unitMax = 15;
        private int augmentMax = 3;

        public void PerformOneHotEncoding()
        {
            var uniqueTraits = traits.Select(t => t.name).Distinct().ToArray();
            traitEncoding = OneHotEncoding(traits.Select(t => t.name).ToArray(), uniqueTraits, TraitMax);

            var uniqueUnits = units.Select(u => u.name).Distinct().ToArray();
            unitEncoding = OneHotEncoding(units.Select(u => u.name).ToArray(), uniqueUnits, unitMax);

            var uniqueAugments = augments.Distinct().ToArray();
            augmentEncoding = OneHotEncoding(augments, uniqueAugments, augmentMax);
        }

        private float[] OneHotEncoding(string[] categories, string[] uniqueCategories, int maxLength)
        {
            float[] encoding = new float[maxLength];

            for (int i = 0; i < categories.Length; i++)
            {
                int index = Array.IndexOf(uniqueCategories, categories[i]);
                encoding[i] = 1f;
            }

            return encoding;
        }

    }

    [System.Serializable]
    public class Match
    {
        public Metadata metadata;
        public Info info;
    }

    [System.Serializable]
    public class Metadata
    {
        public string data_version;
        public string match_id;
        public string[] participants;
    }

    [System.Serializable]
    public class Info
    {
        public long game_datetime;
        public float game_length;
        public string game_version;
        public Participant[] participants;
        public int queue_id;
        public string tft_game_type;
        public string tft_set_core_name;
        public int tft_set_number;
    }

    [System.Serializable]
    public class Participant
    {
        public string[] augments;
        public Companion companion;
        public int gold_left;
        public int last_round;
        public int level;
        public int placement;
        public int players_eliminated;
        public string puuid;
        public float time_eliminated;
        public int total_damage_to_players;
        public Trait[] traits;
        public Units[] units;
    }

    [System.Serializable]
    public class Companion
    {
        public string content_ID;
        public int item_ID;
        public int skin_ID;
        public string species;
    }

    [System.Serializable]
    public class Trait
    {
        public string name;
        public int num_units;
        public int style;
        public int tier_current;
        public int tier_total;
    }

    [System.Serializable]
    public class Units
    {
        public string character_id;
        public string[] itemNames;
        public string name;
        public int rarity;
        public int tier;
    }

    [System.Serializable]
    public class TraitJsonDataWrapper
    {
        public Dictionary<string, TraitDataSet> tft_trait { get; set; }
    }

    [System.Serializable]
    public class TraitDataSet
    {
        public string id { get; set; }
        public string name { get; set; }
        public int num_of_units { get; set; }
        public List<int> trait_active_nums { get; set; }
        public List<UnitData> units { get; set; }
    }

    [System.Serializable]
    public class UnitData
    {
        public string id { get; set; }
        public string name { get; set; }
        public int tier { get; set; }
    }
#endregion