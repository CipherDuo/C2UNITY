using System;

//TODO reformat as modelViewPresenter

[Serializable]
public struct VideoPacket
{
	public int m_ID;

	public int m_Width;

	public int m_Height;

	public int m_Compressions;

	public byte[] m_Data;
}