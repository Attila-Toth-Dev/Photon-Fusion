using Fusion;
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
