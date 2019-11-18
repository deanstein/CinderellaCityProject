using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class GlobalVariables
{
    // if true, this audiosource is a slave to some other speaker and will be kept in sync with it
    public static bool isSlave;

    // the speaker type is determined by the name of the object
    public static string speakerType;

    // max amount of sync discrepancy a slave speaker can have with its master, in seconds
    public static float maxSlaveTimeDelta = 0.05f;

    // valid speaker names and properties to key off of for initialization and update
    // these names are based on FormIt object names found in the associated FBX file
    public const string mallChatter80s90sPartialName = "mall-ambient-chatter-80s90s";
    public static int mallChatter80s90sSpeakerCount = 0;
    public static AudioSource mallChatter80s90sMasterAudioSource;
    public static string[] mallChatter80s90sAudioClips = { "80s-90s/Ambient/BDalton/CinCityAmbience" };

    public const string mallMusic80s90sPartialName = "mall-music-80s90s";
    public static int mallMusic80s90sSpeakerCount = 0;
    public static AudioSource mallMusic80s90sMasterAudioSource;
    public static string[] mallMusic80s90sAudioClips = { "80s-90s/Music/Betamaxx/6. woolworth", "80s-90s/Music/Betamaxx/8. mall walking",  "80s-90s/Music/Betamaxx/12. casual menswear", "80s-90s/Music/Betamaxx/1. grand opening", "80s-90s/Music/Betamaxx/7. crystal fountain", "80s-90s/Music/Betamaxx/11. retail dystopia", "80s-90s/Music/BDalton/nick" };

    public const string storeMusicGeneric80s90sPartialName = "store-music-generic-80s90s";
    public static int storeMusicGeneric80s90sSpeakerCount = 0;
    public static AudioSource storeMusicGeneric80s90sMasterAudioSource;
    public static string[] storeMusicGeneric80s90sAudioClips = { "80s-90s/Music/Betamaxx/9. a'gaci", "14. smoking section" };

    public static string[] storeMusicConsumerBeauty80s90sAudioClips = { "80s-90s/Music/DeadMall/05 Thunderhead" };

    public const string storeMusicDolcis80s90sPartialName = "store-music-dolcis-80s90s";
    public static int storeMusicDolcis80s90sSpeakerCount = 0;
    public static AudioSource storeMusicDolcis80s90sMasterAudioSource;
    public static string[] storeMusicDolcis80s90sAudioClips = { "80s-90s/Music/Betamaxx/5. kauffmans", "80s-90s/Music/Betamaxx/10. lazarus" };

    public const string storeMusicMusicland80s90sPartialName = "store-music-musicland-80s90s";
    public static int storeMusicMusicland80s90sSpeakerCount = 0;
    public static AudioSource storeMusicMusicland80s90sMasterAudioSource;
    public static string[] storeMusicMusicland80s90sAudioClips = { "80s-90s/Music/DeadMall/01 Wheels", "80s-90s/Music/DeadMall/09 Pressure" };

}

// this script needs to be attached to an object that should be playing a sequence of audio tracks
public class PlayAudioSequencesByName : MonoBehaviour {

    private float nextSyncTime = 0.0f;
    public float audioSourceSyncPeriod = 1.0f;

    // gets the master audio source for this speaker based on its name
    public static AudioSource associateMasterAudioSourceByName(string name)
    {
        switch (name)
        {
            case string a when a.Contains(GlobalVariables.mallChatter80s90sPartialName):
                return GlobalVariables.mallChatter80s90sMasterAudioSource;
            case string a when a.Contains(GlobalVariables.mallMusic80s90sPartialName):
                return GlobalVariables.mallMusic80s90sMasterAudioSource;
            case string a when a.Contains(GlobalVariables.storeMusicDolcis80s90sPartialName):
                return GlobalVariables.storeMusicDolcis80s90sMasterAudioSource;
            case string a when a.Contains(GlobalVariables.storeMusicGeneric80s90sPartialName):
                return GlobalVariables.storeMusicGeneric80s90sMasterAudioSource;
            case string a when a.Contains(GlobalVariables.storeMusicMusicland80s90sPartialName):
                return GlobalVariables.storeMusicMusicland80s90sMasterAudioSource;
            default:
                return null;
        }
    }

    // Use this for initialization
    void Start()
    {
        // get the audio source for this GameObject
        AudioSource audioSourceComponent = this.GetComponent<AudioSource>();
        //Debug.Log("This audio source: " + audioSourceComponent);

        // set default audio source options
        audioSourceComponent.spatialBlend = 1.0F;
        audioSourceComponent.dopplerLevel = 0F;
        audioSourceComponent.rolloffMode = AudioRolloffMode.Custom;
        audioSourceComponent.loop = false;
        audioSourceComponent.bypassEffects = true;
        audioSourceComponent.bypassListenerEffects = true;
        audioSourceComponent.bypassReverbZones = true;

        // set volumes based on type
        float mallAmbientChatterSpeakerVolume = 0.15F;
        float mallMusicSpeakerVolume = 0.2F;
        float storeMusicSpeakerVolume = 0.2F;

        // set max distances based on type
        float mallAmbientChatterSpeakerMaxDistance = 500F;
        float mallMusicSpeakerMaxDistance = 20F;
        float storeMusicSpeakerMaxDistance = 15F;

        // configures and plays the master/slave sequences by speaker type
        void PlayMasterOrSlaveSequences(ref int currentSpeakerTypeCount, ref AudioSource masterAudioSource, string[] clipSequence)
        {
            // increment the speaker count
            currentSpeakerTypeCount++;
            //Debug.Log("Current speaker type count: " + currentSpeakerTypeCount);

            // if the speaker count is only 1, this is the master speaker
            if (currentSpeakerTypeCount == 1)
            {
                GlobalVariables.isSlave = false;
                StartCoroutine(playMasterClipSequenceInOrder(masterAudioSource, clipSequence));
                masterAudioSource = this.GetComponent<AudioSource>();
            }
            // otherwise, this is a slave
            else
            {
                GlobalVariables.isSlave = true;
                StartCoroutine(playSlaveClipSequence(audioSourceComponent, clipSequence, masterAudioSource));
            }
        }


        //
        // play the clip sequences based on the object name
        //

        if (this.name.Contains(GlobalVariables.mallChatter80s90sPartialName))
        {
            audioSourceComponent.volume = mallAmbientChatterSpeakerVolume;
            audioSourceComponent.maxDistance = mallAmbientChatterSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            GlobalVariables.speakerType = GlobalVariables.mallChatter80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref GlobalVariables.mallChatter80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref GlobalVariables.mallChatter80s90sMasterAudioSource;
            ref string[] clipSequence = ref GlobalVariables.mallChatter80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }

        if (this.name.Contains(GlobalVariables.mallMusic80s90sPartialName))
        {
            audioSourceComponent.volume = mallMusicSpeakerVolume;
            audioSourceComponent.maxDistance = mallMusicSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            GlobalVariables.speakerType = GlobalVariables.mallMusic80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref GlobalVariables.mallMusic80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref GlobalVariables.mallMusic80s90sMasterAudioSource;
            ref string[] clipSequence = ref GlobalVariables.mallMusic80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);

            /*

            // increment the speaker count
            GlobalVariables.mallMusic80s90sSpeakerCount++;

            // if the speaker count is only 1, this is the master speaker
            if (GlobalVariables.mallMusic80s90sSpeakerCount == 1)
            {
                GlobalVariables.isSlave = false;
                StartCoroutine(playMasterClipSequenceInOrder(audioSourceComponent, GlobalVariables.mallMusic80s90sAudioClips));
                GlobalVariables.mallMusic80s90sMasterAudioSource = audioSourceComponent;
                //Debug.Log("Global audio source: " + GlobalVariables.mallMusic80s90sMasterAudioSource);
            }
            // otherwise, this is a slave
            else
            {
                GlobalVariables.isSlave = true;
                StartCoroutine(playSlaveClipSequence(audioSourceComponent, GlobalVariables.mallMusic80s90sAudioClips, GlobalVariables.mallMusic80s90sMasterAudioSource));
            }
            
        */
        }

        if (this.name.Contains(GlobalVariables.storeMusicGeneric80s90sPartialName))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            GlobalVariables.speakerType = GlobalVariables.storeMusicGeneric80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref GlobalVariables.storeMusicGeneric80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref GlobalVariables.storeMusicGeneric80s90sMasterAudioSource;
            ref string[] clipSequence = ref GlobalVariables.storeMusicGeneric80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }

        if (this.name.Contains(GlobalVariables.storeMusicDolcis80s90sPartialName))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            GlobalVariables.speakerType = GlobalVariables.storeMusicDolcis80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref GlobalVariables.storeMusicDolcis80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref GlobalVariables.storeMusicDolcis80s90sMasterAudioSource;
            ref string[] clipSequence = ref GlobalVariables.storeMusicDolcis80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }

        if (this.name.Contains(GlobalVariables.storeMusicMusicland80s90sPartialName))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            GlobalVariables.speakerType = GlobalVariables.storeMusicMusicland80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref GlobalVariables.storeMusicMusicland80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref GlobalVariables.storeMusicMusicland80s90sMasterAudioSource;
            ref string[] clipSequence = ref GlobalVariables.storeMusicMusicland80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }

    }

    // specify and play master speaker clip sequences
    IEnumerator playMasterClipSequenceInOrder(AudioSource masterAudioSource, string[] clipNames)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        int counter = 0;
        while (true)
        {
            masterAudioSource = this.GetComponent<AudioSource>();
            masterAudioSource.clip = (AudioClip)Resources.Load(clipNames[counter]);
            masterAudioSource.Play();
            Debug.Log("Playing master music " + masterAudioSource.clip.name + " on " + masterAudioSource);

            counter = (counter + 1) % clipNames.Length;

            yield return new WaitForSeconds(masterAudioSource.clip.length);
        }
    }

    // play slave speaker sequences
    IEnumerator playSlaveClipSequence(AudioSource audioSourceComponent, string[] clipNames, AudioSource masterAudioSource)
    {
        //Debug.Log("Master audio clip: " + masterAudioClip);

        // for each clip in the array, play it, and wait until the end to play the next
        int counter = 0;
        while (true)
        {
            masterAudioSource = this.GetComponent<AudioSource>();
            masterAudioSource.clip = (AudioClip)Resources.Load(clipNames[counter]);
            masterAudioSource.Play();
            Debug.Log("Playing master music " + masterAudioSource.clip.name + " on " + masterAudioSource);

            counter = (counter + 1) % clipNames.Length;

            yield return new WaitForSeconds(masterAudioSource.clip.length);
        }
    }

    // synchronize two AudioSources
    IEnumerator syncAudioSources(AudioSource masterAudioSource, AudioSource slaveAudioSource)
    {
        //Debug.Log("Master AudioSource (from sync): " + masterAudioSource);

        // ensure the slave's time is reasonably in-sync with the master's time
        if (Mathf.Abs(slaveAudioSource.time - masterAudioSource.time) > GlobalVariables.maxSlaveTimeDelta)
        {
            slaveAudioSource.time = masterAudioSource.time;
            Debug.Log("Synchronized AudioSource time between two AudioSources at " + Time.time);
            //Debug.Log("Synchronized AudioSource time between master: " + masterAudioSource + " and slave: " + slaveAudioSource + " at " + Time.time);
        }

        // ensure the slave's clip matches the master clip
        if (masterAudioSource.clip.name != slaveAudioSource.clip.name)
        {
            slaveAudioSource.clip = masterAudioSource.clip;
            Debug.Log("Synchronized AudioSource clip sequence at " + Time.time);
        }

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        // runs only if it's time to sync
        if (Time.time > nextSyncTime)
        {
            StartCoroutine(syncAudioSources(associateMasterAudioSourceByName(this.name), this.GetComponent<AudioSource>()));   

            nextSyncTime += audioSourceSyncPeriod;
        }
    }
}

