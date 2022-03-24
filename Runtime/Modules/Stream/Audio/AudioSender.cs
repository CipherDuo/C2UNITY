using System.Collections;
using System.Collections.Generic;
using CipherDuo.IPFS.Serialization;
using UnityEngine;

public class AudioSender : MonoBehaviour
{
    public static AudioSource localMic;
    public static AudioPacket audioPacket;
    public static float micThreshold;
    public static float micVolume;
    public static int micFrequency;
    public static float micFixedTimestep;

    public static void ClearMicLocal()
    {
        if (localMic != null)
        {
            float[] array = new float[localMic.clip.samples];

            localMic.clip.SetData(array, 0);
        }
    }

    // public static void PlayMicLocal()
    // {
    //     if (localMic && Microphone.devices.Length >= 1)
    //     {
    //         localMic.clip = Microphone.Start("", true, 1, micFrequency);
    //         while ((float)Microphone.GetPosition("") <= (float)micFrequency * micFixedTimestep)
    //         {
    //         }
    //     }
    // }

    public static byte[] GetAudio()
    {
        if (audioPacket != null)
        {
            return Serializator.SerializeXML(audioPacket);
        }

        return null;
    }


    
}
