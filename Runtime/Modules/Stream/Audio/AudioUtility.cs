using System;
using UnityEngine;

public static class AudioUtility
{

	private static AudioConfiguration audioConfiguration;


	public static void SetAudioSampleRate(int sampleRate)
	{
		audioConfiguration = AudioSettings.GetConfiguration();
		audioConfiguration.sampleRate = sampleRate;
		AudioSettings.Reset(audioConfiguration);
	}

	public static void SetDSPBufferSize(int DSPBufferSize)
	{
		audioConfiguration.dspBufferSize = DSPBufferSize;
		audioConfiguration.speakerMode = AudioSpeakerMode.Stereo;
		AudioSettings.Reset(audioConfiguration);
	}

	public static byte[] AudioDataToByte(float[] floatArray)
	{
		byte[] array = new byte[floatArray.Length * 4];
		int num = 0;
		for (int i = 0; i < floatArray.Length; i++)
		{
			Array.Copy(BitConverter.GetBytes(floatArray[i]), 0, array, num, 4);
			num += 4;
		}
		return array;
	}

	public static float[] BytesToAudioData(byte[] byteArray)
	{
		float[] array = new float[byteArray.Length / 4];
		for (int i = 0; i < byteArray.Length; i += 4)
		{
			array[i / 4] = BitConverter.ToSingle(byteArray, i);
		}
		return array;
	}



}
