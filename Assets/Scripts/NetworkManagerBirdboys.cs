using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Birdboys
{
    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).
    [AddComponentMenu("")]
    public class NetworkManagerBirdboys : NetworkManager
    {

        public GameObject welcomePanel;

        List<NetworkConnection> connections = new List<NetworkConnection>();

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);
            connections.Add(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            connections.Remove(conn);
        }

        void Update()
        {
            if (isNetworkActive)
            {

                if (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly)
                {
                    foreach (NetworkConnection conn in connections)
                    {
                        PlayerBird player = conn.identity.GetComponent<PlayerBird>();

                        if (player.respawnReady)
                        {
                            Transform start = GetStartPosition();
                            conn.identity.transform.position = start.position;
                            conn.identity.transform.rotation = start.rotation;
                            player.respawnTrigger = true;
                            player.RespawnSyncVar();
                        }
                    }
                }

                if (mode == NetworkManagerMode.Offline || mode == NetworkManagerMode.ServerOnly)
                {
                    GetComponent<NetworkManagerHUD>().enabled = true;
                }
                else
                {
                    GetComponent<NetworkManagerHUD>().enabled = false;
                }

                welcomePanel.SetActive(mode == NetworkManagerMode.Offline);

            }
        }
    }

}
