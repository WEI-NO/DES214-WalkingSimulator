using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject player;

    private void Start()
    {
        // At the start of the game, spawn player to the current position.
        if (!player)
        {
            // Attempts to find player
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player)
        {
            player.transform.position = transform.position;
        }
    }
}
