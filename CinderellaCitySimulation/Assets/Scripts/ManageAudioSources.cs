using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the master and slave audio sources 
/// </summary>

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
    // keep track of the master audio source in each scene
    public static AudioSource masterAudioSource60s70s;
    public static AudioSource masterAudioSource80s90s;

    // some audio clip sequences will change depending on 
    // whether the player is considered outside the mall or inside
    public static bool isPlayerOutside;

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

public class ManageAudioSources
{
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.clipSequence = AudioSourceGlobals.isPlayerOutside ? ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-exterior-ambient")) : ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-ambient-chatter"));
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? AudioSourceGlobals.defaultSpeakerVolumeExteriorAmbient : AudioSourceGlobals.defaultSpeakerVolumeChatter;
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
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallFountain;
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
                        speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallFountain,
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-fountain-2"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallFountain;
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
                        clipSequence = ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/music-mall-60s70s"))
                    };
                    AudioSourceGlobals.allKnownSpeakerParams.Add(matchingParams);
                }
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallCommon;
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
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
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
                    matchingParams.clipSequence = AudioSourceGlobals.isPlayerOutside ? ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-exterior-ambient")) : ArrayUtils.ShuffleArray(Resources.LoadAll<AudioClip>("Audio/sfx-mall-ambient-chatter"));
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? AudioSourceGlobals.defaultSpeakerVolumeExteriorAmbient : AudioSourceGlobals.defaultSpeakerVolumeChatter;
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
                else // but if the params exist, make sure they're updated
                {
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeMallCommon;
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
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
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
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
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
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
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
                    matchingParams.speakerVolume = AudioSourceGlobals.isPlayerOutside ? 0 : AudioSourceGlobals.defaultSpeakerVolumeStore;
                }
                return matchingParams;

            default:
                DebugUtils.DebugLog("Failed to associate speaker params with this speaker: " + name);
                return null;
        }
    }
}