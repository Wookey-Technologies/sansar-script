//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;
using System.Collections.Generic;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;


public class gotMojoLooper : SceneObjectScript
{

    #region ConstantsVariables
    private const int c0 = 12; const int db0 = 13; const int d0 = 14; const int eb0 = 15; const int e0 = 16; const int f0 = 17; const int gb0 = 18; const int g0 = 19; const int ab0 = 20; const int a0 = 21; const int bb0 = 22; const int b0 = 23;
    private const int c1 = 24; const int db1 = 25; const int d1 = 26; const int eb1 = 27; const int e1 = 28; const int f1 = 29; const int gb1 = 30; const int g1 = 31; const int ab1 = 32; const int a1 = 33; const int bb1 = 34; const int b1 = 35;
    private const int c2 = 36; const int db2 = 37; const int d2 = 38; const int eb2 = 39; const int e2 = 40; const int f2 = 41; const int gb2 = 52; const int g2 = 43; const int ab2 = 44; const int a2 = 45; const int bb2 = 46; const int b2 = 47;
    private const int c3 = 48; const int db3 = 49; const int d3 = 50; const int eb3 = 51; const int e3 = 52; const int f3 = 53; const int gb3 = 54; const int g3 = 55; const int ab3 = 56; const int a3 = 57; const int bb3 = 58; const int b3 = 59;
    private const int c4 = 60; const int db4 = 61; const int d4 = 62; const int eb4 = 63; const int e4 = 64; const int f4 = 65; const int gb4 = 66; const int g4 = 67; const int ab4 = 68; const int a4 = 69; const int bb4 = 70; const int b4 = 71;
    private const int c5 = 72; const int db5 = 73; const int d5 = 74; const int eb5 = 75; const int e5 = 76; const int f5 = 77; const int gb5 = 78; const int g5 = 79; const int ab5 = 80; const int a5 = 81; const int bb5 = 82; const int b5 = 83;
    private const int c6 = 84; const int db6 = 85; const int d6 = 86; const int eb6 = 87; const int e6 = 88; const int f6 = 89; const int gb6 = 90; const int g6 = 91; const int ab6 = 92; const int a6 = 93; const int bb6 = 94; const int b6 = 95;
    private const int c7 = 96; const int db7 = 97; const int d7 = 98; const int eb7 = 99; const int e7 = 100; const int f7 = 101; const int gb7 = 102; const int g7 = 103; const int ab7 = 104; const int a7 = 105; const int bb7 = 106; const int b7 = 107;
    private const int c8 = 108; const int db8 = 109; const int d8 = 110; const int eb8 = 111; const int e8 = 112; const int f8 = 113; const int gb8 = 114; const int g8 = 115; const int ab8 = 116; const int a8 = 117; const int bb8 = 118; const int b8 = 119;
    private const int c9 = 120; const int db9 = 121; const int d9 = 122; const int eb9 = 123; const int e9 = 124; const int f9 = 125; const int gb9 = 126; const int g9 = 127;

    //scales
    private static int[] major = {2, 2, 1, 2, 2, 2, 1};
    private static int[] dorian = {1, 2, 2, 1, 2, 2, 2};
    private static int[] phrygian = {2, 1, 2, 2, 1, 2, 2};
    private static int[] lydian = {2, 2, 1, 2, 2, 1, 2};
    private static int[] mixolydian = {2, 2, 2, 1, 2, 2, 1};
    private static int[] aelian = {1, 2, 2, 2, 1, 2, 2};
    private static int[] minor = {2, 1, 2, 2, 2, 1, 2};
    private static int[] minor_pentatonic = { 3, 2, 2, 3, 2 };
    private static int[] major_pentatonic = { 2, 3, 2, 2, 3 };
    private static int[] egyptian = { 3, 2, 3, 2, 2 };
    private static int[] jiao = { 2, 3, 2, 3, 2 };
    private static int[] zhi = { 2, 2, 3, 2, 3 };
    private static int[] whole_tone = {2, 2, 2, 2, 2, 2};
    private static int[] whole = {2, 2, 2, 2, 2, 2};
    private static int[] chromatic = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
    private static int[] harmonic_minor = {2, 1, 2, 2, 1, 3, 1};
    private static int[] melodic_minor_asc = {2, 1, 2, 2, 2, 2, 1};
    private static int[] hungarian_minor = {2, 1, 3, 1, 1, 3, 1};
    private static int[] octatonic = {2, 1, 2, 1, 2, 1, 2, 1};
    private static int[] messiaen1 = {2, 2, 2, 2, 2, 2};
    private static int[] messiaen2 = {1, 2, 1, 2, 1, 2, 1, 2};
    private static int[] messiaen3 = {2, 1, 1, 2, 1, 1, 2, 1, 1};
    private static int[] messiaen4 = {1, 1, 3, 1, 1, 1, 3, 1};
    private static int[] messiaen5 = {1, 4, 1, 1, 4, 1};
    private static int[] messiaen6 = {2, 2, 1, 1, 2, 2, 1, 1};
    private static int[] messiaen7 = {1, 1, 1, 2, 1, 1, 1, 1, 2, 1};
    private static int[] super_locrian = {1, 2, 1, 2, 2, 2, 2};
    private static int[] hirajoshi = {2, 1, 4, 1, 4};
    private static int[] kumoi = {2, 1, 4, 2, 3};
    private static int[] neapolitan_major = {1, 2, 2, 2, 2, 2, 1};
    private static int[] bartok = {2, 2, 1, 2, 1, 2, 2};
    private static int[] bhairav = {1, 3, 1, 2, 1, 3, 1};
    private static int[] locrian_major = {2, 2, 1, 1, 2, 2, 2};
    private static int[] ahirbhairav = {1, 3, 1, 2, 2, 1, 2};
    private static int[] enigmatic = {1, 3, 2, 2, 2, 1, 1};
    private static int[] neapolitan_minor = {1, 2, 2, 2, 1, 3, 1};
    private static int[] pelog = {1, 2, 4, 1, 4};
    private static int[] augmented2 = {1, 3, 1, 3, 1, 3};
    private static int[] scriabin = {1, 3, 3, 2, 3};
    private static int[] harmonic_major = {2, 2, 1, 2, 1, 3, 1};
    private static int[] melodic_minor_desc = {2, 1, 2, 2, 1, 2, 2};
    private static int[] romanian_minor = {2, 1, 3, 1, 2, 1, 2};
    private static int[] hindu = {2, 2, 1, 2, 1, 2, 2};
    private static int[] iwato = {1, 4, 1, 4, 2};
    private static int[] melodic_minor = {2, 1, 2, 2, 2, 2, 1};
    private static int[] diminished2 = {2, 1, 2, 1, 2, 1, 2, 1};
    private static int[] marva = {1, 3, 2, 1, 2, 2, 1};
    private static int[] melodic_major = {2, 2, 1, 2, 1, 2, 2};
    private static int[] indian = {4, 1, 2, 3, 2};
    private static int[] spanish = {1, 3, 1, 2, 1, 2, 2};
    private static int[] prometheus = {2, 2, 2, 5, 1};
    private static int[] diminished = {1, 2, 1, 2, 1, 2, 1, 2};
    private static int[] todi = {1, 2, 3, 1, 1, 3, 1};
    private static int[] leading_whole = {2, 2, 2, 2, 2, 1, 1};
    private static int[] augmented = {3, 1, 3, 1, 3, 1};
    private static int[] purvi = {1, 3, 2, 1, 1, 3, 1};
    private static int[] chinese = {4, 2, 1, 4, 1};
    //chords
    private static int[] chdmajor = { 0, 4, 7 };
    private static int[] chdminor = { 0, 3, 7 };
    private static int[] chdmajor7 = { 0, 4, 7, 11 };
    private static int[] chddom7 = { 0, 4, 7, 10 };
    private static int[] chdminor7 = { 0, 3, 7, 10 };
    private static int[] chdaug = { 0, 4, 8 };
    private static int[] chddim = { 0, 3, 6 };
    private static int[] chddim7 = { 0, 3, 6, 9 };
    private static int[] chdhalfdim = { 0, 3, 6, 10 };
    private static int[] chd1 = { 0 };
    private static int[] chd5 = { 0, 7 };
    private static int[] chdaug5 = {0, 4, 8};
    private static int[] chdmaug5 = {0, 3, 8};
    private static int[] chdsus2 = { 0, 2, 7 };
    private static int[] chdsus4 = { 0, 5, 7 };
    private static int[] chd6 = { 0, 4, 7, 9 };
    private static int[] chdm6 = { 0, 3, 7, 9 };
    private static int[] chd7sus2 = { 0, 2, 7, 10 };
    private static int[] chd7sus4 = { 0, 5, 7, 10 };
    private static int[] chd7dim5 = {0, 4, 6, 10};
    private static int[] chd7aug5 = {0, 4, 8, 10};
    private static int[] chdm7aug5 = {0, 3, 8, 10};
    private static int[] chd9 = { 0, 4, 7, 10, 14 };
    private static int[] chdm9 = { 0, 3, 7, 10, 14 };
    private static int[] chdm7aug9 = {0, 3, 7, 10, 14};
    private static int[] chdmaj9 = { 0, 4, 7, 11, 14 };
    private static int[] chd9sus4 = { 0, 5, 7, 10, 14 };
    private static int[] chd6sus9 = {0, 4, 7, 9, 14};
    private static int[] chdm6sus9 = {0, 3, 9, 7, 14};
    private static int[] chd7dim9 = {0, 4, 7, 10, 13};
    private static int[] chdm7dim9 = {0, 3, 7, 10, 13};
    private static int[] chd7dim10 = {0, 4, 7, 10, 15};
    private static int[] chd7dim11 = {0, 4, 7, 10, 16};
    private static int[] chd7dim13 = {0, 4, 7, 10, 20};
    private static int[] chd9dim5 = {0, 10, 13};
    private static int[] chdm9dim5 = {0, 10, 14};
    private static int[] chd7aug5dim9 = {0, 4, 8, 10, 13};
    private static int[] chdm7aug5dim9 = {0, 3, 8, 10, 13};
    private static int[] chd11 = { 0, 4, 7, 10, 14, 17 };
    private static int[] chdm11 = { 0, 3, 7, 10, 14, 17 };
    private static int[] chdmaj11 = { 0, 4, 7, 11, 14, 17 };
    private static int[] chd11aug = {0, 4, 7, 10, 14, 18};
    private static int[] chdm11aug = {0, 3, 7, 10, 14, 18};
    private static int[] chd13 = { 0, 4, 7, 10, 14, 17, 21 };
    private static int[] chdm13 = { 0, 3, 7, 10, 14, 17, 21 };
    private static int[] chdadd2 = { 0, 2, 4, 7 };
    private static int[] chdadd4 = { 0, 4, 5, 7 };
    private static int[] chdadd9 = { 0, 4, 7, 14 };
    private static int[] chdadd11 = { 0, 4, 7, 17 };
    private static int[] add13 = {0, 4, 7, 21};
    private static int[] madd2 = {0, 2, 3, 7};
    private static int[] madd4 = {0, 3, 5, 7};
    private static int[] madd9 = {0, 3, 7, 14};
    private static int[] madd11 = {0, 3, 7, 17};
    private static int[] madd13 = {0, 3, 7, 21};

    private static string[] validNotes = {"c0","db0","d0","eb0","e0","f0","gb0","g0","ab0","a0","bb0","b0","c1","db1","d1","eb1","e1","f1","gb1","g1","ab1","a1","bb1","b1","c2","db2","d2","eb2","e2","f2","gb2","g2","ab2","a2","bb2","b2","c3","db3","d3","eb3","e3","f3","gb3","g3","ab3","a3","bb3","b3","c4","db4","d4","eb4","e4","f4","gb4","g4","ab4","a4","bb4","b4","c5","db5","d5","eb5","e5","f5","gb5","g5","ab5","a5","bb5","b5","c6","db6","d6","eb6","e6","f6","gb6","g6","ab6","a6","bb6","b6","c7","db7","d7","eb7","e7","f7","gb7","g7","ab7","a7","bb7","b7","c8","db8","d8","eb8","e8","f8","gb8","g8","ab8","a8","bb8","b8","c9","db9","d9","eb9","e9","f9","gb9","g9"};

    private List<string> MidiNote = new List<string>();

    private List<string> Errorlog = new List<string>();
    private string Errors = "Errors: ";
    private string Errormsg = "No Errors";
    private string oldGenre = "FirstOne";
    private bool strErrors = false;
    private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    private List<Vector> SoundPos = new List<Vector>();
    private int instrumentcntr = 0;
    private List<string> InstrumentName = new List<string>();
    private int bpm = 100;
    private const char s = 's';  //sample
    private const char b = 'b';  //beat
    private const char m = 'm';  //multibeat
    private const char i = 'i';  //instrument

    private int loopNum = 0;
    private const int numTracks = 9;
    private bool[] CueActive = new bool[numTracks];
    private List<SoundResource>[] TrackSamples = new List<SoundResource>[numTracks];

    private string[] TrackDataCmd = new string[numTracks];
    private List<int>[] TrackOffsets = new List<int>[numTracks];
    private List<float>[] TrackMilliSeconds = new List<float>[numTracks];
    private float[] TrackTotalMilliseconds = new float[numTracks];
    private List<char>[] TrackSequence = new List<char>[numTracks];
    private List<int>[] TrackNotes = new List<int>[numTracks];
    private string[] TrackArrayAccess = new string[numTracks]; //"ring";
    private bool[] TrackRunning = new bool[numTracks];
    private bool[] TrackStop = new bool[numTracks];
    private float[] TrackPitchShift = new float[numTracks];  //0f;

    private bool[] TrackPlay_Once = new bool[numTracks];  //true;
    private bool[] TrackDont_Sync = new bool[numTracks];  //true;

    private bool[] TrackBlock = { false, false, false, false, false, false, false, false, false, false };
    private int[] SyncTrack = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    private bool resetSwitch = false;
    ObjectId hitter;

    private SoundResource[] SampleForTracks = new SoundResource[numTracks];
    private string[] SampleNameToPlay = new string[numTracks];
    private float[] TrackVolume = new float[numTracks];  //0f;
    private Vector[] TrackPan = new Vector[numTracks];

    private IEventSubscription TimerSub = null;
    TimeSpan initialDelayTimeSpan = TimeSpan.Zero;
    TimeSpan intervalTimeSpan = TimeSpan.Zero;

    private PlayHandle playHandle;
    private PlayHandle[] playHandleArray = new PlayHandle[numTracks];
    private PlaySettings[] playSettingsArray = new PlaySettings[numTracks];
    private int[] SampleLength = new int[numTracks];
    private int[] SampleCountdown = new int[numTracks];
    private float SamplePitchShift = 0.0f; 

    private float MasterVolume = 80.0f;
    private int LoopProgress = 36;
    private AgentInfo Jammer;
    private AgentPrivate agent;
    private string SoundType = "Local";
    private double ZRotation = new double();
    private float[] BPMPitchShiftArray = { -8.84f, -8.56f, -8.28f, -8.00f, -7.73f, -7.46f, -7.19f, -6.93f, -6.68f, -6.42f, 
                                           -6.17f, -5.93f, -5.69f, -5.45f, -5.21f, -4.98f, -4.75f, -4.52f, -4.30f, -4.08f,
                                           -3.86f, -3.65f, -3.44f, -3.23f, -3.02f, -2.81f, -2.61f, -2.41f, -2.21f, -2.02f,
                                           -1.82f, -1.63f, -1.44f, -1.26f, -1.07f, -0.89f, -0.71f, -0.53f, -0.35f, -0.17f,
                                            1.00f, 0.17f, 0.34f, 0.51f, 0.68f, 0.84f, 1.01f, 1.17f, 1.33f, 1.49f,
                                            1.65f, 1.81f, 1.96f, 2.12f, 2.27f, 2.42f, 2.57f, 2.72f, 2.87f, 3.01f,
                                            3.16f, 3.30f, 3.44f, 3.58f, 3.72f, 3.86f, 4.00f, 4.14f, 4.27f, 4.471f,
                                            4.54f, 4.67f, 4.81f, 4.94f, 5.07f, 5.20f, 5.32f, 5.45f, 5.58f, 5.70f, 5.83f};


    #endregion

    #region Communications

    public interface BPMStripInfo
    {    
        int BPMStripPosition { get; }
    }

    private void getBPMStripInfo(ScriptEventData gotBPMStripInfo)
    {
        //Log.Write("gotmojoLooper: In getVolumeStripInfo");
        if (gotBPMStripInfo.Data == null)
        {
            return;
        }

        BPMStripInfo sendBPMStripInfo = gotBPMStripInfo.Data.AsInterface<BPMStripInfo>();

        if (sendBPMStripInfo == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        
        double BPMPosition = sendBPMStripInfo.BPMStripPosition;
        Log.Write("BPM Passed: " + BPMPosition);
        int BPMIndex = (int)Math.Round(BPMPosition * 0.80, 0);
        Log.Write("BPM Index: " + BPMIndex);
        float BPMPitchShift = BPMPitchShiftArray[BPMIndex];
        Log.Write("Pitch Shift: " + BPMPitchShift);
        //0 to 80  60 to 140 
        bpm = BPMIndex + 60;
        Log.Write("bpm: " + bpm);
        int cntr = 0;
        do
        {
            playSettingsArray[cntr].PitchShift = BPMPitchShift;
            cntr++;
        }
        while (cntr < numTracks);

    }

    public interface VolumeStripInfo
    {
        int TrackStripNumber { get; }
        int TrackStripPosition { get; }
    }

    private void getPanStripInfo(ScriptEventData gotVolumeStripInfo)
    {
        //Log.Write("gotmojoLooper: In getPanStripInfo");
        if (gotVolumeStripInfo.Data == null)
        {
            return;
        }

        VolumeStripInfo sendPanStripInfo = gotVolumeStripInfo.Data.AsInterface<VolumeStripInfo>();

        if (sendPanStripInfo == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }

        int trackStripNumber = sendPanStripInfo.TrackStripNumber;
        int trackStripPosition = sendPanStripInfo.TrackStripPosition;
        //Log.Write("trackStripNumber: " + trackStripNumber + " trackStripPosition: " + trackStripPosition);
        int CalculatedPanPosition = trackStripPosition - 15;
        string strPanPosition = sendPanStripInfo.TrackStripPosition.ToString();
        int trkToUpdate = trackStripNumber - 1;
        TrackPan[trkToUpdate] = BuildPan(strPanPosition);
        Log.Write("OneMoreTme");
    }

    private void getVolumeStripInfo(ScriptEventData gotVolumeStripInfo)
    {
        //Log.Write("gotmojoLooper: In getVolumeStripInfo");
        if (gotVolumeStripInfo.Data == null)
        {
            return;
        }

        VolumeStripInfo sendVolumeStripInfo = gotVolumeStripInfo.Data.AsInterface<VolumeStripInfo>();

        if (sendVolumeStripInfo == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }

        int trackStripNumber = sendVolumeStripInfo.TrackStripNumber;
        int trackStripPosition = sendVolumeStripInfo.TrackStripPosition;
        //Log.Write("gotMojoLooper: trackStripNumber: " + trackStripNumber + " trackStripPosition: " + trackStripPosition);
        int trkToUpdate = trackStripNumber - 1;
        if (trackStripNumber == 0)  // Master Volume Change
        {
            MasterVolume = (float)trackStripPosition;
            int trkCounter = 0;
            do
            {
                float CombinedVolume = (((TrackVolume[trkCounter]) * (MasterVolume)) / 80);
                playSettingsArray[trkCounter].Loudness = LoudnessPercentToDb(CombinedVolume);
                //Log.Write("MASTER VOL CHANGE - Track Volume: " + TrackVolume[trkCounter] + " Master Volume: " + MasterVolume + " Combined Volume: " + CombinedVolume);
                trkCounter++;
            } while (trkCounter < numTracks);
        }
        else
        {
            TrackVolume[trkToUpdate] = (float)trackStripPosition;
            float CombinedVolume = (((TrackVolume[trkToUpdate]) * (MasterVolume)) / 80);
            playSettingsArray[trkToUpdate].Loudness = LoudnessPercentToDb(CombinedVolume);
            //Log.Write("gotMojoLooper: Track Volume: " + TrackVolume[trkToUpdate] + " Master Volume: " + MasterVolume + " Combined Volume: " + CombinedVolume);
        }
        

    }

    private float LoudnessPercentToDb(float loudnessPercent)  //assumes 80%
    {
        loudnessPercent = Math.Min(Math.Max(loudnessPercent, 0.0f), 100.0f);
        return 60.0f * (loudnessPercent / 100.0f) - 48.0f;
    }

    private float LoudnessDbToPercent(float loudnessDb)
    {
        float percent = (loudnessDb + 48.0f) * 100.0f / 60.0f;
        return Math.Min(Math.Max(percent, 0.0f), 100.0f);
    }

    #region SimpleHelpers v2
    // Update the region tag above by incrementing the version when updating anything in the region.

    // If a Group is set, will only respond and send to other SimpleScripts with the same Group tag set.
    // Does NOT accept CSV lists of groups.
    // To send or receive events to/from a specific group from outside that group prepend the group name with a > to the event name
    // my_group>on
    [DefaultValue("")]
    [DisplayName("Group")]
    public string Group = "";

    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    public class SimpleData : Reflective, ISimpleData
    {
        public SimpleData(ScriptBase script) { ExtraData = script; }
        public AgentInfo AgentInfo { get; set; }
        public ObjectId ObjectId { get; set; }
        public ObjectId SourceObjectId { get; set; }

        public Reflective ExtraData { get; set; }
    }

    public interface IDebugger { bool DebugSimple { get; } }
    private bool __debugInitialized = false;
    private bool __SimpleDebugging = false;
    private string __SimpleTag = "";

    private string GenerateEventName(string eventName)
    {
        eventName = eventName.Trim();
        if (eventName.EndsWith("@"))
        {
            // Special case on@ to send the event globally (the null group) by sending w/o the @.
            return eventName.Substring(0, eventName.Length - 1);
        }
        else if (Group == "" || eventName.Contains("@"))
        {
            // No group was set or already targeting a specific group as is.
            return eventName;
        }
        else
        {
            // Append the group
            return $"{eventName}@{Group}";
        }
    }

    private void SetupSimple()
    {
        __debugInitialized = true;
        __SimpleTag = GetType().Name + " [S:" + Script.ID.ToString() + " O:" + ObjectPrivate.ObjectId.ToString() + "]";
        Wait(TimeSpan.FromSeconds(1));
        IDebugger debugger = ScenePrivate.FindReflective<IDebugger>("SimpleDebugger").FirstOrDefault();
        if (debugger != null) __SimpleDebugging = debugger.DebugSimple;
    }

    System.Collections.Generic.Dictionary<string, Func<string, Action<ScriptEventData>, Action>> __subscribeActions = new System.Collections.Generic.Dictionary<string, Func<string, Action<ScriptEventData>, Action>>();
    private Action SubscribeToAll(string csv, Action<ScriptEventData> callback)
    {
        if (!__debugInitialized) SetupSimple();
        if (string.IsNullOrWhiteSpace(csv)) return null;

        Func<string, Action<ScriptEventData>, Action> subscribeAction;
        if (__subscribeActions.TryGetValue(csv, out subscribeAction))
        {
            return subscribeAction(csv, callback);
        }

        // Simple case.
        if (!csv.Contains(">>"))
        {
            __subscribeActions[csv] = SubscribeToAllInternal;
            return SubscribeToAllInternal(csv, callback);
        }

        // Chaining
        __subscribeActions[csv] = (_csv, _callback) =>
        {
            System.Collections.Generic.List<string> chainedCommands = new System.Collections.Generic.List<string>(csv.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries));

            string initial = chainedCommands[0];
            chainedCommands.RemoveAt(0);
            chainedCommands.Add(initial);

            Action unsub = null;
            Action<ScriptEventData> wrappedCallback = null;
            wrappedCallback = (data) =>
            {
                string first = chainedCommands[0];
                chainedCommands.RemoveAt(0);
                chainedCommands.Add(first);
                if (unsub != null) unsub();
                unsub = SubscribeToAllInternal(first, wrappedCallback);
                Log.Write(LogLevel.Info, "CHAIN Subscribing to " + first);
                _callback(data);
            };

            unsub = SubscribeToAllInternal(initial, wrappedCallback);
            return unsub;
        };

        return __subscribeActions[csv](csv, callback);
    }

    private Action SubscribeToAllInternal(string csv, Action<ScriptEventData> callback)
    {
        Action unsubscribes = null;
        string[] events = csv.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (__SimpleDebugging)
        {
            Log.Write(LogLevel.Info, __SimpleTag, "Subscribing to " + events.Length + " events: " + string.Join(", ", events));
        }
        Action<ScriptEventData> wrappedCallback = callback;

        foreach (string eventName in events)
        {
            if (__SimpleDebugging)
            {
                var sub = SubscribeToScriptEvent(GenerateEventName(eventName), (ScriptEventData data) =>
                {
                    Log.Write(LogLevel.Info, __SimpleTag, "Received event " + GenerateEventName(eventName));
                    wrappedCallback(data);
                });
                unsubscribes += sub.Unsubscribe;
            }
            else
            {
                var sub = SubscribeToScriptEvent(GenerateEventName(eventName), wrappedCallback);
                unsubscribes += sub.Unsubscribe;
            }
        }
        return unsubscribes;
    }

    System.Collections.Generic.Dictionary<string, Action<string, Reflective>> __sendActions = new System.Collections.Generic.Dictionary<string, Action<string, Reflective>>();
    private void SendToAll(string csv, Reflective data)
    {
        if (!__debugInitialized) SetupSimple();
        if (string.IsNullOrWhiteSpace(csv)) return;

        Action<string, Reflective> sendAction;
        if (__sendActions.TryGetValue(csv, out sendAction))
        {
            sendAction(csv, data);
            return;
        }

        // Simple case.
        if (!csv.Contains(">>"))
        {
            __sendActions[csv] = SendToAllInternal;
            SendToAllInternal(csv, data);
            return;
        }

        // Chaining
        System.Collections.Generic.List<string> chainedCommands = new System.Collections.Generic.List<string>(csv.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries));
        __sendActions[csv] = (_csv, _data) =>
        {
            string first = chainedCommands[0];
            chainedCommands.RemoveAt(0);
            chainedCommands.Add(first);

            Log.Write(LogLevel.Info, "CHAIN Sending to " + first);
            SendToAllInternal(first, _data);
        };
        __sendActions[csv](csv, data);
    }

    private void SendToAllInternal(string csv, Reflective data)
    {
        if (string.IsNullOrWhiteSpace(csv)) return;
        string[] events = csv.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (__SimpleDebugging) Log.Write(LogLevel.Info, __SimpleTag, "Sending " + events.Length + " events: " + string.Join(", ", events) + (Group != "" ? (" to group " + Group) : ""));
        foreach (string eventName in events)
        {
            PostScriptEvent(GenerateEventName(eventName), data);
        }
    }
    #endregion

    /*
    public class SendActiveBins : Reflective
    {
        public ScriptId SourceScriptId { get; internal set; }

        public List<string> ActiveBin { get; internal set; }

        public List<string> SendActiveBin
        {
            get
            {
                return ActiveBin;
            }
        }
    }

    public class SendStartedRaver : Reflective
    {
        public ScriptId SourceScriptId { get; internal set; }

        public List<string> RaverStart { get; internal set; }

        public List<string> SetRaverStart
        {
            get
            {
                return RaverStart;
            }
        }
    }

    private SendStartedRaver sendStartedRaver = new SendStartedRaver();

    public interface SendBlockNames
    {
        List<string> SendBlockArray { get; }
    }

    private void getBlock(ScriptEventData gotBlock)
    {
        //Log.Write("Raver: In gotBlock");
        if (gotBlock.Data == null)
        {
            return;
        }
        SendBlockNames sendBlock = gotBlock.Data.AsInterface<SendBlockNames>();
        if (sendBlock == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        
        if (sendBlock.SendBlockArray.Count() > 0)
        {
            string sampleFromBeatBlock;
            string loopFromBeatBlock;
            string beatFromBeatBlock;
            string dataCmdFromBeatBlock = "none";
            sampleFromBeatBlock = sendBlock.SendBlockArray[0];
            loopFromBeatBlock = sendBlock.SendBlockArray[1];
            //Log.Write("Raver: sampleFromBeatBlock: " + sampleFromBeatBlock);
            //Log.Write("Raver: loopFromBeatBlock: " + loopFromBeatBlock);
            if (sampleFromBeatBlock == "stop")
            {
                Log.Write("loopFromBeatBlock: " + loopFromBeatBlock);
                if (loopFromBeatBlock == "0")
                {
                    dataCmdFromBeatBlock = "stopall";
                    SendActiveBins sendActiveBins = new SendActiveBins();
                    sendActiveBins.ActiveBin = new List<string>();
                    sendActiveBins.SendActiveBin.Add("all");
                    sendActiveBins.SendActiveBin.Add("samples");
                    PostScriptEvent(ScriptId.AllScripts, "ReturnBeatBlock", sendActiveBins);
                    ParseCommands(dataCmdFromBeatBlock);
                }
                else
                {
                    dataCmdFromBeatBlock = "/loop" + loopFromBeatBlock + " stop";
                    ParseCommands(dataCmdFromBeatBlock);
                }
            }
            else if (sampleFromBeatBlock == "genre")
            {
                {
                    dataCmdFromBeatBlock = "stopall";
                    SendActiveBins sendActiveBins = new SendActiveBins();
                    sendActiveBins.ActiveBin = new List<string>();
                    sendActiveBins.SendActiveBin.Add("all");
                    sendActiveBins.SendActiveBin.Add("samples");
                    PostScriptEvent(ScriptId.AllScripts, "ReturnBeatBlock", sendActiveBins);
                    ParseCommands(dataCmdFromBeatBlock);
                }
            }
            else if (sampleFromBeatBlock == "volume")
            {
                string volumeChange = sendBlock.SendBlockArray[2];
                switch (loopFromBeatBlock)
                {
                    case "0":
                        dataCmdFromBeatBlock = TrackDataCmd[1] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        dataCmdFromBeatBlock = TrackDataCmd[2] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        dataCmdFromBeatBlock = TrackDataCmd[3] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        dataCmdFromBeatBlock = TrackDataCmd[4] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        dataCmdFromBeatBlock = TrackDataCmd[5] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        dataCmdFromBeatBlock = TrackDataCmd[6] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        dataCmdFromBeatBlock = TrackDataCmd[7] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        dataCmdFromBeatBlock = TrackDataCmd[8] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        dataCmdFromBeatBlock = TrackDataCmd[9] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "1":
                        dataCmdFromBeatBlock = TrackDataCmd[1] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "2":
                        dataCmdFromBeatBlock = TrackDataCmd[2] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "3":
                        dataCmdFromBeatBlock = TrackDataCmd[3] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "4":
                        dataCmdFromBeatBlock = TrackDataCmd[4] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "5":
                        dataCmdFromBeatBlock = TrackDataCmd[5] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "6":
                        dataCmdFromBeatBlock = TrackDataCmd[6] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "7":
                        dataCmdFromBeatBlock = TrackDataCmd[7] + " vol(" + volumeChange + ")";
                        //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "8":
                        dataCmdFromBeatBlock = TrackDataCmd[8] + " vol(" + volumeChange + ")";
                       // Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                    case "9":
                        dataCmdFromBeatBlock = TrackDataCmd[9] + " vol(" + volumeChange + ")";
                        // Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                        ParseCommands(dataCmdFromBeatBlock);
                        break;
                }
            }
            else
            {
                // a Sample command
                beatFromBeatBlock = sendBlock.SendBlockArray[2];
                if (loopFromBeatBlock == "0")
                {
                    //BPM Loop
                    Wait(TimeSpan.FromSeconds(2.0));
                    //Log.Write("Raver: In BPM Sample Creation .. loopFromBeatBlock: " + loopFromBeatBlock + " sampleFromBeatBlock: " + sampleFromBeatBlock + "beatFromBeatBlock: " + beatFromBeatBlock);
                    dataCmdFromBeatBlock = "/loop" + loopFromBeatBlock + " sample(" + sampleFromBeatBlock + ") beats(" + beatFromBeatBlock + ")";
                    //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                    ParseCommands(dataCmdFromBeatBlock);
                }
                else
                {
                    //Sample Block
                    Wait(TimeSpan.FromSeconds(1));
                    dataCmdFromBeatBlock = "/loop" + loopFromBeatBlock + " sample(" + sampleFromBeatBlock + ") beats(" + beatFromBeatBlock + ") sync(loop0)";
                    //Log.Write("dataCmdFromBeatBlock: " + dataCmdFromBeatBlock);
                    ParseCommands(dataCmdFromBeatBlock);
                }
            }

        }
    }

    */

    public interface ISendSampletoPlayInfo
    {

        string SampleName { get; }
        SoundResource SampleSoundResource { get; }
        string TrackPan { get; }
        string TrackToUse { get; }
        AgentInfo Jammer { get; }
    }

    private void getSampleToPlay(ScriptEventData gotSampleToPlay)
    {
        //Log.Write("gotmojoLooper: In getSampleToPlay");
        if (gotSampleToPlay.Data == null)
        {
            return;
        }
        //Log.Write("A");
        //SendBlockNames sendBlock = gotBlock.Data.AsInterface<SendBlockNames>();
        ISendSampletoPlayInfo sendSampleToPlay = gotSampleToPlay.Data.AsInterface<ISendSampletoPlayInfo>();
        //Log.Write("B");
        if (sendSampleToPlay == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        //Log.Write("C");
        //Log.Write("D");
        string sampleNameToPlay = sendSampleToPlay.SampleName;
        SoundResource sampleSoundResourceToPlay = sendSampleToPlay.SampleSoundResource;
        string trackNumberToUse = sendSampleToPlay.TrackToUse;
        agent = ScenePrivate.FindAgent(sendSampleToPlay.Jammer.SessionId);
        //Log.Write("E");
        int intTrackToUse = Int32.Parse(trackNumberToUse) - 1;
        SampleForTracks[intTrackToUse] = sampleSoundResourceToPlay;
        SampleNameToPlay[intTrackToUse] = sampleNameToPlay;
        string SampleGetName = sampleSoundResourceToPlay.GetName();
        int beg = SampleGetName.LastIndexOf("_") + 1;
        //Log.Write("beg: " + beg);
        int length = SampleGetName.Length;
        //Log.Write("length: " + length);
        string test = SampleGetName.Substring(beg, length - beg);
        //Log.Write("TEST TEST TEST TEST TEST TEST TEST TEST: " + test);
        SampleLength[intTrackToUse] = Int32.Parse(test);
        SampleCountdown[intTrackToUse] = SampleLength[intTrackToUse];
        TrackPan[intTrackToUse] = BuildPan(sendSampleToPlay.TrackPan);

        //Log.Write("SampleForTracks[" + intTrackToUse + "]: " + SampleForTracks[intTrackToUse].GetName() + " Length: " + SampleLength[intTrackToUse]);
    }

    /*
    public interface SendBPM
    {
        List<string> SendBPMArray { get; }
    }

    private void getBPM(ScriptEventData gotBPM)
    {
        //Log.Write("Raver: In gotBPM");
        if (gotBPM.Data == null)
        {
            return;
        }
        SendBPM sendBPM = gotBPM.Data.AsInterface<SendBPM>();
        if (sendBPM == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }

        if (sendBPM.SendBPMArray.Count() > 0)
        {
            //Log.Write("Raver: Processing BPM Parameters");
            bpm = Int32.Parse(sendBPM.SendBPMArray[2]);
            //Log.Write("RaverL BPM in BeatRaver: " + bpm);
        }
    }
    */

    #region BuildSampleLibraries
/*
    public List<SoundResource> SampleLibrary = new List<SoundResource>();

    public interface SendSamples
    {
        List<SoundResource> SendSampleLibrary { get; }
    }

    private void getSamples(ScriptEventData gotSamples)
    {
        if (gotSamples.Data == null)
        {
            Log.Write(LogLevel.Warning, Script.ID.ToString(), "Expected non-null event data");
            return;
        }
        SendSamples sendSamples = gotSamples.Data.AsInterface<SendSamples>();
        if (sendSamples == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        //Log.Write("Raver: Sample Count: " + sendSamples.SendSampleLibrary.Count());
        SoundResource tempSample;
        int cntr = 0;
        do
        {
            tempSample = sendSamples.SendSampleLibrary.ElementAt(cntr);
            //Log.Write("Raver: tempSample: " + tempSample.GetName());
            SampleLibrary.Add(tempSample);
            Errors = Errors + ", " + tempSample.GetName();
            cntr++;
        } while (cntr < sendSamples.SendSampleLibrary.Count());
        //Log.Write("Raver: Sample Count in SampleLibrary: " + SampleLibrary.Count());
    }

    public List<string>[] InstrumentArray = new List<string>[99];

    public interface SendInstrument
    {
        List<string> SendInstrumentArray { get; }
  }

    private void getInstrument(ScriptEventData gotInstrument)
    {
        if (gotInstrument.Data == null)
        {
            return;
        }
        SendInstrument sendInstrument = gotInstrument.Data.AsInterface<SendInstrument>();
        if (sendInstrument == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        if (sendInstrument.SendInstrumentArray.Count() > 0)
        {
            InstrumentArray[instrumentcntr] = new List<string>();
            int notecntr = 0;
            do
            {
                if (notecntr > 0)
                {
                    InstrumentArray[instrumentcntr].Add(sendInstrument.SendInstrumentArray[notecntr]);
                }
                else
                {
                    InstrumentName.Add(sendInstrument.SendInstrumentArray[0]);  //first entry of SendInstrumentArray is the name of the instrument
                }
                notecntr++;
            } while (notecntr < sendInstrument.SendInstrumentArray.Count());
            instrumentcntr++;
        }
    }
*/
    #endregion

    private void BuildMidiNotes()
    {
        MidiNote.Add("c-"); MidiNote.Add("db-"); MidiNote.Add("d-"); MidiNote.Add("eb-"); MidiNote.Add("e-"); MidiNote.Add("f-"); MidiNote.Add("gb-"); MidiNote.Add("g-"); MidiNote.Add("ab-"); MidiNote.Add("a-"); MidiNote.Add("bb-"); MidiNote.Add("b-");
        MidiNote.Add("c0"); MidiNote.Add("db0"); MidiNote.Add("d0"); MidiNote.Add("eb0"); MidiNote.Add("e0"); MidiNote.Add("f0"); MidiNote.Add("gb0"); MidiNote.Add("g0"); MidiNote.Add("ab0"); MidiNote.Add("a0"); MidiNote.Add("bb0"); MidiNote.Add("b0");
        MidiNote.Add("c1"); MidiNote.Add("db1"); MidiNote.Add("d1"); MidiNote.Add("eb1"); MidiNote.Add("e1"); MidiNote.Add("f1"); MidiNote.Add("gb1"); MidiNote.Add("g1"); MidiNote.Add("ab1"); MidiNote.Add("a1"); MidiNote.Add("bb1"); MidiNote.Add("b1");
        MidiNote.Add("c2"); MidiNote.Add("db2"); MidiNote.Add("d2"); MidiNote.Add("eb2"); MidiNote.Add("e2"); MidiNote.Add("f2"); MidiNote.Add("gb2"); MidiNote.Add("g2"); MidiNote.Add("ab2"); MidiNote.Add("a2"); MidiNote.Add("bb2"); MidiNote.Add("b2");
        MidiNote.Add("c3"); MidiNote.Add("db3"); MidiNote.Add("d3"); MidiNote.Add("eb3"); MidiNote.Add("e3"); MidiNote.Add("f3"); MidiNote.Add("gb3"); MidiNote.Add("g3"); MidiNote.Add("ab3"); MidiNote.Add("a3"); MidiNote.Add("bb3"); MidiNote.Add("b3");
        MidiNote.Add("c4"); MidiNote.Add("db4"); MidiNote.Add("d4"); MidiNote.Add("eb4"); MidiNote.Add("e4"); MidiNote.Add("f4"); MidiNote.Add("gb4"); MidiNote.Add("g4"); MidiNote.Add("ab4"); MidiNote.Add("a4"); MidiNote.Add("bb4"); MidiNote.Add("b4");
        MidiNote.Add("c5"); MidiNote.Add("db5"); MidiNote.Add("d5"); MidiNote.Add("eb5"); MidiNote.Add("e5"); MidiNote.Add("f5"); MidiNote.Add("gb5"); MidiNote.Add("g5"); MidiNote.Add("ab5"); MidiNote.Add("a5"); MidiNote.Add("bb5"); MidiNote.Add("b5");
        MidiNote.Add("c6"); MidiNote.Add("db6"); MidiNote.Add("d6"); MidiNote.Add("eb6"); MidiNote.Add("e6"); MidiNote.Add("f6"); MidiNote.Add("gb6"); MidiNote.Add("g6"); MidiNote.Add("ab6"); MidiNote.Add("a6"); MidiNote.Add("bb6"); MidiNote.Add("b6");
        MidiNote.Add("c7"); MidiNote.Add("db7"); MidiNote.Add("d7"); MidiNote.Add("eb7"); MidiNote.Add("e7"); MidiNote.Add("f7"); MidiNote.Add("gb7"); MidiNote.Add("g7"); MidiNote.Add("ab7"); MidiNote.Add("a7"); MidiNote.Add("bb7"); MidiNote.Add("b7");
        MidiNote.Add("c8"); MidiNote.Add("db8"); MidiNote.Add("d8"); MidiNote.Add("eb8"); MidiNote.Add("e8"); MidiNote.Add("f8"); MidiNote.Add("gb8"); MidiNote.Add("g8"); MidiNote.Add("ab8"); MidiNote.Add("a8"); MidiNote.Add("bb8"); MidiNote.Add("b8");
        MidiNote.Add("c9"); MidiNote.Add("db8"); MidiNote.Add("d9"); MidiNote.Add("eb9"); MidiNote.Add("e9"); MidiNote.Add("f9"); MidiNote.Add("gb9"); MidiNote.Add("g9");
    }

    #endregion

    #region Init
    public override void Init()
    {
        Log.Write("gotMojoLooper: Script Started");
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal

        InitializeSamples();
        RigidBodyComponent rigidBody;
        if (ObjectPrivate.TryGetFirstComponent(out rigidBody))
        {
            CurPos = rigidBody.GetPosition();
            ZRotation = rigidBody.GetOrientation().GetEulerAngles().Z * 57.2958;
            //rigidBody.Subscribe(CollisionEventType.Trigger, OnCollide);
        }
        else
        {
        }

        SubscribeToScriptEvent("PlaySample", getSampleToPlay);
        SubscribeToScriptEvent("Track1Off", stopTrack);
        SubscribeToScriptEvent("Track2Off", stopTrack);
        SubscribeToScriptEvent("Track3Off", stopTrack);
        SubscribeToScriptEvent("Track4Off", stopTrack);
        SubscribeToScriptEvent("Track5Off", stopTrack);
        SubscribeToScriptEvent("Track6Off", stopTrack);
        SubscribeToScriptEvent("Track7Off", stopTrack);
        SubscribeToScriptEvent("Track8Off", stopTrack);
        SubscribeToScriptEvent("Track9Off", stopTrack);

        SubscribeToScriptEvent("BPM", getBPMStripInfo);;
        SubscribeToScriptEvent("GlobalDown", changeSound);
        SubscribeToScriptEvent("LocalDown", changeSound);
        SubscribeToScriptEvent("TrackVolume", getVolumeStripInfo);
        SubscribeToScriptEvent("PanPosition", getPanStripInfo);
        SetTmer();
        //sendStartedRaver.RaverStart = new List<string>();
        //sendStartedRaver.SetRaverStart.Add("off");
        //PostScriptEvent(ScriptId.AllScripts, "RaverStarted", sendStartedRaver);
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

/*
    private void ResetScene()
    {
        ModalDialog Dlg;
        AgentPrivate agent = ScenePrivate.FindAgent(hitter);
        if (agent == null)
            return;

        Dlg = agent.Client.UI.ModalDialog;
        WaitFor(Dlg.Show, "Are you sure you want to reset the entire scene?", "YES", "NO");
        if (Dlg.Response == "YES")
        {
            StartCoroutine(() =>
            {
                ScenePrivate.Chat.MessageAllUsers("Resetting scene");
                Wait(TimeSpan.FromSeconds(1));
                ScenePrivate.ResetScene();
            });
        }
    }
    */

    /*
    //swap out with a simple collision
    private void OnCollide(CollisionData Data)
    {
        if (resetSwitch == true)
        {
            ResetScene();
        }
        if (Data.Phase == CollisionEventPhase.TriggerEnter)
        {
            //Log.Write(LogLevel.Info, "Welcome to Beat Block Raver");       
            hitter = Data.HitComponentId.ObjectId;
            Log.Write(Data.HitComponentId.ObjectId.ToString());
            ScenePrivate.Chat.MessageAllUsers("Welcome to Beat Block Raver");
            //sendStartedRaver.RaverStart = new List<string>();
            sendStartedRaver.RaverStart.Clear();
            sendStartedRaver.SetRaverStart.Add("on");
            PostScriptEvent(ScriptId.AllScripts, "RaverStarted", sendStartedRaver);
            string eventString = null;
            int samplepackcntr = 1;
            do
            {
                eventString = "Samples" + samplepackcntr;
                SubscribeToScriptEvent(eventString, getSamples);
                eventString = "Instrument" + samplepackcntr;
                SubscribeToScriptEvent(eventString, getInstrument);
                samplepackcntr++;
            } while (samplepackcntr < 64);
            ScenePrivate.Chat.Subscribe(0, null, GetChatCommand);
            SubscribeToScriptEvent("BeatBlockSample", getSamples);
            SubscribeToScriptEvent("BeatBlock", getBlock);
            SubscribeToScriptEvent("StopBlock", getBlock);
            SubscribeToScriptEvent("BPMBlock", getBPM);
            SubscribeToScriptEvent("VolumeBlock", getBlock);
            //SubscribeToScriptEvent("Genre", getGenre);
            BuildMidiNotes();
            Initalize();
        }
        else
        {
            //Log.Write("has left my volume!");
            //StopAll();
            resetSwitch = true;

        }
    }
    */

    #endregion

    #region InitializeBuildSamples
    private Vector BuildPan(string InString)
    {
        float fltPan = float.Parse(InString);
        float CosAngle = (float)Math.Cos(ZRotation * 0.0174533);
        float SinAngle = (float)Math.Sin(ZRotation * 0.0174533);
        float newX = (fltPan * CosAngle) - (0 * SinAngle);
        float newY = (0 * CosAngle) + (fltPan * SinAngle);
        Vector PanPos = new Vector(newX, newY, 0.0f);
        PanPos = CurPos + PanPos;
        //Log.Write("newX: " + newX + " newY: " + newY + " PanPos" + PanPos);
        //Log.Write("CurPos: " + CurPos + " ZRotation: " + ZRotation);
        return PanPos;
    }


    /*
    private void Initalize()
    {
        Vector InitPos = new Vector(-2.0f, 0.0f, 0.0f);
        Vector IncPos = new Vector(0.25f, 0.0f, 0.0f);
        Vector LastPos = CurPos + InitPos;
        int cntr = 0;
        do
        {
            SoundPos.Add(LastPos + IncPos);
            LastPos = SoundPos[cntr];
            TrackArrayAccess[cntr] = "ring";
            TrackVolume[cntr] = 0f;
            TrackPitchShift[cntr] = 0f;
            TrackPlay_Once[cntr] = true;
            TrackDont_Sync[cntr] = true;
            cntr++;
        } while (cntr < numTracks);
    }

    private List<string> ParseIt(string InString)
    {
        List<string> cmds = new List<string>();
        int strlen = 0;
        char semi = ')';
        string test = InString;
        string cmd;
        int beg = 0;
        int endcmd = 0;

        do
        {
            endcmd = test.IndexOf(semi);
            cmd = test.Substring(beg, endcmd);
            cmd = cmd.Replace('(', ' ');
            cmds.Add(cmd);
            test = test.Remove(beg, endcmd + 1);
            strlen = test.Length;
        } while (test.Length > 1);
        return cmds;
    }

    private List<SoundResource> BuildSamples(List<string> Tempcmds)
    {
        string strtest;
        string cmdline;
        int cntr = 0;
        bool SampleFound = false;
        List<SoundResource> TempSamples = new List<SoundResource>();
        do
        {
            cmdline = Tempcmds[cntr];
            //Log.Write("Raver: TempCmd: " + Tempcmds[cntr]);
            if (cmdline.Contains("sample")  || cmdline.Contains("inst"))
            {
                SampleFound = false;
                foreach (SoundResource Sample in SampleLibrary)
                {
                    strtest = Sample.GetName();
                    //Log.Write("Raver: sample being tested: " + strtest);
                    //if (cmdline.Contains("DKSoulness_32_100")) Log.Write("strtest: " + strtest);
                    if (cmdline.Contains(strtest))
                    {
                        //Log.Write("Raver: Sample Matched Command Line");
                        SampleFound = true;
                        TempSamples.Add(Sample);
                    }
                }
                if (!SampleFound)
                {
                    ScenePrivate.Chat.MessageAllUsers("Sample - " + cmdline + " - not found");
                }
            }
            cntr++;
        } while (cntr < Tempcmds.Count);

        return TempSamples;
    }

    private float BuildVolume(string InString)
    {

        string substr = "vol(";
        int from = InString.IndexOf(substr, StringComparison.CurrentCulture);

        int length = InString.LastIndexOf(")", StringComparison.CurrentCulture);
        int last = length - from;
        string chunk = InString.Substring(from, last + 1);
        int next = chunk.IndexOf(")", StringComparison.CurrentCulture);
        string strVolume = chunk.Substring(4, next - 4);
        float fltVolume = float.Parse(strVolume);
        return fltVolume;
    }

    private Vector BuildPanOld(string InString)
    {
        string substr = "pan(";
        int from = InString.IndexOf(substr, StringComparison.CurrentCulture);

        int length = InString.LastIndexOf(")", StringComparison.CurrentCulture);
        int last = length - from;
        string chunk = InString.Substring(from, last + 1);
        int next = chunk.IndexOf(")", StringComparison.CurrentCulture);
        string strPan = chunk.Substring(4, next - 4);
        float fltPan = float.Parse(strPan);
        Vector PanPos = new Vector(fltPan, 0.0f, 0.0f);
        PanPos = CurPos + PanPos;
        return PanPos;
    }

    private float BuildPitchShift(string InString)
    {
        string substr = "pitch(";
        int from = InString.IndexOf(substr, StringComparison.CurrentCulture);
        int length = InString.Length;
        int last = length - from;
        string chunk = InString.Substring(from, last);
        int next = chunk.IndexOf(")", StringComparison.CurrentCulture);
        string strPitch = chunk.Substring(6, next - 6);
        float fltPitch = float.Parse(strPitch);
        return fltPitch;
       }

    private string StripDataCmd(string InString)
    {
        //Log.Write("InString: " + InString);
        int findvol = InString.IndexOf("vol");
        //Log.Write("findvol: " + findvol);
        //Log.Write("Instring.Length: " + InString.Length);
        string strippedString;
        strippedString = InString.Remove(findvol, InString.Length-findvol);
        //Log.Write("strippedString: " + strippedString);
        return InString;
    }

    */

    #endregion

    #region NotesScalesChords

    private List<string> BuildScale(string[] ScaleIn)
    {
        int[] TempScaleNotes = null;
        List<string> ReturnNotes = new List<string>();
        int notecntr = 0;
        int basenote = 0;
        //find index of base note in MidiNoteArray
        do
        {
            if (MidiNote[notecntr] == ScaleIn[0])
            {
                basenote = notecntr;
                break;
            }
            notecntr++;
        } while (notecntr < MidiNote.Count());
        ScaleIn[1] = ScaleIn[1].Substring(1, ScaleIn[1].Length - 1);
        switch (ScaleIn[1])
        {
            case "major":
                TempScaleNotes = major;
                break;
            case "dorian":
                TempScaleNotes = dorian;
                break;
            case "phrygian":
                TempScaleNotes = phrygian;
                break;
            case "lydian":
                TempScaleNotes = lydian;
                break;
            case "mixolydian":
                TempScaleNotes = mixolydian;
                break;
            case "aelian":
                TempScaleNotes = aelian;
                break;
            case "minor":
                TempScaleNotes = minor;
                break;
            case "minor_pentatonic":
                TempScaleNotes = minor_pentatonic;
                break;
            case "major_pentatonic":
                TempScaleNotes = major_pentatonic;
                break;
            case "egyptian":
                TempScaleNotes = egyptian;
                break;
            case "jiao":
                TempScaleNotes = jiao;
                break;
            case "zhi":
                TempScaleNotes = zhi;
                break;
            case "whole_tone":
                TempScaleNotes = whole_tone;
                break;
            case "whole":
                TempScaleNotes = whole;
                break;
            case "chromatic":
                TempScaleNotes = chromatic;
                break;
            case "harmonic_minor":
                TempScaleNotes = harmonic_minor;
                break;
            case "melodic_minor_asc":
                TempScaleNotes = melodic_minor_asc;
                break;
            case "hungarian_minor":
                TempScaleNotes = hungarian_minor;
                break;
            case "octatonic":
                TempScaleNotes = octatonic;
                break;
            case "messiaen1":
                TempScaleNotes = messiaen1;
                break;
            case "messiaen2":
                TempScaleNotes = messiaen2;
                break;
            case "messiaen3":
                TempScaleNotes = messiaen3;
                break;
            case "messiaen4":
                TempScaleNotes = messiaen4;
                break;
            case "messiaen5":
                TempScaleNotes = messiaen5;
                break;
            case "messiaen6":
                TempScaleNotes = messiaen6;
                break;
            case "messiaen7":
                TempScaleNotes = messiaen7;
                break;
            case "super_locrian":
                TempScaleNotes = super_locrian;
                break;
            case "hirajoshi":
                TempScaleNotes = hirajoshi;
                break;
            case "kumoi":
                TempScaleNotes = kumoi;
                break;
            case "neapolitan_major":
                TempScaleNotes = neapolitan_major;
                break;
            case "bartok":
                TempScaleNotes = bartok;
                break;
            case "bhairav":
                TempScaleNotes = bhairav;
                break;
            case "locrian_major":
                TempScaleNotes = locrian_major;
                break;
            case "ahirbhairav":
                TempScaleNotes = ahirbhairav;
                break;
            case "enigmatic":
                TempScaleNotes = enigmatic;
                break;
            case "neapolitan_minor":
                TempScaleNotes = neapolitan_minor;
                break;
            case "pelog":
                TempScaleNotes = pelog;
                break;
            case "augmented2":
                TempScaleNotes = augmented2;
                break;
            case "scriabin":
                TempScaleNotes = scriabin;
                break;
            case "harmonic_major":
                TempScaleNotes = harmonic_major;
                break;
            case "melodic_minor_desc":
                TempScaleNotes = melodic_minor_desc;
                break;
            case "romanian_minor":
                TempScaleNotes = romanian_minor;
                break;
            case "hindu":
                TempScaleNotes = hindu;
                break;
            case "iwato":
                TempScaleNotes = iwato;
                break;
            case "melodic_minor":
                TempScaleNotes = melodic_minor;
                break;
            case "diminished":
                TempScaleNotes = diminished;
                break;
            case "marva":
                TempScaleNotes = marva;
                break;
            case "melodic_major":
                TempScaleNotes = melodic_major;
                break;
            case "indian":
                TempScaleNotes = indian;
                break;
            case "spanish":
                TempScaleNotes = spanish;
                break;
            case "prometheus":
                TempScaleNotes = prometheus;
                break;
            case "todi":
                TempScaleNotes = todi;
                break;
            case "leading_whole":
                TempScaleNotes = leading_whole;
                break;
            case "augmented":
                TempScaleNotes = augmented;
                break;
            case "purvi":
                TempScaleNotes = purvi;
                break;
            case "chinese":
                TempScaleNotes = chinese;
                break;
            case "chdmajor":
                TempScaleNotes = chdmajor;
                break;
            case "chdminor":
                TempScaleNotes = chdminor;
                break;
            case "chdmajor7":
                TempScaleNotes = chdmajor7;
                break;
            case "chddom7":
                TempScaleNotes = chddom7;
                break;
            case "chdminor7":
                TempScaleNotes = chdminor7;
                break;
            case "chdaug":
                TempScaleNotes = chdaug;
                break;
            case "chddim":
                TempScaleNotes = chddim;
                break;
            case "chddim7":
                TempScaleNotes = chddim7;
                break;
            case "chdhalfdim":
                TempScaleNotes = chdhalfdim;
                break;
            case "chd1":
                TempScaleNotes = chd1;
                break;
            case "chd5":
                TempScaleNotes = chd5;
                break;
            case "chdmaug5":
                TempScaleNotes = chdmaug5;
                break;
            case "chdsus2":
                TempScaleNotes = chdsus2;
                break;
            case "chdsus4":
                TempScaleNotes = chdsus4;
                break;
            case "chd6":
                TempScaleNotes = chd6;
                break;
            case "chdm6":
                TempScaleNotes = chdm6;
                break;
            case "chd7sus2":
                TempScaleNotes = chd7sus2;
                break;
            case "chd7sus4":
                TempScaleNotes = chd7sus4;
                break;
            case "chd7dim5":
                TempScaleNotes = chd7dim5;
                break;
            case "chd7aug5":
                TempScaleNotes = chd7aug5;
                break;
            case "chdm7aug5":
                TempScaleNotes = chdm7aug5;
                break;
            case "chd9":
                TempScaleNotes = chd9;
                break;
            case "chdm9":
                TempScaleNotes = chdm9;
                break;
            case "chdm7aug9":
                TempScaleNotes = chdm7aug9;
                break;
            case "chdmaj9":
                TempScaleNotes = chdmaj9;
                break;
            case "chd9sus4":
                TempScaleNotes = chd9sus4;
                break;
            case "chd6sus9":
                TempScaleNotes = chd6sus9;
                break;
            case "chdm6sus9":
                TempScaleNotes = chdm6sus9;
                break;
            case "chd7dim9":
                TempScaleNotes = chd7dim9;
                break;
            case "chdm7dim9":
                TempScaleNotes = chdm7dim9;
                break;
            case "chd7dim10":
                TempScaleNotes = chd7dim10;
                break;
            case "chd7dim11":
                TempScaleNotes = chd7dim11;
                break;
            case "chd7dim13":
                TempScaleNotes = chd7dim13;
                break;
            case "chd9dim5":
                TempScaleNotes = chd9dim5;
                break;
            case "chdm9dim5":
                TempScaleNotes = chdm9dim5;
                break;
            case "chd7aug5dim9":
                TempScaleNotes = chd7aug5dim9;
                break;
            case "chdm7aug5dim9":
                TempScaleNotes = chdm7aug5dim9;
                break;
            case "chd11":
                TempScaleNotes = chd11;
                break;
            case "chdm11":
                TempScaleNotes = chdm11;
                break;
            case "chdmaj11":
                TempScaleNotes = chdmaj11;
                break;
            case "chd11aug":
                TempScaleNotes = chd11aug;
                break;
            case "chdm11aug":
                TempScaleNotes = chdm11aug;
                break;
            case "chd13":
                TempScaleNotes = chd13;
                break;
            case "chdm13":
                TempScaleNotes = chdm13;
                break;
            case "chdadd2":
                TempScaleNotes = chdadd2;
                break;
            case "chdadd4":
                TempScaleNotes = chdadd4;
                break;
            case "chdadd9":
                TempScaleNotes = chdadd9;
                break;
            case "chdadd11":
                TempScaleNotes = chdadd11;
                break;
            case "add13":
                TempScaleNotes = add13;
                break;
            case "madd2":
                TempScaleNotes = madd2;
                break;
            case "madd4":
                TempScaleNotes = madd4;
                break;
            case "madd9":
                TempScaleNotes = madd9;
                break;
            case "madd11":
                TempScaleNotes = madd11;
                break;
            case "madd13":
                TempScaleNotes = madd13;
                break;
            default:
                Errormsg = "Scale or Chord Not Found";
                break;
        }

        ReturnNotes.Add(ScaleIn[0]); //first note is the base note
      if (!(Errormsg == "Scale or Chord Not Found"))
        {
            //Get the Rest of the notes of the scale
            notecntr = 0;
            do
            {
                ReturnNotes.Add(MidiNote[basenote + TempScaleNotes[notecntr]]);
                basenote = basenote + TempScaleNotes[notecntr];
                notecntr++;
            } while (notecntr < TempScaleNotes.Count() - 1);
        }

        return ReturnNotes;
    }

    private int FindMidiNote(string MidiNoteIn)
    {
        int x = 0;
        do
        {
            if (MidiNote[x] == MidiNoteIn) break;
            x++;
        } while (x < MidiNote.Count());
        return x;
    }

    private bool FindValidNote(string NoteCheck)
    {
        int x = 0;
        bool noteTest = false;
        do
        {
            if (validNotes[x] == NoteCheck)
            {
                noteTest = true;
                break;
            }
            x++;
        } while (x < validNotes.Count());
        return noteTest;
   }

    private List<int> BuildNotes(List<string> Tempcmds)
    {
        string cmdline;
        string strNotes = "";
        char comma = ',';
        int octaves = 0;
        int cntr = 0;
        int notecntr = 0;
        bool noteFound = false;
        List<string> strTempNotes = new List<string>();
        List<int> intTempNotes= new List<int>();
        do
        {
            cmdline = Tempcmds[cntr];
            if (cmdline.Contains("notes"))
            {
                //parse note arrays
                if (cmdline.Contains("["))
                {
                    string[] NoteArray = cmdline.Split(comma);
                    //fix up first entry
                    int from = NoteArray[0].IndexOf("[", StringComparison.CurrentCulture);
                    if (NoteArray[0].Length == 11) strNotes = NoteArray[0].Substring(from+1, 3);
                    if (NoteArray[0].Length == 10) strNotes = NoteArray[0].Substring(from+1, 2);
                    strNotes = strNotes.Trim();
                    noteFound = FindValidNote(strNotes);
                    if (!noteFound) Errormsg = "Invalid Note Name";
                    else
                    {
                        strTempNotes.Add(strNotes);
                        NoteArray[0] = strNotes;
                        //fix up last entry
                        int lastentry = NoteArray.Count();
                        strNotes = NoteArray[lastentry - 1];
                        if (NoteArray[lastentry - 1].Length == 4) strNotes = NoteArray[lastentry - 1].Substring(0, 3);
                        if (NoteArray[lastentry - 1].Length == 3) strNotes = NoteArray[lastentry - 1].Substring(0, 2);
                        strNotes = strNotes.Trim();
                        noteFound = FindValidNote(strNotes);
                        if (!noteFound) Errormsg = "Invalid Note Name";
                        else
                        {
                            NoteArray[lastentry - 1] = strNotes;
                            notecntr = 0;
                            do
                            {
                                NoteArray[notecntr] = NoteArray[notecntr].Trim();
                                noteFound = FindValidNote(NoteArray[notecntr]);
                                if (!noteFound)
                                {
                                    Errormsg = "Invalid Note Name";
                                    notecntr = NoteArray.Count();
                                }
                                else
                                {
                                    if (notecntr > 0) strTempNotes.Add(NoteArray[notecntr]);
                                    notecntr++;
                                }
                            } while (notecntr < NoteArray.Count());
                        }
                    }
                }
                //parse single note
                else
                {
                    if (cmdline.Length == 10) strNotes = cmdline.Substring(cmdline.Length - 3, 3);
                    if (cmdline.Length == 9) strNotes = cmdline.Substring(cmdline.Length - 2, 2);
                    noteFound = FindValidNote(strNotes);
                    if (noteFound) strTempNotes.Add(strNotes);
                    else Errormsg = "Invalid Note Name";
                    //notecntr++;
                }
            }
            //parse scales
            if (cmdline.Contains("scale") || cmdline.Contains("chord"))
            {
                string[] NoteArray = cmdline.Split(comma);
                NoteArray[0] = NoteArray[0].Substring(7, NoteArray[0].Length-7);

                if (FindMidiNote(NoteArray[0]) == 128)
                {
                    Errormsg = "Base Note of Scale or Chord is Not Correct";
                }
                else
                {
                    string scale = NoteArray[1];
                    strTempNotes = BuildScale(NoteArray);
                }
            }

            //octaves processing
            if (!(Errormsg == "Base Note of Scale or Chord is Not Correct"))
            {
                if (cmdline.Contains("octaves"))
                {
                    string strOctaves = cmdline.Substring(9, 1);
                    octaves = Int32.Parse(cmdline.Substring(9, 1));
                }
            }

            cntr++;
        } while (cntr < Tempcmds.Count);
        if (strTempNotes.Count() > 0)
        {
            notecntr = 0;
            do
            {
                intTempNotes.Add(FindMidiNote(strTempNotes[notecntr]));
                notecntr++;
            } while (notecntr < strTempNotes.Count());
        }

        if (octaves > 0)
        {
            int octcntr = 0;
            int arraylength = intTempNotes.Count();
            do
            {
                notecntr = 0;
                do
                {
                    intTempNotes.Add(intTempNotes[notecntr]+(12*(octcntr+1)));
                    notecntr++;
                } while (notecntr < arraylength);
                octcntr++;
            } while (octcntr < octaves-1);
        }

        return intTempNotes;
   }

    private double RateToPitch(double rateIn)
    {
        double semitone = 1.059463094359;
        double exponent = 1.0;
        double testval = 0;
        double pitchval = 0.0;
        double adder = 1.0;
        int iterator = 1;
        int precision = 1;

        if (rateIn > 1.0)
        {
            while (precision < 11)
            {
                iterator = 1;
                while (iterator < 12)
                {
                    testval = Math.Pow(semitone, exponent);  //2
                    if (testval < rateIn)
                    {
                        iterator++;
                        exponent = exponent + adder;  //1.1
                    }
                    else
                    {
                        pitchval = exponent - adder;  //1.1-.1
                        adder = adder * .1;  //.01
                        exponent = pitchval;  //1.01
                        precision++;
                        iterator = 12;
                    }
                }
            }
        }
        else if (rateIn < 1.0)
        {
            exponent = -1;
            adder = -1;
            while (precision < 11)
            {
                iterator = 1;
                while (iterator < 12)
                {
                    testval = Math.Pow(semitone, exponent);  //2
                    if (testval > rateIn)
                    {
                        iterator++;
                        exponent = exponent + adder;  //-2
                    }
                    else
                    {
                        pitchval = exponent - adder;  //1.1-.1
                        adder = adder * .1;  //.01
                        exponent = pitchval;  //1.01
                        precision++;
                        iterator = 12;
                    }
                }
            }
        }
        return pitchval;
    }

#endregion

    #region BuildTimingandSequence
    private List<float> BuildTiming(List<string> Tempcmds)
    {
        string cmdline;
        int cntr = 0;
        float sleepbeats = 0;
        float sleeptime = 0;
        int start = 0;
        int timecntr = 0;
        char comma = ',';
        string strTime = "";
        string secs = "";
        List<float> TempMilliSeconds = new List<float>();

        do
        {
            cmdline = Tempcmds[cntr];
            if (cmdline.Contains("beats"))
            {
                if (cmdline.Contains("["))
                {
                    string[] TimeArray = cmdline.Split(comma);
                    do
                    {
                        timecntr++;
                    } while (timecntr < TimeArray.Count());
                    //fix up first entry
                    int from = TimeArray[0].IndexOf("[", StringComparison.CurrentCulture);
                    strTime = TimeArray[0].Substring(from + 1, TimeArray[0].Length - from - 1);
                    TimeArray[0] = strTime;
                    //fix up last entry
                    int lastentry = TimeArray.Count();
                    strTime = TimeArray[lastentry - 1];
                    strTime = TimeArray[lastentry - 1].Substring(0, TimeArray[lastentry-1].Length-1);
                    TimeArray[lastentry - 1] = strTime;
                    timecntr = 0;
                    do
                    {
                        secs = TimeArray[timecntr];
                        sleepbeats = float.Parse(secs);
                        //secs = "";
                        sleeptime = sleepbeats / bpm* 60 * 1000;
                        TempMilliSeconds.Add(sleeptime - 10);  //10 milliseconds less
                        timecntr++;
                    } while (timecntr < TimeArray.Count());
                }
                else
                {

                    start = cmdline.LastIndexOf("beats ", StringComparison.CurrentCulture);
                    cmdline = cmdline.Substring(start, cmdline.Length - start);
                    secs = cmdline.Substring(6, cmdline.Length - 6);
                    sleepbeats = float.Parse(secs);
                    secs = "";
                    sleeptime = sleepbeats / bpm * 60 * 1000;
                    TempMilliSeconds.Add(sleeptime - 10);  //10 milliseconds less
                }
            }
            cntr++;
        } while (cntr < Tempcmds.Count);

        return TempMilliSeconds;
   }

    private float BuildTotalTime(List<float> TimingArray)
    {
        float totalLoopTime = 0;
        int cntr = 0;

        do
        {
            totalLoopTime = totalLoopTime + TimingArray[cntr] + 10;  //Add back in the 10 milliseconds
            cntr++;
        } while (cntr < TimingArray.Count);
        return totalLoopTime;
  }

    private List<char> BuildSequence(List<string> Tempcmds)
    {
        string cmdline;
        int cntr = 0;
        List<char> TempSequence = new List<char>();
        do
        {
            cmdline = Tempcmds[cntr];
            if (cmdline.Contains("sample")) TempSequence.Add(s);
            else if  (cmdline.Contains("inst")) TempSequence.Add(i);
            else if  (cmdline.Contains("beats [")) TempSequence.Add(m);
            else if  (cmdline.Contains("beats")) TempSequence.Add(b);
            cntr++;
        } while (cntr < Tempcmds.Count);
        return TempSequence;
   }

    #endregion

    #region dump
    private void dump()
    {
        Log.Write("In dump");
        if (!(TrackSequence[loopNum] == null))
        {
            int cntr = 0;
            do
            {
                Log.Write("TrackSequence " + loopNum + ": " + TrackSequence[loopNum][cntr]);
                cntr++;
            } while (cntr < TrackSequence[loopNum].Count());
        }
        else Log.Write("Track Sequence is null");

        Log.Write("TrackSamplesCount: " + TrackSamples[loopNum].Count());
            int xcntr = 0;
            do
            {
                Log.Write("TrackSample" + loopNum + ": " + TrackSamples[loopNum][xcntr].GetName());
                xcntr++;
            } while (xcntr < TrackSamples[loopNum].Count());

        Log.Write("TrackOffsetsCount: " + TrackOffsets[loopNum].Count());
        if (!(TrackOffsets[loopNum] == null))
        {
            int cntr = 0;
            do
            {
                Log.Write("TrackOffset" + loopNum + ": " + TrackOffsets[loopNum][cntr]);
                cntr++;
            } while (cntr < TrackOffsets[loopNum].Count());
        }
        else Log.Write("TrackOffset is null");

        Log.Write("TrackMilliSecondsCount: " + TrackMilliSeconds[loopNum].Count());
        if (!(TrackMilliSeconds[loopNum] == null))
        {
            int cntr = 0;
            do
            {
                Log.Write("TrackBeats: " + TrackMilliSeconds[loopNum][cntr]);
                cntr++;
            } while (cntr < TrackMilliSeconds[loopNum].Count());
        }
        else Log.Write("Track MilliSeconds is null");
    }
    #endregion

    #region Tracks

    private void InitializeSamples()
    {
        int trkCounter = 0;
        do
        {
            SampleForTracks[trkCounter] = null;
            playSettingsArray[trkCounter] = PlaySettings.PlayOnce;
            //playSettingsArray[trkCounter].Loudness = LoudnessPercentToDb(50.0f);
            TrackVolume[trkCounter] = 80.0f;
            playHandleArray[trkCounter] = null; 
            trkCounter++;
        } while (trkCounter < numTracks);
    }

    private void SetTmer()
    {
        Log.Write("bpm in SetTmer: " + bpm);
        float newDelay = 2.4f * 100 / bpm;
        intervalTimeSpan = TimeSpan.FromSeconds(newDelay);
        Log.Write("newDelay: " + newDelay);
        initialDelayTimeSpan = TimeSpan.FromSeconds(0.0f);
        TimerSub = Timer.Create(initialDelayTimeSpan, intervalTimeSpan, () =>
        {
            PlayTracks();
        });
    }

    private void stopTrack(ScriptEventData sed)
    {
        //Log.Write("stopTrack: " + sed.Message);
        if (sed.Message == "Track1Off") SampleNameToPlay[0] = null;
        else if (sed.Message == "Track2Off") SampleNameToPlay[1] = null;
        else if (sed.Message == "Track3Off") SampleNameToPlay[2] = null;
        else if (sed.Message == "Track4Off") SampleNameToPlay[3] = null;
        else if (sed.Message == "Track5Off") SampleNameToPlay[4] = null;
        else if (sed.Message == "Track6Off") SampleNameToPlay[5] = null;
        else if (sed.Message == "Track7Off") SampleNameToPlay[6] = null;
        else if (sed.Message == "Track8Off") SampleNameToPlay[7] = null;
        else if (sed.Message == "Track9Off") SampleNameToPlay[8] = null;
    }

/*
    private void changeBPM(ScriptEventData sed)
    {
        //Log.Write("stopTrack: " + sed.Message);


        if (sed.Message == "60Down")
        {
            bpm = 60;
            SamplePitchShift = -8.84f;
        }
        else if (sed.Message == "80Down")
        {
            bpm = 80;
            SamplePitchShift = -3.86f;
        }
        else if (sed.Message == "100Down")
        {
            bpm = 100;
            SamplePitchShift = 0.0f;
        }
        else if (sed.Message == "120Down")
        {
            bpm = 120;
            SamplePitchShift = 3.16f;
        }
        else if (sed.Message == "140Down")
        {
            bpm = 140;
            SamplePitchShift = 5.83f;
        }
        else if (sed.Message == "160Down")
        {
            bpm = 160;
            SamplePitchShift = 8.14f;
        }

        int cntr = 0;
        do
        {
            playSettingsArray[cntr].PitchShift = SamplePitchShift;
            cntr++;
        }
        while (cntr < numTracks);
    }

    */

    private void changeSound(ScriptEventData sed)
    {
        //Log.Write("stopTrack: " + sed.Message);
        if (sed.Message == "GlobalDown")
        {
            SoundType = "Global";
        }
        else if (sed.Message == "LocalDown")
        {
            SoundType = "Local";
        }
    }


    private void PlayTracks()
    {
        //Log.Write("In PlayTracks");
        //PlaySettings playSettings = TrackPlay_Once[loopIn] ? PlaySettings.PlayOnce : PlaySettings.Looped;
        //PlaySettings playSettings = PlaySettings.PlayOnce;
        //playSettings.Loudness = 50;
        //playSettings.Loudness = TrackVolume[loopIn];
        //Log.Write("TrackVolume in PlayTrack: " + TrackVolume[loopIn]);
        //playSettings.DontSync = TrackDont_Sync[loopIn];
        //playSettings.PitchShift = TrackPitchShift[loopIn];
        //PlayHandle playHandle = null;
        //playHandle = ScenePrivate.PlaySoundAtPosition(TrackSamples[loopIn][samplecntr], SoundPos[loopIn], playSettings);
        int trkCounter = 0;
        LoopProgress = LoopProgress - 4;

        do
        {
            //Log.Write("A");
            //Log.Write("trkCounter: " + trkCounter + "  Sample: " + SampleForTracks[trkCounter].GetName());

            if (SampleNameToPlay[trkCounter] != null)
            {
                //Log.Write("A");
                //Log.Write("trkCounter: " + trkCounter);
                //Log.Write("SampleNameToPlay: " + SampleNameToPlay[trkCounter]);
                //Log.Write("PlaySettings.Loudness: " + playSettingsArray[trkCounter].Loudness);
                //Log.Write("TrackVolume: " + TrackVolume[trkCounter]);
                //Log.Write("Master Volume: " + MasterVolume);
                //float CombinedVolume = (((TrackVolume[trkCounter]) * (MasterVolume))/80);
                //Log.Write("Combined Volume: " + CombinedVolume);
                //playSettingsArray[trkCounter].Loudness = LoudnessPercentToDb(CombinedVolume);

                //Log.Write("PlaySettings.Loudness After: " + playSettingsArray[trkCounter].Loudness);

                //Log.Write("LoopProgress: " + LoopProgress + " SampleLength: " + SampleLength[trkCounter] + " Remainder: " + Remainder);
                //playHandleArray[trkCounter] = ScenePrivate.PlaySound(SampleForTracks[trkCounter], playSettingsArray[trkCounter]);
                if (SampleLength[trkCounter] < 33)
                {
                    int Remainder = LoopProgress % SampleLength[trkCounter];
                    if (Remainder == 0)
                    {
                        if (trkCounter < 8)
                        {
                            //playHandleArray[trkCounter] = ScenePrivate.PlaySound(SampleForTracks[trkCounter], playSettingsArray[trkCounter]);
                            if (SoundType == "Local")
                            {
                                ScenePrivate.PlaySoundAtPosition(SampleForTracks[trkCounter], TrackPan[trkCounter], playSettingsArray[trkCounter]);
                            }
                            else
                            {
                                ScenePrivate.PlaySound(SampleForTracks[trkCounter], playSettingsArray[trkCounter]);
                            }
                        }
                        else
                        {
                            if (SoundType == "Local")
                            {
                                //playHandleArray[trkCounter] = agent.PlaySoundAtPosition(SampleForTracks[trkCounter], TrackPan[trkCounter], playSettingsArray[trkCounter]);
                                agent.PlaySoundAtPosition(SampleForTracks[trkCounter], TrackPan[trkCounter], playSettingsArray[trkCounter]);
                            }
                            else
                            {
                                agent.PlaySound(SampleForTracks[trkCounter], playSettingsArray[trkCounter]);
                            }
                        }
                    }
                }
                else
                {
                    if (SampleLength[trkCounter] == SampleCountdown[trkCounter])
                    {
                        if (SoundType == "Local")
                        {
                            ScenePrivate.PlaySoundAtPosition(SampleForTracks[trkCounter], TrackPan[trkCounter], playSettingsArray[trkCounter]);
                        }
                        else
                        {
                            ScenePrivate.PlaySound(SampleForTracks[trkCounter], playSettingsArray[trkCounter]);
                        }

                    }
                    SampleCountdown[trkCounter] = SampleCountdown[trkCounter] - 4;
                    if (SampleCountdown[trkCounter] < 0)
                    {
                        SampleCountdown[trkCounter] = SampleLength[trkCounter];
                    }
                }
                //Log.Write("B");
            }
            trkCounter++;
        } while (trkCounter < numTracks);

        if (LoopProgress < 1)
        {
            LoopProgress = 36;
            TimerSub.Unsubscribe();
            TimerSub = null;
            SetTmer();
        }
    }

    /*
    private void StopTrack()
    {
        int loopIn = loopNum;
        TrackStop[loopIn] = true;
        Log.Write("In StopTrack");
        while (TrackRunning[loopIn])
        {
            Wait(TimeSpan.FromMilliseconds(10));
        }
        TrackStop[loopIn] = false;
        Log.Write("actually stopped loop" + loopNum);
    }
    */


    /*
    private void PlayTrack()
    {
        int loopIn = loopNum;
        int seqcntr = 0;
        int samplecntr = 0;
        int playindex = 0;
        int beatscntr = 0;
        int beatscntr2 = 0;
        DateTime CurrentTime;
        DateTime LastTime = new DateTime(2017, 10, 1);
        TimeSpan timerdif;
        float tickadjust;
        double sleepadjusted;
        Random r = new Random();
        //float Volume_Diff_Percent;
        //Log.Write("In PlayTrack");
        while (!TrackStop[loopIn]) //live loop
        {
            //Log.Write("TrackStop[loopIn]: " + TrackStop[loopIn]);
            CurrentTime = DateTime.Now;
            timerdif = CurrentTime.Subtract(LastTime);

            if (CueActive[loopIn])
            {
                while (TrackBlock[SyncTrack[loopIn]]) Wait(TimeSpan.FromMilliseconds(5));
                CueActive[loopIn] = false;
            }
            TrackRunning[loopIn] = true;
            //Log.Write("(TrackSequence[loopIn][seqcntr]: " + TrackSequence[loopIn][seqcntr]);
            if (TrackSequence[loopIn].Count > 0)
            {
                TrackBlock[loopIn] = true;
                do
                {
                    if (TrackSequence[loopIn][seqcntr] == s)
                    {
                        PlaySettings playSettings = TrackPlay_Once[loopIn] ? PlaySettings.PlayOnce : PlaySettings.Looped;
                        playSettings.Loudness = TrackVolume[loopIn];
                        //Log.Write("TrackVolume in PlayTrack: " + TrackVolume[loopIn]);
                        playSettings.DontSync = TrackDont_Sync[loopIn];
                        playSettings.PitchShift = TrackPitchShift[loopIn];
                        PlayHandle playHandle;
                        //playHandle = ScenePrivate.PlaySoundAtPosition(TrackSamples[loopIn][samplecntr], SoundPos[loopIn], playSettings);
                        playHandle = ScenePrivate.PlaySound(TrackSamples[loopIn][samplecntr], playSettings);
                        samplecntr++;
                    }
                    else if (TrackSequence[loopIn][seqcntr] == i)
                    {
                        PlaySettings playSettings = TrackPlay_Once[loopIn] ? PlaySettings.PlayOnce : PlaySettings.Looped;
                        playSettings.Loudness = TrackVolume[loopIn];
                        playSettings.DontSync = TrackDont_Sync[loopIn];
                        if (TrackArrayAccess[loopIn] == "random")
                        {
                            playindex = r.Next(0, TrackSamples[loopIn].Count());
                        }
                        else if (TrackArrayAccess[loopIn] == "shuffle")
                        {
                        }
                        else if (TrackArrayAccess[loopIn] == "invert")
                        {
                        }
                        else playindex = samplecntr;
                        playSettings.PitchShift = TrackOffsets[loopIn][playindex];

                        //playHandle = ScenePrivate.PlaySoundAtPosition(TrackSamples[loopIn][playindex], SoundPos[loopIn], playSettings);
                        playHandle = ScenePrivate.PlaySound(TrackSamples[loopIn][samplecntr], playSettings);
                        samplecntr++;
                    }
                    else if (TrackSequence[loopIn][seqcntr] == b)
                    {
                        if (beatscntr == TrackMilliSeconds[loopIn].Count - 1)  //last wait in loop
                        {
                            //Adjust the wait time based on how much time we have lost during the loop playing
                            tickadjust = (timerdif.Ticks - (TrackTotalMilliseconds[loopIn] * 10000) - 100000);  //calculate using ticks
                            sleepadjusted = TrackMilliSeconds[loopIn][beatscntr] - (tickadjust / 10000);  //apply using milliseconds
                            if (tickadjust < 1000000)
                            {
                                Wait(TimeSpan.FromMilliseconds(sleepadjusted));  //skips the first time the loop is executed
                            }
                            else
                            {
                                Wait(TimeSpan.FromMilliseconds(TrackMilliSeconds[loopIn][beatscntr]));
                            }
                        }
                        else
                        {
                            //not the last wait in the loop, so, do not adjust the wait time and use the beats statement
                            Wait(TimeSpan.FromMilliseconds(TrackMilliSeconds[loopIn][beatscntr]));
                        }
                        TrackBlock[loopIn] = false;
                        Wait(TimeSpan.FromMilliseconds(10));
                        beatscntr++;
                    }
                    else if (TrackSequence[loopIn][seqcntr] == m)
                    {
                        if (beatscntr2 == TrackMilliSeconds[loopIn].Count - 1)  //last wait in loop
                        {
                            //Adjust the wait time based on how much time we have lost during the loop playing
                            tickadjust = (timerdif.Ticks - (TrackTotalMilliseconds[loopIn] * 10000) - 100000);  //calculate using ticks
                            sleepadjusted = TrackMilliSeconds[loopIn][beatscntr2] - (tickadjust / 10000);  //apply using milliseconds
                            if (tickadjust < 1000000)
                            {
                                Wait(TimeSpan.FromMilliseconds(sleepadjusted));  //skips the first time the loop is executed
                            }
                            else
                            {
                                Wait(TimeSpan.FromMilliseconds(TrackMilliSeconds[loopIn][beatscntr2]));
                            }
                        }
                        else
                        {
                            //not the last wait in the loop, so, do not adjust the wait time and use the beats statement
                            Wait(TimeSpan.FromMilliseconds(TrackMilliSeconds[loopIn][beatscntr2]));
                        }
                        TrackBlock[loopIn] = false;
                        Wait(TimeSpan.FromMilliseconds(10));
                        beatscntr2++;
                        if (beatscntr2 == TrackMilliSeconds[loopIn].Count())
                        {
                            beatscntr2 = 0;  //make it a ring
                        }

                    }
                    seqcntr++;
                } while (seqcntr < TrackSequence[loopIn].Count);
                seqcntr = 0;
                samplecntr = 0;
                beatscntr = 0;
            }
            LastTime = CurrentTime;
        }
        TrackRunning[loopIn] = false;
    }

    /*
    private void StopAll()
    {
        loopNum = 0;
        int loopcntr = 0;  //don't stop BPM
        Log.Write("TrackRunning.Count(): " + TrackRunning.Count());
        int RunningTracks = TrackRunning.Count();
        do
        {
            //if (TrackRunning[loopNum])
            //{
                loopNum = loopcntr;
                Log.Write("Stopping loop" + loopNum);
                StartCoroutine(StopTrack);
                Log.Write("After Stop Track has been called");
                Wait(TimeSpan.FromSeconds(1.0));
                loopcntr++;
            //}
        } while (loopcntr < RunningTracks);
    }
    */

    #endregion

    #region CommandSection

    /*
        private void GetChatCommand(ChatData Data)
        {
            string DataCmd = Data.Message;
            //Log.Write("DataCmd: " + DataCmd);
            ParseCommands(DataCmd); 
        }

        private void ParseCommands(string DataCmdIn)
        {
            loopNum = 0;
            Errormsg = "No Errors";
            Log.Write(DataCmdIn);
            if (DataCmdIn.Contains("/"))
            {
                int loopStop = 0;
                strErrors = false;

                if (DataCmdIn.Contains("/loop"))
                {
                    if (DataCmdIn.Contains("/loop0")) loopNum = 0;
                    else if (DataCmdIn.Contains("/loop1")) loopNum = 1;
                    else if (DataCmdIn.Contains("/loop2")) loopNum = 2;
                    else if (DataCmdIn.Contains("/loop3")) loopNum = 3;
                    else if (DataCmdIn.Contains("/loop4")) loopNum = 4;
                    else if (DataCmdIn.Contains("/loop5")) loopNum = 5;
                    else if (DataCmdIn.Contains("/loop6")) loopNum = 6;
                    else if (DataCmdIn.Contains("/loop7")) loopNum = 7;
                    else if (DataCmdIn.Contains("/loop8")) loopNum = 8;
                    else if (DataCmdIn.Contains("/loop9")) loopNum = 9;

                    if (DataCmdIn.Contains("vol"))
                    {
                        string BareDataCmd = StripDataCmd(DataCmdIn);
                        TrackDataCmd[loopNum] = BareDataCmd;
                    }
                    else
                    {
                        TrackDataCmd[loopNum] = DataCmdIn;
                    }


                    SendActiveBins sendActiveBins = new SendActiveBins();
                    sendActiveBins.ActiveBin = new List<string>();
                    //Log.Write("A1");
                    sendActiveBins.SendActiveBin.Add(loopNum.ToString());
                    // Log.Write("A2");
                    if (DataCmdIn.Contains("vol("))
                    {
                        sendActiveBins.SendActiveBin.Add("volume");
                    }
                    else
                    {
                        sendActiveBins.SendActiveBin.Add("sample");
                    }
                    //Log.Write("A3");
                    PostScriptEvent(ScriptId.AllScripts, "ReturnBeatBlock", sendActiveBins);
                    //Log.Write("A4");
                    if (!(DataCmdIn.Contains("beats")))
                    {
                        strErrors = true;
                        ScenePrivate.Chat.MessageAllUsers("Loop must have a beats command");
                    }

                    if (DataCmdIn.Contains("stop"))
                    {
                        if (TrackRunning[loopNum])
                        {
                            loopStop = 0;
                            StartCoroutine(StopTrack);
                        }
                    }
                    else if (DataCmdIn.Contains("sample("))
                    {
                        //Log.Write("processing Sample");
                        List<string> TempCmdsIn;
                        TempCmdsIn = ParseIt(DataCmdIn);
                        if (TrackRunning[loopNum]) StartCoroutine(StopTrack);
                        if (DataCmdIn.Contains("sync("))
                        {
                            CueActive[loopNum] = true;
                            int from = DataCmdIn.IndexOf("sync(", StringComparison.CurrentCulture);
                            string test = DataCmdIn.Substring(from, DataCmdIn.Length - from);
                            int to = test.IndexOf(")", StringComparison.CurrentCulture);
                            string sx = test.Substring(5, to - 5);
                            if (sx == "loop0" || sx == "loop1" || sx == "loop2" || sx == "loop3" || sx == "loop4" || sx == "loop5" || sx == "loop6" || sx == "loop7")
                            {
                                SyncTrack[loopNum] = Int32.Parse(test.Substring(9, to - 9));  //SyncTrack[2] = 1
                            }
                            else
                            {
                                strErrors = true;
                                ScenePrivate.Chat.MessageAllUsers("Not Valid Loop to Sync To");
                            }
                        }
                        if (DataCmdIn.Contains("vol(")) TrackVolume[loopNum] = BuildVolume(DataCmdIn);
                        if (DataCmdIn.Contains("pan(")) SoundPos[loopNum] = BuildPan(DataCmdIn);
                        if (DataCmdIn.Contains("pitch(")) TrackPitchShift[loopNum] = BuildPitchShift(DataCmdIn);
                        if (DataCmdIn.Contains("rate("))
                        {
                            int from = DataCmdIn.IndexOf("rate(", StringComparison.CurrentCulture);
                            string test = DataCmdIn.Substring(from, DataCmdIn.Length - from);
                            int to = test.IndexOf(")", StringComparison.CurrentCulture);
                            //proccessing to handle 100/80 for easy sample timing matching
                            double tempRate = 0.0;
                            if (test.Substring(5, to - 5).Contains("/"))
                            {
                                char slash = '/';
                                string[] PitchRatio = test.Substring(5, to - 5).Split(slash);
                                tempRate = double.Parse(PitchRatio[0]) / double.Parse(PitchRatio[1]);
                            }
                            else tempRate = Double.Parse(test.Substring(5, to-5));
                            //Log.Write("new Rate: " + tempRate);
                            string tempPitch = RateToPitch(tempRate).ToString("G");
                            string tempPitch2 = "pitch(" + tempPitch + ")";
                            TrackPitchShift[loopNum] = BuildPitchShift(tempPitch2);
                        }
                        if ((bpm < 100) || (bpm > 100)) 
                        {
                            //proccessing to handle 100/80 for easy sample timing matching
                            double tempRate = 0.0;
                            tempRate = bpm / 100.0;
                            //Log.Write("new Rate: " + tempRate);
                            string tempPitch = RateToPitch(tempRate).ToString("G");
                            string tempPitch2 = "pitch(" + tempPitch + ")";
                            TrackPitchShift[loopNum] = BuildPitchShift(tempPitch2);
                        }
                        if (!(TrackSamples[loopNum] == null)) TrackSamples[loopNum].Clear();
                        if (!(TrackMilliSeconds[loopNum] == null)) TrackMilliSeconds[loopNum].Clear();
                        //Log.Write("Raver: loopNum: " + loopNum);
                        TrackSamples[loopNum] = BuildSamples(TempCmdsIn);

                        if (TrackSamples[loopNum].Count == 0)
                        {
                            strErrors = true;
                            ScenePrivate.Chat.MessageAllUsers("Sample Not Found");
                            //Log.Write("Raver: Sample Not Found");
                        }
                        TrackMilliSeconds[loopNum] = BuildTiming(TempCmdsIn);
                        TrackTotalMilliseconds[loopNum] = BuildTotalTime(TrackMilliSeconds[loopNum]);
                        TrackSequence[loopNum] = BuildSequence(TempCmdsIn);

                        //dump();
                        if (!strErrors) StartCoroutine(PlayTrack);
                    }
                    else if (DataCmdIn.Contains("inst("))
                    {
                        List<string> TempCmdsIn;
                        List<string> TempSamplesIn = new List<string>();
                        List<int> TempOffsetsIn = new List<int>();
                        //List<SoundResource> TempSoundResources = new List<SoundResource>();
                        List<float> TempTimingIn = new List<float>();
                        List<float> TempTimingIn2 = new List<float>();
                        List<char> TempSequenceIn = new List<char>();
                        TempCmdsIn = ParseIt(DataCmdIn);
                        if (TrackRunning[loopNum]) StartCoroutine(StopTrack);
                        if (DataCmdIn.Contains("sync("))
                        {
                            CueActive[loopNum] = true;
                            int from = DataCmdIn.IndexOf("sync(", StringComparison.CurrentCulture);
                            string test = DataCmdIn.Substring(from, DataCmdIn.Length - from);
                            int to = test.IndexOf(")", StringComparison.CurrentCulture);
                            string sx = test.Substring(5, to - 5);
                            if (sx == "loop0" || sx == "loop1" || sx == "loop2" || sx == "loop3" || sx == "loop4" || sx == "loop5" || sx == "loop6" || sx == "loop7")
                            {
                                SyncTrack[loopNum] = Int32.Parse(test.Substring(9, to - 9));  //SyncTrack[2] = 1
                            }
                            else
                            {
                                strErrors = true;
                                ScenePrivate.Chat.MessageAllUsers("Not Valid Loop to Sync To");
                            }
                        }
                        if (DataCmdIn.Contains("vol(")) TrackVolume[loopNum] = BuildVolume(DataCmdIn);
                        if (DataCmdIn.Contains("pan(")) SoundPos[loopNum] = BuildPan(DataCmdIn);
                        if (DataCmdIn.Contains("pitch(")) TrackPitchShift[loopNum] = BuildPitchShift(DataCmdIn);

                        if (DataCmdIn.Contains("random")) TrackArrayAccess[loopNum] = "random";
                        else if (DataCmdIn.Contains("shuffle")) TrackArrayAccess[loopNum] = "shuffle";
                        else if (DataCmdIn.Contains("invert")) TrackArrayAccess[loopNum] = "invert";
                        else TrackArrayAccess[loopNum] = "ring";

                        TrackNotes[loopNum] = BuildNotes(TempCmdsIn);
                        if (!(Errormsg == "No Errors"))
                        {
                            strErrors = true;
                            Log.Write("Error: " + Errormsg);
                            ScenePrivate.Chat.MessageAllUsers("Error: " + Errormsg);
                        }
                        else strErrors = false;
                        if (!strErrors)
                        {
                            if (!(TrackSequence[loopNum] == null)) TrackSequence[loopNum].Clear();
                            if (!(TrackSamples[loopNum] == null)) TrackSamples[loopNum].Clear();
                            if (!(TrackMilliSeconds[loopNum] == null)) TrackMilliSeconds[loopNum].Clear();
                            if (!(TrackOffsets[loopNum] == null)) TrackOffsets[loopNum].Clear();
                            //Log.Write("E");
                            int notecntr = 0;
                            //int timecntr = 0;
                            string strOffset = "";
                            int intOffset = 0;
                            do
                            {
                                //Build Instrument
                                // match instrument named in chat commad to instrument loaded into instrumentname array
                                int from = DataCmdIn.IndexOf("inst(", StringComparison.CurrentCulture);
                                string test = DataCmdIn.Substring(from, DataCmdIn.Length - from);
                                int to = test.IndexOf(")", StringComparison.CurrentCulture);
                                string InstrumentToFind = test.Substring(5, to - 5);
                                int instcntr = 0;
                                int instindex = 99;
                                do  //Find Instrument in Instrument Array
                                {
                                    if (InstrumentToFind == InstrumentName[instcntr]) instindex = instcntr;
                                    instcntr++;
                                } while (instcntr < InstrumentName.Count());
                                //Find Sample Name

                                if (instindex == 99)
                                {
                                    strErrors = true;
                                    ScenePrivate.Chat.MessageAllUsers("Instrument Not Found");
                                }
                                else
                                {
                                    TempSamplesIn.Add("sample " + InstrumentArray[instindex][TrackNotes[loopNum][notecntr] * 2]);  // add string of sample to be used in temp array
                                    //Build Offsets
                                    strOffset = InstrumentArray[instindex][TrackNotes[loopNum][notecntr] * 2 + 1];
                                    intOffset = Int32.Parse(strOffset);
                                    TempOffsetsIn.Add(intOffset);  // add int of offset to be used in temp array
                                }
                                //Build Timing Array
                                TempTimingIn = BuildTiming(TempCmdsIn);
                                int timecntr = 0;
                                do
                                {
                                    TempTimingIn2.Add(TempTimingIn[timecntr]);
                                    timecntr++;
                                } while (timecntr < TempTimingIn.Count());

                                //Build Sequence
                                TempSequenceIn.Add(i);
                                if (!DataCmdIn.Contains("chord"))
                                {
                                    if (DataCmdIn.Contains("beats([")) TempSequenceIn.Add(m);
                                    else TempSequenceIn.Add(b);  //for set or scale note, beats, note, beats
                                }
                                notecntr++;
                            } while (notecntr < TrackNotes[loopNum].Count());

                            //Build the SoundResources Samples List by passing a list of strings that have the names of the samples to be built by BuildSamples
                            TrackSamples[loopNum] = BuildSamples(TempSamplesIn);
                            if (TrackSamples[loopNum].Count == 0)
                            {
                                strErrors = true;
                                ScenePrivate.Chat.MessageAllUsers("Sample Not Found");
                            }
                            TrackOffsets[loopNum] = TempOffsetsIn;
                            TrackMilliSeconds[loopNum] = TempTimingIn2;
                            if (DataCmdIn.Contains("chord"))  // If it is a chord you only want to play it onnce per beat
                            {
                                if (DataCmdIn.Contains("beats([")) TempSequenceIn.Add(m);
                                else TempSequenceIn.Add(b);  //for set or scale note, beats, note, beats
                            }

                            TrackSequence[loopNum] = TempSequenceIn;

                            TrackTotalMilliseconds[loopNum] = BuildTotalTime(TrackMilliSeconds[loopNum]);

                            //dump();
                        }
                        if (!strErrors) StartCoroutine(PlayTrack);
                    }
                }

            }

            if (DataCmdIn.Contains("stopall"))
            {
                Log.Write("DataCmdIn was stopall");
                StartCoroutine(StopAll);
            }

            if (DataCmdIn.Contains("memory"))
            {
              Log.Write("Bytes Used: " + Memory.UsedBytes);
            }

            if (DataCmdIn.Contains("bpm"))
            {
                string InString = DataCmdIn;
                int from = InString.IndexOf("(", StringComparison.CurrentCulture);
                int to = InString.IndexOf(")", StringComparison.CurrentCulture);
                int length = InString.Length;
                int last = to - from;
                string chunk = InString.Substring(from + 1, last - 1);
                bpm = Int32.Parse(chunk);
            }
        }

        */

    #endregion
}
