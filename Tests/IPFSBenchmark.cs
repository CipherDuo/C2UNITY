using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System.Text;
using CipherDuo.IPFS.Modules;
using CipherDuo.IPFS.Serialization;
using Ipfs;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways, RequireComponent(typeof(IPFSService))]
public class IPFSBenchmark : MonoBehaviour
{
    private IPFSService ipfs => GetComponent<IPFSService>();

    [Space, ShowInInspector] List<Texture2D> textures = new List<Texture2D>();
    
    [ShowInInspector] List<Mesh> meshes = new List<Mesh>();

    [ShowInInspector] private List<Material> materials = new List<Material>();

    [ShowInInspector] private List<GameObject> gameObjects = new List<GameObject>();
    
    

    [TitleGroup("MULTIPLAYER")]
    [ShowInInspector] private int myID = 0;
    [ShowInInspector] private Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
    [ShowInInspector] private bool isUpdatingMovement;
    
    //MULTIPLAYER
    [Button]
    public void ConnectMultiplayer()
    {
        ipfs.Init();

        IPFSPubSub.SubribeToTopic("Multiplayer", MultiplayerReceived);
        isUpdatingMovement = true;
    }
    
    void FixedUpdate()
    {
        if (!Mathf.Approximately(Input.GetAxis("Horizontal"), 0) && isUpdatingMovement|| !Mathf.Approximately(Input.GetAxis("Vertical"), 0) && isUpdatingMovement)
        {
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * 5;
            var y = Input.GetAxis("Vertical") * Time.deltaTime * 5;

            MultiplayerPacket packet = new MultiplayerPacket();
            packet.id = myID;
            packet.x = BitConverter.GetBytes(x);
            packet.y = BitConverter.GetBytes(y);
            
            var bytes = Serializator.SerializeJson(packet);
            IPFSPubSub.WriteToTopic("Multiplayer", Encoding.ASCII.GetBytes(bytes));
        }
    }

    [TitleGroup("STREAM")]
    [Button]
    public void TestWrite_Stream(int id)
    {
        ipfs.Init();
        
        var go = new GameObject("Stream Sender");
        
        var sender = go.AddComponent<VideoSender>();
        
        sender.id = id;
        sender.StartVideoStream();
    }

    [Button]
    public void TestRead_Stream(int id)
    {
        ipfs.Init();

        Canvas canvas;
        if (!FindObjectOfType<Canvas>())
        {
            canvas = new GameObject("Canvas").AddComponent<Canvas>();
            var layout = canvas.gameObject.AddComponent<GridLayoutGroup>();
            
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            
            layout.cellSize = new Vector2(Screen.currentResolution.width/2, Screen.currentResolution.height/2);

        }
        else
        {
            canvas = FindObjectOfType<Canvas>();
        }
        

        var go = new GameObject("Stream Receiver");
        go.transform.SetParent(canvas.transform);
        
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        
        var receiver = go.AddComponent<VideoReceiver>();

        receiver.id = id;
        receiver.StartVideoReceiving();
    }
    
    [TitleGroup("SCENE"), PropertySpace]
    [Button, ShowInInspector]
    public async void TestWrite_Scene(GameObject sceneRoot)
    {
        ipfs.Init();
        
        var sceneOnIPFS = await UnityWrite.WriteScene(sceneRoot);
        Debug.Log(sceneOnIPFS);
    }
    
    [Button]
    public async void TestRead_Scene(string cid)
    {
        ipfs.Init();
        
        var sceneFromIPFS = await UnityRead.ReadScene(cid);
    }

    [TitleGroup("GAMEOBJECT"), PropertySpace]
    [Button]
    public async void TestWrite_GameObject(GameObject go)
    {
        ipfs.Init();

        var modelOnIPFS = await UnityWrite.WriteGameObject(go);
        Debug.Log(modelOnIPFS);
    }
    
    [Button]
    public async void TestRead_GameObject(string cid)
    {
        ipfs.Init();
        
        var goFromIPFS = await UnityRead.ReadGameObject(cid);
        gameObjects.Add(goFromIPFS);
        
    }
    
    [TitleGroup("MATERIAL"), PropertySpace]
    [Button]
    public async void TestWrite_Material(Material material)
    {
        ipfs.Init();
        
        var materialOnIPFS = await UnityWrite.WriteMaterial(material);
        Debug.Log(materialOnIPFS);
    }
    
    [Button]
    public async void TestRead_Material(string cid)
    {
        ipfs.Init();
        
        var materialFromIPFS = await UnityRead.ReadMaterial(cid);
        materials.Add(materialFromIPFS);
    }
    
    
    [TitleGroup("MESH"), PropertySpace]
    [Button]
    public async void TestWrite_Mesh(Mesh mesh)
    {
        ipfs.Init();
        
        var meshOnIPFS = await UnityWrite.WriteMesh(mesh);
        Debug.Log(meshOnIPFS);
    }
    
    [Button]
    public async void TestRead_Mesh(string cid)
    {
        ipfs.Init();
        
        var meshFromIPFS = await UnityRead.ReadMesh(cid);
        meshes.Add(meshFromIPFS);
    }


    [TitleGroup("TEXTURE"), PropertySpace]
    [Button]
    public async void TestWrite_Texture(Texture2D image)
    {
        ipfs.Init();
        
        var textureOnIPFS = await UnityWrite.WriteTexture(image);
        Debug.Log(textureOnIPFS);
    }
    
    [Button]
    public async void TestRead_Texture(string cid)
    {
        ipfs.Init();
        
        var textureFromIPFS = await UnityRead.ReadTexture(cid);
        textures.Add(textureFromIPFS);
    }

    
    [TitleGroup("STRING"), PropertySpace]
    [Button]
    public async void TestWrite_String(string msg = "This is some default text used to avoid duplicates :D",
        bool useRandomNumber = true, bool useConsoleChat = false)
    {
        ipfs.Init();
        
        var random = useRandomNumber ? UnityEngine.Random.Range(0f, 1000f).ToString() : "";
        var msgOnIPFS = await UnityWrite.WriteString(msg + random);
        
        Debug.Log(msgOnIPFS);
        
        if (useConsoleChat)
        {
            IPFSPubSub.WriteToTopic("ConsoleChat", Encoding.UTF8.GetBytes(msg + random));
        }
    }
    
    [Button]
    public async void TestRead_String(string cid)
    {
        ipfs.Init();
        
        var msgFromIPFS = await UnityRead.ReadString(cid);
        Debug.Log(msgFromIPFS);
    }
    public async void TestWrite_String(string cid)
    {
        ipfs.Init();
        
        var msgFromIPFS = await UnityWrite.WriteString(cid);
        Debug.Log(msgFromIPFS);
    }



    void MultiplayerReceived(IPublishedMessage obj)
    {
        //TODO create multiplayer packet & appropriate serialization (DAG only)

        var bytes = obj.DataBytes;
        var textBase64 = Encoding.ASCII.GetString(bytes).TrimStart('u').FromBase64NoPad();

        MultiplayerPacket packet = new MultiplayerPacket();

        packet = Serializator.DeserializeJson<MultiplayerPacket>(new MemoryStream(textBase64));

        int id = packet.id;
        Vector2 playerInput = new Vector2(BitConverter.ToSingle(packet.x), BitConverter.ToSingle(packet.y));

        UnityThread.executeInUpdate(() => MovePlayer(id, playerInput));

    }

    void MovePlayer(int playerID, Vector2 playerInput)
    {
        if ( /*playerID != myID && */!players.ContainsKey(playerID))
        {
            var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            
            players[playerID] = playerGO;
        }

        players[playerID].transform.position += new Vector3(playerInput.x, playerInput.y, 0);
    }
}



