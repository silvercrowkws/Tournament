using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Child_Character : MonoBehaviour
{
    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    /// <summary>
    /// 적 플레이어
    /// </summary>
    EnemyPlayer enemyPlayer;

    private void Awake()
    {
        
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        enemyPlayer = GameManager.Instance.EnemyPlayer;
    }

    public void AttackEnd()
    {
        // 만약 부모가 있고
        if(transform.parent != null)
        {
            if(transform.parent.gameObject == player)               // 부모가 플레이어인 경우
            {
                // 내 공격이 끝났다고 적에게 알림
                player.playerAttackEnd = true;
            }
            else if(transform.parent.gameObject == enemyPlayer)     // 부모가 적 플레이어인 경우
            {
                // 적의 공격이 끝났다고 나에게 알림
                enemyPlayer.enemyAttackEnd = true;
            }
        }
        Debug.Log("공격 끝");
    }
}
