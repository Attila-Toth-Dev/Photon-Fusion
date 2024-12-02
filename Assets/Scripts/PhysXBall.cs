using Fusion;
using UnityEngine;

public class PhysXBall : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }

    public void Init(Vector3 forward)
    {
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        GetComponent<Rigidbody>().linearVelocity = forward;
    }
    
    public override void FixedUpdateNetwork()
    {
        if(Life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += 5 * transform.forward * Runner.DeltaTime;
    }
}
