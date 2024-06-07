using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Random = System.Random;

public enum DangerLevel
{
    NoDanger,
    LowDanger,
    MidDanger,
    HighDanger,
    BeingChased
}

enum PlayType
{
    RestBeat,
    IndividualNotes,
    Chords
} 
[RequireComponent(typeof(AudioSource))]
public class GenerativeSynth : MonoBehaviour
{
    #region Data
    
    private const float samplingRate = 44100f;
    
    // break down of chords
    // key is chord name and value is list of both string array and int array, the string array is contained notes to make the chord, and the int array is the octave those notes should be played at.
    private static Dictionary<string, List<(string[], int[])>> chordVariations = new Dictionary<string, List<(string[], int[])>>
    {
        {"Dominant_7th_Chord", new List<(string[], int[])> { (new string[] {"G", "B" , "D", "F"}, new int[] {4, 4, 4, 3}) }},
        {"Major_9th_Chord", new List<(string[], int[])> { (new string[] {"C", "E" , "G", "B", "D"}, new int[] {4, 4, 4, 4, 3}) }},
        {"Minor_7th_Chord", new List<(string[], int[])> { (new string[] {"F", "A", "C", "E"}, new int[] {4, 4, 4, 3}) }},
        {"Minor_9th_Chord", new List<(string[], int[])> { (new string[] {"A", "C", "E", "G", "B"}, new int[] {4, 4, 4, 4, 3}) }},
        {"Csus4", new List<(string[], int[])> { (new string[] {"C", "F", "G"}, new int[] {4, 4, 4}) }},
        {"C_Major", new List<(string[], int[])> { (new string[] {"C", "E" , "G"}, new int[] {4, 4, 4}) }},
        {"G_Major", new List<(string[], int[])> { (new string[] {"G", "B" , "D"}, new int[] {4, 4, 4}) }},
        {"A_Minor", new List<(string[], int[])> { (new string[] {"A", "C" , "E"}, new int[] {4, 4, 4}) }},
        {"F_Major", new List<(string[], int[])> { (new string[] {"F", "A" , "C"}, new int[] {4, 4, 4}) }}
        
    };
    
    // big dictionary of all the variations of notes of going to play, based on the current chord, and danger level.
    // key is name of chord, variation of 1 or 2 and the current danger level.
    // Value is the string array of notes to play during the next chord, and int array of the octaves of said notes.
    private static Dictionary<(string, int, DangerLevel),(string[], int[])> assosiatedNotesToChords = new Dictionary<(string, int, DangerLevel), (string[], int[])>
    {
        {("Dominant_7th_Chord", 1, DangerLevel.NoDanger), (new string[] {"C#", "E#", "G#"}, new int[] {4, 4, 4}) },
        {("Dominant_7th_Chord", 2, DangerLevel.NoDanger), (new string[] {"A#", "D#", "G#"}, new int[] {4, 4, 4}) },
        {("Dominant_7th_Chord", 1, DangerLevel.LowDanger), (new string[] {"C", "E", "G"}, new int[] {4, 4, 5}) },
        {("Dominant_7th_Chord", 2, DangerLevel.LowDanger), (new string[] {"B", "D#", "F#"}, new int[] {4, 5, 4}) },
        {("Dominant_7th_Chord", 1, DangerLevel.MidDanger), (new string[] {"E", "G#", "B"}, new int[] {4, 4, 5}) },
        {("Dominant_7th_Chord", 2, DangerLevel.MidDanger), (new string[] {"A#", "C#", "E#"}, new int[] {4, 5, 4}) },
        {("Dominant_7th_Chord", 1, DangerLevel.HighDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 5}) },
        {("Dominant_7th_Chord", 2, DangerLevel.HighDanger), (new string[] {"G#", "B", "D#"}, new int[] {4, 5, 5}) },
        {("Dominant_7th_Chord", 1, DangerLevel.BeingChased), (new string[] {"F#", "A#", "C#"}, new int[] {4, 4, 5}) },
        {("Dominant_7th_Chord", 2, DangerLevel.BeingChased), (new string[] {"G#", "B", "D#"}, new int[] {4, 4, 5}) },
        
        {("Major_9th_Chord", 1, DangerLevel.NoDanger), (new string[] {"D", "F", "A"}, new int[] {4, 4, 4}) },
        {("Major_9th_Chord", 2, DangerLevel.NoDanger), (new string[] {"E", "G", "B"}, new int[] {4, 4, 5}) },
        {("Major_9th_Chord", 1, DangerLevel.LowDanger), (new string[] {"G", "B", "D"}, new int[] {4, 4, 5}) },
        {("Major_9th_Chord", 2, DangerLevel.LowDanger), (new string[] {"A#", "C#", "E#"}, new int[] {5, 5, 5}) },
        {("Major_9th_Chord", 1, DangerLevel.MidDanger), (new string[] {"C#", "F#", "A#"}, new int[] {5, 5, 5}) },
        {("Major_9th_Chord", 2, DangerLevel.MidDanger), (new string[] {"F#", "A#", "C#"}, new int[] {5, 5, 5}) },
        {("Major_9th_Chord", 1, DangerLevel.HighDanger), (new string[] {"A#", "D#", "G#"}, new int[] {4, 5, 5}) },
        {("Major_9th_Chord", 2, DangerLevel.HighDanger), (new string[] {"C#", "E#", "G#"}, new int[] {4, 4, 5}) },
        {("Major_9th_Chord", 1, DangerLevel.BeingChased), (new string[] {"E#", "A#", "D#"}, new int[] {4, 5, 5}) },
        {("Major_9th_Chord", 2, DangerLevel.BeingChased), (new string[] {"A#", "C#", "E#"}, new int[] {5, 5, 5}) },
        
        {("Minor_7th_Chord", 1, DangerLevel.NoDanger), (new string[] {"E", "G", "B"}, new int[] {4, 4, 4}) },
        {("Minor_7th_Chord", 2, DangerLevel.NoDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 4, 4}) },
        {("Minor_7th_Chord", 1, DangerLevel.LowDanger), (new string[] {"G#", "C", "E#"}, new int[] {4, 4, 5}) },
        {("Minor_7th_Chord", 2, DangerLevel.LowDanger), (new string[] {"A#", "C#", "F"}, new int[] {4, 5, 5}) },
        {("Minor_7th_Chord", 1, DangerLevel.MidDanger), (new string[] {"C", "E", "G"}, new int[] {4, 4, 5}) },
        {("Minor_7th_Chord", 2, DangerLevel.MidDanger), (new string[] {"D#", "F#", "A#"}, new int[] {4, 5, 4}) },
        {("Minor_7th_Chord", 1, DangerLevel.HighDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 5}) },
        {("Minor_7th_Chord", 2, DangerLevel.HighDanger), (new string[] {"G#", "C", "E#"}, new int[] {4, 5, 5}) },
        {("Minor_7th_Chord", 1, DangerLevel.BeingChased), (new string[] {"A#", "C#", "F"}, new int[] {4, 5, 5}) },
        {("Minor_7th_Chord", 2, DangerLevel.BeingChased), (new string[] {"C", "E", "G"}, new int[] {5, 5, 5}) },
        
        {("Minor_9th_Chord", 1, DangerLevel.NoDanger), (new string[] {"B", "D", "F"}, new int[] {4, 4,4 }) },
        {("Minor_9th_Chord", 2, DangerLevel.NoDanger), (new string[] {"C", "E", "G"}, new int[] {4, 4, 4}) },
        {("Minor_9th_Chord", 1, DangerLevel.LowDanger), (new string[] {"E", "G", "B"}, new int[] {4, 4, 5}) },
        {("Minor_9th_Chord", 2, DangerLevel.LowDanger), (new string[] {"G", "B", "D"}, new int[] {4, 5, 4}) },
        {("Minor_9th_Chord", 1, DangerLevel.MidDanger), (new string[] {"A", "C", "E"}, new int[] {4, 4, 5}) },
        {("Minor_9th_Chord", 2, DangerLevel.MidDanger), (new string[] {"D", "F#", "A"}, new int[] {4, 5, 4}) },
        {("Minor_9th_Chord", 1, DangerLevel.HighDanger), (new string[] {"G#", "B", "D#"}, new int[] {4, 5, 5}) },
        {("Minor_9th_Chord", 2, DangerLevel.HighDanger), (new string[] {"C#", "E#", "G#"}, new int[] {4, 5, 5}) },
        {("Minor_9th_Chord", 1, DangerLevel.BeingChased), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 5}) },
        {("Minor_9th_Chord", 2, DangerLevel.BeingChased), (new string[] {"A#", "C#", "E#"}, new int[] {5, 5, 5}) },
        
        {("Csus4", 1, DangerLevel.NoDanger), (new string[] {"F", "A", "C"}, new int[] {4, 4, 4}) },
        {("Csus4", 2, DangerLevel.NoDanger), (new string[] {"G", "C", "E"}, new int[] {4, 4, 4}) },
        {("Csus4", 1, DangerLevel.LowDanger), (new string[] {"D", "G", "B"}, new int[] {4, 4, 5}) },
        {("Csus4", 2, DangerLevel.LowDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 4}) },
        {("Csus4", 1, DangerLevel.MidDanger), (new string[] {"D", "G", "B"}, new int[] {4, 4, 5}) },
        {("Csus4", 2, DangerLevel.MidDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 4}) },
        {("Csus4", 1, DangerLevel.HighDanger), (new string[] {"F#", "B", "D#"}, new int[] {4, 5, 5}) },
        {("Csus4", 2, DangerLevel.HighDanger), (new string[] {"A#", "D#", "G#"}, new int[] {4, 5, 4}) },
        {("Csus4", 1, DangerLevel.BeingChased), (new string[] {"F#", "B", "D#"}, new int[] {4, 5, 5}) },
        {("Csus4", 2, DangerLevel.BeingChased), (new string[] {"A#", "D#", "G#"}, new int[] {4, 5, 3}) },
        
        {("C_Major", 1, DangerLevel.NoDanger), (new string[] {"E", "G", "C"}, new int[] {4, 4, 5}) },
        {("C_Major", 2, DangerLevel.NoDanger), (new string[] {"G", "C", "E"}, new int[] {3, 4, 4}) },
        {("C_Major", 1, DangerLevel.LowDanger), (new string[] {"A", "D", "G"}, new int[] {4, 4, 5}) },
        {("C_Major", 2, DangerLevel.LowDanger), (new string[] {"C", "F", "A"}, new int[] {4, 5, 4}) },
        {("C_Major", 1, DangerLevel.MidDanger), (new string[] {"D", "G", "B"}, new int[] {4, 4, 5}) },
        {("C_Major", 2, DangerLevel.MidDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 4})},
        {("C_Major", 1, DangerLevel.HighDanger), (new string[] {"F#", "A#", "D#"}, new int[] {4, 5, 5}) },
        {("C_Major", 2, DangerLevel.HighDanger), (new string[] {"A#", "D#", "G#"}, new int[] {4, 5, 5}) },
        {("C_Major", 1, DangerLevel.BeingChased), (new string[] {"F#", "A#", "D#"}, new int[] {4, 5, 5}) },
        {("C_Major", 2, DangerLevel.BeingChased), (new string[] {"A#", "D#", "G#"}, new int[] {4, 5, 5}) },
        
        {("G_Major", 1, DangerLevel.NoDanger), (new string[] {"B", "D", "G"}, new int[] {4, 4, 4}) },
        {("G_Major", 2, DangerLevel.NoDanger), (new string[] {"D", "G", "B"}, new int[] {4, 4, 4}) },
        {("G_Major", 1, DangerLevel.LowDanger), (new string[] {"G", "B", "D"}, new int[] {3, 3, 4}) },
        {("G_Major", 2, DangerLevel.LowDanger), (new string[] {"B", "D", "G"}, new int[] {4, 4, 4}) },
        {("G_Major", 1, DangerLevel.MidDanger), (new string[] {"C#", "E", "G#"}, new int[] {4, 4, 5}) },
        {("G_Major", 2, DangerLevel.MidDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 4}) },
        {("G_Major", 1, DangerLevel.HighDanger), (new string[] {"C#", "E#", "G#"}, new int[] {4, 5, 5}) },
        {("G_Major", 2, DangerLevel.HighDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 5}) },
        {("G_Major", 1, DangerLevel.BeingChased), (new string[] {"C#", "E#", "G#"}, new int[] {4, 5, 5}) },
        {("G_Major", 2, DangerLevel.BeingChased), (new string[] {"F#", "A#", "C#"}, new int[] {5, 5, 5}) },
        
        {("A_Minor", 1, DangerLevel.NoDanger), (new string[] {"C", "E", "A"}, new int[] {4, 4, 4}) },
        {("A_Minor", 2, DangerLevel.NoDanger), (new string[] {"E", "A", "C"}, new int[] {4, 4, 5}) },
        {("A_Minor", 1, DangerLevel.LowDanger), (new string[] {"A", "C", "E"}, new int[] {4, 4, 5}) },
        {("A_Minor", 2, DangerLevel.LowDanger), (new string[] {"C", "E", "A"}, new int[] {4, 5, 4}) },
        {("A_Minor", 1, DangerLevel.MidDanger), (new string[] {"D#", "G#", "C#"}, new int[] {4, 4, 5}) },
        {("A_Minor", 2, DangerLevel.MidDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 4}) },
        {("A_Minor", 1, DangerLevel.HighDanger), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 5}) },
        {("A_Minor", 2, DangerLevel.HighDanger), (new string[] {"A#", "C#", "E#"}, new int[] {5, 6, 5}) },
        {("A_Minor", 1, DangerLevel.BeingChased), (new string[] {"F#", "A#", "C#"}, new int[] {4, 5, 5}) },
        {("A_Minor", 2, DangerLevel.BeingChased), (new string[] {"A#", "C#", "E#"}, new int[] {5, 5, 5}) },
        
        {("F_Major", 1, DangerLevel.NoDanger), (new string[] {"A", "C", "F"}, new int[] {4, 4, 4}) },
        {("F_Major", 2, DangerLevel.NoDanger), (new string[] {"C", "F", "A"}, new int[] {4, 4, 4}) },
        {("F_Major", 1, DangerLevel.LowDanger), (new string[] {"F", "A", "C"}, new int[] {4, 4, 5}) },
        {("F_Major", 2, DangerLevel.LowDanger), (new string[] {"A", "C", "F"}, new int[] {4, 5, 4}) },
        {("F_Major", 1, DangerLevel.MidDanger), (new string[] {"A#", "D#", "G#"}, new int[] {4, 4, 5}) },
        {("F_Major", 2, DangerLevel.MidDanger), (new string[] {"C#", "E#", "G#"}, new int[] {4, 5, 4}) },
        {("F_Major", 1, DangerLevel.HighDanger), (new string[] {"C#", "E#", "G#"}, new int[] {4, 5, 5}) },
        {("F_Major", 2, DangerLevel.HighDanger), (new string[] {"G#", "C#", "F#"}, new int[] {5, 5, 5}) },
        {("F_Major", 1, DangerLevel.BeingChased), (new string[] {"C#", "E#", "G#"}, new int[] {4, 5, 5}) },
        {("F_Major", 2, DangerLevel.BeingChased), (new string[] {"G#", "C#", "F#"}, new int[] {5, 5, 5}) }
    };
    
    // dictionary of all the notes i need, name of note and octave for key, and the frequency for the value.
    private readonly Dictionary<(string, int), float> notes = new()
    {
        { ("C", 3), 130.81f },
        { ("C#", 3), 138.59f },
        { ("D", 3), 146.83f },
        { ("D#", 3), 155.56f },
        { ("E", 3), 164.81f },
        { ("E#", 3), 169.81f },
        { ("F", 3), 174.61f },
        { ("F#", 3), 185.00f },
        { ("G", 3), 196.00f },
        { ("G#", 3), 207.65f },
        { ("A", 3), 220.00f },
        { ("A#", 3), 233.08f },
        { ("B", 3), 246.94f },
        { ("C", 4), 261.63f },
        { ("C#", 4), 277.18f },
        { ("D", 4), 293.66f },
        { ("D#", 4), 311.13f },
        { ("E", 4), 329.63f },
        { ("E#", 4), 339.63f },
        { ("F", 4), 349.23f },
        { ("F#", 4), 369.99f },
        { ("G", 4), 392.00f },
        { ("G#", 4), 415.30f },
        { ("A", 4), 440.00f },
        { ("A#", 4), 466.16f },
        { ("B", 4), 493.88f },
        { ("C", 5), 523.25f },
        { ("C#", 5), 554.37f },
        { ("D", 5), 587.33f },
        { ("D#", 5), 622.25f },
        { ("E", 5), 659.25f },
        { ("E#", 5), 679.25f },
        { ("F", 5), 698.46f },
        { ("F#", 5), 739.99f },
        { ("G", 5), 783.99f },
        { ("G#", 5), 830.61f },
        { ("A", 5), 880.00f },
        { ("A#", 5), 932.33f },
        { ("B", 5), 987.77f },
        { ("C", 6), 1046.50f },
        { ("C#", 6), 1108.73f },
        { ("D", 6), 1174.66f },
        { ("D#", 6), 1244.51f },
        { ("E", 6), 1318.51f },
        { ("E#", 6), 1368.71f },
        { ("F", 6), 1396.91f },
        { ("F#", 6), 1479.98f },
        { ("G", 6), 1567.98f },
        { ("G#", 6), 1661.22f },
        { ("A", 6), 1760.00f },
        { ("A#", 6), 1864.66f },
        { ("B", 6), 1975.53f }
    };
    
    #endregion
    
    private float chordDuration = 3f;
    
    private FilterButterworth filter;
    private PlayType playType = PlayType.Chords;
    private readonly float lowPassCutoff = 8000f;
    private readonly float lowPassResonance = 0.1f;

    private bool isRunning = false;
    
    private float[] noteData;
    private float[] chordData;
    private float[] actionData;
    
    private IEnumerator<KeyValuePair<(string, int), (PlayType, float)>> actionEnumerator;

    private List<(string, int, float)> currentNoteSequence = new List<(string, int, float)>();
    private float chordTimer = 0f;
 
    float timeSinceLastAdditionalNote = 0f;
    private int noteInterator = 0;
    float individualNoteFrequency = 0;
    float individualNotePlayed = 0;
    private bool playingIndividualNotes = false;
    private bool needNewNotes = false;
    private float lastFilteredOutput = 0f;
    private Random random = new System.Random();
    private List<string> chordNotes = new List<string>();
    private List<int> chordOctaves = new List<int>();
    private string currentChordName = "";
    private float timer;
    private Dictionary<(string, int), float> chordFrequencies;
    private float maxRandCordLength = 3;
    private float minRandCordLength = 1.5f;
    private float randForChordLength = 0f;
    private int randForAssosiatedNotes = 0;
    private float noteDuration = 0f;
    private DangerLevel dangerLevel = DangerLevel.NoDanger;
    private float randAmp1 = 0;
    private float randAmp2 = 0;
    private float randAmp3 = 0;
    private float randAmp4 = 0;
    private int currentSizeOfChordsList = 0;
    [SerializeField] private float vibratoAmount = 0.02f;
    [SerializeField] private float vibratoFrequency = 5f;
    [SerializeField] private float tremoloFrequency = 3.0f;
    [SerializeField] private float tremoloAmount = 0.2f;
    [SerializeField] private LevelUIManager manager;
    [SerializeField] private List<Transform> enemies;
    
    private float previousSineWave = 1f;
    private float previousTriangleWave = 1f;
    private float previousSquareWave = 1f;
    
    private Transform player;
    private void Start()
    {
        // init variables for chords and notes.
        RandVars();
        
        // init filter.
        filter = new FilterButterworth(lowPassCutoff, (int)samplingRate, PassType.Lowpass, lowPassResonance);
        
        // generate first chord and notes.
        GenerateChordAction();
        GenerateIndividualNoteAction();
        
        // start running the music.
        isRunning = true;
    }
    
    private void GenerateIndividualNoteAction()
    {
        // clear out container.
        currentNoteSequence.Clear();
        
        // chooses random notes, but its more based off the senario the player is in than truely random, the random element is just which variation they get.
        (string[], int[]) container = GetRandomNoteSequence();
       
        // generate note lengths based of the chord duration.; 
        float[] noteLengths = GenerateRandomNoteLengths(chordDuration, 0.025f);
       
        // go through all the notes 
        for (int i = 0; i < container.Item1.Length; i++)
        {
            currentNoteSequence.Add((container.Item1[i], container.Item2[i], noteLengths[i]));
        }
        
        // ready to play the new notes.
        playingIndividualNotes = true;
        currentSizeOfChordsList = currentNoteSequence.Count;
    }
    
    private void GenerateChordAction()
    {
        // reset chord containers.
        chordNotes.Clear();
        chordOctaves.Clear();
    
        // generate the random chord to be played based off dictionary.
        KeyValuePair<string, List<(string[], int[])>> currentChordInfo = GetRandomChord();
        List<(string[], int[])> container = currentChordInfo.Value;
        
        // record the chord name.
        currentChordName = currentChordInfo.Key;
        
        // save the duration here since it seems to causes the least in issues in the OnAudioFilterRead function when doing time stuff.
        chordDuration = randForChordLength;
        
        // breaking down the notes and octaves returned.
        foreach (string note in container[0].Item1)
        {
            chordNotes.Add(note);
        }
        
        foreach (int oct in container[0].Item2)
        {
            chordOctaves.Add(oct);
        }
    }
    
    private KeyValuePair<string, List<(string[], int[])>> GetRandomChord()
    {
        // just returns a random KVP from chords.
        int index = random.Next(chordVariations.Count);
        return chordVariations.ElementAt(index);
    }

    private (string[], int[]) GetRandomNoteSequence()
    {
        // sifts through the assosiatedNotesToChordDictionary, use chord name, a random number either 1/2 so theres some variation and the current danger level for the intensity.
        (string[], int[]) notesAndOctaves = (new string[]{}, new int[]{});

        notesAndOctaves = assosiatedNotesToChords[(currentChordName ,randForAssosiatedNotes, dangerLevel)];

        return notesAndOctaves;
    }
    static float[] GenerateRandomNoteLengths(float chordDuration, float maxVariationPercentage)
    {
        // generates random note lengths that wont be longer than the chord but can add some personality to the composition.
        int numNotes = 3;
        float[] noteLengths = new float[numNotes];
        
        float minNoteLength = chordDuration / numNotes;
        float maxVariation = chordDuration * maxVariationPercentage;

        Random random = new Random();

        // Generate random note lengths
        float remainingTime = chordDuration;
        for (int i = 0; i < numNotes - 1; i++)
        {
            float randomFactor = (float)random.NextDouble();
            float randomNoteLength = minNoteLength + randomFactor * maxVariation;

            noteLengths[i] = randomNoteLength;
            remainingTime -= randomNoteLength;
        }
        
        noteLengths[numNotes - 1] = remainingTime;

        return noteLengths;
    }
    
    private void Update()
    {
        RandVars();
        
        switch (dangerLevel)
        {
            // altering the chord times, based off the danger so its reactive to the moment.
            case DangerLevel.NoDanger:
                minRandCordLength = 1.85f;
                maxRandCordLength = 2.35f;
                break;
            case DangerLevel.LowDanger:
                minRandCordLength = 1.5f;
                maxRandCordLength = 1.85f;
                break;
            case DangerLevel.MidDanger:
                minRandCordLength = 1.3f;
                maxRandCordLength = 1.65f;
                break;
            case DangerLevel.HighDanger:
                minRandCordLength = 1.15f;
                maxRandCordLength = 1.5f;
                break;
            case DangerLevel.BeingChased:
                minRandCordLength = 1.0f;
                maxRandCordLength = 1.3f;
                break;
        }

      
        
        foreach (Transform enemy in enemies)
        {
            AI ai = enemy.GetComponent<AI>();
            if (ai.GetState() == State.Chasing)
            {
                dangerLevel = DangerLevel.BeingChased;
                break;
            }
            else
            {
                dangerLevel = DangerLevel.NoDanger;
            }
        }
        noteDuration = currentNoteSequence[noteInterator].Item3;
        
        CalculateDangerLevel();
    }
    
    private void RandVars()
    {
        // doing random values in update since OnAudioFilter really doesnt like it.
        randAmp1 = UnityEngine.Random.Range(0.3f, 0.5f);
        randAmp2 = UnityEngine.Random.Range(0.2f, 0.4f);
        randAmp3 = UnityEngine.Random.Range(0.1f, 0.3f);
        randAmp4 = UnityEngine.Random.Range(0.05f, 0.2f);
        
        randForChordLength =  UnityEngine.Random.Range(maxRandCordLength, minRandCordLength);
        randForAssosiatedNotes = UnityEngine.Random.Range(1, 3);
    }
    private void CalculateDangerLevel()
    {
        Transform closestEnemy = FindClosestEnemy();
        
        if (closestEnemy != null && dangerLevel != DangerLevel.BeingChased)
        {
            float distance = Vector3.Distance(player.position, closestEnemy.position);
            
            dangerLevel = GetDangerLevel(distance);
        }
        
        Debug.Log($"Danger Level: {dangerLevel}");
    }
    
    private Transform FindClosestEnemy()
    {
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Transform enemyTransform in enemies)
        {
            AI ai = enemyTransform.GetComponent<AI>();
            
            if (ai.GetState() == State.Inactive)
            {
                enemies.Remove(enemyTransform);
                continue;
            }
            
          
            float distance = Vector3.Distance(player.position, enemyTransform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyTransform;
            }
        }

        return closestEnemy;
    }

    public void SetPlayerTransform(Transform newTransform)
    {
        player = newTransform;
    }
    
    private DangerLevel GetDangerLevel(float distance)
    {
        float percentage = distance / 50f;

        if (percentage >= 0.75f)
        {
            return DangerLevel.LowDanger;
        }
        else if (percentage >= 0.5f)
        {
            return DangerLevel.MidDanger;
        }
        else if (percentage >= 0.25f)
        {
            return DangerLevel.HighDanger;
        }

        return DangerLevel.NoDanger;
    }
    
    // comment this.
    private void OnAudioFilterRead(float[] data, int channels)
{
    // Check if the audio system is running
    if (!isRunning) return;

    // Loop through the audio data
    for (var i = 0; i < data.Length; i += channels)
    {
        switch (playType)
        {
            case PlayType.Chords:
                // Initialize the total sound for the chord
                float chordSound = 0f;

                // Iterate through each note in the chord
                for (int k = 0; k < chordNotes.Count; k++)
                {
                    // Get the base frequency of the note
                    float noteFreqBaseChord = notes[(chordNotes[k], chordOctaves[k])];

                    // Set the amplitude and frequency for the note
                    float amplitude = 0.5f;
                    float frequency = noteFreqBaseChord;

                    // Generate a sine wave for the note
                    float sineWave = Mathf.Sin(2f * Mathf.PI * frequency * chordTimer);

                    // Apply ADSR envelope to the note
                    frequency = amplitude * sineWave;
                    frequency *= CalculateADSR(chordTimer, noteDuration, 0.1f, 0.1f, 0.8f, 0.2f);

                    // Apply low-pass filtering and normalization
                    float lowPassFilteredNote = LowPassFilter(frequency, 400f, samplingRate);
                    lowPassFilteredNote = Normalise(lowPassFilteredNote, 0.01f, 0.35f);

                    // Accumulate the note sound to the total chord sound
                    chordSound += lowPassFilteredNote;
                }

                // Process the total chord sound through a filter
                chordSound = filter.Process(chordSound);

                // Add the chord sound to both audio channels
                data[i] += chordSound;
                data[i + 1] += chordSound;

                // Check if playing individual notes is enabled and new notes are not needed
                if (playingIndividualNotes && !needNewNotes)
                {
                    // Check if there are more notes in the sequence
                    if (noteInterator < currentNoteSequence.Count)
                    {
                        // Check if enough time has passed for the next additional note
                        if (timeSinceLastAdditionalNote >= noteDuration)
                        {
                            // Set the frequency for the next additional note
                            individualNoteFrequency = notes[(currentNoteSequence[noteInterator].Item1, currentNoteSequence[noteInterator].Item2)];
                            Debug.Log("Starting new note");
                            timeSinceLastAdditionalNote = 0f;
                            noteInterator++;
                        }
                    }

                    // Initialize the low-pass filtered note sound
                    float lowPassFilteredNote = 0f;

                    // Check the danger level for specific note generation
                    if (dangerLevel == DangerLevel.BeingChased)
                    {
                        // Set amplitudes and frequency for sine and triangle waves
                        float amplitudeSine = 0.5f;
                        float amplitudeTriangle = 0.3f;
                        float frequency = individualNoteFrequency;

                        // Generate sine and triangle waves
                        float sineWave = Mathf.Sin(2f * Mathf.PI * frequency * chordTimer);
                        float triangleWave = Mathf.PingPong(2f * Mathf.PI * frequency * chordTimer, 2f) - 1f;

                        // Combine sine and triangle waves, apply ADSR, modulation effects, and filtering
                        individualNotePlayed = amplitudeSine * sineWave + amplitudeTriangle * triangleWave;
                        individualNotePlayed *= CalculateADSR(chordTimer, chordDuration, 0.2f, 0.1f, 0.6f, 0.2f);
                        individualNotePlayed = ApplyModulationEffects(individualNotePlayed, chordTimer);

                        lowPassFilteredNote = LowPassFilter(individualNotePlayed, 750f, samplingRate);
                        lowPassFilteredNote = Normalise(individualNotePlayed, -0.25f, 0.5f);
                    }
                    else
                    {
                        // Set frequency for harmonics
                        float frequency = individualNoteFrequency;

                        // Generate harmonics, apply ADSR, modulation effects, and filtering
                        float harmonic1 = Mathf.Sin(2f * Mathf.PI * frequency * chordTimer);
                        float harmonic2 = Mathf.Sin(2f * Mathf.PI * 2f * frequency * chordTimer);
                        float harmonic3 = Mathf.Sin(2f * Mathf.PI * 3f * frequency * chordTimer);
                        float harmonic4 = Mathf.Sin(2f * Mathf.PI * 4f * frequency * chordTimer);

                        individualNotePlayed = randAmp1 * harmonic1 +
                                               randAmp2 * harmonic2 +
                                               randAmp3 * harmonic3 +
                                               randAmp4 * harmonic4;

                        individualNotePlayed *= CalculateADSR(chordTimer, noteDuration, 0.08f, 0.1f, 0.15f, 0.1f);
                        individualNotePlayed = ApplyModulationEffects(individualNotePlayed, chordTimer);

                        lowPassFilteredNote = LowPassFilter(individualNotePlayed, 350f, samplingRate);
                        lowPassFilteredNote = Normalise(individualNotePlayed, -0.15f, 0.35f);
                    }

                    // Process the low-pass filtered note sound through a filter
                    lowPassFilteredNote = filter.Process(lowPassFilteredNote);

                    // Add the low-pass filtered note sound to both audio channels
                    data[i] += lowPassFilteredNote;
                    data[i + 1] += lowPassFilteredNote;
                }

                // Update chord timer and time since the last additional note
                chordTimer += 1f / samplingRate;
                timeSinceLastAdditionalNote += 1f / samplingRate;

                // Check if the chord duration has been reached
                if (chordTimer >= chordDuration)
                {
                    // Reset timers and generate new chord and additional notes
                    chordTimer = 0f;
                    GenerateChordAction();
                    GenerateIndividualNoteAction();
                    noteInterator = 0;
                    timeSinceLastAdditionalNote = 0f;
                }
                break;
        }
    }
}

    float GeneratePianoChord(float[] noteFrequencies, float chordTimer, float chordDuration)
    {
        float chordAmplitude = 0f;

        // Iterate through each note in the chord
        for (int i = 0; i < noteFrequencies.Length; i++)
        {
            float noteAmplitude = GeneratePianoSound(noteFrequencies[i], chordTimer, chordDuration);

            // Adjust the amplitude of each note in the chord
            noteAmplitude *= 1.0f / noteFrequencies.Length;

            // Sum up the amplitudes of all notes in the chord
            chordAmplitude += noteAmplitude;
        }

        // Limit the amplitude to avoid clipping
       chordAmplitude *= CalculateADSR(chordTimer, chordDuration, 0.075f, 0.2f, 0.35f, 0.1f);
       chordAmplitude = LowPassFilter(chordAmplitude, 150f, samplingRate);
       chordAmplitude = Normalise(chordAmplitude, -0.15f, 0.15f);
        return chordAmplitude * 0.35f;
    }
    float GeneratePianoSound(float noteFrequency, float chordTimer, float chordDuration)
    {
        float amplitude = 0f;

        // Add harmonics 
        float fundamental = Mathf.Sin(2f * Mathf.PI * noteFrequency * chordTimer);
        float harmonic1 = Mathf.Sin(2f * Mathf.PI * noteFrequency * 2f * chordTimer) * 0.5f;
        float harmonic2 = Mathf.Sin(2f * Mathf.PI * noteFrequency * 3f * chordTimer) * 0.3f;

        // Combine harmonics
        amplitude = fundamental + harmonic1 + harmonic2;

        // Add key release for natural sound
        float keyReleaseNoise = Mathf.Pow(2, -10 * chordTimer) * Mathf.Sin(2f * Mathf.PI * 2000f * chordTimer);
        amplitude += keyReleaseNoise;

        // Apply ADSR envelope
        amplitude *= CalculateADSR(chordTimer, chordDuration, 0.1f, 0.2f, 0.5f, 0.1f);
        amplitude = LowPassFilter(amplitude, 250f, samplingRate);
        amplitude = Normalise(amplitude, -0.15f, 0.35f);
       
        // Limit the amplitude to avoid clipping
        amplitude = Mathf.Clamp(amplitude, -1f, 1f);

        return amplitude;
    }

    private void TestIntenseSound(float nextNote)
    {
        
        float smoothnessFactor = 0.05f; 
        float smoothingFactor = 0.1f;    
        float crossfadeFactor = 0.1f;  
        float amplitudeSine = 0.4f;
        float amplitudeTriangle = 0.3f;
        float amplitudeSquare = 0.2f;
        float frequency = individualNoteFrequency;
                
        frequency = Mathf.Lerp(frequency,nextNote, smoothnessFactor);
                
        float sineWave = Mathf.Sin(2f * Mathf.PI * frequency * chordTimer);
        float triangleWave = Mathf.PingPong(2f * Mathf.PI * frequency * chordTimer, 1f) * 2f - 1f;
        float squareWave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * frequency * chordTimer));
                
        float smoothedSineWave = Mathf.Lerp(previousSineWave, sineWave, smoothingFactor);
        float smoothedTriangleWave = Mathf.Lerp(previousTriangleWave, triangleWave, smoothingFactor);
        float smoothedSquareWave = Mathf.Lerp(previousSquareWave, squareWave, smoothingFactor);
                
        float amplitudeSum = amplitudeSine + amplitudeTriangle + amplitudeSquare;
        if (amplitudeSum > 1.0f)
        {
            amplitudeSine /= amplitudeSum;
            amplitudeTriangle /= amplitudeSum;
            amplitudeSquare /= amplitudeSum;
        }
                
        float individualNotePlayed = amplitudeSine * smoothedSineWave +
                                        amplitudeTriangle * smoothedTriangleWave +
                                        amplitudeSquare * smoothedSquareWave;
                
        individualNotePlayed *= crossfadeFactor;

        individualNotePlayed *= CalculateADSR(chordTimer, chordDuration, 0.1f, 0.2f, 0.3f, 0.1f);
                
        previousSineWave = smoothedSineWave;
        previousTriangleWave = smoothedTriangleWave;
        previousSquareWave = smoothedSquareWave;

        if (noteInterator == 3)
        {
            individualNotePlayed = 0;
        }
    }
    private float ApplyModulationEffects(float input, float timer)
    {
        // Vibrato
        float vibrato = Mathf.Sin(2 * Mathf.PI * vibratoFrequency * timer) * vibratoAmount;

        // Tremolo
        float tremolo = (Mathf.Sin(2 * Mathf.PI * tremoloFrequency * timer) + 1) * tremoloAmount;

        // Apply effects
        return input * (1.0f + vibrato) * tremolo;
    }
    private float LowPassFilter(float input, float cutoffFrequency, float samplingRate)
    {
        // Calculate the time constant
        float tc = 1.0f / (cutoffFrequency * 2 * Mathf.PI);

        // Calculate the time step based on the sampling rate
        float dt = 1.0f / samplingRate;

        // Calculate smoothing factor
        float alpha = dt / (tc + dt);
    
        // Apply it to filter the input
        float filteredOutput = alpha * input + (1 - alpha) * lastFilteredOutput;
        
        lastFilteredOutput = filteredOutput;
        
        return filteredOutput;
    }
    private float Normalise(float input, float minAmplitude, float maxAmplitude)
    {
        // basic normalise function
        float maxAbs = Mathf.Max(Mathf.Abs(input), 1);
        // claming then values to get clean audio,
        float scaleFactor = Mathf.Clamp(maxAmplitude / maxAbs, minAmplitude, maxAmplitude);
        return input * scaleFactor;
    }
    private float CalculateADSR(float time, float duration, float attackTimeNew, float floatDecayTimeNew, float releaseTimeNew, float sustainLevelNew)
    {
        var envelope = 0f;

        if (time < attackTimeNew)
            envelope = Mathf.Lerp(0f, 1f, time / attackTimeNew);
        else if (time < attackTimeNew + floatDecayTimeNew)
            envelope = Mathf.Lerp(1f, sustainLevelNew, (time - attackTimeNew) / floatDecayTimeNew);
        else if (time < duration - releaseTimeNew)
            envelope = sustainLevelNew;
        else
            envelope = Mathf.Lerp(sustainLevelNew, 0f, (time - (duration - releaseTimeNew)) / releaseTimeNew);

        return envelope;
    }
}

public enum PassType
{
    Lowpass,
    Highpass
}

#region Filter

public class FilterButterworth
{
    private float a1, a2, a3, b1, b2;

    private float c;
    private readonly float frequency;
    private readonly PassType passType;
    private readonly float resonance;
    private readonly int sampleRate;
    private float x1, x2, y1, y2;

    public FilterButterworth(float frequency, int sampleRate, PassType passType, float resonance)
    {
        this.resonance = resonance;
        this.frequency = frequency;
        this.sampleRate = sampleRate;
        this.passType = passType;

        ComputeCoefficients();
    }

    private void ComputeCoefficients()
    {
        c = 1.0f / Mathf.Tan(Mathf.PI * frequency / sampleRate);

        switch (passType)
        {
            // try messing around with these a bit more
            case PassType.Lowpass:
                a1 = 1.0f / (1.0f + resonance * c + c * c);
                a2 = 2f * a1;
                a3 = a1;
                b1 = 2.0f * (1.0f - c * c) * a1;
                b2 = (1.0f - resonance * c + c * c) * a1;
                break;
            case PassType.Highpass:
                a1 = 1.0f / (1.0f + resonance * c + c * c);
                a2 = -2f * a1;
                a3 = a1;
                b1 = 2.0f * (c * c - 1.0f) * a1;
                b2 = (1.0f - resonance * c + c * c) * a1;
                break;
        }
    }

    public float Process(float input)
    {
        var output = 0f;

        switch (passType)
        {
            case PassType.Lowpass:
                output = a1 * input + a2 * x1 + a3 * x2 - b1 * y1 - b2 * y2;
                x2 = x1;
                x1 = input;
                y2 = y1;
                y1 = output;
                break;
            case PassType.Highpass:
                output = a1 * input + a2 * x1 + a3 * x2 - b1 * y1 - b2 * y2;
                x2 = x1;
                x1 = input;
                y2 = y1;
                y1 = output;
                break;
        }

        return output;
    }
}

#endregion