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

    /// <summary>
    /// 애니메이션 이벤트로 실행되는 함수
    /// </summary>
    public void ActiveEnd()
    {
        // 만약 부모가 있고
        if(transform.parent != null)
        {
            if(transform.parent.gameObject == player.gameObject)               // 부모가 플레이어인 경우
            {
                // 내 행동이 끝났다고 적에게 알림
                player.playerActiveEnd = true;
                //Debug.Log($"player.playerActiveEnd : {player.playerActiveEnd}");
            }
            else if(transform.parent.gameObject == enemyPlayer.gameObject)     // 부모가 적 플레이어인 경우
            {
                // 적의 행동이 끝났다고 나에게 알림
                enemyPlayer.enemyActiveEnd = true;
                //Debug.Log($"enemyPlayer.enemyActiveEnd : {enemyPlayer.enemyActiveEnd}");
            }
        }
        Debug.Log("행동 끝");
    }
}
