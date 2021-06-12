using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Cloud.Example
{
    /// <summary>
    ///     Uses the ApiConnector on NetworkManager to update the Server list
    /// </summary>
    public class ServerListManager : MonoBehaviour
    {
        [Header("UI")] [SerializeField] private ServerListUI listUI;

        [Header("Buttons")] [SerializeField] private Button refreshButton;

        [SerializeField] private Button startServerButton;


        [Header("Auto Refresh")] [SerializeField]
        private bool autoRefreshServerlist;

        [SerializeField] private int refreshinterval = 20;

        private ApiConnector connector;

        private void Start()
        {
            NetworkManager manager = NetworkManager.singleton;
            connector = manager.GetComponent<ApiConnector>();

            connector.ListServer.ClientApi.onServerListUpdated += listUI.UpdateList;

            if (autoRefreshServerlist)
                connector.ListServer.ClientApi.StartGetServerListRepeat(refreshinterval);

            AddButtonHandlers();
        }

        private void OnDestroy()
        {
            if (connector == null)
                return;

            if (autoRefreshServerlist)
                connector.ListServer.ClientApi.StopGetServerListRepeat();

            connector.ListServer.ClientApi.onServerListUpdated -= listUI.UpdateList;
        }

        private void AddButtonHandlers()
        {
            refreshButton.onClick.AddListener(RefreshButtonHandler);
            startServerButton.onClick.AddListener(StartServerButtonHandler);
        }

        public void RefreshButtonHandler()
        {
            connector.ListServer.ClientApi.GetServerList();
        }

        public void StartServerButtonHandler()
        {
            NetworkManager.singleton.StartServer();
        }
    }
}