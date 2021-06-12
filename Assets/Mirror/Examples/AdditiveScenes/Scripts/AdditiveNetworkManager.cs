using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror.Examples.Additive
{
    [AddComponentMenu("")]
    public class AdditiveNetworkManager : NetworkManager
    {
        [Tooltip("Trigger Zone Prefab")] public GameObject Zone;

        [Scene] [Tooltip("Add all sub-scenes to this list")]
        public string[] subScenes;

        public override void OnStartServer()
        {
            base.OnStartServer();

            // load all subscenes on the server only
            StartCoroutine(LoadSubScenes());

            // Instantiate Zone Handler on server only
            Instantiate(Zone);
        }

        private IEnumerator LoadSubScenes()
        {
            Debug.Log("Loading Scenes");

            foreach (string sceneName in subScenes)
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            // Debug.Log($"Loaded {sceneName}");
        }

        public override void OnStopServer()
        {
            StartCoroutine(UnloadScenes());
        }

        public override void OnStopClient()
        {
            StartCoroutine(UnloadScenes());
        }

        private IEnumerator UnloadScenes()
        {
            Debug.Log("Unloading Subscenes");

            foreach (string sceneName in subScenes)
                if (SceneManager.GetSceneByName(sceneName).IsValid() ||
                    SceneManager.GetSceneByPath(sceneName).IsValid())
                    yield return SceneManager.UnloadSceneAsync(sceneName);
                // Debug.Log($"Unloaded {sceneName}");

            yield return Resources.UnloadUnusedAssets();
        }
    }
}