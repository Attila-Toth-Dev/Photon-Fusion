using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float bulletSpeed;
    
    // Networked flags the variable to be synced in the TickTimer
    // Networked variables must have an empty get and set
    // They are used to generate serialization code
    [Networked] private TickTimer Life { get; set; }

    public void Init()
    {
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }
    
    public override void FixedUpdateNetwork()
    {
        if(Life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += bulletSpeed * transform.forward * Runner.DeltaTime;
    }
}
