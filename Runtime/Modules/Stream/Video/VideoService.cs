using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CipherDuo.IPFS.Modules;
using CipherDuo.IPFS.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;


//TODO reformat as modelViewPresenter
//TODO switch turboJpeg/ffmpeg

//using static FFmpegOut.Blitter;

[RequireComponent(typeof(Camera))]
public class VideoSender : MonoBehaviour
{
    [SerializeField] int width = 1920;
    [SerializeField] int height = 1080;
    [SerializeField] int quality = 25;

    [SerializeField] int depthBuffer = 24;
    [SerializeField] int compression = 25;
    public int id = 0;

    [SerializeField] bool isStreaming;

    private RenderTexture streamTexture;
    private RenderTexture proxyStreamTexture;
    private Camera streamCamera;
    private Camera proxyCamera;
    
    private float lastPixelAverage;
    private Texture2D streamFrame;
    private Texture2D proxyFrame;

    private void Awake()
    {
        streamCamera = GetComponent<Camera>();
        var GO = new GameObject("CameraProxy");
        GO.transform.SetParent(streamCamera.transform);
        GO.transform.localPosition = Vector3.zero;
        GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        proxyCamera = GO.AddComponent<Camera>();
    }

    [Button]
    public void InitVideo()
    {
        JpegEncoder.Init();
        streamTexture = new RenderTexture(width, height, depthBuffer, RenderTextureFormat.ARGB32);
        proxyStreamTexture = new RenderTexture(width/8, height/8, depthBuffer/8, RenderTextureFormat.ARGB32);
        
        proxyFrame = new Texture2D(proxyStreamTexture.width, proxyStreamTexture.height, TextureFormat.RGB24, false);
        streamFrame = new Texture2D(streamTexture.width, streamTexture.height, TextureFormat.RGB24, false);
        
        streamCamera.targetTexture = streamTexture;
        proxyCamera.targetTexture = proxyStreamTexture;
    }

    public void StartVideoStream()
    {
        InitVideo();

        isStreaming = true;
    }

    public void UpdateStreamFrame()
    {
        RenderTexture.active = streamTexture;
        streamFrame.ReadPixels(new Rect(0f, 0f, streamTexture.width, streamTexture.height), 0, 0);
        RenderTexture.active = null;

        RenderTexture.active = proxyStreamTexture;
        proxyFrame.ReadPixels(new Rect(0f,0f, proxyStreamTexture.width, proxyStreamTexture.height),0,0);
        RenderTexture.active = null;
    }
    
    public void Request(Texture a_Texture, Action<AsyncGPUReadbackRequest> a_Callback)
    {
        IEnumerator RequestAsync (AsyncGPUReadbackRequest a_Request, Action<AsyncGPUReadbackRequest> a_Callback)
        {
            yield return a_Request;
            a_Callback?.Invoke (a_Request);
        } 
 
        AsyncGPUReadbackRequest req = AsyncGPUReadback.Request (a_Texture);
        StartCoroutine (RequestAsync (req, a_Callback));
    }

    void LateUpdate()
    {
        if (isStreaming)
        {
            UpdateStreamFrame();
            
            var num = proxyFrame.GetPixels().ToList().Average((x) => (x.grayscale));
            if (Mathf.Abs(num - lastPixelAverage) > float.Epsilon && num > 0.02f)
            {
                byte[] textureBytes = JpegEncoder.Encode
                (
                    streamFrame.GetRawTextureData(),
                    streamTexture.width,
                    streamTexture.height,
                    3,
                    GraphicsFormat.R8G8B8A8_UNorm,
                    quality,
                    JpegEncoder.Flags.TJ_BOTTOMUP |
                    JpegEncoder.Flags.TJFLAG_FASTDCT
                );


                var data = VideoUtility.MultiCompress(textureBytes, compression);


                VideoPacket packet;
                packet.m_Compressions = compression;
                packet.m_Data = data;
                packet.m_Height = streamTexture.height;
                packet.m_Width = streamTexture.width;
                packet.m_ID = 0;

                var serializedPacket = Serializator.SerializeJson<VideoPacket>(packet);
                var bytes = Encoding.ASCII.GetBytes(serializedPacket);
                
                Debug.Log(bytes.Length);
                
                IPFSPubSub.WriteToTopic($"Video{id}", bytes);
                lastPixelAverage = num;
            }
        }
    }
}
