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
// NOTE: this script assumes AudioClips have the proper import settings applied (Load in Background, 
// Compressed in Memory, etc) which should be handled by AssetImportPipeline
// if not, the game may stutter or hang at startup when invoking "Resources.Load()" on an AudioClip

// the object this script is attached to should have these components
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CanDisableComponents))]

public class PlayAudioSequencesByName : MonoBehaviour
{
    // the audio source, scripts, and speaker parameters for this script instance
    AudioSource thisAudioSourceComponent;
    CanDisableComponents thisCanToggleComponentsScript;
    SpeakerParams thisSpeakerParams;
    bool isMaster = false;

    private void Awake()
    {
        // record the audio source, scripts, and speaker parameters for this script instance
        thisAudioSourceComponent = this.GetComponent<AudioSource>();
        thisCanToggleComponentsScript = this.GetComponent<CanDisableComponents>();
        thisSpeakerParams = ManageAudioSources.AssociateSpeakerParamsByName(this.name);

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

        // determine whether the player is inside the mall or outside
        // based on the name of the game object below the player
        // this is only calculable when the player is on a floor surface ("grounded")
        if (ManageFPSControllers.FPSControllerGlobals.activeFPSController?.GetComponent<CharacterController>().isGrounded ?? false && ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform?.position != null)
        {
        AudioSourceGlobals.isPlayerOutside = Utils.StringUtils.TestIfAnyListItemContainedInString(ObjectVisibilityGlobals.exteriorObjectKeywordsList, Utils.GeometryUtils.GetTopLevelSceneContainerChildNameAtNearestNavMeshPoint(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position, 1.0f));
        }

        // update the audio source with new speaker params if necessary
        SpeakerParams newSpeakerParams = ManageAudioSources.AssociateSpeakerParamsByName(this.name);
        bool requireAudioSourceResume = UpdateAudioSourceWithSpeakerParams(thisAudioSourceComponent, newSpeakerParams);

        if (requireAudioSourceResume)
        {
            ResumeAudioSource();
        }

        //if (thisAudioSourceComponent.name.Contains("ambient-chatter"))
        //{
        //    DebugUtils.DebugLog("This speaker: " + thisAudioSourceComponent?.name + " is playing: " + thisAudioSourceComponent.clip.name + " at this index: " + thisSpeakerParams?.lastKnownClipIndex + " and at this time: " + thisSpeakerParams?.lastKnownClipTime);
        //}
    }

    public static SpeakerParams GetAmbientChatterSpeakerParamsByScene(string sceneName)
    {
        SpeakerParams matchingParams = null;

        switch (sceneName)
        {
            case string partialName when partialName.Contains("60s70s") ||
            partialName.Contains(SceneGlobals.experimentalSceneName):

                matchingParams = ManageAudioSources.AssociateSpeakerParamsByName("mall-ambient-chatter-60s70s");
                return matchingParams;

            case string partialName when partialName.Contains("80s90s"):

                matchingParams = ManageAudioSources.AssociateSpeakerParamsByName("mall-ambient-chatter-80s90s");
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

                matchingParams = ManageAudioSources.AssociateSpeakerParamsByName("mall-music-60s70s");
                return matchingParams;

            case string partialName when partialName.Contains("80s90s"):

                matchingParams = ManageAudioSources.AssociateSpeakerParamsByName("mall-music-80s90s");
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
            thisSpeakerParams.lastKnownClipIndex = thisSpeakerParams.lastKnownClipIndex < thisSpeakerParams.clipSequence.Length - 1 && thisSpeakerParams.lastKnownClipIndex != -1 && thisSpeakerParams != null  ? thisSpeakerParams.lastKnownClipIndex : 0;

            // only set clip, time, and play if the index is available in the clipSequence array
            if (thisSpeakerParams.lastKnownClipIndex < thisSpeakerParams.clipSequence.Length && thisSpeakerParams.clipSequence.Length > 0)
            {
                // set and play the clip based on the list of clip names
                masterAudioSource.clip = thisSpeakerParams.clipSequence[thisSpeakerParams.lastKnownClipIndex];
                // if resuming, use the last-known time; otherwise, start at the beginning (new song)
                masterAudioSource.time = thisSpeakerParams.isResuming ? thisSpeakerParams.lastKnownClipTime : 0f;
                masterAudioSource.Play();

                DebugUtils.DebugLog("Playing master music " + masterAudioSource.clip.name + " on " + masterAudioSource);

                // sync all suborinates
                SynchronizeAllSlavesWithMaster(masterAudioSource);

                // if we're at the end of the list, reset the index to return to the beginning
                thisSpeakerParams.lastKnownClipIndex = (thisSpeakerParams.lastKnownClipIndex + 1) % audioClips.Length;
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
        SpeakerParams masterParams = ManageAudioSources.AssociateSpeakerParamsByName(masterAudioSource.name);
        List<AudioSource> slaveAudioSources = masterParams.activeSlaveAudioSources;

        foreach (AudioSource slaveAudiosource in slaveAudioSources)
        {
            SyncAudioSources(masterAudioSource, slaveAudiosource);
        }
    }
}