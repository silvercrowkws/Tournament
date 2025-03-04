using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_06_HP : TestBase
{
#if UNITY_EDITOR

    GameManager gameManager;

    Player player;
    EnemyPlayer enemyPlayer;

    Board board;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
        enemyPlayer = gameManager.EnemyPlayer;
        board = FindAnyObjectByType<Board>();

        player.currentSection += AAA;
    }

    private void AAA(int section)
    {
        Debug.Log($"플레이어의 위치 정보 받아옴 {section} 에 있음");
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Debug.Log("적 HP 깎음");
        enemyPlayer.HP -= 10;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Debug.Log("플레이어 HP 깎음");
        player.HP -= 10;
    }

#endif
}