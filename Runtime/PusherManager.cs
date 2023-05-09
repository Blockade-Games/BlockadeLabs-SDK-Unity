#if PUSHER_PRESENT // compile only is pusher package is installed
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PusherClient;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public class PusherManager : MonoBehaviour
    {
        // A mutation of https://unity3d.com/learn/tutorials/projects/2d-roguelike-tutorial/writing-game-manager
        public static PusherManager instance = null;
        private Pusher _pusher;
        private const string APP_KEY = "a6a7b7662238ce4494d5";
        private const string APP_CLUSTER = "mt1";
        private List<string> imagineObfuscatedIds = new List<string>();
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

        public async Task SubscribeToChannel(string id)
        {
            Debug.Log("SubscribeToChannel");
            await _pusher.SubscribeAsync("status_update_" + id);
        }

        public void CheckIfAssetsReady()
        {
            if (imagineObfuscatedIds.Count != previousImagineCount)
            {
                var blockadeImaginariums = FindObjectsOfType<BlockadeLabsSkybox>();

                foreach (var blockadeImaginarium in blockadeImaginariums)
                {
                    if (imagineObfuscatedIds.Last() == blockadeImaginarium.imagineObfuscatedId)
                    {
                        // Unsubscribe from channels and events
                        _pusher?.UnbindAll();
                        _ = UnsubscribeFromChannel();
                        // Get the complete asset
                        _ = blockadeImaginarium.GetAssets();
                    }
                }

                previousImagineCount = imagineObfuscatedIds.Count;
            }
        }

        private void PusherOnConnected(object sender)
        {
            Debug.Log("Connected");
        }

        private async Task UnsubscribeFromChannel()
        {
            await _pusher.UnsubscribeAllAsync().ConfigureAwait(false);
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
            channel?.Bind("status_update", (string response) =>
            {
                Debug.Log("status_update event received");
                var pusherResponse = JsonConvert.DeserializeObject<PusherResponse>(response);
                var pusherResponseData = JsonConvert.DeserializeObject<PusherResponseData>(pusherResponse.data);

                if (pusherResponseData.status == "complete")
                {
                    imagineObfuscatedIds.Add(pusherResponseData.obfuscated_id);
                }
            });
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
        public string obfuscated_id { get; set; }
    }
}
#endif