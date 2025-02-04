using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.DefaultInputActions;

public class ActiveEnemyPlayer : MonoBehaviour
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
    /// 적 플레이어
    /// </summary>
    EnemyPlayer enemyPlayer;

    /// <summary>
    /// 보드(플레이어의 위치를 표시하기 위함)
    /// </summary>
    Board board;

    /// <summary>
    /// 행동에 맞는 카드를 보이라고 알리는 델리게이트
    /// </summary>
    public Action<int> onNextCard;

    private void Start()
    {
        gameManager = GameManager.Instance;
        turnManager = TurnManager.Instance;
        enemyPlayer = gameManager.EnemyPlayer;
        controlZone = FindAnyObjectByType<ControlZone>();
        board = FindAnyObjectByType<Board>();

        //turnManager.onTurnStart += OnPlayerActive;
        turnManager.onTurnStart += (_) => OnPlay();

        enemyPlayer.currentSection += PlayerSction;
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
        Debug.Log("적의 OnPlayerActive 함수");
        Debug.Log($"적이 본 플레이어의 1번째 카드 인덱스 : {controlZone.firstTurnCardIndex}");
        Debug.Log($"적이 본 플레이어의 2번째 카드 인덱스 : {controlZone.secondTurnCardIndex}");
        Debug.Log($"적이 본 플레이어의 3번째 카드 인덱스 : {controlZone.thirdTurnCardIndex}");

        int activeNumber = 0;                               // 몇번째 행동인지 확인하는 변수

        // 턴 시작시 턴매니저에서 플레이어가 턴 중임을 표시 isEnemyPlayerDone = false;
        yield return new WaitForSeconds(1);                 // 1초 대기

        // 첫번째 행동
        enemyPlayer.enemyActiveEnd = false;                 // 적 플레이어가 행동 중임을 표시
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기
        ActiveCard(controlZone.firstTurnCardIndex);         // 첫 번째 카드 행동 실행
        yield return StartCoroutine(WaitForPlayerAction()); // 행동 완료 대기
        Debug.Log("적의 첫 번째 행동 완료");
        yield return new WaitForSeconds(2);                 // 2초 대기

        // 두번째 행동
        activeNumber++;
        enemyPlayer.enemyActiveEnd = false;
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기
        ActiveCard(controlZone.secondTurnCardIndex);
        yield return StartCoroutine(WaitForPlayerAction());
        Debug.Log("적의 두 번째 행동 완료");
        yield return new WaitForSeconds(2);                 // 2초 대기

        // 세번째 행동
        activeNumber++;
        enemyPlayer.enemyActiveEnd = false;
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기
        ActiveCard(controlZone.thirdTurnCardIndex);
        yield return StartCoroutine(WaitForPlayerAction());
        Debug.Log("적의 세 번째 행동 완료");

        gameManager.isEnemyPlayerDone = true;               // 적이 행동을 완료했다고 표시
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
                Debug.Log("적이 아래로 움직임");
                enemyPlayer.EselectedMove = EnemyPlayerMove.Down;
                break;
            case 1:
                Debug.Log("적이 위로 움직임");
                enemyPlayer.EselectedMove = EnemyPlayerMove.Up;
                break;
            case 2:
                Debug.Log("적이 오른쪽으로 움직임");
                enemyPlayer.EselectedMove = EnemyPlayerMove.Right;
                break;
            case 3:
                Debug.Log("적이 왼쪽으로 움직임");
                enemyPlayer.EselectedMove = EnemyPlayerMove.Left;
                break;
            case 4:
                Debug.Log("적이 가드");
                enemyPlayer.EselectedProtect = EnemyPlayerProtect.Guard;
                break;
            case 5:
                Debug.Log("적이 Attack");
                enemyPlayer.EselectedAttack = EnemyPlayerAttack.Attack;
                break;
            case 6:
                Debug.Log("적이 MagicAttack");
                enemyPlayer.EselectedAttack = EnemyPlayerAttack.MagicAttack;
                break;
            case 7:
                Debug.Log("적이 LimitAttack");
                enemyPlayer.EselectedAttack = EnemyPlayerAttack.LimitAttack;
                break;
            case 8:
                Debug.Log("적이 에너지 업");
                enemyPlayer.EselectedProtect = EnemyPlayerProtect.EnergyUp;
                break;
            case 9:
                Debug.Log("적이 더블 오른쪽 움직임");
                enemyPlayer.EselectedMove = EnemyPlayerMove.DoubleRight;
                break;
            case 10:
                Debug.Log("적이 더블 왼쪽 움직임");
                enemyPlayer.EselectedMove = EnemyPlayerMove.DoubleLeft;
                break;
            case 11:
                Debug.Log("적이 퍼펙트 가드");
                enemyPlayer.EselectedProtect = EnemyPlayerProtect.PerfectGuard;
                break;
            case 12:
                Debug.Log("적이 힐");
                enemyPlayer.EselectedProtect = EnemyPlayerProtect.Heal;
                break;
        }
    }

    /// <summary>
    /// 플레이어의 행동이 끝날 때까지 기다리는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForPlayerAction()
    {
        Debug.Log("적 대기 코루틴 시작");
        while (!enemyPlayer.enemyActiveEnd)
        {
            yield return null;
        }
        Debug.Log("적 대기 코루틴 끝");
    }

    private void PlayerSction(int section)
    {
        Debug.Log($"적 플레이어의 위치 정보 받아옴 {section} 에 있음");
    }
}
