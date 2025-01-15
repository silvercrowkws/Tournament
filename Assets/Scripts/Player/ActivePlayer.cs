using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivePlayer : MonoBehaviour
{
    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 턴 매니저
    /// </summary>
    TurnManager turnManager;

    /// <summary>
    /// 컨트롤 존 클래스
    /// </summary>
    ControlZone controlZone;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    Board board;

    bool firstMove = false;
    bool secondMove = false;
    bool thirdMove = false;

    private void Start()
    {
        gameManager = GameManager.Instance;
        turnManager = TurnManager.Instance;
        player = gameManager.Player;
        controlZone = FindAnyObjectByType<ControlZone>();
        board = FindAnyObjectByType<Board>();

        //turnManager.onTurnStart += OnPlayerActive;
        turnManager.onTurnStart += (_) => OnPlay();

        player.currentSection += PlayerSction;
    }

    void OnPlay()
    {
        StartCoroutine(OnPlayerActive());
    }


    private IEnumerator OnPlayerActive()
    {
        // 이제 n초 기다리는게 아니라 애니메이션 끝날때? 정도로 바꿔야 됨

        Debug.Log("OnPlayerActive 함수");
        Debug.Log($"플레이어의 1번째 카드 인덱스 : {controlZone.firstTurnCardIndex}");
        Debug.Log($"플레이어의 2번째 카드 인덱스 : {controlZone.secondTurnCardIndex}");
        Debug.Log($"플레이어의 3번째 카드 인덱스 : {controlZone.thirdTurnCardIndex}");

        firstMove = true;

        if (firstMove)
        {
            switch (controlZone.firstTurnCardIndex)
            {
                case 0:
                    Debug.Log("1 아래로 움직임");
                    player.selectedMove = PlayerMove.Down;
                    break;
                case 1:
                    Debug.Log("1 위로 움직임");
                    player.selectedMove = PlayerMove.Up;
                    break;
                case 2:
                    Debug.Log("1 오른쪽으로 움직임");
                    player.selectedMove = PlayerMove.Right;
                    break;
                case 3:
                    Debug.Log("1 왼쪽으로 움직임");
                    player.selectedMove = PlayerMove.Left;
                    break;
            }

            firstMove = false;
            secondMove = true;

            // 첫 번째 턴 후 기다리기
            yield return new WaitForSeconds(3.0f);
        }

        if (secondMove)
        {
            switch (controlZone.secondTurnCardIndex)
            {
                case 0:
                    Debug.Log("2 아래로 움직임");
                    player.selectedMove = PlayerMove.Down;
                    break;
                case 1:
                    Debug.Log("2 위로 움직임");
                    player.selectedMove = PlayerMove.Up;
                    break;
                case 2:
                    Debug.Log("2 오른쪽으로 움직임");
                    player.selectedMove = PlayerMove.Right;
                    break;
                case 3:
                    Debug.Log("2 왼쪽으로 움직임");
                    player.selectedMove = PlayerMove.Left;
                    break;
            }

            secondMove = false;
            thirdMove = true;

            // 두 번째 턴 후 기다리기
            yield return new WaitForSeconds(3.0f);
        }

        if (thirdMove)
        {
            switch (controlZone.thirdTurnCardIndex)
            {
                case 0:
                    Debug.Log("3 아래로 움직임");
                    player.selectedMove = PlayerMove.Down;
                    break;
                case 1:
                    Debug.Log("3 위로 움직임");
                    player.selectedMove = PlayerMove.Up;
                    break;
                case 2:
                    Debug.Log("3 오른쪽으로 움직임");
                    player.selectedMove = PlayerMove.Right;
                    break;
                case 3:
                    Debug.Log("3 왼쪽으로 움직임");
                    player.selectedMove = PlayerMove.Left;
                    break;
            }

            thirdMove = false;
        }
    }

    private void PlayerSction(int section)
    {
        Debug.Log($"플레이어의 위치 정보 받아옴 {section} 에 있음");
    }
}
