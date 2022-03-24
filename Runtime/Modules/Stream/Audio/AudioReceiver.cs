using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioReceiver : MonoBehaviour
{
    [Serializable]
    public struct AudioObject
    {
        public GameObject gameObject;
        public AudioSource source;
        public bool isPlaying;
        public int samplePosition;
    }

    public static Dictionary<int, AudioObject> remoteAudioSources = new Dictionary<int, AudioObject>();

    public static void SetAudioRemote(int id, bool play)
    {
        if (remoteAudioSources[id].source != null)
        {
            var source = remoteAudioSources[id].source;
            source.enabled = true;

            if(!source.isPlaying && source.clip.samples > 0)
            {
                if (play)
                {
                    source.Play();
                }
                else
                {
                    source.Stop();
                    ClearAudio(source);
                }
            }
        }
    }

    public static void InitAudio(int id, int frequency, int length, bool microphone)
    {
        if (remoteAudioSources[id].gameObject == null)
        {
            var GO = new GameObject($"AudioSource: {id}");
            var component = GO.AddComponent<AudioSource>();

            var audioObject = new AudioObject();
            audioObject.gameObject = GO;
            audioObject.source = component;

            remoteAudioSources[id] = audioObject;
        }

        if (microphone)
        {
            remoteAudioSources[id].source.clip = AudioClip.Create($"RemoteMic: {id}", frequency * length, 1, frequency, false);
        }
        else
        {
            remoteAudioSources[id].source.clip = AudioClip.Create($"RemoteAudio: {id}", frequency * length, 2, frequency, false);
        }
    }

    public static void InitAudioMetadata()
    {
        for (int i = 0; i < remoteAudioSources.Count; i++)
        {
            var audio = remoteAudioSources[i];
            audio.isPlaying = audio.source.isPlaying;
            audio.samplePosition = audio.source.timeSamples;
        }
    }

    public static void ClearAllRemoteAudio()
    {
        for (int i = 0; i < remoteAudioSources.Count; i++)
        {
            ClearAudio(remoteAudioSources[i].source);
        }
    }

    public static void ClearAudio(AudioSource source)
    {
        if (source != null)
        {
            var clip = source.clip;
            float[] array = new float[clip.samples];

            clip.SetData(array, 0);

        }
    }
}
