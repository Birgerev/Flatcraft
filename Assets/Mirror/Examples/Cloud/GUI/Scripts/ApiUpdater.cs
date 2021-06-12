using System;
using Mirror.Cloud.ListServerService;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mirror.Cloud.Example
{
    /// <summary>
    ///     This component should be put on the NetworkManager object
    /// </summary>
    public class ApiUpdater : MonoBehaviour
    {
        [SerializeField] private NetworkManagerListServer manager;
        [SerializeField] private ApiConnector connector;
        public string gameName = "Game";

        private void Awake()
        {
            if (manager == null)
                manager = FindObjectOfType<NetworkManagerListServer>();
            if (connector == null)
                connector = manager.GetComponent<ApiConnector>();

            Debug.Assert(manager != null, "ApiUpdater could not find NetworkManagerListServer");
            Debug.Assert(connector != null, "ApiUpdater could not find ApiConnector");

            manager.onPlayerListChanged += onPlayerListChanged;
            manager.onServerStarted += ServerStartedHandler;
            manager.onServerStopped += ServerStoppedHandler;
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.onPlayerListChanged -= onPlayerListChanged;
                manager.onServerStarted -= ServerStartedHandler;
                manager.onServerStopped -= ServerStoppedHandler;
            }
        }

        private void onPlayerListChanged(int playerCount)
        {
            if (connector.ListServer.ServerApi.ServerInList)
            {
                // update player count so that other players can see
                if (playerCount < manager.maxConnections)
                {
                    Debug.Log($"Updating Server, player count: {playerCount} ");
                    connector.ListServer.ServerApi.UpdateServer(playerCount);
                }
                // remove server when there is max players
                else
                {
                    Debug.Log($"Removing Server, player count: {playerCount}");
                    connector.ListServer.ServerApi.RemoveServer();
                }
            }
            else
            {
                // if not in list, and player counts drops below 2, add server to list
                if (playerCount < 2)
                {
                    Debug.Log($"Adding Server, player count: {playerCount}");
                    AddServer(playerCount);
                }
            }
        }

        private void ServerStartedHandler()
        {
            AddServer(0);
        }

        private void AddServer(int playerCount)
        {
            Transport transport = Transport.activeTransport;

            Uri uri = transport.ServerUri();
            int port = uri.Port;
            string protocol = uri.Scheme;

            connector.ListServer.ServerApi.AddServer(new ServerJson
            {
                displayName = $"{gameName} {(Random.value * 1000).ToString("0")}", protocol = protocol, port = port
                , maxPlayerCount = NetworkManager.singleton.maxConnections, playerCount = playerCount
            });
        }

        private void ServerStoppedHandler()
        {
            connector.ListServer.ServerApi.RemoveServer();
        }
    }
}