using Fusion;
using Network.Data_Types;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private TMP_Text _messages;

    private Vector3 _lookDir;
    
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            transform.position += 5 * data.moveDirection * Runner.DeltaTime;

            if (Camera.main == null) return;
            
            Ray ray = Camera.main.ScreenPointToRay(data.lookDirection);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 lookDir = hit.point - transform.position;
                transform.rotation = Quaternion.LookRotation(lookDir * Runner.DeltaTime);
            }
        }
    }
    
    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
            RPC_SendMessage("Hey Mate!");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_SendMessage(string message, RpcInfo info = default) => RPC_RelayMessage(message, info.Source);

    // ReSharper disable Unity.PerformanceAnalysis
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    private void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_messages == null)
            _messages = FindFirstObjectByType<TMP_Text>();

        message = messageSource == Runner.LocalPlayer ? $"You said: {message}\n" : $"Some other player said: {message}\n";
        
        _messages.text += message;
    }
}
