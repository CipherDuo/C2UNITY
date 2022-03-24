using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CipherDuo.IPFS.Modules;
using CipherDuo.IPFS.Serialization;
using Ipfs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

//TODO reformat as modelViewPresenter

[RequireComponent(typeof(RawImage))]
public class VideoReceiver : MonoBehaviour
{
    RawImage localDisplay;
    Texture2D localFrame;

    public int id;

    private void Start()
    {
        localDisplay = GetComponent<RawImage>();
    }

    public void StartVideoReceiving()
    {
        UnityThread.initUnityThread();
        JpegEncoder.Init();
        IPFSPubSub.SubribeToTopic($"Video{id}", VideoPacketReceived);
    }

    public void UpdateTextureRemote(VideoPacket packet)
    {
        localFrame = new Texture2D(packet.m_Width, packet.m_Height, TextureFormat.RGB24, false);

        var decompressed = VideoUtility.MultiDecompress(packet.m_Data, packet.m_Compressions);
        var jpeg = JpegEncoder.Decode
        (
            decompressed,
            3,
            ref packet.m_Width,
            ref packet.m_Height,
            packet.m_ID,
            GraphicsFormat.R8G8B8A8_UNorm,
            JpegEncoder.Flags.TJ_BOTTOMUP |
            JpegEncoder.Flags.TJFLAG_FASTDCT
        );

        localFrame.LoadRawTextureData(jpeg);
        localFrame.Apply();


        localDisplay.texture = localFrame;
    }

    private void VideoPacketReceived(IPublishedMessage obj)
    {
        var bytes = obj.DataBytes;
        var textBase64 = Encoding.ASCII.GetString(bytes).TrimStart('u').FromBase64NoPad();
        
        VideoPacket packet = Serializator.DeserializeJson<VideoPacket>(new MemoryStream(textBase64));
        
        UnityThread.executeInUpdate(()=>UpdateTextureRemote(packet));
    }

}
