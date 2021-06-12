using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Mirror.Cloud.ListServerService
{
    public sealed class ListServerServerApi : ListServerBaseApi, IListServerServerApi
    {
        private const int PingInterval = 20;
        private const int MaxPingFails = 15;

        private Coroutine _pingCoroutine;

        private ServerJson currentServer;

        /// <summary>
        ///     How many failed pings in a row
        /// </summary>
        private int pingFails;

        /// <summary>
        ///     if a request is currently sending
        /// </summary>
        private bool sending;

        private string serverId;

        /// <summary>
        ///     If an update request was recently sent
        /// </summary>
        private bool skipNextPing;

        public ListServerServerApi(ICoroutineRunner runner, IRequestCreator requestCreator) : base(runner
            , requestCreator)
        {
        }

        /// <summary>
        ///     If the server has already been added
        /// </summary>
        public bool ServerInList { get; private set; }

        public void Shutdown()
        {
            stopPingCoroutine();
            if (ServerInList)
                removeServerWithoutCoroutine();
            ServerInList = false;
        }

        public void AddServer(ServerJson server)
        {
            if (ServerInList)
            {
                Logger.LogWarning("AddServer called when server was already adding or added");
                return;
            }

            bool valid = server.Validate();
            if (!valid)
                return;

            runner.StartCoroutine(addServer(server));
        }

        public void UpdateServer(int newPlayerCount)
        {
            if (!ServerInList)
            {
                Logger.LogWarning("UpdateServer called when before server was added");
                return;
            }

            currentServer.playerCount = newPlayerCount;
            UpdateServer(currentServer);
        }

        public void UpdateServer(ServerJson server)
        {
            // TODO, use PartialServerJson as Arg Instead
            if (!ServerInList)
            {
                Logger.LogWarning("UpdateServer called when before server was added");
                return;
            }

            PartialServerJson partialServer = new PartialServerJson
            {
                displayName = server.displayName, playerCount = server.playerCount
                , maxPlayerCount = server.maxPlayerCount, customData = server.customData
            };
            partialServer.Validate();

            runner.StartCoroutine(updateServer(partialServer));
        }

        public void RemoveServer()
        {
            if (!ServerInList)
                return;

            if (string.IsNullOrEmpty(serverId))
            {
                Logger.LogWarning("Can not remove server because serverId was empty");
                return;
            }

            stopPingCoroutine();
            runner.StartCoroutine(removeServer());
        }

        private void stopPingCoroutine()
        {
            if (_pingCoroutine != null)
            {
                runner.StopCoroutine(_pingCoroutine);
                _pingCoroutine = null;
            }
        }

        private IEnumerator addServer(ServerJson server)
        {
            ServerInList = true;
            sending = true;
            currentServer = server;

            UnityWebRequest request = requestCreator.Post("servers", currentServer);
            yield return requestCreator.SendRequestEnumerator(request, onSuccess, onFail);
            sending = false;

            void onSuccess(string responseBody)
            {
                CreatedIdJson created = JsonUtility.FromJson<CreatedIdJson>(responseBody);
                serverId = created.id;

                // Start ping to keep server alive
                _pingCoroutine = runner.StartCoroutine(ping());
            }

            void onFail(string responseBody)
            {
                ServerInList = false;
            }
        }

        private IEnumerator updateServer(PartialServerJson server)
        {
            // wait to not be sending
            while (sending)
                yield return new WaitForSeconds(1);

            // We need to check added in case Update is called soon after Add, and add failed
            if (!ServerInList)
            {
                Logger.LogWarning("UpdateServer called when before server was added");
                yield break;
            }

            sending = true;
            UnityWebRequest request = requestCreator.Patch("servers/" + serverId, server);
            yield return requestCreator.SendRequestEnumerator(request, onSuccess);
            sending = false;

            void onSuccess(string responseBody)
            {
                skipNextPing = true;

                if (_pingCoroutine == null)
                    _pingCoroutine = runner.StartCoroutine(ping());
            }
        }

        /// <summary>
        ///     Keeps server alive in database
        /// </summary>
        /// <returns></returns>
        private IEnumerator ping()
        {
            while (pingFails <= MaxPingFails)
            {
                yield return new WaitForSeconds(PingInterval);
                if (skipNextPing)
                {
                    skipNextPing = false;
                    continue;
                }

                sending = true;
                UnityWebRequest request = requestCreator.Patch("servers/" + serverId, new EmptyJson());
                yield return requestCreator.SendRequestEnumerator(request, onSuccess, onFail);
                sending = false;
            }

            Logger.LogWarning("Max ping fails reached, stopping to ping server");
            _pingCoroutine = null;


            void onSuccess(string responseBody)
            {
                pingFails = 0;
            }

            void onFail(string responseBody)
            {
                pingFails++;
            }
        }

        private IEnumerator removeServer()
        {
            sending = true;
            UnityWebRequest request = requestCreator.Delete("servers/" + serverId);
            yield return requestCreator.SendRequestEnumerator(request);
            sending = false;

            ServerInList = false;
        }

        private void removeServerWithoutCoroutine()
        {
            if (string.IsNullOrEmpty(serverId))
            {
                Logger.LogWarning("Can not remove server because serverId was empty");
                return;
            }

            UnityWebRequest request = requestCreator.Delete("servers/" + serverId);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            operation.completed += op => { Logger.LogResponse(request); };
        }
    }
}