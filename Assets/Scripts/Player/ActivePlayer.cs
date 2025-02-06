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

    // 적의 행동 시 들어가는 코스트
    int attackCost = 0;
    int magicAttackCost = 0;
    int limitAttackCost = 0;
    int perfectGuardCost = 0;

    // 적의 행동들
    public int EfirstTurnCardIndex = 0;
    public int EsecondTurnCardIndex = 0;
    public int EthirdTurnCardIndex = 0;

    private void Start()
    {
        gameManager = GameManager.Instance;
        turnManager = TurnManager.Instance;
        player = gameManager.Player;
        enemyPlayer = gameManager.EnemyPlayer;
        controlZone = FindAnyObjectByType<ControlZone>();
        board = FindAnyObjectByType<Board>();

        // 적 캐릭터에 따라
        switch (gameManager.enemyPlayerCharacterIndex)
        {
            case 0:
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 40;
                break;
            case 1:
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;
                break;
            case 2:
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;
                break;
            case 3:
                attackCost = 25;
                magicAttackCost = 25;
                limitAttackCost = 40;
                break;
            case 4:
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;
                break;
            case 5:
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 40;
                break;
            case 6:
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 40;
                break;
            case 7:
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;
                break;
            case 8:
                attackCost = 25;
                magicAttackCost = 25;
                limitAttackCost = 45;
                break;
            case 9:
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 40;
                break;
        }

        //turnManager.onTurnStart += OnPlayerActive;
        turnManager.onTurnStart += (_) => OnPlay();

        player.currentSection += PlayerSction;
        enemyPlayer.currentSection += EnemyPlayerSction;
    }

    void OnPlay()
    {
        StartCoroutine(OnPlayerActive());
    }


    /// <summary>
    /// 플레이어와 적 플레이어를 카드에 맞게 행동시키는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnPlayerActive()
    {
        Debug.Log("OnPlayerActive 함수");
        Debug.Log($"플레이어의 1번째 카드 인덱스 : {controlZone.firstTurnCardIndex}");
        Debug.Log($"플레이어의 2번째 카드 인덱스 : {controlZone.secondTurnCardIndex}");
        Debug.Log($"플레이어의 3번째 카드 인덱스 : {controlZone.thirdTurnCardIndex}");

        // 게임 난이도에 따라
        if (gameManager.gamediff == GameDifficulty.Hard)
        {
            Debug.Log("하드모드 들어옴");
            HardMode();
        }
        else
        {
            // NormalMode();
        }

        int activeNumber = 0;                               // 몇번째 행동인지 확인하는 변수
        int enemyActiveNumver = 0;                          // 적의 몇번째 행동인지 확인하는 변수

        // 턴 시작시 턴매니저에서 플레이어가 턴 중임을 표시 isPlayerDone = false;
        yield return new WaitForSeconds(1);                 // 1초 대기

        // 첫번째 행동
        player.playerActiveEnd = false;                     // 플레이어가 행동 중임을 표시
        enemyPlayer.enemyActiveEnd = false;                 // 적 플레이어가 행동 중임을 표시
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기

        // 우선순위
        // 1. 움직임
        // 2. 수비
        // 3. 공격

        // 만약 플레이어의 첫번째 행동이 움직임이면
        if(controlZone.firstTurnCardIndex == 0 || controlZone.firstTurnCardIndex == 1 || controlZone.firstTurnCardIndex == 2 || controlZone.firstTurnCardIndex == 3 ||
            controlZone.firstTurnCardIndex == 9 || controlZone.firstTurnCardIndex == 10)
        {
            // 플레이어 먼저 행동
            PlayerActiveCard(controlZone.firstTurnCardIndex);       // 플레이어의 첫번째 카드 행동 실행
            yield return StartCoroutine(WaitForPlayerAction());     // 플레이어의 행동이 끝날때까지 기다림
            yield return StartCoroutine(WaitForSecond(1.0f));       // 플레이어 행동이 끝나고 잠시 기다림
            EnemyActiveCard(EfirstTurnCardIndex);                   // 적의 첫번째 행동 실행
        }
        // 플레이어의 첫번째 행동이 공격이면
        else if(controlZone.firstTurnCardIndex == 5 || controlZone.firstTurnCardIndex == 6 || controlZone.firstTurnCardIndex == 7)
        {
            // 적의 행동이 움직임 or 수비이면 => 공격이 아니면
            // 근데 수비는 다른 캐릭터의 공격이 끝나야 엔드일텐데..?
            if (EfirstTurnCardIndex != 5 && EfirstTurnCardIndex != 6 && EfirstTurnCardIndex != 7)
            {
                // 적이 먼저 행동
                EnemyActiveCard(EfirstTurnCardIndex);                           // 적이 먼저 행동 후(수비인 경우) 0.5초 후에 공격을 실행하면 공격이 끝날때 수비도 끝남
                yield return StartCoroutine(WaitForSecond(1.0f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.firstTurnCardIndex);               // 적의 행동이 끝나고 플레이어의 행동 시작
            }
            else
            {
                // 여기로 들어오면 플레이어 & 적의 행동이 공격인 경우
                PlayerActiveCard(controlZone.firstTurnCardIndex);               // 플레이어의 공격을 먼저 하고
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 기다림
                yield return StartCoroutine(WaitForSecond(1.0f));               // 플레이어 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EfirstTurnCardIndex);                           // 적의 행동 실행
            }
        }
        // 플레이어의 행동이 움직임X, 공격X => 수비이다
        else
        {
            // 적의 행동이 움직임이면 먼저 행동
            if(EfirstTurnCardIndex == 0 || EfirstTurnCardIndex == 1 || EfirstTurnCardIndex == 2 || EfirstTurnCardIndex == 3 ||
                EfirstTurnCardIndex == 9 || EfirstTurnCardIndex == 10)
            {
                EnemyActiveCard(EfirstTurnCardIndex);
                yield return StartCoroutine(WaitForEnemyPlayerAction());        // 적의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.0f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.firstTurnCardIndex);               // 플레이어의 행동 실행
            }
            // 적의 행동이 공격이면 플레이어 먼저 행동
            else if(EfirstTurnCardIndex == 5 || EfirstTurnCardIndex == 6 || EfirstTurnCardIndex == 7)
            {
                PlayerActiveCard(controlZone.firstTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.0f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EfirstTurnCardIndex);                           // 적의 행동 실행
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.firstTurnCardIndex);
                EnemyActiveCard(EfirstTurnCardIndex);
                yield return StartCoroutine(WaitForSecond(1.0f));               // 모두의 행동을 잠시 지속
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("첫 번째 행동 완료");
        yield return new WaitForSeconds(2);                 // 2초 대기
        

        // 두번째 행동
        activeNumber++;
        player.playerActiveEnd = false;
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기

        // 만약 플레이어의 두번째 행동이 움직임이면
        if (controlZone.secondTurnCardIndex == 0 || controlZone.secondTurnCardIndex == 1 || controlZone.secondTurnCardIndex == 2 || controlZone.secondTurnCardIndex == 3 ||
            controlZone.secondTurnCardIndex == 9 || controlZone.secondTurnCardIndex == 10)
        {
            // 플레이어 먼저 행동
            PlayerActiveCard(controlZone.secondTurnCardIndex);      // 플레이어의 두번째 카드 행동 실행
            yield return StartCoroutine(WaitForPlayerAction());     // 플레이어의 행동이 끝날때까지 기다림
            yield return StartCoroutine(WaitForSecond(1.0f));       // 플레이어 행동이 끝나고 잠시 기다림
            EnemyActiveCard(EsecondTurnCardIndex);                  // 적의 두번째 행동 실행
        }
        // 플레이어의 두번째 행동이 공격이면
        else if (controlZone.secondTurnCardIndex == 5 || controlZone.secondTurnCardIndex == 6 || controlZone.secondTurnCardIndex == 7)
        {
            // 적의 행동이 움직임 or 수비이면 => 공격이 아니면
            // 근데 수비는 다른 캐릭터의 공격이 끝나야 엔드일텐데..?
            if (EsecondTurnCardIndex != 5 && EsecondTurnCardIndex != 6 && EsecondTurnCardIndex != 7)
            {
                // 적이 먼저 행동
                EnemyActiveCard(EsecondTurnCardIndex);                          // 적이 먼저 행동 후(수비인 경우) 0.5초 후에 공격을 실행하면 공격이 끝날때 수비도 끝남
                yield return StartCoroutine(WaitForSecond(1.0f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.secondTurnCardIndex);              // 적의 행동이 끝나고 플레이어의 행동 시작
            }
            else
            {
                // 여기로 들어오면 플레이어 & 적의 행동이 공격인 경우
                PlayerActiveCard(controlZone.secondTurnCardIndex);              // 플레이어의 공격을 먼저 하고
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 기다림
                yield return StartCoroutine(WaitForSecond(1.0f));               // 플레이어 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EsecondTurnCardIndex);                          // 적의 행동 실행
            }
        }
        // 플레이어의 행동이 움직임X, 공격X => 수비이다
        else
        {
            // 적의 행동이 움직임이면 먼저 행동
            if (EsecondTurnCardIndex == 0 || EsecondTurnCardIndex == 1 || EsecondTurnCardIndex == 2 || EsecondTurnCardIndex == 3 ||
                EsecondTurnCardIndex == 9 || EsecondTurnCardIndex == 10)
            {
                EnemyActiveCard(EsecondTurnCardIndex);
                yield return StartCoroutine(WaitForEnemyPlayerAction());        // 적의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.0f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.secondTurnCardIndex);              // 플레이어의 행동 실행
            }
            // 적의 행동이 공격이면 플레이어 먼저 행동
            else if (EsecondTurnCardIndex == 5 || EsecondTurnCardIndex == 6 || EsecondTurnCardIndex == 7)
            {
                PlayerActiveCard(controlZone.secondTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.0f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EsecondTurnCardIndex);                           // 적의 행동 실행
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.secondTurnCardIndex);
                EnemyActiveCard(EsecondTurnCardIndex);
                yield return StartCoroutine(WaitForSecond(1.0f));               // 모두의 행동을 잠시 지속
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("두 번째 행동 완료");
        yield return new WaitForSeconds(2);                 // 2초 대기



        // 세번째 행동
        activeNumber++;
        player.playerActiveEnd = false;
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기


        // 만약 플레이어의 세번째 행동이 움직임이면
        if (controlZone.thirdTurnCardIndex == 0 || controlZone.thirdTurnCardIndex == 1 || controlZone.thirdTurnCardIndex == 2 || controlZone.thirdTurnCardIndex == 3 ||
            controlZone.thirdTurnCardIndex == 9 || controlZone.thirdTurnCardIndex == 10)
        {
            // 플레이어 먼저 행동
            PlayerActiveCard(controlZone.thirdTurnCardIndex);       // 플레이어의 세번째 카드 행동 실행
            yield return StartCoroutine(WaitForPlayerAction());     // 플레이어의 행동이 끝날때까지 기다림
            yield return StartCoroutine(WaitForSecond(1.0f));       // 플레이어 행동이 끝나고 잠시 기다림
            EnemyActiveCard(EthirdTurnCardIndex);                   // 적의 세번째 행동 실행
        }
        // 플레이어의 세번째 행동이 공격이면
        else if (controlZone.thirdTurnCardIndex == 5 || controlZone.thirdTurnCardIndex == 6 || controlZone.thirdTurnCardIndex == 7)
        {
            // 적의 행동이 움직임 or 수비이면 => 공격이 아니면
            // 근데 수비는 다른 캐릭터의 공격이 끝나야 엔드일텐데..?
            if (EthirdTurnCardIndex != 5 && EthirdTurnCardIndex != 6 && EthirdTurnCardIndex != 7)
            {
                // 적이 먼저 행동
                EnemyActiveCard(EthirdTurnCardIndex);                           // 적이 먼저 행동 후(수비인 경우) 0.5초 후에 공격을 실행하면 공격이 끝날때 수비도 끝남
                yield return StartCoroutine(WaitForSecond(1.0f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.thirdTurnCardIndex);               // 적의 행동이 끝나고 플레이어의 행동 시작
            }
            else
            {
                // 여기로 들어오면 플레이어 & 적의 행동이 공격인 경우
                PlayerActiveCard(controlZone.thirdTurnCardIndex);               // 플레이어의 공격을 먼저 하고
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 기다림
                yield return StartCoroutine(WaitForSecond(1.0f));               // 플레이어 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EthirdTurnCardIndex);                           // 적의 행동 실행
            }
        }
        // 플레이어의 행동이 움직임X, 공격X => 수비이다
        else
        {
            // 적의 행동이 움직임이면 먼저 행동
            if (EthirdTurnCardIndex == 0 || EthirdTurnCardIndex == 1 || EthirdTurnCardIndex == 2 || EthirdTurnCardIndex == 3 ||
                EthirdTurnCardIndex == 9 || EthirdTurnCardIndex == 10)
            {
                EnemyActiveCard(EthirdTurnCardIndex);
                yield return StartCoroutine(WaitForEnemyPlayerAction());        // 적의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.0f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.thirdTurnCardIndex);               // 플레이어의 행동 실행
            }
            // 적의 행동이 공격이면 플레이어 먼저 행동
            else if (EthirdTurnCardIndex == 5 || EthirdTurnCardIndex == 6 || EthirdTurnCardIndex == 7)
            {
                PlayerActiveCard(controlZone.thirdTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.0f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EthirdTurnCardIndex);                           // 적의 행동 실행
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.thirdTurnCardIndex);
                EnemyActiveCard(EthirdTurnCardIndex);
                yield return StartCoroutine(WaitForSecond(1.0f));               // 모두의 행동을 잠시 지속
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("첫 번째 행동 완료");
        yield return new WaitForSeconds(2);                 // 2초 대기

        gameManager.isPlayerDone = true;                    // 행동을 완료했다고 표시
        gameManager.isEnemyPlayerDone = true;               // 행동을 완료했다고 표시
    }

    /// <summary>
    /// 카드의 인덱스에 따라 행동을 실행하는 함수
    /// </summary>
    /// <param name="cardIndex">실행시킬 행동(카드)의 인덱스</param>
    private void PlayerActiveCard(int cardIndex)
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
    /// 적이 카드의 인덱스에 따라 행동을 실행하는 함수
    /// </summary>
    /// <param name="cardIndex">실행시킬 행동(카드)의 인덱스</param>
    private void EnemyActiveCard(int cardIndex)
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
        Debug.Log("대기 코루틴 시작");
        while (!player.playerActiveEnd)
        {
            yield return null;
        }
        Debug.Log("대기 코루틴 끝");
    }

    /// <summary>
    /// 적의 행동이 끝날 때까지 기다리는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForEnemyPlayerAction()
    {
        Debug.Log("적 행동 끝을 기다리는 코루틴");
        while (!enemyPlayer.enemyActiveEnd)
        {
            yield return null;
        }
        Debug.Log("적 행동 끝을 기다리는 코루틴 끝");
    }

    /// <summary>
    /// 잠시 기다리는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForSecond(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    private void PlayerSction(int section)
    {
        Debug.Log($"플레이어의 위치 정보 받아옴 {section} 에 있음");
    }

    private void EnemyPlayerSction(int section)
    {
        Debug.Log($"적 플레이어의 위치 정보 받아옴 {section} 에 있음");
    }

    /// <summary>
    /// 하드 모드로 플레이 할 경우의 적 플레이어의 행동
    /// </summary>
    private void HardMode()
    {
        Debug.Log("HardMode 함수 실행");
        // 하드 모드일 경우
        EfirstTurnCardIndex = 0;
        EsecondTurnCardIndex = 0;
        EthirdTurnCardIndex = 0;

        // 첫번째 턴 카드 시작 ----------------------------------------------------------------------------------------------------
        // 만약 플레이어가 기본 공격 or 마법 공격 or 특수 공격을 했고
        if (controlZone.firstTurnCardIndex == 5 || controlZone.firstTurnCardIndex == 6 || controlZone.firstTurnCardIndex == 7)
        {
            // 플레이어의 공격 범위에 적이 있다면
            if (player.attackRange != null && Array.Exists(player.attackRange, element => element == enemyPlayer.EcurrentSectionIndex) ||
                player.magicAttackRange != null && Array.Exists(player.magicAttackRange, element => element == enemyPlayer.EcurrentSectionIndex) ||
                player.limitAttackRange != null && Array.Exists(player.limitAttackRange, element => element == enemyPlayer.EcurrentSectionIndex))
            {
                Debug.Log("ActiveEnemyPlayer 적이 공격 범위에 있음");
                if (UnityEngine.Random.value < 0.6f)                        // 60% 확률로 움직임
                {
                    Debug.Log("ActiveEnemyPlayer 움직이기로 함");
                    int randomCard = UnityEngine.Random.Range(0, 4);        // 0 1 2 3 중 1개 선택
                    EfirstTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
                }
                else
                {
                    // 움직이지 않기로 했으면
                    Debug.Log("ActiveEnemyPlayer 움직이지 않기로 함");
                    int[] numbers = { 4, 5, 6, 7, 11 };
                    // 1. 가드를 하거나
                    // 2. 공격을 하거나
                    int randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                    int randomCard = numbers[randomIndex];                              // 실제 값 선택
                    EfirstTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.Log("ActiveEnemyPlayer 플레이어가 공격했으나 적이 공격 범위에 없음");
                int randomCard = UnityEngine.Random.Range(0, 4);        // 0 1 2 3 중 1개 선택
                EfirstTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
            }
        }
        // 플레이어가 움직이면
        else if (controlZone.firstTurnCardIndex == 0 || controlZone.firstTurnCardIndex == 1 || controlZone.firstTurnCardIndex == 2 || controlZone.firstTurnCardIndex == 3 ||
            controlZone.firstTurnCardIndex == 9 || controlZone.firstTurnCardIndex == 10)
        {
            // 적의 공격 범위에 플레이어가 있다면
            if (enemyPlayer.attackRange != null && Array.Exists(enemyPlayer.attackRange, element => element == player.currentSectionIndex) ||
                enemyPlayer.magicAttackRange != null && Array.Exists(enemyPlayer.magicAttackRange, element => element == player.currentSectionIndex) ||
                enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.currentSectionIndex))
            {
                Debug.Log("ActiveEnemyPlayer 플레이어가 움직였는데 적의 공격 범위에 포함됨");
                // 플레이어가 움직였을 때 플레이어가 적의 공격 범위에 있으면
                if (UnityEngine.Random.value < 0.5f)                         // 50% 확률로 공격
                {
                    Debug.Log("ActiveEnemyPlayer 공격하기로 함");
                    int randomCard = UnityEngine.Random.Range(5, 8);        // 5 6 7 중 1개 선택
                    EfirstTurnCardIndex = randomCard;
                }
                else
                {
                    // 공격을 안하기로 했으면 나도 움직임
                    Debug.Log("ActiveEnemyPlayer 공격 안하기로 하고 나도 움직임");
                    int[] numbers = { 0, 1, 2, 3, 9, 10 };
                    int randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                    int randomCard = numbers[randomIndex];                              // 실제 값 선택
                    EfirstTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 움직였으나 플레이어가 적의 공격 범위에 없음
            else
            {
                Debug.Log("ActiveEnemyPlayer 플레이어가 움직였으나 플레이어가 적의 공격 범위에 없음");
                int randomCard = UnityEngine.Random.Range(0, 4);        // 0 1 2 3 중 1개 선택
                EfirstTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("ActiveEnemyPlayer 플레이어가 공격, 이동을 안해서 진짜 랜덤");
            int randomCard = UnityEngine.Random.Range(0, 14);
            EfirstTurnCardIndex = randomCard;
        }

        // 첫번째 턴 카드 끝 ----------------------------------------------------------------------------------------------------

        // 두번째 턴 카드 시작 ----------------------------------------------------------------------------------------------------
        // 만약 플레이어가 기본 공격 or 마법 공격 or 특수 공격을 했고
        if (controlZone.secondTurnCardIndex == 5 || controlZone.secondTurnCardIndex == 6 || controlZone.secondTurnCardIndex == 7)
        {
            // 플레이어의 공격 범위에 적이 있다면
            if (player.attackRange != null && Array.Exists(player.attackRange, element => element == enemyPlayer.EcurrentSectionIndex) ||
                player.magicAttackRange != null && Array.Exists(player.magicAttackRange, element => element == enemyPlayer.EcurrentSectionIndex) ||
                player.limitAttackRange != null && Array.Exists(player.limitAttackRange, element => element == enemyPlayer.EcurrentSectionIndex))
            {
                Debug.Log("ActiveEnemyPlayer 적이 공격 범위에 있음");
                if (UnityEngine.Random.value < 0.6f)                        // 60% 확률로 움직임
                {
                    Debug.Log("ActiveEnemyPlayer 움직이기로 함");
                    int randomCard;
                    do
                    {
                        randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                    }
                    while (randomCard == EfirstTurnCardIndex);              // 같은 값이면 다시 뽑기

                    EsecondTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
                }
                else
                {
                    // 움직이지 않기로 했으면
                    Debug.Log("ActiveEnemyPlayer 움직이지 않기로 함");
                    int[] numbers = { 4, 5, 6, 7, 11 };
                    // 1. 가드를 하거나
                    // 2. 공격을 하거나
                    int randomIndex;
                    int randomCard;
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                        randomCard = numbers[randomIndex];                              // 실제 값 선택
                    }
                    while (randomCard == EfirstTurnCardIndex);

                    EsecondTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.Log("ActiveEnemyPlayer 플레이어가 공격했으나 적이 공격 범위에 없음");
                int randomCard;
                do
                {
                    randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                }
                while (randomCard == EfirstTurnCardIndex);              // 같은 값이면 다시 뽑기

                EsecondTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
            }
        }
        // 플레이어가 움직이면
        else if (controlZone.secondTurnCardIndex == 0 || controlZone.secondTurnCardIndex == 1 || controlZone.secondTurnCardIndex == 2 || controlZone.secondTurnCardIndex == 3 ||
            controlZone.secondTurnCardIndex == 9 || controlZone.secondTurnCardIndex == 10)
        {
            // 적의 공격 범위에 플레이어가 있다면
            if (enemyPlayer.attackRange != null && Array.Exists(enemyPlayer.attackRange, element => element == player.currentSectionIndex) ||
                enemyPlayer.magicAttackRange != null && Array.Exists(enemyPlayer.magicAttackRange, element => element == player.currentSectionIndex) ||
                enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.currentSectionIndex))
            {
                // 플레이어가 움직였을 때 플레이어가 적의 공격 범위에 있으면
                Debug.Log("ActiveEnemyPlayer 플레이어가 움직였는데 적의 공격 범위에 있음");
                if (UnityEngine.Random.value < 0.5f)                         // 50% 확률로 공격
                {
                    Debug.Log("ActiveEnemyPlayer 공격하기로 함");
                    int randomCard;
                    do
                    {
                        randomCard = UnityEngine.Random.Range(5, 8);        // 5 6 7 중 1개 선택
                    }
                    while (randomCard == EfirstTurnCardIndex);

                    EsecondTurnCardIndex = randomCard;
                }
                else
                {
                    // 공격을 안하기로 했으면 나도 움직임
                    Debug.Log("ActiveEnemyPlayer 공격 안하기로 하고 나도 움직임");
                    int[] numbers = { 0, 1, 2, 3, 9, 10 };

                    int randomIndex;
                    int randomCard;
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                        randomCard = numbers[randomIndex];                              // 실제 값 선택
                    }
                    while (randomCard == EfirstTurnCardIndex);
                    EsecondTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 움직였으나 플레이어가 적의 공격 범위에 없음
            else
            {
                Debug.Log("ActiveEnemyPlayer 플레이어가 움직였으나 플레이어가 적의 공격 범위에 없음");
                int randomCard;
                do
                {
                    randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                }
                while (randomCard == EfirstTurnCardIndex);              // 같은 값이면 다시 뽑기

                EsecondTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("ActiveEnemyPlayer 플레이어가 공격, 이동을 안해서 진짜 랜덤");
            int randomCard;
            do
            {
                randomCard = UnityEngine.Random.Range(0, 14);
            }
            while (randomCard == EfirstTurnCardIndex);
            EsecondTurnCardIndex = randomCard;
        }

        // 두번째 턴 카드 끝 ----------------------------------------------------------------------------------------------------

        // 세번째 턴 카드 시작 ----------------------------------------------------------------------------------------------------
        // 만약 플레이어가 기본 공격 or 마법 공격 or 특수 공격을 했고
        if (controlZone.secondTurnCardIndex == 5 || controlZone.secondTurnCardIndex == 6 || controlZone.secondTurnCardIndex == 7)
        {
            // 플레이어의 공격 범위에 적이 있다면
            if (player.attackRange != null && Array.Exists(player.attackRange, element => element == enemyPlayer.EcurrentSectionIndex) ||
                player.magicAttackRange != null && Array.Exists(player.magicAttackRange, element => element == enemyPlayer.EcurrentSectionIndex) ||
                player.limitAttackRange != null && Array.Exists(player.limitAttackRange, element => element == enemyPlayer.EcurrentSectionIndex))
            {
                Debug.Log("ActiveEnemyPlayer 적이 공격 범위에 있음");
                if (UnityEngine.Random.value < 0.6f)                        // 60% 확률로 움직임
                {
                    Debug.Log("ActiveEnemyPlayer 움직이기로 함");
                    int randomCard;
                    do
                    {
                        randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                    }
                    while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);              // 같은 값이면 다시 뽑기

                    EthirdTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
                }
                else
                {
                    // 움직이지 않기로 했으면
                    Debug.Log("ActiveEnemyPlayer 움직이지 않기로 함");
                    int[] numbers = { 4, 5, 6, 7, 11 };
                    // 1. 가드를 하거나
                    // 2. 공격을 하거나
                    int randomIndex;
                    int randomCard;
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                        randomCard = numbers[randomIndex];                              // 실제 값 선택
                    }
                    while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);

                    EthirdTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.Log("ActiveEnemyPlayer 플레이어가 공격했으나 적이 공격 범위에 없음");
                int randomCard;
                do
                {
                    randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                }
                while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);              // 같은 값이면 다시 뽑기

                EthirdTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
            }
        }
        // 플레이어가 움직이면
        else if (controlZone.secondTurnCardIndex == 0 || controlZone.secondTurnCardIndex == 1 || controlZone.secondTurnCardIndex == 2 || controlZone.secondTurnCardIndex == 3 ||
            controlZone.secondTurnCardIndex == 9 || controlZone.secondTurnCardIndex == 10)
        {
            // 적의 공격 범위에 플레이어가 있다면
            if (enemyPlayer.attackRange != null && Array.Exists(enemyPlayer.attackRange, element => element == player.currentSectionIndex) ||
                enemyPlayer.magicAttackRange != null && Array.Exists(enemyPlayer.magicAttackRange, element => element == player.currentSectionIndex) ||
                enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.currentSectionIndex))
            {
                // 플레이어가 움직였을 때 플레이어가 적의 공격 범위에 있으면
                Debug.Log("ActiveEnemyPlayer 플레이어가 움직였는데 적의 공격 범위에 있음");
                if (UnityEngine.Random.value < 0.5f)                         // 50% 확률로 공격
                {
                    Debug.Log("ActiveEnemyPlayer 공격하기로 함");
                    int randomCard;
                    do
                    {
                        randomCard = UnityEngine.Random.Range(5, 8);        // 5 6 7 중 1개 선택
                    }
                    while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);

                    EthirdTurnCardIndex = randomCard;
                }
                else
                {
                    // 공격을 안하기로 했으면 나도 움직임
                    Debug.Log("ActiveEnemyPlayer 공격 안하기로 하고 나도 움직임");
                    int[] numbers = { 0, 1, 2, 3, 9, 10 };

                    int randomIndex;
                    int randomCard;
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                        randomCard = numbers[randomIndex];                              // 실제 값 선택
                    }
                    while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);
                    EthirdTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 움직였으나 플레이어가 적의 공격 범위에 없음
            else
            {
                Debug.Log("ActiveEnemyPlayer 플레이어가 움직였으나 플레이어가 적의 공격 범위에 없음");
                int randomCard;
                do
                {
                    randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                }
                while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);              // 같은 값이면 다시 뽑기

                EthirdTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("ActiveEnemyPlayer 플레이어가 공격, 이동을 안해서 진짜 랜덤");
            int randomCard;
            do
            {
                randomCard = UnityEngine.Random.Range(0, 14);
            }
            while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);
            EthirdTurnCardIndex = randomCard;
        }

    }


}
