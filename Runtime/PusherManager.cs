#if PUSHER_PRESENT // compile only is pusher package is installed
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PusherClient;
using UnityEngine;

public class PusherManager : MonoBehaviour
{
    // A mutation of https://unity3d.com/learn/tutorials/projects/2d-roguelike-tutorial/writing-game-manager
    [Tooltip("API Secret Key from Blockade Labs")]
    [SerializeField]
    public string apiSecretKey;
    
    public static PusherManager instance = null;
    private Pusher _pusher;
    private Channel _channel;
    private const string APP_KEY = "a6a7b7662238ce4494d5";
    private const string APP_CLUSTER = "mt1";
    private List<int> imagineIds = new List<int>();
    private int previousImagineCount = 0;
    
    async Task Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        await InitialisePusher();
    }

    public async Task InitialisePusher()
    {
        //Environment.SetEnvironmentVariable("PREFER_DNS_IN_ADVANCE", "true");
        if (_pusher == null && (APP_KEY != "APP_KEY") && (APP_CLUSTER != "APP_CLUSTER"))
        {
            _pusher = new Pusher(APP_KEY, new PusherOptions()
            {
                Cluster = APP_CLUSTER,
                Encrypted = true
            });

            _pusher.Error += OnPusherOnError;
            _pusher.ConnectionStateChanged += PusherOnConnectionStateChanged;
            _pusher.Connected += PusherOnConnected;
            _channel = await _pusher.SubscribeAsync("api_client_status_update_" + apiSecretKey);
			_pusher.Subscribed += OnChannelOnSubscribed;
            await _pusher.ConnectAsync();
        }
        else
        {
            Debug.LogError("APP_KEY and APP_CLUSTER must be correctly set. Find how to set it at https://dashboard.pusher.com");
        }
    }

    private void OnGUI()
    {
        CheckIfAssetsReady();
    }

    public void CheckIfAssetsReady()
    {
        if (imagineIds.Count != previousImagineCount)
        {
            var blockadeImaginariums = FindObjectsOfType<BlockadeImaginarium>();

            foreach (var blockadeImaginarium in blockadeImaginariums)
            {
                if (imagineIds.Last() == blockadeImaginarium.imagineId)
                {
                    _ = blockadeImaginarium.GetAssets();
                }
            }

            previousImagineCount = imagineIds.Count;
        }
    }

    private void PusherOnConnected(object sender)
    {
        Debug.Log("Connected");
        _channel.Bind("status_update", (string response) =>
        {
            Debug.Log("status_update event received");
            var pusherResponse = JsonConvert.DeserializeObject<PusherResponse>(response);
            var pusherResponseData = JsonConvert.DeserializeObject<PusherResponseData>(pusherResponse.data);

            if (pusherResponseData.status == "complete")
            {
                imagineIds.Add(pusherResponseData.id);
            }
        });
    }

    private void PusherOnConnectionStateChanged(object sender, ConnectionState state)
    {
        Debug.Log("Connection state changed");
    }

    private void OnPusherOnError(object s, PusherException e)
    {
        Debug.Log("Errored");
        Debug.Log(s);
        Debug.Log(e);
    }

    private void OnChannelOnSubscribed(object s, Channel channel)
    {
        Debug.Log("Subscribed");
    }

    async Task OnApplicationQuit()
    {
        if (_pusher != null)
        {
            await _pusher.DisconnectAsync();
        }
    }
}

// JSON fields representations
[System.Serializable]
public class PusherResponse
{
    public string data { get; set; }
}

public class PusherResponseData
{
    public string status { get; set; }
    public int id { get; set; }
}

#endif