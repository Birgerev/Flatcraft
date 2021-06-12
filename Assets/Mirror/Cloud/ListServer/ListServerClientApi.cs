using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Mirror.Cloud.ListServerService
{
    public sealed class ListServerClientApi : ListServerBaseApi, IListServerClientApi
    {
        private readonly ServerListEvent _onServerListUpdated;

        private Coroutine getServerListRepeatCoroutine;

        public ListServerClientApi(ICoroutineRunner runner, IRequestCreator requestCreator
            , ServerListEvent onServerListUpdated) : base(runner, requestCreator)
        {
            _onServerListUpdated = onServerListUpdated;
        }

        public event UnityAction<ServerCollectionJson> onServerListUpdated
        {
            add => _onServerListUpdated.AddListener(value);
            remove => _onServerListUpdated.RemoveListener(value);
        }

        public void Shutdown()
        {
            StopGetServerListRepeat();
        }

        public void GetServerList()
        {
            runner.StartCoroutine(getServerList());
        }

        public void StartGetServerListRepeat(int interval)
        {
            getServerListRepeatCoroutine = runner.StartCoroutine(GetServerListRepeat(interval));
        }

        public void StopGetServerListRepeat()
        {
            // if runner is null it has been destroyed and will already be null
            if (runner.IsNotNull() && getServerListRepeatCoroutine != null)
                runner.StopCoroutine(getServerListRepeatCoroutine);
        }

        private IEnumerator GetServerListRepeat(int interval)
        {
            while (true)
            {
                yield return getServerList();

                yield return new WaitForSeconds(interval);
            }
        }

        private IEnumerator getServerList()
        {
            UnityWebRequest request = requestCreator.Get("servers");
            yield return requestCreator.SendRequestEnumerator(request, onSuccess);

            void onSuccess(string responseBody)
            {
                ServerCollectionJson serverlist = JsonUtility.FromJson<ServerCollectionJson>(responseBody);
                _onServerListUpdated?.Invoke(serverlist);
            }
        }
    }
}