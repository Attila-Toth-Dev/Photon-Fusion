using System;
using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked] public bool SpawnedProjectile { get; set; }

    [SerializeField] private Ball prefabBall;
    [SerializeField] private PhysXBall prefabPhysXBall;
    
    [Networked] private TickTimer Delay { get; set; }

    private Vector3 _forward;
    private NetworkCharacterController _characterController;
    
    private ChangeDetector _changeDetector;
    private Material _material;

    private TMP_Text _messages;

    private void Awake()
    {
        _characterController = GetComponent<NetworkCharacterController>();
        _forward = Vector3.forward;
        
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_messages == null)
            _messages = FindObjectOfType<TMP_Text>();

        if (messageSource == Runner.LocalPlayer)
        {
            message = $"You said: {message}\n";
        }
        else
        {
            message = $"Some other player said: {message}\n";
        }
        
        _messages.text += message;
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(SpawnedProjectile):
                    _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);;
                    break;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _characterController.Move(5 * data.direction * Runner.DeltaTime);
            
            if(data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (HasStateAuthority && Delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    
                    Runner.Spawn(prefabBall, 
                        transform.position + _forward, Quaternion.LookRotation(_forward, Vector3.up),
                        Object.InputAuthority, (runner, o) =>
                        {
                            // Initialize the Ball before synchronizing it
                            o.GetComponent<Ball>().Init();
                        });
                    
                    SpawnedProjectile = !SpawnedProjectile;
                }
                else if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                    Runner.Spawn(prefabPhysXBall,
                        transform.position + _forward, Quaternion.LookRotation(_forward, Vector3.up),
                        Object.InputAuthority, (runner, o) =>
                        {
                            o.GetComponent<PhysXBall>().Init(10 * _forward);
                        });
                    
                    SpawnedProjectile = !SpawnedProjectile;
                }
            }
        }
    }
}
