using UnityEngine;

namespace Mirror.Experimental
{
    [AddComponentMenu("Network/Experimental/NetworkRigidbody")]
    [HelpURL("https://mirror-networking.com/docs/Articles/Components/NetworkRigidbody.html")]
    public class NetworkRigidbody : NetworkBehaviour
    {
        [Header("Settings")] [SerializeField] internal Rigidbody target;

        [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
        [SerializeField]
        private bool clientAuthority;

        [Header("Velocity")] [Tooltip("Syncs Velocity every SyncInterval")] [SerializeField]
        private bool syncVelocity = true;

        [Tooltip("Set velocity to 0 each frame (only works if syncVelocity is false")] [SerializeField]
        private bool clearVelocity;

        [Tooltip("Only Syncs Value if distance between previous and current is great than sensitivity")]
        [SerializeField]
        private float velocitySensitivity = 0.1f;


        [Header("Angular Velocity")] [Tooltip("Syncs AngularVelocity every SyncInterval")] [SerializeField]
        private bool syncAngularVelocity = true;

        [Tooltip("Set angularVelocity to 0 each frame (only works if syncAngularVelocity is false")] [SerializeField]
        private bool clearAngularVelocity;

        [Tooltip("Only Syncs Value if distance between previous and current is great than sensitivity")]
        [SerializeField]
        private float angularVelocitySensitivity = 0.1f;

        /// <summary>
        ///     Values sent on client with authority after they are sent to the server
        /// </summary>
        private readonly ClientSyncState previousValue = new ClientSyncState();


        internal void Update()
        {
            if (isServer)
                SyncToClients();
            else if (ClientWithAuthority)
                SendToServer();
        }

        internal void FixedUpdate()
        {
            if (clearAngularVelocity && !syncAngularVelocity)
                target.angularVelocity = Vector3.zero;

            if (clearVelocity && !syncVelocity)
                target.velocity = Vector3.zero;
        }

        private void OnValidate()
        {
            if (target == null)
                target = GetComponent<Rigidbody>();
        }

        /// <summary>
        ///     Updates sync var values on server so that they sync to the client
        /// </summary>
        [Server]
        private void SyncToClients()
        {
            // only update if they have changed more than Sensitivity

            Vector3 currentVelocity = syncVelocity ? target.velocity : default;
            Vector3 currentAngularVelocity = syncAngularVelocity ? target.angularVelocity : default;

            bool velocityChanged = syncVelocity && (previousValue.velocity - currentVelocity).sqrMagnitude >
                velocitySensitivity * velocitySensitivity;
            bool angularVelocityChanged = syncAngularVelocity &&
                                          (previousValue.angularVelocity - currentAngularVelocity).sqrMagnitude >
                                          angularVelocitySensitivity * angularVelocitySensitivity;

            if (velocityChanged)
            {
                velocity = currentVelocity;
                previousValue.velocity = currentVelocity;
            }

            if (angularVelocityChanged)
            {
                angularVelocity = currentAngularVelocity;
                previousValue.angularVelocity = currentAngularVelocity;
            }

            // other rigidbody settings
            isKinematic = target.isKinematic;
            useGravity = target.useGravity;
            drag = target.drag;
            angularDrag = target.angularDrag;
        }

        /// <summary>
        ///     Uses Command to send values to server
        /// </summary>
        [Client]
        private void SendToServer()
        {
            if (!hasAuthority)
            {
                Debug.LogWarning("SendToServer called without authority");
                return;
            }

            SendVelocity();
            SendRigidBodySettings();
        }

        [Client]
        private void SendVelocity()
        {
            float now = Time.time;
            if (now < previousValue.nextSyncTime)
                return;

            Vector3 currentVelocity = syncVelocity ? target.velocity : default;
            Vector3 currentAngularVelocity = syncAngularVelocity ? target.angularVelocity : default;

            bool velocityChanged = syncVelocity && (previousValue.velocity - currentVelocity).sqrMagnitude >
                velocitySensitivity * velocitySensitivity;
            bool angularVelocityChanged = syncAngularVelocity &&
                                          (previousValue.angularVelocity - currentAngularVelocity).sqrMagnitude >
                                          angularVelocitySensitivity * angularVelocitySensitivity;

            // if angularVelocity has changed it is likely that velocity has also changed so just sync both values
            // however if only velocity has changed just send velocity
            if (angularVelocityChanged)
            {
                CmdSendVelocityAndAngular(currentVelocity, currentAngularVelocity);
                previousValue.velocity = currentVelocity;
                previousValue.angularVelocity = currentAngularVelocity;
            }
            else if (velocityChanged)
            {
                CmdSendVelocity(currentVelocity);
                previousValue.velocity = currentVelocity;
            }


            // only update syncTime if either has changed
            if (angularVelocityChanged || velocityChanged)
                previousValue.nextSyncTime = now + syncInterval;
        }

        [Client]
        private void SendRigidBodySettings()
        {
            // These shouldn't change often so it is ok to send in their own Command
            if (previousValue.isKinematic != target.isKinematic)
            {
                CmdSendIsKinematic(target.isKinematic);
                previousValue.isKinematic = target.isKinematic;
            }

            if (previousValue.useGravity != target.useGravity)
            {
                CmdSendUseGravity(target.useGravity);
                previousValue.useGravity = target.useGravity;
            }

            if (previousValue.drag != target.drag)
            {
                CmdSendDrag(target.drag);
                previousValue.drag = target.drag;
            }

            if (previousValue.angularDrag != target.angularDrag)
            {
                CmdSendAngularDrag(target.angularDrag);
                previousValue.angularDrag = target.angularDrag;
            }
        }

        /// <summary>
        ///     Called when only Velocity has changed on the client
        /// </summary>
        [Command]
        private void CmdSendVelocity(Vector3 velocity)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            this.velocity = velocity;
            target.velocity = velocity;
        }

        /// <summary>
        ///     Called when angularVelocity has changed on the client
        /// </summary>
        [Command]
        private void CmdSendVelocityAndAngular(Vector3 velocity, Vector3 angularVelocity)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            if (syncVelocity)
            {
                this.velocity = velocity;

                target.velocity = velocity;
            }

            this.angularVelocity = angularVelocity;
            target.angularVelocity = angularVelocity;
        }

        [Command]
        private void CmdSendIsKinematic(bool isKinematic)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            this.isKinematic = isKinematic;
            target.isKinematic = isKinematic;
        }

        [Command]
        private void CmdSendUseGravity(bool useGravity)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            this.useGravity = useGravity;
            target.useGravity = useGravity;
        }

        [Command]
        private void CmdSendDrag(float drag)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            this.drag = drag;
            target.drag = drag;
        }

        [Command]
        private void CmdSendAngularDrag(float angularDrag)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            this.angularDrag = angularDrag;
            target.angularDrag = angularDrag;
        }

        /// <summary>
        ///     holds previously synced values
        /// </summary>
        public class ClientSyncState
        {
            public float angularDrag;
            public Vector3 angularVelocity;
            public float drag;
            public bool isKinematic;

            /// <summary>
            ///     Next sync time that velocity will be synced, based on syncInterval.
            /// </summary>
            public float nextSyncTime;

            public bool useGravity;
            public Vector3 velocity;
        }


        #region Sync vars

        [SyncVar(hook = nameof(OnVelocityChanged))]
        private Vector3 velocity;

        [SyncVar(hook = nameof(OnAngularVelocityChanged))]
        private Vector3 angularVelocity;

        [SyncVar(hook = nameof(OnIsKinematicChanged))]
        private bool isKinematic;

        [SyncVar(hook = nameof(OnUseGravityChanged))]
        private bool useGravity;

        [SyncVar(hook = nameof(OnuDragChanged))]
        private float drag;

        [SyncVar(hook = nameof(OnAngularDragChanged))]
        private float angularDrag;

        /// <summary>
        ///     Ignore value if is host or client with Authority
        /// </summary>
        /// <returns></returns>
        private bool IgnoreSync => isServer || ClientWithAuthority;

        private bool ClientWithAuthority => clientAuthority && hasAuthority;

        private void OnVelocityChanged(Vector3 _, Vector3 newValue)
        {
            if (IgnoreSync)
                return;

            target.velocity = newValue;
        }


        private void OnAngularVelocityChanged(Vector3 _, Vector3 newValue)
        {
            if (IgnoreSync)
                return;

            target.angularVelocity = newValue;
        }

        private void OnIsKinematicChanged(bool _, bool newValue)
        {
            if (IgnoreSync)
                return;

            target.isKinematic = newValue;
        }

        private void OnUseGravityChanged(bool _, bool newValue)
        {
            if (IgnoreSync)
                return;

            target.useGravity = newValue;
        }

        private void OnuDragChanged(float _, float newValue)
        {
            if (IgnoreSync)
                return;

            target.drag = newValue;
        }

        private void OnAngularDragChanged(float _, float newValue)
        {
            if (IgnoreSync)
                return;

            target.angularDrag = newValue;
        }

        #endregion
    }
}