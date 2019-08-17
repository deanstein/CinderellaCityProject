using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class PlayAudioSequencesByName : MonoBehaviour {

    // Use this for initialization
    void Start ()
    {
        // get the audio source for this GameObject
        AudioSource audioSourceComponent = this.GetComponent<AudioSource>();
        //Debug.Log("This audio source: " + audioSourceComponent);

        // set default audio source options
        audioSourceComponent.spatialBlend = 1.0F;
        audioSourceComponent.dopplerLevel = 0F;
        audioSourceComponent.rolloffMode = AudioRolloffMode.Custom;
        audioSourceComponent.loop = true;

        // set volumes based on type
        float mallAmbientChatterSpeakerVolume = 0.15F;
        float mallMusicSpeakerVolume = 0.25F;
        float storeMusicSpeakerVolume = 0.25F;

        // set max distances based on type
        float mallAmbientChatterSpeakerMaxDistance = 500F;
        float mallMusicSpeakerMaxDistance = 20F;
        float storeMusicSpeakerMaxDistance = 15F;


        //
        // define the clips that should be played per area
        //

        // mall
        string[] mallAmbientChatter80s90sAudioClips = { "80s-90s/Ambient/BDalton/CinCityAmbience" };
        string[] mallMusic80s90sAudioClips = { "80s-90s/Music/Betamaxx/6. woolworth", "80s-90s/Music/Betamaxx/1. grand opening", "80s-90s/Music/Betamaxx/8. mall walking", "80s-90s/Music/BDalton/nick", "80s-90s/Music/Betamaxx/7. crystal fountain", "80s-90s/Music/BDalton/uhh" };

        // stores
        string[] storeMusicGeneric80s90sAudioClips = { "80s-90s/Music/Betamaxx/9. a'gaci", "80s-90s/Music/Betamaxx/10. lazarus", "80s-90s/Music/Betamaxx/5. kauffmans" };
        string[] storeMusicConsumerBeauty80s90sAudioClips = { "80s-90s/Music/DeadMall/05 Thunderhead" };
        string[] storeMusicDolcis80s90sAudioClips = { "80s-90s/Music/Betamaxx/5. kauffmans", "80s-90s/Music/Betamaxx/10. lazarus" };
        string[] storeMusicMusicland80s90sAudioClips = { "80s-90s/Music/DeadMall/01 Wheels", "80s-90s/Music/DeadMall/09 Pressure" };


        //
        // play the clip sequences based on the object name
        //

        if (this.name.Contains("mall-ambient-chatter-80s90s"))
        {
            audioSourceComponent.volume = mallAmbientChatterSpeakerVolume;
            audioSourceComponent.maxDistance = mallAmbientChatterSpeakerMaxDistance;
            StartCoroutine(playClipSequenceInOrder(audioSourceComponent, mallAmbientChatter80s90sAudioClips));
        }

        if (this.name.Contains("mall-music-80s90s"))
        {
            audioSourceComponent.volume = mallMusicSpeakerVolume;
            audioSourceComponent.maxDistance = mallMusicSpeakerMaxDistance;
            StartCoroutine(playClipSequenceInOrder(audioSourceComponent, mallMusic80s90sAudioClips));
        }

        if (this.name.Contains("store-music-generic-80s90s"))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;
            StartCoroutine(playClipSequenceInOrder(audioSourceComponent, storeMusicGeneric80s90sAudioClips));
        }

        if (this.name.Contains("store-music-dolcis-80s90s"))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;
            StartCoroutine(playClipSequenceInOrder(audioSourceComponent, storeMusicDolcis80s90sAudioClips));
        }

        if (this.name.Contains("store-music-musicland-80s90s"))
        {
            audioSourceComponent.volume = storeMusicSpeakerVolume;
            audioSourceComponent.maxDistance = storeMusicSpeakerMaxDistance;
            StartCoroutine(playClipSequenceInOrder(audioSourceComponent, storeMusicMusicland80s90sAudioClips));
        }

    }

    IEnumerator playClipSequenceInOrder(AudioSource audioSourceComponent, string[] clipNames)
    {
        // for each clip in the array, play it, and wait until the end to play the next
        for (var i = 0; i < clipNames.Length; i++)
        {
            AudioClip clip = (AudioClip)Resources.Load(clipNames[i]);
            audioSourceComponent.clip = clip;
            audioSourceComponent.Play();
            Debug.Log("Playing music: " + clip.name);

            yield return new WaitForSeconds(clip.length);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

