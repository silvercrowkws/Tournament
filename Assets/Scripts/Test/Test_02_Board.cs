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
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        //player.transform.position = board.line_0_Section[0].transform.position;
        player.transform.position = board.player1_Section[0].transform.position;
    }

#endif
}
