using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using TMPro;
using FPSNetwork;

public class GameManager : MonoSingleton<GameManager>
{
    public List<PlayerNetwork> Players = new List<PlayerNetwork>();

    public TMP_Text KillTexts;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void AddPlayer(PlayerNetwork player)
    {
        Instance.Players.Add(player);
    }

    public static void RemovePlayer(PlayerNetwork playerNetwork)
    {
        Instance.Players.Remove(playerNetwork);
    }

    internal void KillPlayer(PlayerNetwork player)
    {
        MessageNetworker.Instance.RpcKillPlayer(player.connectionToClient.connectionId);
        StartCoroutine(DoRespawnPlayer(player));
        KillTexts.text += $"Kill : Player {player.connectionToClient.connectionId}\n";
    }

    internal void KillPlayerOnClient(PlayerNetwork player)
    {
        KillTexts.text += $"Kill : Player {player.connectionToClient.connectionId}\n";
    }

    internal void KillPlayerOnClient(int connectionId)
    {
        KillTexts.text += $"Kill : Player {connectionId}\n";
    }

    private IEnumerator DoRespawnPlayer(PlayerNetwork player)
    {
        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        NetworkConnection netCon = player.connectionToClient;
        PlayerInformations infos = new PlayerInformations(player.Informations);
        Transform t = NetworkManager.singleton.GetStartPosition();
        NetworkServer.Destroy(player.gameObject);

        yield return new WaitForSeconds(2f);

        //ClientScene.AddPlayer();

        GameObject newPlayer = Instantiate(NetworkManager.singleton.playerPrefab, t.position, t.rotation);
        newPlayer.gameObject.SetActive(true);
        newPlayer.GetComponent<PlayerNetwork>().Lifepoints = 3;
        NetworkServer.AddPlayerForConnection(netCon, newPlayer);
    }
}
