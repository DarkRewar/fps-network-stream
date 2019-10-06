using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;

public class GameManager : MonoSingleton<GameManager>
{
    public List<PlayerNetwork> Players = new List<PlayerNetwork>();

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
        StartCoroutine(DoRespawnPlayer(player));

        //NetworkServer.Destroy(player.gameObject);
    }

    private IEnumerator DoRespawnPlayer(PlayerNetwork player)
    {
        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        player.gameObject.SetActive(true);
        player.Lifepoints = 3;
    }
}
