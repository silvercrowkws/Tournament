using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Test_02_Board : TestBase
{
#if UNITY_EDITOR    

    GameManager gameManager;

    Player player;

    Board board;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
        board = FindAnyObjectByType<Board>();

        player.currentSection += AAA;
    }

    private void AAA(int section)
    {
        Debug.Log($"플레이어의 위치 정보 받아옴 {section} 에 있음");
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        //player.transform.position = board.line_0_Section[0].transform.position;
        //player.transform.position = board.player1_Section[0].transform.position;
        Debug.Log("위로 움직임");
        player.selectedMove = PlayerMove.Up;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Debug.Log("아래로 움직임");
        player.selectedMove = PlayerMove.Down;
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        Debug.Log("왼쪽으로 움직임");
        player.selectedMove = PlayerMove.Left;
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        Debug.Log("오른쪽으로 움직임");
        player.selectedMove = PlayerMove.Right;
    }

#endif
}
