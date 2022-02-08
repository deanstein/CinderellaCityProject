using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a specific sequence of audio tracks, 
/// depending on the name of the host object 
/// (typically a speaker, mechanical device, or NPC)
/// </summary>

// this script needs to be attached to an object that plays a series of AudioClips
// NOTE: this script assumes AudioClips have the proper import settings applied (Load in Background, Compressed in Memory, etc) which should be handled by AssetImportPipeline
// if not, the game may stutter or hang at startup when invoking "Resources.Load()" on an AudioClip

// the object this script is attached to should have these components
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CanDisableComponents))]

// define the type of data/parameters that a set of speakers can get and set
[System.Serializable]
public class SpeakerParams
{
    public AudioSource masterAudioSource;
    public AudioClip[] clipSequence;
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

    // default volume levels and max distances
    public static float defaultSpeakerVolumeChatter = 0.03f;
    public static float defaultSpeakerMaxDistanceMallChatter = 500f;

    public static float defaultSpeakerVolumeMallCommon = 0.02f;
    public static float defaultSpeakerMaxDistanceMallCommon = 20f;

    public static float defaultSpeakerVolumeMallFountain = 0.03f;
    public static float defaultSpeakerDistanceMallFountain = 90f;

    public static float defaultSpeakerVolumeStore = 0.03f;
    public static float defaultSpeakerMaxDistanceStore = 15f;

    // define the available types of mall audio sources
    // only one can exist per type, so all speakers of one type are following the same params

    // mall - 60s70s
    public static SpeakerParams MallChatter60s70sParams = null;
    public static SpeakerParams MallFountain60s70sParams1 = null;
    public static SpeakerParams MallFountain60s70sParams2 = null;
    public static SpeakerParams MallMusic60s70sParams = null;

    // stores - 60s70s
    public static SpeakerParams StoreMusicMusicland60s70sParams = null;

    // mall - 80s90s
    public static SpeakerParams MallChatter80s90sParams = null;
    public static SpeakerParams MallMusic80s90sParams = null;

    // stores - 80s90s
    public static SpeakerParams StoreMusicConsumerBeauty80s90sParams = null;
    public static SpeakerParams StoreMusicDolcis80s90sParams = null;
    public static SpeakerParams StoreMusicGeneric80s90sParams = null;
    public static SpeakerParams StoreMusicMusicland80s90sParams = null;

}

public class PlayAudioSequencesByName : MonoBehaviour
{
    // define components to be accessed or modified
    AudioSource thisAudioSourceComponent;
    CanDisableComponents thisCanToggleComponentsScript;
    SpeakerParams thisSpeakerParams;

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
    // most audio source distances and volumes are assigned to defaults, but can be overridden here
    // primarily, this is used for differentiating audio sequences (playlists) between speakers
    public static SpeakerParams AssociateSpeakerParamsByName(string name)
    {
        switch (name)
        {
            // mall - 60s70s

            // ambient chatter
            case string partialName when partialName.Contains("mall-ambient-chatter-60s70s"):

                AudioSourceGlobals.MallChatter60s70sParams = AudioSourceGlobals.MallChatter60s70sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallChatter,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeChatter,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-ambient-chatter"))
                };
                return AudioSourceGlobals.MallChatter60s70sParams;

            // fountain type 1
            case string partialName when partialName.Contains("mall-fountain-60s70s-1"):

                AudioSourceGlobals.MallFountain60s70sParams1 = AudioSourceGlobals.MallFountain60s70sParams1 ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerDistanceMallFountain,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallFountain,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-fountain-1"))
                };
                return AudioSourceGlobals.MallFountain60s70sParams1;
            
            // fountain type 2
            case string partialName when partialName.Contains("mall-fountain-60s70s-2"):

                AudioSourceGlobals.MallFountain60s70sParams2 = AudioSourceGlobals.MallFountain60s70sParams2 ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerDistanceMallFountain,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallFountain,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-fountain-2"))
                };
                return AudioSourceGlobals.MallFountain60s70sParams2;

            // common area music
            case string partialName when partialName.Contains("mall-music-60s70s"):

                AudioSourceGlobals.MallMusic60s70sParams = AudioSourceGlobals.MallMusic60s70sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallCommon,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallCommon,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-mall-60s70s"))
                };
                return AudioSourceGlobals.MallMusic60s70sParams;

            // stores - 60s70s

            // store - musicland
            case string partialName when partialName.Contains("store-music-musicland-60s70s"):

                AudioSourceGlobals.StoreMusicMusicland60s70sParams = AudioSourceGlobals.StoreMusicMusicland60s70sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-musicland-60s70s"))
                };
                return AudioSourceGlobals.StoreMusicMusicland60s70sParams;

            // mall - 80s90s

            // ambient chatter
            case string partialName when partialName.Contains("mall-ambient-chatter-80s90s"):

                AudioSourceGlobals.MallChatter80s90sParams = AudioSourceGlobals.MallChatter80s90sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallChatter,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeChatter,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-ambient-chatter"))
                };
                return AudioSourceGlobals.MallChatter80s90sParams;

            // common area music
            case string partialName when partialName.Contains("mall-music-80s90s"):

                AudioSourceGlobals.MallMusic80s90sParams = AudioSourceGlobals.MallMusic80s90sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallCommon,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallCommon,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-mall-80s90s"))
                };
                return AudioSourceGlobals.MallMusic80s90sParams;

            // store - consumer beauty
            case string partialName when partialName.Contains("store-music-consumer-beauty-80s90s"):

                AudioSourceGlobals.StoreMusicConsumerBeauty80s90sParams = AudioSourceGlobals.StoreMusicConsumerBeauty80s90sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-consumer-beauty-80s90s"))
                };
                return AudioSourceGlobals.StoreMusicConsumerBeauty80s90sParams;

            // store - dolcis
            case string partialName when partialName.Contains("store-music-dolcis-80s90s"):

                AudioSourceGlobals.StoreMusicDolcis80s90sParams = AudioSourceGlobals.StoreMusicDolcis80s90sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-dolcis-80s90s"))
                };
                return AudioSourceGlobals.StoreMusicDolcis80s90sParams;
            
            // store - generic
            case string partialName when partialName.Contains("store-music-generic-80s90s"):

                AudioSourceGlobals.StoreMusicGeneric80s90sParams = AudioSourceGlobals.StoreMusicGeneric80s90sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-generic-80s90s"))
                };
                return AudioSourceGlobals.StoreMusicGeneric80s90sParams;

            // store - musicland
            case string partialName when partialName.Contains("store-music-musicland-80s90s"):

                AudioSourceGlobals.StoreMusicMusicland80s90sParams = AudioSourceGlobals.StoreMusicMusicland80s90sParams ?? new SpeakerParams
                {
                    maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                    speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                    clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-musicland-80s90s"))
                };
                return AudioSourceGlobals.StoreMusicMusicland80s90sParams;

            default:
                Utils.DebugUtils.DebugLog("Failed to associate speaker params with this speaker: " + name);
                return null;
        }
    }

    private void Awake()
    {
        // scripts and params for this speaker
        thisAudioSourceComponent = this.GetComponent<AudioSource>();
        thisCanToggleComponentsScript = this.GetComponent<CanDisableComponents>();
        thisSpeakerParams = AssociateSpeakerParamsByName(this.name);

        if (!thisAudioSourceComponent || (thisSpeakerParams == null))
        {
            return;
        }

        thisAudioSourceComponent.volume = thisSpeakerParams.speakerVolume;
        thisAudioSourceComponent.maxDistance = thisSpeakerParams.maxDistance;
        thisAudioSourceComponent.spatialBlend = 1.0F;
        thisAudioSourceComponent.dopplerLevel = 0F;
        thisAudioSourceComponent.rolloffMode = AudioRolloffMode.Custom;
        thisAudioSourceComponent.loop = false;
        thisAudioSourceComponent.bypassEffects = true;
        thisAudioSourceComponent.bypassListenerEffects = true;
        thisAudioSourceComponent.bypassReverbZones = true;
    }

    void OnEnable()
    {
        // execute the AutoResumeCoRoutine OnEnable() first
        //base.OnEnable();

        if (!ManageFPSControllers.FPSControllerGlobals.activeFPSController)
        {
            return;
        }

        // if no master audio source for this type, this AudioSource should be considered the master
        if (!thisSpeakerParams.masterAudioSource)
        {
            // set this speaker AudioSource as the Master AudioSource for this type
            thisSpeakerParams.masterAudioSource = thisAudioSourceComponent;

            // ensure this master cannot be disabled until checks are made in update()
            //thisToggleComponentByProximityScript.isExcepted = true;
            thisCanToggleComponentsScript.canDisableComponents = false;

           StartCoroutine(PlayMasterClipSequenceInOrder(thisSpeakerParams.clipSequence));

           needsFastForwarding = false;
        }
        // otherwise, this must be a slave AudioSource
        else
        {
            // increase the count so we know when there are slaves relying on a master
            thisSpeakerParams.activeSlaveCount++;

            StartCoroutine(PlaySlaveClipSequence(thisSpeakerParams.clipSequence));
        }
    }

    void OnDisable()
    {
        // execute the AutoResumeCoRoutine OnDisable() first
        //base.OnDisable();

        // check if this is the master AudioSource
        if (thisAudioSourceComponent == thisSpeakerParams.masterAudioSource)
        {
            //look up the current clip index, and subtract it by one to avoid skipping tracks when re-enabled
            if (thisSpeakerParams.currentClipIndex > 0)
            {
                thisSpeakerParams.currentClipIndex--;
            }

            // set the flag that fast-forwarding should happen on next enable
            needsFastForwarding = true;

            // set the master audio source for this type as null, so the next instance of this type is considered the new master
            thisSpeakerParams.masterAudioSource = null;
        }
        // otherwise, this must be a slave AudioSource
        else
        {
            // decrease the count so we can keep track of active slaves
            thisSpeakerParams.activeSlaveCount--;
        }
    }

    // specify and play master speaker clip sequences
    IEnumerator PlayMasterClipSequenceInOrder(AudioClip[] audioClips)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        int counter = thisSpeakerParams.currentClipIndex;
        while (true)
        {
            // set this object as the master audio source
            AudioSource masterAudioSource = thisAudioSourceComponent;
            // set and play the clip based on the list of clip names
            masterAudioSource.clip = audioClips[counter];
            masterAudioSource.Play();
            Utils.DebugUtils.DebugLog("Playing master music " + masterAudioSource.clip.name + " on " + masterAudioSource);

            float remainingClipTime;

            // fast forward time if the flag is set
            if (needsFastForwarding)
            {
                // calculate how much time is left in the clip to accurately set the WaitForSeconds
                remainingClipTime = FastForwardMasterAudioSourceToMatchGameTime(masterAudioSource);
                needsFastForwarding = false;
                //Utils.DebugUtils.DebugLog("Clip was set to fast forward.");
            }
            // otherwise the remaining clip time is just the clip's length
            else
            {
                remainingClipTime = masterAudioSource.clip.length;
                needsFastForwarding = false;
                //Utils.DebugUtils.DebugLog("Clip was NOT set to fast forward.");
            }

            // if we're at the end of the list, reset to return to the beginning
            counter = (counter + 1) % audioClips.Length;
            thisSpeakerParams.currentClipIndex = counter;

            yield return new WaitForSeconds(remainingClipTime);
        }
    }

    // play slave speaker sequences
    IEnumerator PlaySlaveClipSequence(AudioClip[] audioclips)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        int counter = thisSpeakerParams.currentClipIndex;
        while (true)
        {
            // get the corresponding master AudioSource based on this object's name
            AudioSource masterAudioSource = thisSpeakerParams.masterAudioSource;
            // this object is the slave
            AudioSource slaveAudioSource = thisAudioSourceComponent;
            // set and sync the master and slave audio source clips
            slaveAudioSource.clip = masterAudioSource.clip;
            slaveAudioSource.time = masterAudioSource.time;
            slaveAudioSource.Play();
            Utils.DebugUtils.DebugLog("Playing slave music " + masterAudioSource.clip.name + " on " + slaveAudioSource);

            // calculate how much time is left in the clip to accurately set the WaitForSeconds
            float remainingClipTime = CalculateRemainingClipTime(slaveAudioSource);

            // if we're at the end of the list, reset to return to the beginning
            counter = (counter + 1) % audioclips.Length;

            yield return new WaitForSeconds(remainingClipTime);
        }
    }

    // synchronize two AudioSources
    IEnumerator SyncAudioSources(AudioSource masterAudioSource, AudioSource slaveAudioSource)
    {
        //Utils.DebugUtils.DebugLog"Master AudioSource (from sync): " + masterAudioSource);

        // ensure the slave's clip name matches the master clip
        if (masterAudioSource.clip.name != slaveAudioSource.clip.name)
        {
            Utils.DebugUtils.DebugLog("Corrected mismatched clips. Master: " + masterAudioSource.clip.name + " Slave: " + slaveAudioSource.clip.name);
            slaveAudioSource.clip = masterAudioSource.clip;
        }

        // ensure the slave's time is reasonably in-sync with the master's time
        if (Mathf.Abs(slaveAudioSource.time - masterAudioSource.time) > maxMasterSlaveTimeDelta)
        {
            slaveAudioSource.timeSamples = masterAudioSource.timeSamples;
            slaveAudioSource.Play();

            Utils.DebugUtils.DebugLog("Synchronized AudioSource time between master: " + masterAudioSource + " and slave: " + slaveAudioSource + " at " + Time.time);
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

    // fast-forward a master AudioSource to keep up with game time
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
        // sync only if it's time to sync, and if there is a valid master and slave to sync between
        if (Time.time > masterSlaveInitialSyncTime && thisSpeakerParams.masterAudioSource != null)
        {
            StartCoroutine(SyncAudioSources(thisSpeakerParams.masterAudioSource, thisAudioSourceComponent));

            masterSlaveInitialSyncTime += masterSlaveSyncPeriod;
        }

        // if this is a master, check if it has active slaves
        // if so, we cannot disable it when it gets out of range, so except it from the proximity script
        if (thisSpeakerParams.masterAudioSource == thisAudioSourceComponent && thisSpeakerParams.activeSlaveCount > 0)
        {
            thisCanToggleComponentsScript.canDisableComponents = false;
        }
        // if no active slaves, allow it to be disabled when it gets out of range
        else if (thisSpeakerParams.masterAudioSource == thisAudioSourceComponent && thisSpeakerParams.activeSlaveCount == 0)
        {
            thisCanToggleComponentsScript.canDisableComponents = true;
        }
    }
}