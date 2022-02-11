using System.Collections;
using System.Collections.Generic;
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
    public string keyName = "";
    public AudioSource masterAudioSource;
    public List<AudioSource> activeSlaveAudioSources = new List<AudioSource>();
    public AudioClip[] clipSequence;
    public int lastKnownClipIndex = 0;
    public AudioClip lastKnownClip;
    public float lastKnownClipTime = 0f;
    public float speakerVolume = 0; // initialize at 0 to prevent a frame of extra-loud music
    public float maxDistance = 0f;
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

    // only one set of params can exist for each type, so keep track of them here
    public static List<SpeakerParams> allKnownSpeakerParams = new List<SpeakerParams>();
}

public class PlayAudioSequencesByName : MonoBehaviour
{
    // define components to be accessed or modified
    AudioSource thisAudioSourceComponent;
    CanDisableComponents thisCanToggleComponentsScript;
    SpeakerParams thisSpeakerParams;

    // return speaker parameters by object name
    // most audio source distances and volumes are assigned to defaults, but can be overridden here
    // primarily, this is used for differentiating audio sequences (playlists) between speakers
    public static SpeakerParams AssociateSpeakerParamsByName(string name)
    {
        string thisKeyName = "";
        SpeakerParams matchingParams = null;

        switch (name)
        {
            // mall - 60s70s

            // ambient chatter
            case string partialName when partialName.Contains("mall-ambient-chatter-60s70s"):

                thisKeyName = "mall-ambient-chatter-60s70s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallChatter,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeChatter,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-ambient-chatter"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }

                return matchingParams;

            // fountain type 1
            case string partialName when partialName.Contains("mall-fountain-60s70s-1"):

                thisKeyName = "mall-fountain-60s70s-1";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerDistanceMallFountain,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallFountain,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-fountain-1"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;
            
            // fountain type 2
            case string partialName when partialName.Contains("mall-fountain-60s70s-2"):

                thisKeyName = "mall-fountain-60s70s-2";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerDistanceMallFountain,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallFountain,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-fountain-2"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;

            // common area music
            case string partialName when partialName.Contains("mall-music-60s70s"):

                thisKeyName = "mall-music-60s70s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallCommon,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallCommon,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-mall-experimental"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;

            // stores - 60s70s

            // store - musicland
            case string partialName when partialName.Contains("store-music-musicland-60s70s"):

                thisKeyName = "store-music-musicland-60s70s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-musicland-60s70s"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;

            // mall - 80s90s

            // ambient chatter
            case string partialName when partialName.Contains("mall-ambient-chatter-80s90s"):

                thisKeyName = "mall-ambient-chatter-80s90s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallChatter,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeChatter,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-ambient-chatter"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;

            // common area music
            case string partialName when partialName.Contains("mall-music-80s90s"):

                thisKeyName = "mall-music-80s90s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallCommon,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallCommon,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-mall-80s90s"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;

            // store - consumer beauty
            case string partialName when partialName.Contains("store-music-consumer-beauty-80s90s"):

                thisKeyName = "store-music-consumer-beauty-80s90s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-consumer-beauty-80s90s"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;

            // store - dolcis
            case string partialName when partialName.Contains("store-music-dolcis-80s90s"):

                thisKeyName = "store-music-dolcis-80s90s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-dolcis-80s90s"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;
            
            // store - generic
            case string partialName when partialName.Contains("store-music-generic-80s90s"):

                thisKeyName = "store-music-generic-80s90s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-generic-80s90s"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;

            // store - musicland
            case string partialName when partialName.Contains("store-music-musicland-80s90s"):

                thisKeyName = "store-music-musicland-80s90s";
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);
                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceStore,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeStore,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-store-musicland-80s90s"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                return matchingParams;

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
            thisCanToggleComponentsScript.canDisableComponents = false;

            StartCoroutine(PlayMasterClipSequence(thisSpeakerParams.clipSequence));
            SynchronizeAllSlavesWithMaster(thisAudioSourceComponent);
        }
        // otherwise, this must be a slave AudioSource
        else
        {
            // keep track of the active slaves
            thisSpeakerParams.activeSlaveAudioSources.Add(thisAudioSourceComponent);

            SyncAudioSources(thisSpeakerParams.masterAudioSource, thisAudioSourceComponent);
            //PlaySlaveClipSequence();
        }
    }

    void OnDisable()
    {
        // execute the AutoResumeCoRoutine OnDisable() first
        //base.OnDisable();

        // check if this is the master AudioSource
        if (thisAudioSourceComponent == thisSpeakerParams.masterAudioSource)
        {
            thisSpeakerParams.lastKnownClip = thisAudioSourceComponent.clip;
            thisSpeakerParams.lastKnownClipIndex = System.Array.IndexOf(thisSpeakerParams.clipSequence, thisAudioSourceComponent.clip);
            thisSpeakerParams.lastKnownClipTime = thisAudioSourceComponent.time;
            // TODO: why is this always zero?!
            Utils.DebugUtils.DebugLog("Last known clip time: " + thisSpeakerParams.masterAudioSource.time + " " + thisSpeakerParams.lastKnownClipTime);

            // set the master audio source for this type as null, so the next instance of this type is considered the new master
            thisSpeakerParams.masterAudioSource = null;
        }
        // otherwise, this must be a slave AudioSource
        else
        {
            // decrease the count so we can keep track of active slaves
            thisSpeakerParams.activeSlaveAudioSources.Remove(thisAudioSourceComponent);
        }
    }


    // Update is called once per frame
    void Update()
    {
        // if this is a master, check if it has active slaves
        // if so, we cannot disable it when it gets out of range, so except it from the proximity script
        if (thisSpeakerParams.masterAudioSource == thisAudioSourceComponent && thisSpeakerParams.activeSlaveAudioSources.Count > 0)
        {
            thisCanToggleComponentsScript.canDisableComponents = false;
        }
        // if no active slaves, allow it to be disabled when it gets out of range
        else if (thisSpeakerParams.masterAudioSource == thisAudioSourceComponent && thisSpeakerParams.activeSlaveAudioSources.Count == 0)
        {
            thisCanToggleComponentsScript.canDisableComponents = true;
        }
    }

    // retrieve the known speaker params if it exists in the list
    public static SpeakerParams GetSpeakerParamsIfKnown(string keyName)
    {
        SpeakerParams matchingParams = null;

        foreach (SpeakerParams knownParams in AudioSourceGlobals.allKnownSpeakerParams)
        {
            if (knownParams.keyName == keyName)
            {
                matchingParams = knownParams;
            }
        }

        return matchingParams;
    }

    // specify and play master speaker clip sequences
    IEnumerator PlayMasterClipSequence(AudioClip[] audioClips)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        while (true)
        {
            // set this object as the master audio source
            AudioSource masterAudioSource = thisAudioSourceComponent;
            // set and play the clip based on the list of clip names
            masterAudioSource.clip = thisSpeakerParams.clipSequence[thisSpeakerParams.lastKnownClipIndex];
            masterAudioSource.Play();
            masterAudioSource.time = thisSpeakerParams.lastKnownClipTime;
            Utils.DebugUtils.DebugLog("Playing master music " + masterAudioSource.clip.name + " on " + masterAudioSource);

            SynchronizeAllSlavesWithMaster(masterAudioSource);

            float remainingClipTime = masterAudioSource.clip.length - masterAudioSource.time;

            // if we're at the end of the list, reset to return to the beginning
            thisSpeakerParams.lastKnownClipIndex = (thisSpeakerParams.lastKnownClipIndex + 1) % audioClips.Length;

            yield return new WaitForSeconds(remainingClipTime);
        }
    }

    // play slave speaker sequences
    void PlaySlaveClipSequence()
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
    }

    // synchronize two AudioSources
    void SyncAudioSources(AudioSource masterAudioSource, AudioSource slaveAudioSource)
    {
        //Utils.DebugUtils.DebugLog"Master AudioSource (from sync): " + masterAudioSource);

        slaveAudioSource.clip = masterAudioSource.clip;
        slaveAudioSource.time = masterAudioSource.time;
        slaveAudioSource.Play();

        Utils.DebugUtils.DebugLog("Synchronized master: " + masterAudioSource.name + " and slave: " + slaveAudioSource.name);
    }

    void SynchronizeAllSlavesWithMaster(AudioSource masterAudioSource)
    {
        SpeakerParams masterParams = AssociateSpeakerParamsByName(masterAudioSource.name);
        List<AudioSource> slaveAudioSources = masterParams.activeSlaveAudioSources;

        foreach (AudioSource slaveAudiosource in slaveAudioSources)
        {
            SyncAudioSources(masterAudioSource, slaveAudiosource);
        }
    }
}