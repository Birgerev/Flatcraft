using Mirror.Cloud.Example;
using UnityEngine;

namespace Mirror.Cloud.Examples.Pong
{
    public class BallManager : NetworkBehaviour
    {
        [SerializeField] private GameObject ballPrefab;
        private GameObject ball;
        private NetworkManagerListServerPong manager;

        public override void OnStartServer()
        {
            manager = NetworkManager.singleton as NetworkManagerListServerPong;
            manager.onPlayerListChanged += onPlayerListChanged;
        }

        public override void OnStopServer()
        {
            manager.onPlayerListChanged -= onPlayerListChanged;
        }

        private void onPlayerListChanged(int playerCount)
        {
            if (playerCount >= 2)
                SpawnBall();
            if (playerCount < 2)
                DestroyBall();
        }

        private void SpawnBall()
        {
            if (ball != null)
                return;

            ball = Instantiate(ballPrefab);
            NetworkServer.Spawn(ball);
        }

        private void DestroyBall()
        {
            if (ball == null)
                return;

            // destroy ball
            NetworkServer.Destroy(ball);
            ball = null;
        }
    }
}