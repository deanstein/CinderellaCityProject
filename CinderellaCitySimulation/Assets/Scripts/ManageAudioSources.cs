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