using Fusion;
using UnityEngine;

namespace Network.Data_Types
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 moveDirection;
        public Vector3 lookDirection;
    }
}
