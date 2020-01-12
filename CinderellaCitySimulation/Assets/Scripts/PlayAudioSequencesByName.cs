using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script needs to be attached to an object that plays a series of AudioClips
// NOTE: this script assumes AudioClips have the proper import settings applied (Load in Background, Compressed in Memory, etc) which should be handled by AssetImportPipeline
// if not, the game may stutter or hang when invoking "Resources.Load()" on an AudioClip

// the object this script is attached to should have these components
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(ToggleComponentByProximityToPlayer))]

// define the type of data/parameters that a set of speakers can get and set
[System.Serializable]
public class SpeakerParams
{
    public AudioSource masterAudioSource;
    public string[] clipSequence;
    public int currentClipIndex = 0;
    public int activeSlaveCount = 0;
    public float speakerVolume = 0; // initialize at 0 to prevent a frame of extra-loud music
    public float maxDistance;
}

public class AudioSourceGlobals
{
    //
    // initialize speaker parameter sets
    //

    // mall
    public static SpeakerParams MallChatter80s90sParams = new SpeakerParams();
    public static SpeakerParams MallMusic80s90sParams = new SpeakerParams();

    // stores
    public static SpeakerParams StoreMusicGeneric80s90sParams = new SpeakerParams();
    public static SpeakerParams StoreMusicConsumerBeauty80s90sParams = new SpeakerParams();
    public static SpeakerParams StoreMusicDolcis80s90sParams = new SpeakerParams();
    public static SpeakerParams StoreMusicMusicland80s90sParams = new SpeakerParams();
}

public class PlayAudioSequencesByName : MonoBehaviour
{
    // define components to be accessed or modified
    AudioSource thisAudioSourceComponent;
    ToggleComponentByProximityToPlayer thisToggleComponentByProximityScript;

    // used for synchronizing a slave and a master audiosource
    private float masterSlaveInitialSyncTime = 0.0f;
    public static float maxMasterSlaveTimeDelta = 0.05f; // max out-of-sync a slave and master can get
    public float masterSlaveSyncPeriod = 1.0f;

    // used to differentiate between a scene change and a song change
    // when the scene is changed, the object was disabled/re-enabled and the clip should be fast-forwarded
    // if the song is simply changing, start the next song without fast-forwarding
    // mark this true so fast-forwarding happens on first load
    private bool needsFastForwarding = true;

    // return speaker parameters by object name
    public SpeakerParams AssociateSpeakerParamsByName(string name)
    {
        switch (name)
        {
            case string partialName when partialName.Contains("mall-ambient-chatter-80s90s"):
                return AudioSourceGlobals.MallChatter80s90sParams;
            case string partialName when partialName.Contains("mall-music-80s90s"):
                return AudioSourceGlobals.MallMusic80s90sParams;
            case string partialName when partialName.Contains("store-music-generic-80s90s"):
                return AudioSourceGlobals.StoreMusicGeneric80s90sParams;
            case string partialName when partialName.Contains("store-music-consumer-beauty-80s90s"):
                return AudioSourceGlobals.StoreMusicConsumerBeauty80s90sParams;
            case string partialName when partialName.Contains("store-music-dolcis-80s90s"):
                return AudioSourceGlobals.StoreMusicDolcis80s90sParams;
            case string partialName when partialName.Contains("store-music-musicland-80s90s"):
                return AudioSourceGlobals.StoreMusicMusicland80s90sParams;
            default:
                return null;
        }
    }

    private void Awake()
    {
        // set options for this object's AudioSource component
        thisAudioSourceComponent = this.GetComponent<AudioSource>();
        thisAudioSourceComponent.volume = AssociateSpeakerParamsByName(this.name).speakerVolume;
        thisAudioSourceComponent.maxDistance = AssociateSpeakerParamsByName(this.name).maxDistance;
        thisAudioSourceComponent.spatialBlend = 1.0F;
        thisAudioSourceComponent.dopplerLevel = 0F;
        thisAudioSourceComponent.rolloffMode = AudioRolloffMode.Custom;
        thisAudioSourceComponent.loop = false;
        thisAudioSourceComponent.bypassEffects = true;
        thisAudioSourceComponent.bypassListenerEffects = true;
        thisAudioSourceComponent.bypassReverbZones = true;

        // set options for this object's ToggleComponentByProximity script component
        thisToggleComponentByProximityScript = this.GetComponent<ToggleComponentByProximityToPlayer>();
        thisToggleComponentByProximityScript.maxDistance = AssociateSpeakerParamsByName(this.name).maxDistance;

        // 
        // assign AudioSource data per type
        //

        // mall - ambient chatter
        AudioSourceGlobals.MallChatter80s90sParams.maxDistance = 500f;
        AudioSourceGlobals.MallChatter80s90sParams.speakerVolume = 0.15f;
        AudioSourceGlobals.MallChatter80s90sParams.clipSequence = new string[] { "80s-90s/Ambient/BDalton/CinCityAmbience" };

        // mall - common area music
        AudioSourceGlobals.MallMusic80s90sParams.maxDistance = 20f;
        AudioSourceGlobals.MallMusic80s90sParams.speakerVolume = 0.2f;
        AudioSourceGlobals.MallMusic80s90sParams.clipSequence = new string[] { "80s-90s/Music/Betamaxx/6. woolworth", "80s-90s/Music/Betamaxx/8. mall walking", "80s-90s/Music/Betamaxx/12. casual menswear", "80s-90s/Music/Betamaxx/1. grand opening", "80s-90s/Music/Betamaxx/7. crystal fountain", "80s-90s/Music/Betamaxx/11. retail dystopia", "80s-90s/Music/BDalton/nick" };

        // store - generic
        AudioSourceGlobals.StoreMusicGeneric80s90sParams.maxDistance = 15f;
        AudioSourceGlobals.StoreMusicGeneric80s90sParams.speakerVolume = 0.2f;
        AudioSourceGlobals.StoreMusicGeneric80s90sParams.clipSequence = new string[] { "80s-90s/Music/Betamaxx/9. a'gaci", "80s-90s/Music/Betamaxx/14. smoking section" };

        // store - consumer beauty
        AudioSourceGlobals.StoreMusicConsumerBeauty80s90sParams.maxDistance = 15f;
        AudioSourceGlobals.StoreMusicConsumerBeauty80s90sParams.speakerVolume = 0.2f;
        AudioSourceGlobals.StoreMusicConsumerBeauty80s90sParams.clipSequence = new string[] { "80s-90s/Music/DeadMall/05 Thunderhead" };

        // store - dolcis
        AudioSourceGlobals.StoreMusicDolcis80s90sParams.maxDistance = 15f;
        AudioSourceGlobals.StoreMusicDolcis80s90sParams.speakerVolume = 0.2f;
        AudioSourceGlobals.StoreMusicDolcis80s90sParams.clipSequence = new string[] { "80s-90s/Music/Betamaxx/5. kauffmans", "80s-90s/Music/Betamaxx/10. lazarus" };

        // store - musicland
        AudioSourceGlobals.StoreMusicMusicland80s90sParams.maxDistance = 15f;
        AudioSourceGlobals.StoreMusicMusicland80s90sParams.speakerVolume = 0.2f;
        AudioSourceGlobals.StoreMusicMusicland80s90sParams.clipSequence = new string[] { "80s-90s/Music/DeadMall/01 Wheels", "80s-90s/Music/DeadMall/09 Pressure" };        
    }

    void OnEnable()
    {
        // execute the AutoResumeCoRoutine OnEnable() first
        //base.OnEnable();

        // if no master audio source for this type, this AudioSource should be considered the master
        if (!AssociateSpeakerParamsByName(this.name).masterAudioSource)
        {
            // set this speaker AudioSource as the Master AudioSource for this type
            AssociateSpeakerParamsByName(this.name).masterAudioSource = thisAudioSourceComponent;

            // ensure this master cannot be disabled until checks are made in update()
            thisToggleComponentByProximityScript.isExcepted = true;

           StartCoroutine(PlayMasterClipSequenceInOrder(AssociateSpeakerParamsByName(this.name).clipSequence));

            needsFastForwarding = false;
        }
        // otherwise, this must be a slave AudioSource
        else
        {
            // increase the count so we know when there are slaves relying on a master
            AssociateSpeakerParamsByName(this.name).activeSlaveCount++;

            StartCoroutine(PlaySlaveClipSequence(AssociateSpeakerParamsByName(this.name).clipSequence));
        }
    }

    void OnDisable()
    {
        // execute the AutoResumeCoRoutine OnDisable() first
        //base.OnDisable();

        // check if this is the master AudioSource
        if (thisAudioSourceComponent == AssociateSpeakerParamsByName(this.name).masterAudioSource)
        {
            //look up the current clip index, and subtract it by one to avoid skipping tracks when re-enabled
            if (AssociateSpeakerParamsByName(this.name).currentClipIndex > 0)
            {
                AssociateSpeakerParamsByName(this.name).currentClipIndex--;
            }

            // set the flag that fast-forwarding should happen on next enable
            needsFastForwarding = true;

            // set the master audio source for this type as null, so the next instance of this type is considered the new master
            AssociateSpeakerParamsByName(this.name).masterAudioSource = null;
        }
        // otherwise, this must be a slave AudioSource
        else
        {
            // decrease the count so we can keep track of active slaves
            AssociateSpeakerParamsByName(this.name).activeSlaveCount--;
        }
    }

    // specify and play master speaker clip sequences
    IEnumerator PlayMasterClipSequenceInOrder(string[] clipNames)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        int counter = AssociateSpeakerParamsByName(this.name).currentClipIndex;
        while (true)
        {
            // set this object as the master audio source
            AudioSource masterAudioSource = thisAudioSourceComponent;
            // set and play the clip based on the list of clip names
            masterAudioSource.clip = (AudioClip)Resources.Load(clipNames[counter]);
            masterAudioSource.Play();
            Debug.Log("Playing master music " + masterAudioSource.clip.name + " on " + masterAudioSource);

            float remainingClipTime;

            Debug.Log("Test: Did I need fast forwarding?" + needsFastForwarding);

            // fast forward time if the flag is set
            if (needsFastForwarding)
            {
                // calculate how much time is left in the clip to accurately set the WaitForSeconds
                remainingClipTime = FastForwardMasterAudioSourceToMatchGameTime(masterAudioSource);
                needsFastForwarding = false;
                Debug.Log("Clip was set to fast forward.");
            }
            // otherwise the remaining clip time is just the clip's length
            else
            {
                remainingClipTime = masterAudioSource.clip.length;
                needsFastForwarding = false;
                Debug.Log("Clip was NOT set to fast forward.");
            }

            // if we're at the end of the list, reset to return to the beginning
            counter = (counter + 1) % clipNames.Length;
            AssociateSpeakerParamsByName(this.name).currentClipIndex = counter;

            yield return new WaitForSeconds(remainingClipTime);
        }
    }

    // play slave speaker sequences
    IEnumerator PlaySlaveClipSequence(string[] clipNames)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        int counter = AssociateSpeakerParamsByName(this.name).currentClipIndex;
        while (true)
        {
            // get the corresponding master AudioSource based on this object's name
            AudioSource masterAudioSource = AssociateSpeakerParamsByName(this.name).masterAudioSource;
            // this object is the slave
            AudioSource slaveAudioSource = thisAudioSourceComponent;
            // set and sync the master and slave audio source clips
            slaveAudioSource.clip = masterAudioSource.clip;
            slaveAudioSource.time = masterAudioSource.time;
            slaveAudioSource.Play();
            Debug.Log("Playing slave music " + masterAudioSource.clip.name + " on " + slaveAudioSource);

            // calculate how much time is left in the clip to accurately set the WaitForSeconds
            float remainingClipTime = CalculateRemainingClipTime(slaveAudioSource);

            // if we're at the end of the list, reset to return to the beginning
            counter = (counter + 1) % clipNames.Length;
            //AssociateSpeakerParamsByName(this.name).currentClipIndex = counter;

            yield return new WaitForSeconds(remainingClipTime);
        }
    }

    // synchronize two AudioSources
    IEnumerator SyncAudioSources(AudioSource masterAudioSource, AudioSource slaveAudioSource)
    {
        //Debug.Log("Master AudioSource (from sync): " + masterAudioSource);

        // ensure the slave's clip name matches the master clip
        if (masterAudioSource.clip.name != slaveAudioSource.clip.name)
        {
            Debug.Log("Corrected mismatched clips. Master: " + masterAudioSource.clip.name + " Slave: " + slaveAudioSource.clip.name);
            slaveAudioSource.clip = masterAudioSource.clip;
        }

        // ensure the slave's time is reasonably in-sync with the master's time
        if (Mathf.Abs(slaveAudioSource.time - masterAudioSource.time) > maxMasterSlaveTimeDelta)
        {
            slaveAudioSource.time = masterAudioSource.time;
            slaveAudioSource.Play();

            Debug.Log("Synchronized AudioSource time between master: " + masterAudioSource + " and slave: " + slaveAudioSource + " at " + Time.time);
            //Debug.Log("Master audiosource clip: " + masterAudioSource.clip + " and time: " + masterAudioSource.time + " Slave audiosource clip: " + slaveAudioSource.clip + " and time: " +  slaveAudioSource.time);
        }

        yield return null;
    }

    // determine how many multiples of the current clip length can fit into the time since game started
    float CalculateFastForwardClipTime(AudioSource audioSourceComponent)
    {
        float fastForwardTimeAmount = Time.time % audioSourceComponent.clip.length;

        return fastForwardTimeAmount;
    }

    // determine how much remaining clip time is left in order to set a WaitForSeconds accurately
    float CalculateRemainingClipTime(AudioSource audioSourceComponent)
    {

        float fastForwardTimeAmount = CalculateFastForwardClipTime(audioSourceComponent);

        // the remaining clip time - used to update the WaitForSeconds
        float remainingClipTime = audioSourceComponent.clip.length - fastForwardTimeAmount;

        return remainingClipTime;
    }

    // fast-forward a master AudioSource to keep up with game tmie
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
        // sync only if it's time to sync, and if this is a slave and there's a master to sync to
        if (Time.time > masterSlaveInitialSyncTime && AssociateSpeakerParamsByName(this.name).masterAudioSource != thisAudioSourceComponent && (AssociateSpeakerParamsByName(this.name).masterAudioSource && thisAudioSourceComponent))
        {
            StartCoroutine(SyncAudioSources(AssociateSpeakerParamsByName(this.name).masterAudioSource, thisAudioSourceComponent));

            masterSlaveInitialSyncTime += masterSlaveSyncPeriod;
        }

        // if this is a master, and it has active slaves, it cannot be disabled
        if (AssociateSpeakerParamsByName(this.name).masterAudioSource == thisAudioSourceComponent && AssociateSpeakerParamsByName(this.name).activeSlaveCount > 0)
        {
            thisToggleComponentByProximityScript.isExcepted = true;
        }
        // otherwise, allow it to be disabled
        else
        {
            thisToggleComponentByProximityScript.isExcepted = false;
        }
    }
}