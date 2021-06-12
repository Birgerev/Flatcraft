using UnityEngine;

namespace Mirror.Examples.MultipleAdditiveScenes
{
    public class PhysicsSimulator : MonoBehaviour
    {
        private PhysicsScene physicsScene;
        private PhysicsScene2D physicsScene2D;

        private bool simulatePhysicsScene;
        private bool simulatePhysicsScene2D;

        private void Awake()
        {
            if (NetworkServer.active)
            {
                physicsScene = gameObject.scene.GetPhysicsScene();
                simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;

                physicsScene2D = gameObject.scene.GetPhysicsScene2D();
                simulatePhysicsScene2D = physicsScene2D.IsValid() && physicsScene2D != Physics2D.defaultPhysicsScene;
            }
            else
            {
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;

            if (simulatePhysicsScene)
                physicsScene.Simulate(Time.fixedDeltaTime);

            if (simulatePhysicsScene2D)
                physicsScene2D.Simulate(Time.fixedDeltaTime);
        }
    }
}