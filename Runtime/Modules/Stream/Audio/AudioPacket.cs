using System;

[Serializable]
public class AudioPacket
{
	public int channels;

	public int compressions;

	public byte[] data;

	public int frequency;

	public int id;

	public string metadata;

	public bool microphone;

	public int position;

	public int realFrequency;

	public bool resampled;

	public int samples;

	public int sourceLength;

	public double timestamp;

	public float timestep;
}

