using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRotation2D : MonoBehaviour
{
    public Transform player;         // 플레이어의 Transform
    public Transform enemyPlayer;    // 적 플레이어의 Transform

    private void Update()
    {
        // 플레이어와 적의 상대적인 X 위치를 계산
        float directionToPlayerX = player.position.x - enemyPlayer.position.x;

        // 적 플레이어가 플레이어를 바라보도록 회전 (Y축 기준으로 0도 또는 180도)
        if (directionToPlayerX > 0)
        {
            // 플레이어가 적의 오른쪽에 있으면 0도 (정면)
            enemyPlayer.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (directionToPlayerX < 0)
        {
            // 플레이어가 적의 왼쪽에 있으면 180도 (뒤로)
            enemyPlayer.rotation = Quaternion.Euler(0, 180, 0);
        }

        // 플레이어도 적을 바라보게 하는 부분 (선택적으로)
        float directionToEnemyX = enemyPlayer.position.x - player.position.x;

        if (directionToEnemyX > 0)
        {
            // 적이 플레이어의 오른쪽에 있으면 0도 (정면)
            player.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (directionToEnemyX < 0)
        {
            // 적이 플레이어의 왼쪽에 있으면 180도 (뒤로)
            player.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
