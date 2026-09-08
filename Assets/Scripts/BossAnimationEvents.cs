using System;
using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    private FinalBoss boss;
    private GameObject player;

    private void Start()
    {
        boss = GetComponentInParent<FinalBoss>();
    }

    public void Disparar()
    {
        if (boss != null)
        {
            boss.DispararDesdeAnimacion();
        }

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
    }

    public void Victori()
    {
        player.GetComponent<PlayerController>().PlayVictoriaAudios();
    }
}
