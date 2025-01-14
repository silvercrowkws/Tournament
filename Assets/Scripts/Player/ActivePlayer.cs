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

    private void Start()
    {
        gameManager = GameManager.Instance;
        turnManager = TurnManager.Instance;
        player = gameManager.Player;
        controlZone = FindAnyObjectByType<ControlZone>();

        turnManager.onTurnStart += OnPlayerActive;

        player.currentSection += PlayerSction;
    }

    /// <summary>
    /// 플레이어를 움직이는 함수
    /// </summary>
    /// <param name="turnNumber">턴 숫자</param>
    private void OnPlayerActive(int turnNumber)
    {
        Debug.Log("OnPlayerActive 함수");

        switch (controlZone.firstTurnCardIndex)
        {
            case 0:
                player.selectedMove = PlayerMove.Down;
                break;
            case 1:
                player.selectedMove = PlayerMove.Up;
                break;
            case 2:
                player.selectedMove = PlayerMove.Right;
                break;
            case 3:
                player.selectedMove = PlayerMove.Left;
                break;
        }

        StartCoroutine(Delay());

        switch (controlZone.secondTurnCardIndex)
        {
            case 0:
                player.selectedMove = PlayerMove.Down;
                break;
            case 1:
                player.selectedMove = PlayerMove.Up;
                break;
            case 2:
                player.selectedMove = PlayerMove.Right;
                break;
            case 3:
                player.selectedMove = PlayerMove.Left;
                break;
        }

        StartCoroutine(Delay());

        switch (controlZone.thirdTurnCardIndex)
        {
            case 0:
                player.selectedMove = PlayerMove.Down;
                break;
            case 1:
                player.selectedMove = PlayerMove.Up;
                break;
            case 2:
                player.selectedMove = PlayerMove.Right;
                break;
            case 3:
                player.selectedMove = PlayerMove.Left;
                break;
        }
    }

    private void PlayerSction(int section)
    {
        Debug.Log($"플레이어의 위치 정보 받아옴 {section} 에 있음");
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(3.0f);
    }
}
