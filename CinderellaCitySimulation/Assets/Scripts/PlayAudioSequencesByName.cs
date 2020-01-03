using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class AudioSourceGlobalVariables
{
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
    public static string[] mallMusic80s90sAudioClips = { "80s-90s/Music/Betamaxx/6. woolworth", "80s-90s/Music/Betamaxx/8. mall walking", "80s-90s/Music/Betamaxx/12. casual menswear", "80s-90s/Music/Betamaxx/1. grand opening", "80s-90s/Music/Betamaxx/7. crystal fountain", "80s-90s/Music/Betamaxx/11. retail dystopia", "80s-90s/Music/BDalton/nick" };

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

public class PlayAudioSequencesByName : AutoResumeCoroutines
{

    private float nextSyncTime = 0.0f;
    public float audioSourceSyncPeriod = 1.0f;

    // gets the master audio source for this speaker based on its name
    public static AudioSource AssociateMasterAudioSourceByName(string name)
    {
        switch (name)
        {
            case string a when a.Contains(AudioSourceGlobalVariables.mallChatter80s90sPartialName):
                return AudioSourceGlobalVariables.mallChatter80s90sMasterAudioSource;
            case string a when a.Contains(AudioSourceGlobalVariables.mallMusic80s90sPartialName):
                return AudioSourceGlobalVariables.mallMusic80s90sMasterAudioSource;
            case string a when a.Contains(AudioSourceGlobalVariables.storeMusicDolcis80s90sPartialName):
                return AudioSourceGlobalVariables.storeMusicDolcis80s90sMasterAudioSource;
            case string a when a.Contains(AudioSourceGlobalVariables.storeMusicGeneric80s90sPartialName):
                return AudioSourceGlobalVariables.storeMusicGeneric80s90sMasterAudioSource;
            case string a when a.Contains(AudioSourceGlobalVariables.storeMusicMusicland80s90sPartialName):
                return AudioSourceGlobalVariables.storeMusicMusicland80s90sMasterAudioSource;
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
                // set this as the master AudioSource
                masterAudioSource = this.GetComponent<AudioSource>();
                // start playing the master clip sequence
                StartAutoResumeCoroutine(PlayMasterClipSequenceInOrder(clipSequence));
            }
            // otherwise, this is a slave
            else
            {
                // start playing the clip sequence, but use the master AudioSource's clip
                StartAutoResumeCoroutine(PlaySlaveClipSequence(clipSequence));
            }
        }

        //
        // play the clip sequences based on the object name
        //

        if (this.name.Contains(AudioSourceGlobalVariables.mallChatter80s90sPartialName))
        {
            audioSourceComponent.volume = mallAmbientChatterSpeakerVolume;
            audioSourceComponent.maxDistance = mallAmbientChatterSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            AudioSourceGlobalVariables.speakerType = AudioSourceGlobalVariables.mallChatter80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref AudioSourceGlobalVariables.mallChatter80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref AudioSourceGlobalVariables.mallChatter80s90sMasterAudioSource;
            ref string[] clipSequence = ref AudioSourceGlobalVariables.mallChatter80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }

        if (this.name.Contains(AudioSourceGlobalVariables.mallMusic80s90sPartialName))
        {
            audioSourceComponent.volume = mallMusicSpeakerVolume;
            audioSourceComponent.maxDistance = mallMusicSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            AudioSourceGlobalVariables.speakerType = AudioSourceGlobalVariables.mallMusic80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref AudioSourceGlobalVariables.mallMusic80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref AudioSourceGlobalVariables.mallMusic80s90sMasterAudioSource;
            ref string[] clipSequence = ref AudioSourceGlobalVariables.mallMusic80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }

        if (this.name.Contains(AudioSourceGlobalVariables.storeMusicGeneric80s90sPartialName))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            AudioSourceGlobalVariables.speakerType = AudioSourceGlobalVariables.storeMusicGeneric80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref AudioSourceGlobalVariables.storeMusicGeneric80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref AudioSourceGlobalVariables.storeMusicGeneric80s90sMasterAudioSource;
            ref string[] clipSequence = ref AudioSourceGlobalVariables.storeMusicGeneric80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }

        if (this.name.Contains(AudioSourceGlobalVariables.storeMusicDolcis80s90sPartialName))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            AudioSourceGlobalVariables.speakerType = AudioSourceGlobalVariables.storeMusicDolcis80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref AudioSourceGlobalVariables.storeMusicDolcis80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref AudioSourceGlobalVariables.storeMusicDolcis80s90sMasterAudioSource;
            ref string[] clipSequence = ref AudioSourceGlobalVariables.storeMusicDolcis80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }

        if (this.name.Contains(AudioSourceGlobalVariables.storeMusicMusicland80s90sPartialName))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;

            // define the global speaker type based on the given partial name
            AudioSourceGlobalVariables.speakerType = AudioSourceGlobalVariables.storeMusicMusicland80s90sPartialName;

            // associate data based on the speakerType
            ref int speakerCount = ref AudioSourceGlobalVariables.storeMusicMusicland80s90sSpeakerCount;
            ref AudioSource masterAudioSource = ref AudioSourceGlobalVariables.storeMusicMusicland80s90sMasterAudioSource;
            ref string[] clipSequence = ref AudioSourceGlobalVariables.storeMusicMusicland80s90sAudioClips;

            PlayMasterOrSlaveSequences(ref speakerCount, ref masterAudioSource, clipSequence);
        }
    }

    // specify and play master speaker clip sequences
    IEnumerator PlayMasterClipSequenceInOrder(string[] clipNames)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        int counter = 0;
        while (true)
        {
            // set this object as the master audio source
            AudioSource masterAudioSource = this.GetComponent<AudioSource>();
            // set and play the clip based on the list of clip names
            masterAudioSource.clip = (AudioClip)Resources.Load(clipNames[counter]);
            masterAudioSource.Play();
            Debug.Log("Playing master music " + masterAudioSource.clip.name + " on " + masterAudioSource);

            // calculate how much time is left in the clip to accurately set the WaitForSeconds
            float remainingClipTime = FastForwardMasterAudioSourceToMatchGameTime(masterAudioSource);
            
            // if we're at the end of the list, reset to return to the beginning
            counter = (counter + 1) % clipNames.Length;

            yield return new WaitForSeconds(remainingClipTime);
        }
    }

    // play slave speaker sequences
    IEnumerator PlaySlaveClipSequence(string[] clipNames)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        int counter = 0;
        while (true)
        {
            // get the corresponding master AudioSource based on this object's name
            AudioSource masterAudioSource = AssociateMasterAudioSourceByName(this.name);
            // this object is the slave
            AudioSource slaveAudioSource = this.GetComponent<AudioSource>();
            // set and sync the master and slave audio source clips
            slaveAudioSource.clip = masterAudioSource.clip;
            slaveAudioSource.time = masterAudioSource.time;
            slaveAudioSource.Play();
            Debug.Log("Playing slave music " + masterAudioSource.clip.name);

            // calculate how much time is left in the clip to accurately set the WaitForSeconds
            float remainingClipTime = CalculateRemainingClipTime(slaveAudioSource);

            // if we're at the end of the list, reset to return to the beginning
            counter = (counter + 1) % clipNames.Length;

            yield return new WaitForSeconds(remainingClipTime);
        }
    }

    // synchronize two AudioSources
    IEnumerator SyncAudioSources(AudioSource masterAudioSource, AudioSource slaveAudioSource)
    {
        //Debug.Log("Master AudioSource (from sync): " + masterAudioSource);

        // ensure the slave's clip name mmatches the master clip
        if (masterAudioSource.clip.name != slaveAudioSource.clip.name)
        {
            Debug.Log("Fixed incorrect clips. Master: " + masterAudioSource.clip.name + " Slave: " + slaveAudioSource.clip.name);
            slaveAudioSource.clip = masterAudioSource.clip;
        }

        // ensure the slave's time is reasonably in-sync with the master's time
        if (Mathf.Abs(slaveAudioSource.time - masterAudioSource.time) > AudioSourceGlobalVariables.maxSlaveTimeDelta)
        {
            slaveAudioSource.time = masterAudioSource.time;
            slaveAudioSource.Play();

            Debug.Log("Synchronized AudioSource time between master: " + masterAudioSource + " and slave: " + slaveAudioSource + " at " + Time.time);
            //Debug.Log("Master audiosource clip: " + masterAudioSource.clip + " and time: " + masterAudioSource.time + " Slave audiosource clip: " + slaveAudioSource.clip + " and time: " +  slaveAudioSource.time);
        }

        yield return null;
    }

    float CalculateFastForwardClipTime(AudioSource audioSourceComponent)
    {
        // determine how many multiples of the current clip length can fit into the time since game started
        float fastForwardTimeAmount = Time.time % audioSourceComponent.clip.length;

        return fastForwardTimeAmount;
    }

    float CalculateRemainingClipTime(AudioSource audioSourceComponent)
    {

        float fastForwardTimeAmount = CalculateFastForwardClipTime(audioSourceComponent);

        // the remaining clip time - used to update the WaitForSeconds
        float remainingClipTime = audioSourceComponent.clip.length - fastForwardTimeAmount;

        return remainingClipTime;
    }

    float FastForwardMasterAudioSourceToMatchGameTime(AudioSource audioSourceComponentToFastForward)
    {
        if (audioSourceComponentToFastForward.clip)
        {
            float remainingClipTime = CalculateRemainingClipTime(audioSourceComponentToFastForward);

            // set the current audiosource to the fast-forwarded time amount
            audioSourceComponentToFastForward.time = CalculateFastForwardClipTime(audioSourceComponentToFastForward);

            return remainingClipTime;
        }
        return 0;
    }

    // Update is called once per frame
    void Update()
    {
        // runs only if it's time to sync
        if (Time.time > nextSyncTime)
        {
            StartAutoResumeCoroutine(SyncAudioSources(AssociateMasterAudioSourceByName(this.name), this.GetComponent<AudioSource>()));

            nextSyncTime += audioSourceSyncPeriod;
        }
    }
}