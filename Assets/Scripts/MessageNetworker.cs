using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageNetworker : NetworkBehaviour
{
    public static MessageNetworker Instance;

    private void Awake()
    {
        Instance = this;    
    }

    [ClientRpc]
    public void RpcKillPlayer(int connId)
    {
        if (isServer) return;

        GameManager.Instance.KillPlayerOnClient(connId);
    }
}
