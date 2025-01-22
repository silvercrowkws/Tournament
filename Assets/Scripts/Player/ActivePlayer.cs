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


    /// <summary>
    /// 플레이어를 카드에 맞게 행동시키는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnPlayerActive()
    {
        // 이제 n초 기다리는게 아니라 애니메이션 끝날때? 정도로 바꿔야 됨

        Debug.Log("OnPlayerActive 함수");
        Debug.Log($"플레이어의 1번째 카드 인덱스 : {controlZone.firstTurnCardIndex}");
        Debug.Log($"플레이어의 2번째 카드 인덱스 : {controlZone.secondTurnCardIndex}");
        Debug.Log($"플레이어의 3번째 카드 인덱스 : {controlZone.thirdTurnCardIndex}");


        // 첫번째 행동
        player.playerActiveEnd = false;                     // 플레이어가 행동중임을 표시
        ActiveCard(controlZone.firstTurnCardIndex);         // 첫 번째 카드 행동 실행
        yield return StartCoroutine(WaitForPlayerAction()); // 행동 완료 대기
        Debug.Log("첫 번째 행동 완료");
        yield return new WaitForSeconds(1);                 // 1초 대기

        // 두번째 행동
        player.playerActiveEnd = false;
        ActiveCard(controlZone.secondTurnCardIndex);
        yield return StartCoroutine(WaitForPlayerAction());
        Debug.Log("두 번째 행동 완료");
        yield return new WaitForSeconds(1);                 // 1초 대기

        // 세번째 행동
        player.playerActiveEnd = false;
        ActiveCard(controlZone.thirdTurnCardIndex);
        yield return StartCoroutine(WaitForPlayerAction());
        Debug.Log("세 번째 행동 완료");
        
    }

    /// <summary>
    /// 카드의 인덱스에 따라 행동을 실행하는 함수
    /// </summary>
    /// <param name="cardIndex">실행시킬 행동(카드)의 인덱스</param>
    private void ActiveCard(int cardIndex)
    {
        switch (cardIndex)
        {
            case 0:
                Debug.Log("아래로 움직임");
                player.selectedMove = PlayerMove.Down;
                break;
            case 1:
                Debug.Log("위로 움직임");
                player.selectedMove = PlayerMove.Up;
                break;
            case 2:
                Debug.Log("오른쪽으로 움직임");
                player.selectedMove = PlayerMove.Right;
                break;
            case 3:
                Debug.Log("왼쪽으로 움직임");
                player.selectedMove = PlayerMove.Left;
                break;
            case 4:
                Debug.Log("가드");
                player.selectedProtect = PlayerProtect.Guard;
                break;
            case 5:
                Debug.Log("Attack");
                player.selectedAttack = PlayerAttack.Attack;
                break;
            case 6:
                Debug.Log("MagicAttack");
                player.selectedAttack = PlayerAttack.MagicAttack;
                break;
            case 7:
                Debug.Log("LimitAttack");
                player.selectedAttack = PlayerAttack.LimitAttack;
                break;
            case 8:
                Debug.Log("에너지 업");
                player.selectedProtect = PlayerProtect.EnergyUp;
                break;
            case 9:
                Debug.Log("더블 오른쪽 움직임");
                player.selectedMove = PlayerMove.DoubleRight;
                break;
            case 10:
                Debug.Log("더블 왼쪽 움직임");
                player.selectedMove = PlayerMove.DoubleLeft;
                break;
            case 11:
                Debug.Log("퍼펙트 가드");
                player.selectedProtect = PlayerProtect.PerfectGuard;
                break;
            case 12:
                Debug.Log("힐");
                player.selectedProtect = PlayerProtect.Heal;
                break;
        }
    }

    /// <summary>
    /// 플레이어의 행동이 끝날 때까지 기다리는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForPlayerAction()
    {
        Debug.Log("대기 코루틴 시작");
        while (!player.playerActiveEnd)
        {
            yield return null;
        }
        Debug.Log("대기 코루틴 끝");
    }

    private void PlayerSction(int section)
    {
        Debug.Log($"플레이어의 위치 정보 받아옴 {section} 에 있음");
    }
}
