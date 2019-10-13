using UnityEngine;
using System.Collections;
using Mirror;

public class FPSNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
    }
}
