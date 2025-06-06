using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public bool isResuming = false;
}

public class AudioSourceGlobals
{
    //
    // initialize speaker parameter sets
    //
    public static string mallAmbientChatter60s70sKeyName = "mall-ambient-chatter-60s70s";
    public static string mallMusic60s70sKeyName = "mall-music-60s70s";

    public static string mallAmbientChatter80s90sKeyName = "mall-ambient-chatter-80s90s";
    public static string mallMusic80s90sKeyName = "mall-music-80s90s";

    // default volume levels and max distances
    public static float defaultSpeakerVolumeChatter = 0.3f;
    public static float defaultSpeakerVolumeExteriorAmbient = 0.9f;
    public static float defaultSpeakerMaxDistanceMallChatter = 500f; // also used for exterior ambient

    public static float defaultSpeakerVolumeMallCommon = 0.2f;
    public static float defaultSpeakerMaxDistanceMallCommon = 20f;

    public static float defaultSpeakerVolumeMallFountain = 0.3f;
    public static float defaultSpeakerDistanceMallFountain = 90f;

    public static float defaultSpeakerVolumeStore = 0.3f;
    public static float defaultSpeakerMaxDistanceStore = 15f;

    // only one set of params can exist for each type, so keep track of them here
    public static List<SpeakerParams> allKnownSpeakerParams = new List<SpeakerParams>();
}

public class PlayAudioSequencesByName : MonoBehaviour
{
    // the audio source, scripts, and speaker parameters for this script instance
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
            case string partialName when partialName.Contains(AudioSourceGlobals.mallAmbientChatter60s70sKeyName):

                thisKeyName = AudioSourceGlobals.mallAmbientChatter60s70sKeyName;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.clipSequence = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-exterior-ambient")) : ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-ambient-chatter"));
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? AudioSourceGlobals.defaultSpeakerVolumeExteriorAmbient : AudioSourceGlobals.defaultSpeakerVolumeChatter;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallFountain;
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
                        speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallFountain,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-fountain-2"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallFountain;
                }
                return matchingParams;

            // common area music
            case string partialName when partialName.Contains(AudioSourceGlobals.mallMusic60s70sKeyName):

                thisKeyName = AudioSourceGlobals.mallMusic60s70sKeyName;
                matchingParams = GetSpeakerParamsIfKnown(thisKeyName);

                // if these params do not exist, create them and add them to the list
                if (matchingParams == null)
                {
                    matchingParams = new SpeakerParams
                    {
                        keyName = thisKeyName,
                        maxDistance = AudioSourceGlobals.defaultSpeakerMaxDistanceMallCommon,
                        speakerVolume = AudioSourceGlobals.defaultSpeakerVolumeMallCommon,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-mall-60s70s"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallCommon;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.clipSequence = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-exterior-ambient")) : ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-ambient-chatter"));
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? AudioSourceGlobals.defaultSpeakerVolumeExteriorAmbient : AudioSourceGlobals.defaultSpeakerVolumeChatter;
                }
                return matchingParams;

            // common area music
            case string partialName when partialName.Contains(AudioSourceGlobals.mallMusic80s90sKeyName):

                thisKeyName = AudioSourceGlobals.mallMusic80s90sKeyName;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallCommon;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = ManageFPSControllers.FPSControllerGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
                }
                return matchingParams;

            default:
                DebugUtils.DebugLog("Failed to associate speaker params with this speaker: " + name);
                return null;
        }
    }

    private void Awake()
    {
        // record the audio source, scripts, and speaker parameters for this script instance
        thisAudioSourceComponent = this.GetComponent<AudioSource>();
        thisCanToggleComponentsScript = this.GetComponent<CanDisableComponents>();
        thisSpeakerParams = AssociateSpeakerParamsByName(this.name);

        InitializeAudioSourceWithSpeakerParams(thisAudioSourceComponent, thisSpeakerParams);
    }

    void OnEnable()
    {
        if (!ManageFPSControllers.FPSControllerGlobals.activeFPSController)
        {
            return;
        }

        StartAudioSource();
    }

    void OnDisable()
    {
        // if this is a master audiosource, record the last-known clip and time for the next master to resume
        if (thisAudioSourceComponent == thisSpeakerParams.masterAudioSource)
        {
            thisSpeakerParams.lastKnownClip = thisAudioSourceComponent.clip;
            //thisSpeakerParams.lastKnownClipTime = thisAudioSourceComponent.time;
            thisSpeakerParams.lastKnownClipIndex = System.Array.IndexOf(thisSpeakerParams.clipSequence, thisAudioSourceComponent.clip);
            // the audiosource time should be set here too, but that must happen in Update() for some reason

            // set the master audio source for this type as null
            // so the next instance of this type is considered the new master
            thisSpeakerParams.masterAudioSource = null;
            // set isResuming to true, so the track can be resumed at its last-known seek position
            thisSpeakerParams.isResuming = true;
        }
        // otherwise, this must be a subordinate audiosource
        else
        {
            // keep track of active subordinates
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

        // need to record the last-known clip time in case this speaker is disabled
        // and the next master needs to resume
        // for some reason, this cannot be done in OnDisable() (always results in .time of 0)
        thisSpeakerParams.lastKnownClipTime = thisAudioSourceComponent.time;

        // update the audio source with new speaker params if necessary
        SpeakerParams newSpeakerParams = AssociateSpeakerParamsByName(this.name);
        bool requireAudioSourceResume = UpdateAudioSourceWithSpeakerParams(thisAudioSourceComponent, newSpeakerParams);

        if (requireAudioSourceResume)
        {
            ResumeAudioSource();
        }

        //if (thisAudioSourceComponent.name.Contains("ambient-chatter") || thisAudioSourceComponent.name.Contains("mall-fountain"))
        //{
        //    DebugUtils.DebugLog("This speaker: " + thisAudioSourceComponent?.name + " is playing: " + thisAudioSourceComponent.clip.name + " at this index: " + thisSpeakerParams?.lastKnownClipIndex + " and at this time: " + thisSpeakerParams?.lastKnownClipTime);
        //}
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

    public static SpeakerParams GetAmbientChatterSpeakerParamsByScene(string sceneName)
    {
        SpeakerParams matchingParams = null;

        switch (sceneName)
        {
            case string partialName when partialName.Contains("60s70s") ||
            partialName.Contains(SceneGlobals.experimentalSceneName):

                matchingParams = AssociateSpeakerParamsByName(AudioSourceGlobals.mallAmbientChatter60s70sKeyName);
                return matchingParams;

            case string partialName when partialName.Contains("80s90s"):

                matchingParams = AssociateSpeakerParamsByName(AudioSourceGlobals.mallAmbientChatter80s90sKeyName);
                return matchingParams;

            default:
                return null;
        }
    }

    public static SpeakerParams GetMallMusicSpeakerParamsByScene(string sceneName)
    {
        SpeakerParams matchingParams = null;

        switch (sceneName)
        {
            case string partialName when partialName.Contains("60s70s") ||
            partialName.Contains(SceneGlobals.experimentalSceneName):

                matchingParams = AssociateSpeakerParamsByName(AudioSourceGlobals.mallMusic60s70sKeyName);
                return matchingParams;

            case string partialName when partialName.Contains("80s90s"):

                matchingParams = AssociateSpeakerParamsByName(AudioSourceGlobals.mallMusic80s90sKeyName);
                return matchingParams;

            default:
                return null;
        }
    }

    public static void InitializeAudioSourceWithSpeakerParams
    (AudioSource audioSourceComponent, SpeakerParams speakerParams)
    {
        // don't proceed if either no audio source or speaker params are available
        if (!audioSourceComponent || (speakerParams == null))
        {
            return;
        }

        // set audiosource settings
        audioSourceComponent.volume = speakerParams.speakerVolume;
        audioSourceComponent.maxDistance = speakerParams.maxDistance;
        audioSourceComponent.spatialBlend = 1.0F;
        audioSourceComponent.dopplerLevel = 0F;
        audioSourceComponent.rolloffMode = AudioRolloffMode.Custom;
        audioSourceComponent.loop = false;
        audioSourceComponent.bypassEffects = true;
        audioSourceComponent.bypassListenerEffects = true;
        audioSourceComponent.bypassReverbZones = true;
    }

    public static bool UpdateAudioSourceWithSpeakerParams(AudioSource audioSourceComponent, SpeakerParams newSpeakerParams)
    {
        bool requireRestart = false;

        if (audioSourceComponent != null)
        {
            // only adjust the volume if necessary
            if (audioSourceComponent.volume != newSpeakerParams.speakerVolume)
            {
                audioSourceComponent.volume = newSpeakerParams.speakerVolume;
            };

            // only adjust the clip if necessary
            List<string> audioClipNames = newSpeakerParams.clipSequence.Select(clip => clip.name).ToList<string>();
            if (!Utils.StringUtils.TestIfAnyListItemContainedInString(audioClipNames, audioSourceComponent?.clip?.name))
            {
                requireRestart = true;
            }
        }

        return requireRestart;
    }

    // specify and play master speaker clip sequences
    IEnumerator PlayMasterClipSequence(AudioClip[] audioClips)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        while (true)
        {
            // set this object as the master audio source
            AudioSource masterAudioSource = thisAudioSourceComponent;

            // make sure the last known clip index is within the array bounds
            thisSpeakerParams.lastKnownClipIndex = thisSpeakerParams.lastKnownClipIndex < thisSpeakerParams.clipSequence.Length - 1 && thisSpeakerParams.lastKnownClipIndex != -1 && thisSpeakerParams != null ? thisSpeakerParams.lastKnownClipIndex : 0;

            // only set clip, time, and play if the index is available in the clipSequence array
            if (thisSpeakerParams.lastKnownClipIndex < thisSpeakerParams.clipSequence.Length && thisSpeakerParams.clipSequence.Length > 0)
            {
                // set and play the clip based on the list of clip names
                masterAudioSource.clip = thisSpeakerParams.clipSequence[thisSpeakerParams.lastKnownClipIndex];

                // if resuming, use the last-known time; otherwise, start at the beginning (new song)
                masterAudioSource.time = thisSpeakerParams.isResuming ? thisSpeakerParams.lastKnownClipTime : 0f;
                masterAudioSource.Play();

                DebugUtils.DebugLog("Playing master music " + masterAudioSource.clip.name + " on " + masterAudioSource);

                // sync all subordinates
                SynchronizeAllSlavesWithMaster(masterAudioSource);

                // if only one clip exists, enable looping to prevent stopping
                if (audioClips.Length == 1)
                {
                    masterAudioSource.loop = true;
                    yield break; // exit the coroutine, as looping is handled by the AudioSource itself
                }
                else
                {
                    // if we're at the end of the list, reset the index to return to the beginning
                    thisSpeakerParams.lastKnownClipIndex = (thisSpeakerParams.lastKnownClipIndex + 1) % audioClips.Length;
                }
            }

            // wait until the clip is over to proceed to the next
            float remainingClipTime = masterAudioSource?.clip != null ? masterAudioSource.clip.length - masterAudioSource.time : 0;
            yield return new WaitForSeconds(remainingClipTime);
        }
    }

    public void StartAudioSource()
    {
        // if this is a master audiosource, resume at the clip and time the last-known master was playing
        if (!thisSpeakerParams.masterAudioSource)
        {
            // set this speaker AudioSource as the Master AudioSource for this type
            thisSpeakerParams.masterAudioSource = thisAudioSourceComponent;

            // ensure this master cannot be disabled until checks are made in update()
            thisCanToggleComponentsScript.canDisableComponents = false;

            // play and sync all slaves
            StartCoroutine(PlayMasterClipSequence(thisSpeakerParams.clipSequence));
            SynchronizeAllSlavesWithMaster(thisAudioSourceComponent);

            // set isResuming as false so when the next song plays, it starts at time 0 without errors
            thisSpeakerParams.isResuming = false;
        }
        // otherwise, this must be a subordinate audiosource
        else
        {
            // keep track of the active slaves
            thisSpeakerParams.activeSlaveAudioSources.Add(thisAudioSourceComponent);

            // sync with master
            SyncAudioSources(thisSpeakerParams.masterAudioSource, thisAudioSourceComponent);
        }
    }

    public void ResumeAudioSource()
    {
        thisSpeakerParams.masterAudioSource = null;
        thisSpeakerParams.isResuming = true;

        StartAudioSource();
    }

    // go to the previous track given a speaker type
    public static void PlayPreviousTrack(SpeakerParams speakerParamsToChange)
    {
        GameObject speakerObject = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.speakerObjectKeywords)[0];
        speakerObject.SetActive(false);
        speakerParamsToChange.lastKnownClipIndex--;
        // if already at the beginning, previous goes to end of list
        if (speakerParamsToChange.lastKnownClipIndex < 0)
        {
            speakerParamsToChange.lastKnownClipIndex = speakerParamsToChange.clipSequence.Length - 1;
        }
        speakerParamsToChange.lastKnownClip = speakerParamsToChange.clipSequence[speakerParamsToChange.lastKnownClipIndex];
        speakerParamsToChange.lastKnownClipTime = 0f;
        speakerObject.SetActive(true);
    }

    // go to the next track given a speaker type
    public static void PlayNextTrack(SpeakerParams speakerParamsToChange)
    {
        GameObject speakerObject = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.speakerObjectKeywords)[0];
        speakerObject.SetActive(false);
        speakerParamsToChange.lastKnownClipIndex++;
        // if already at the end, next goes to beginning of list
        if (speakerParamsToChange.lastKnownClipIndex > speakerParamsToChange.clipSequence.Length - 1)
        {
            speakerParamsToChange.lastKnownClipIndex = 0;
        }
        speakerParamsToChange.lastKnownClip = speakerParamsToChange.clipSequence[speakerParamsToChange.lastKnownClipIndex];
        speakerParamsToChange.lastKnownClipTime = 0f;
        speakerObject.SetActive(true);
    }

    public static void MuteSpeakers(SpeakerParams speakerParamsToMute)
    {

        GameObject speakerObject = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.speakerObjectKeywords)[0];

        // if the master is already muted, un-mute it
        if (speakerParamsToMute.masterAudioSource.volume == 0)
        {
            speakerParamsToMute.masterAudioSource.volume = speakerParamsToMute.speakerVolume;
        }
        // otherwise, mute it
        else
        {
            speakerParamsToMute.masterAudioSource.volume = 0;
        }

        speakerObject.SetActive(false);
        speakerObject.SetActive(true);
    }

    // synchronize two AudioSources
    void SyncAudioSources(AudioSource masterAudioSource, AudioSource slaveAudioSource)
    {
        slaveAudioSource.clip = masterAudioSource.clip;
        slaveAudioSource.time = masterAudioSource.time;
        slaveAudioSource.volume = masterAudioSource.volume;
        slaveAudioSource.Play();
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