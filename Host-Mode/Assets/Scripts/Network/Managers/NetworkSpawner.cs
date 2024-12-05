using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Network.Data_Types;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Network.Managers
{
    public class NetworkSpawner : SimulationBehaviour, INetworkRunnerCallbacks
    {
        [Header("Player Prefab")]
        [SerializeField] private NetworkPrefabRef playerPrefab;
        
        [Header("Controls")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        
        private Vector3 _movementInput;
        private Vector3 _lookInput;
        
        private readonly Dictionary<PlayerRef, NetworkObject> _spawnCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private NetworkRunner _runner;
    
        // ReSharper disable once AsyncVoidMethod
        async void StartGame(GameMode mode)
        {
            // Create the Fusion Runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        
            // Create the NetworkSceneInfo from the current scene
            SceneRef scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            NetworkSceneInfo sceneInfo = new NetworkSceneInfo();

            if (scene.IsValid)
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        
            // Start or join (depends on game mode) a session with a specific name
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "Fusion Room",
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }

        private void Awake()
        {
#if UNITY_LINUX_SERVER
            StartGame(GameMode.Server);
#endif
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        private void OnGUI()
        {
            if (_runner == null)
            {
                if (GUI.Button(new Rect(10, 10, 200, 40), "Server"))
                    StartGame(GameMode.Server);
                
                if (GUI.Button(new Rect(10, 55, 200, 40), "Host"))
                    StartGame(GameMode.Host);

                if (GUI.Button(new Rect(10, 100, 200, 40), "Join"))
                    StartGame(GameMode.Client);
            }
        }
#endif
    
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                // Create a unique position for the player
                Vector3 spawnPosition = new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10));
                NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
            
                // Keep track of the player avatars for east access
                _spawnCharacters.Add(player, networkPlayerObject);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnCharacters.Remove(player);
            }
        }

        private void Update()
        {
            _movementInput = new Vector3(moveAction.action.ReadValue<Vector2>().x, 0, moveAction.action.ReadValue<Vector2>().y);
            _lookInput = Input.mousePosition/*new Vector3(lookAction.action.ReadValue<Vector2>().x, 0, lookAction.action.ReadValue<Vector2>().y)*/;
        }

        public void OnEnable()
        {
            if (_runner == null) return;
            
            moveAction.action.Enable();
            lookAction.action.Enable();
                
            _runner.AddCallbacks(this);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            NetworkInputData data = new NetworkInputData();
            
            data.moveDirection += _movementInput;
            data.lookDirection += _lookInput;
            
            input.Set(data);
        }

        public void OnDisable()
        {
            if (_runner == null) return;
            
            moveAction.action.Disable();
            lookAction.action.Disable();

            _runner.RemoveCallbacks(this);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        // ReSharper disable once Unity.IncorrectMethodSignature
        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
    }
}
