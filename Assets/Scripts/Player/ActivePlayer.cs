using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

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

    /// <summary>
    /// 움직일 위치
    /// </summary>
    int playerTargetSection = 0;

    /// <summary>
    /// 적이 움직일 위치
    /// </summary>
    int enemyTargetSection = 0;

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
        yield return StartCoroutine(WaitForSecond(1f));                 // 1초 대기

        // 첫번째 행동
        player.playerActiveEnd = false;                     // 플레이어가 행동 중임을 표시
        enemyPlayer.enemyActiveEnd = false;                 // 적 플레이어가 행동 중임을 표시
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return StartCoroutine(WaitForSecond(1));                 // 1초 대기

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
            yield return StartCoroutine(WaitForSecond(1.5f));       // 플레이어 행동이 끝나고 잠시 기다림
            EnemyActiveCard(EfirstTurnCardIndex);                   // 적의 첫번째 행동 실행
            yield return StartCoroutine(WaitForEnemyPlayerAction());
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
                yield return StartCoroutine(WaitForEnemyPlayerAction());
                yield return StartCoroutine(WaitForSecond(1.5f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.firstTurnCardIndex);               // 적의 행동이 끝나고 플레이어의 행동 시작
                yield return StartCoroutine(WaitForPlayerAction());
            }
            else
            {
                // 여기로 들어오면 플레이어 & 적의 행동이 공격인 경우
                PlayerActiveCard(controlZone.firstTurnCardIndex);               // 플레이어의 공격을 먼저 하고
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 기다림
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EfirstTurnCardIndex);                           // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
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
                yield return StartCoroutine(WaitForSecond(1.5f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.firstTurnCardIndex);               // 플레이어의 행동 실행
                yield return StartCoroutine(WaitForPlayerAction());
            }
            // 적의 행동이 공격이면 플레이어 먼저 행동
            else if(EfirstTurnCardIndex == 5 || EfirstTurnCardIndex == 6 || EfirstTurnCardIndex == 7)
            {
                PlayerActiveCard(controlZone.firstTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EfirstTurnCardIndex);                           // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.firstTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());
                yield return StartCoroutine(WaitForSecond(1.5f));               // 모두의 행동을 잠시 지속
                EnemyActiveCard(EfirstTurnCardIndex);
                yield return StartCoroutine(WaitForEnemyPlayerAction());
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("첫 번째 행동 완료");
        yield return StartCoroutine(WaitForSecond(2));                 // 2초 대기
        

        // 두번째 행동
        activeNumber++;
        player.playerActiveEnd = false;
        enemyPlayer.enemyActiveEnd = false;
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return StartCoroutine(WaitForSecond(1));                 // 1초 대기

        // 만약 플레이어의 두번째 행동이 움직임이면
        if (controlZone.secondTurnCardIndex == 0 || controlZone.secondTurnCardIndex == 1 || controlZone.secondTurnCardIndex == 2 || controlZone.secondTurnCardIndex == 3 ||
            controlZone.secondTurnCardIndex == 9 || controlZone.secondTurnCardIndex == 10)
        {
            // 플레이어 먼저 행동
            PlayerActiveCard(controlZone.secondTurnCardIndex);      // 플레이어의 두번째 카드 행동 실행
            yield return StartCoroutine(WaitForPlayerAction());     // 플레이어의 행동이 끝날때까지 기다림
            yield return StartCoroutine(WaitForSecond(1.5f));       // 플레이어 행동이 끝나고 잠시 기다림
            EnemyActiveCard(EsecondTurnCardIndex);                  // 적의 두번째 행동 실행
            yield return StartCoroutine(WaitForEnemyPlayerAction());
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
                yield return StartCoroutine(WaitForEnemyPlayerAction());
                yield return StartCoroutine(WaitForSecond(1.5f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.secondTurnCardIndex);              // 적의 행동이 끝나고 플레이어의 행동 시작
                yield return StartCoroutine(WaitForPlayerAction());
            }
            else
            {
                // 여기로 들어오면 플레이어 & 적의 행동이 공격인 경우
                PlayerActiveCard(controlZone.secondTurnCardIndex);              // 플레이어의 공격을 먼저 하고
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 기다림
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EsecondTurnCardIndex);                          // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
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
                yield return StartCoroutine(WaitForSecond(1.5f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.secondTurnCardIndex);              // 플레이어의 행동 실행
                yield return StartCoroutine(WaitForPlayerAction());
            }
            // 적의 행동이 공격이면 플레이어 먼저 행동
            else if (EsecondTurnCardIndex == 5 || EsecondTurnCardIndex == 6 || EsecondTurnCardIndex == 7)
            {
                PlayerActiveCard(controlZone.secondTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EsecondTurnCardIndex);                          // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.secondTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());
                yield return StartCoroutine(WaitForSecond(1.5f));               // 모두의 행동을 잠시 지속
                EnemyActiveCard(EsecondTurnCardIndex);
                yield return StartCoroutine(WaitForEnemyPlayerAction());
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("두 번째 행동 완료");
        yield return StartCoroutine(WaitForSecond(2));                 // 2초 대기



        // 세번째 행동
        activeNumber++;
        player.playerActiveEnd = false;
        enemyPlayer.enemyActiveEnd = false;
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return StartCoroutine(WaitForSecond(1));                 // 1초 대기


        // 만약 플레이어의 세번째 행동이 움직임이면
        if (controlZone.thirdTurnCardIndex == 0 || controlZone.thirdTurnCardIndex == 1 || controlZone.thirdTurnCardIndex == 2 || controlZone.thirdTurnCardIndex == 3 ||
            controlZone.thirdTurnCardIndex == 9 || controlZone.thirdTurnCardIndex == 10)
        {
            // 플레이어 먼저 행동
            PlayerActiveCard(controlZone.thirdTurnCardIndex);       // 플레이어의 세번째 카드 행동 실행
            yield return StartCoroutine(WaitForPlayerAction());     // 플레이어의 행동이 끝날때까지 기다림
            yield return StartCoroutine(WaitForSecond(1.5f));       // 플레이어 행동이 끝나고 잠시 기다림
            EnemyActiveCard(EthirdTurnCardIndex);                   // 적의 세번째 행동 실행
            yield return StartCoroutine(WaitForEnemyPlayerAction());
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
                yield return StartCoroutine(WaitForSecond(1.5f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.thirdTurnCardIndex);               // 적의 행동이 끝나고 플레이어의 행동 시작
                yield return StartCoroutine(WaitForPlayerAction());
            }
            else
            {
                // 여기로 들어오면 플레이어 & 적의 행동이 공격인 경우
                PlayerActiveCard(controlZone.thirdTurnCardIndex);               // 플레이어의 공격을 먼저 하고
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 기다림
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EthirdTurnCardIndex);                           // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
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
                yield return StartCoroutine(WaitForSecond(1.5f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.thirdTurnCardIndex);               // 플레이어의 행동 실행
                yield return StartCoroutine(WaitForPlayerAction());
            }
            // 적의 행동이 공격이면 플레이어 먼저 행동
            else if (EthirdTurnCardIndex == 5 || EthirdTurnCardIndex == 6 || EthirdTurnCardIndex == 7)
            {
                PlayerActiveCard(controlZone.thirdTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());             // 플레이어의 행동이 끝날때까지 대기
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EthirdTurnCardIndex);                           // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.thirdTurnCardIndex);
                yield return StartCoroutine(WaitForPlayerAction());
                yield return StartCoroutine(WaitForSecond(1.5f));               // 모두의 행동을 잠시 지속
                EnemyActiveCard(EthirdTurnCardIndex);
                yield return StartCoroutine(WaitForEnemyPlayerAction());
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("세 번째 행동 완료");
        yield return StartCoroutine(WaitForSecond(2));                 // 2초 대기

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

        // 공격 범위에 포함되는지 확인용
        bool inAttackRange = false;
        bool inMagicAttackRange = false;
        bool inLimitAttackRange = false;

        playerTargetSection = player.currentSectionIndex;           // 움직일 위치 초기값을 현재 위치와 동기화
        enemyTargetSection = enemyPlayer.EcurrentSectionIndex;

        // 첫번째 턴 카드 시작 ----------------------------------------------------------------------------------------------------

        // 만약 플레이어가 첫턴에 기본 공격 or 마법 공격 or 특수 공격을 할 예정이면
        if (controlZone.firstTurnCardIndex == 5 || controlZone.firstTurnCardIndex == 6 || controlZone.firstTurnCardIndex == 7)
        {
            // 플레이어의 현재 위치에서 공격 범위 계산
            CharacterAttackRange(gameManager.playerCharacterIndex, playerTargetSection);
            //Debug.LogWarning($"플레이어의 특수 공격 범위 : {string.Join(", ", limitAttackRange)}");

            // 플레이어의 공격 범위에 적이 있으면
            if (attackRange.Contains(enemyPlayer.EcurrentSectionIndex) ||
                magicAttackRange.Contains(enemyPlayer.EcurrentSectionIndex) ||
                limitAttackRange.Contains(enemyPlayer.EcurrentSectionIndex))
            {
                Debug.LogWarning("1턴 플레이어가 공격 할건데, 적이 공격 범위에 있음");
                if (UnityEngine.Random.value < 0.7f)                        // 70% 확률로 움직임
                {
                    Debug.Log("70% 확률로 움직이기로 함");
                    int randomCard = UnityEngine.Random.Range(0, 4);        // 0 1 2 3 중 1개 선택
                    EfirstTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
                    EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                }
                else
                {
                    // 움직이지 않기로 함
                    Debug.LogWarning("30% 확률로 움직이지 않기로 함");
                    int[] numbers = { 4, 5, 6, 7, 11 };
                    int randomCard;
                    int randomIndex;
                    int cardCost = 0;

                    // 적 플레이어의 현재 위치에서 공격 범위 계산
                    CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);
                    // 1. 가드를 하거나 4 11
                    // 2. 공격을 하거나 5 6 7
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                        randomCard = numbers[randomIndex];                              // 실제 값 선택

                        // 선택된 카드에 해당하는 코스트를 확인
                        switch (randomCard)
                        {
                            case 4:
                                // 4번 카드는 가드니까, 코스트를 특정 값으로 설정 (예: 0)
                                cardCost = 0;
                                break;
                            case 5:
                                cardCost = attackCost; // attackCost
                                break;
                            case 6:
                                cardCost = magicAttackCost;  // magicAttackCost
                                break;
                            case 7:
                                cardCost = limitAttackCost; // limitAttackCost
                                break;
                            case 11:
                                cardCost = perfectGuardCost; // perfectGuardCost
                                break;

                        }
                    }
                    while (enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                    EfirstTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.LogWarning("1턴 플레이어가 공격할건데, 적이 공격 범위에 없음");
                Debug.Log("그래서 움직이기로 함");
                int randomCard = UnityEngine.Random.Range(0, 4);        // 0 1 2 3 중 1개 선택
                EfirstTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
                EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
            }
        }
        // 플레이어가 첫턴에 움직일 예정이면
        else if (controlZone.firstTurnCardIndex == 0 || controlZone.firstTurnCardIndex == 1 || controlZone.firstTurnCardIndex == 2 || controlZone.firstTurnCardIndex == 3 ||
            controlZone.firstTurnCardIndex == 9 || controlZone.firstTurnCardIndex == 10)
        {
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.firstTurnCardIndex, playerTargetSection);

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if(attackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                inAttackRange = true;
                availableCards.Add(5);
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                inMagicAttackRange = true;
                availableCards.Add(6);
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                inLimitAttackRange = true;
                availableCards.Add(7);
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;
                int cardCost = 0;

                // 에너지가 최소 기본 공격 코스트 이상 있으면
                if(enemyPlayer.Energy >= attackCost)
                {
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                        randomCard = availableCards[randomIndex];

                        // 선택된 카드에 해당하는 코스트를 확인
                        switch (randomCard)
                        {
                            case 5:
                                cardCost = attackCost; // attackCost
                                break;
                            case 6:
                                cardCost = magicAttackCost;  // magicAttackCost
                                break;
                            case 7:
                                cardCost = limitAttackCost; // limitAttackCost
                                break;
                        }
                    }
                    while (enemyPlayer.Energy < cardCost);
                    //while (enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                    EfirstTurnCardIndex = randomCard;

                    inAttackRange = false;
                    inMagicAttackRange = false;
                    inLimitAttackRange = false;
                    availableCards.Clear();
                }
                // 에너지가 기본 공격 코스트도 없으면
                else
                {
                    // 에너지업, 움직임 0 1 2 3 8
                    int[] numbers = { 0, 1 ,2, 3, 8 };
                    randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                    EfirstTurnCardIndex = numbers[randomIndex];
                }
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("1턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정");
                int randomCard;
                int cardCost = 0;

                do
                {
                    randomCard = UnityEngine.Random.Range(0, 13);

                    // 선택된 카드에 해당하는 코스트를 확인
                    switch (randomCard)
                    {
                        case 5:
                            cardCost = attackCost; // attackCost
                            break;
                        case 6:
                            cardCost = magicAttackCost;  // magicAttackCost
                            break;
                        case 7:
                            cardCost = limitAttackCost; // limitAttackCost
                            break;
                        case 11:
                            cardCost = perfectGuardCost;
                            break;
                        default :
                            cardCost = 0;
                            break;

                    }
                }
                while (enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                EfirstTurnCardIndex = randomCard;
                EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("1턴 플레이어가 공격, 이동을 안해서 진짜 랜덤");
            int randomCard = UnityEngine.Random.Range(0, 13);
            EfirstTurnCardIndex = randomCard;
            EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
        }

        // 첫번째 턴 카드 끝 ----------------------------------------------------------------------------------------------------

        // 두번째 턴 카드 시작 ----------------------------------------------------------------------------------------------------

        // 만약 플레이어가 두번째 턴에 기본 공격 or 마법 공격 or 특수 공격을 할 예정이면
        if (controlZone.secondTurnCardIndex == 5 || controlZone.secondTurnCardIndex == 6 || controlZone.secondTurnCardIndex == 7)
        {
            // 플레이어의 현재 위치에서 공격 범위 계산
            CharacterAttackRange(gameManager.playerCharacterIndex, playerTargetSection);
            //Debug.LogWarning($"플레이어의 특수 공격 범위 : {string.Join(", ", limitAttackRange)}");

            // 플레이어의 공격 범위에 적이 있으면
            if (attackRange.Contains(enemyPlayer.EcurrentSectionIndex) ||
                magicAttackRange.Contains(enemyPlayer.EcurrentSectionIndex) ||
                limitAttackRange.Contains(enemyPlayer.EcurrentSectionIndex))
            {
                Debug.LogWarning("2턴 플레이어가 공격 할건데, 적이 공격 범위에 있음");
                if (UnityEngine.Random.value < 0.7f)                        // 70% 확률로 움직임
                {
                    Debug.Log("70% 확률로 움직이기로 함");
                    int randomCard;
                    do
                    {
                        randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                    }
                    while (randomCard == EfirstTurnCardIndex);              // 같은 값이면 다시 뽑기

                    EsecondTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
                    EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                }
                else
                {
                    // 움직이지 않기로 함
                    Debug.LogWarning("30% 확률로 움직이지 않기로 함");
                    int[] numbers = { 4, 5, 6, 7, 11 };
                    int randomCard;
                    int randomIndex;
                    int cardCost = 0;

                    // 적 플레이어의 현재 위치에서 공격 범위 계산
                    CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);
                    // 1. 가드를 하거나 4 11
                    // 2. 공격을 하거나 5 6 7
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                        randomCard = numbers[randomIndex];                              // 실제 값 선택

                        // 선택된 카드에 해당하는 코스트를 확인
                        switch (randomCard)
                        {
                            case 4:
                                // 4번 카드는 가드니까, 코스트를 특정 값으로 설정 (예: 0)
                                cardCost = 0;
                                break;
                            case 5:
                                cardCost = attackCost; // attackCost
                                break;
                            case 6:
                                cardCost = magicAttackCost;  // magicAttackCost
                                break;
                            case 7:
                                cardCost = limitAttackCost; // limitAttackCost
                                break;
                            case 11:
                                cardCost = perfectGuardCost; // perfectGuardCost
                                break;

                        }
                    }
                    while (randomCard == EfirstTurnCardIndex && enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복(중복도 방지 추가)

                    EsecondTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.LogWarning("2턴 플레이어가 공격할건데, 적이 공격 범위에 없음");
                Debug.Log("그래서 움직이기로 함");
                int randomCard;
                do
                {
                    randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                }
                while (randomCard == EfirstTurnCardIndex);              // 같은 값이면 다시 뽑기

                EsecondTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
                EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
            }
        }
        // 플레이어가 두번째 턴에 움직일 예정이면
        else if (controlZone.secondTurnCardIndex == 0 || controlZone.secondTurnCardIndex == 1 || controlZone.secondTurnCardIndex == 2 || controlZone.secondTurnCardIndex == 3 ||
            controlZone.secondTurnCardIndex == 9 || controlZone.secondTurnCardIndex == 10)
        {
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.secondTurnCardIndex, playerTargetSection);

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (attackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                inAttackRange = true;
                availableCards.Add(5);
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                inMagicAttackRange = true;
                availableCards.Add(6);
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                inLimitAttackRange = true;
                availableCards.Add(7);
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;
                int cardCost = 0;

                // 최소 기본 공격 코스트 이상 있을 때만
                if(enemyPlayer.Energy >= attackCost)
                {
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                        randomCard = availableCards[randomIndex];

                        // 선택된 카드에 해당하는 코스트를 확인
                        switch (randomCard)
                        {
                            case 5:
                                cardCost = attackCost; // attackCost
                                break;
                            case 6:
                                cardCost = magicAttackCost;  // magicAttackCost
                                break;
                            case 7:
                                cardCost = limitAttackCost; // limitAttackCost
                                break;
                        }
                    }
                    while (randomCard == EfirstTurnCardIndex && enemyPlayer.Energy < cardCost);
                    //while (enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                    EsecondTurnCardIndex = randomCard;

                    inAttackRange = false;
                    inMagicAttackRange = false;
                    inLimitAttackRange = false;
                    availableCards.Clear();
                }
                // 기본 공격 코스트 이상의 에너지가 없으면
                else
                {
                    // 에너지업, 움직임 0 1 2 3 8
                    int[] numbers = { 0, 1, 2, 3, 8 };
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                    }
                    while (randomIndex == EfirstTurnCardIndex);
                    EsecondTurnCardIndex = numbers[randomIndex];
                }
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("2턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정");
                int randomCard;
                int cardCost = 0;

                do
                {
                    randomCard = UnityEngine.Random.Range(0, 13);

                    // 선택된 카드에 해당하는 코스트를 확인
                    switch (randomCard)
                    {
                        case 5:
                            cardCost = attackCost; // attackCost
                            break;
                        case 6:
                            cardCost = magicAttackCost;  // magicAttackCost
                            break;
                        case 7:
                            cardCost = limitAttackCost; // limitAttackCost
                            break;
                        case 11:
                            cardCost = perfectGuardCost;
                            break;
                        default:
                            cardCost = 0;
                            break;

                    }
                }
                while (randomCard == EfirstTurnCardIndex && enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                EsecondTurnCardIndex = randomCard;
                EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("2턴 플레이어가 공격, 이동을 안해서 진짜 랜덤");
            int randomCard;
            int cardCost = 0;

            do
            {
                randomCard = UnityEngine.Random.Range(0, 13);

                // 선택된 카드에 해당하는 코스트를 확인
                switch (randomCard)
                {
                    case 5:
                        cardCost = attackCost; // attackCost
                        break;
                    case 6:
                        cardCost = magicAttackCost;  // magicAttackCost
                        break;
                    case 7:
                        cardCost = limitAttackCost; // limitAttackCost
                        break;
                    case 11:
                        cardCost = perfectGuardCost;
                        break;
                    default:
                        cardCost = 0;
                        break;

                }
            }
            while (randomCard == EfirstTurnCardIndex && enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

            EsecondTurnCardIndex = randomCard;
            EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
        }

        // 두번째 턴 카드 끝 ----------------------------------------------------------------------------------------------------

        // 세번째 턴 카드 시작 ----------------------------------------------------------------------------------------------------

        // 만약 플레이어가 세번째 턴에 기본 공격 or 마법 공격 or 특수 공격을 할 예정이면
        if (controlZone.thirdTurnCardIndex == 5 || controlZone.thirdTurnCardIndex == 6 || controlZone.thirdTurnCardIndex == 7)
        {
            // 플레이어의 현재 위치에서 공격 범위 계산
            CharacterAttackRange(gameManager.playerCharacterIndex, playerTargetSection);
            //Debug.LogWarning($"플레이어의 특수 공격 범위 : {string.Join(", ", limitAttackRange)}");

            // 플레이어의 공격 범위에 적이 있으면
            if (attackRange.Contains(enemyTargetSection) ||
                magicAttackRange.Contains(enemyTargetSection) ||
                limitAttackRange.Contains(enemyTargetSection))
            {
                Debug.LogWarning("3턴 플레이어가 공격 할건데, 적이 공격 범위에 있음");
                if (UnityEngine.Random.value < 0.7f)                        // 70% 확률로 움직임
                {
                    Debug.Log("70% 확률로 움직이기로 함");
                    int randomCard;
                    do
                    {
                        randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                    }
                    while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);              // 같은 값이면 다시 뽑기

                    EthirdTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
                    EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                }
                else
                {
                    // 움직이지 않기로 함
                    Debug.LogWarning("30% 확률로 움직이지 않기로 함");
                    int[] numbers = { 4, 5, 6, 7, 11 };
                    int randomCard;
                    int randomIndex;
                    int cardCost = 0;

                    // 적 플레이어의 현재 위치에서 공격 범위 계산
                    CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);
                    // 1. 가드를 하거나 4 11
                    // 2. 공격을 하거나 5 6 7
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);      // 배열 인덱스 선택
                        randomCard = numbers[randomIndex];                              // 실제 값 선택

                        // 선택된 카드에 해당하는 코스트를 확인
                        switch (randomCard)
                        {
                            case 4:
                                // 4번 카드는 가드니까, 코스트를 특정 값으로 설정 (예: 0)
                                cardCost = 0;
                                break;
                            case 5:
                                cardCost = attackCost; // attackCost
                                break;
                            case 6:
                                cardCost = magicAttackCost;  // magicAttackCost
                                break;
                            case 7:
                                cardCost = limitAttackCost; // limitAttackCost
                                break;
                            case 11:
                                cardCost = perfectGuardCost; // perfectGuardCost
                                break;

                        }
                    }
                    while ((randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex) && enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복(중복도 방지 추가)

                    EthirdTurnCardIndex = randomCard;
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.LogWarning("3턴 플레이어가 공격할건데, 적이 공격 범위에 없음");
                Debug.Log("그래서 움직이기로 함");
                int randomCard;
                do
                {
                    randomCard = UnityEngine.Random.Range(0, 4);        // 0, 1, 2, 3 중 하나 선택
                }
                while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);              // 같은 값이면 다시 뽑기

                EthirdTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
                EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
            }
        }
        // 플레이어가 세번째 턴에 움직일 예정이면
        else if (controlZone.secondTurnCardIndex == 0 || controlZone.secondTurnCardIndex == 1 || controlZone.secondTurnCardIndex == 2 || controlZone.secondTurnCardIndex == 3 ||
            controlZone.secondTurnCardIndex == 9 || controlZone.secondTurnCardIndex == 10)
        {
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.secondTurnCardIndex, playerTargetSection);

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (attackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                inAttackRange = true;
                availableCards.Add(5);
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                inMagicAttackRange = true;
                availableCards.Add(6);
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                inLimitAttackRange = true;
                availableCards.Add(7);
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;
                int cardCost = 0;

                // 내가 만약 전에 기본 공격을 했는데 그 다음에 코스트 모자라면 어떡할거임? => 기본공격 이상은 있는데, 마법 특수 코스트가 모자랄수도 있지
                if(enemyPlayer.Energy >= attackCost)
                {
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                        randomCard = availableCards[randomIndex];

                        // 선택된 카드에 해당하는 코스트를 확인
                        switch (randomCard)
                        {
                            case 5:
                                cardCost = attackCost; // attackCost
                                break;
                            case 6:
                                cardCost = magicAttackCost;  // magicAttackCost
                                break;
                            case 7:
                                cardCost = limitAttackCost; // limitAttackCost
                                break;
                        }
                    }
                    while ((randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex) && enemyPlayer.Energy < cardCost);
                    //while (enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                    EthirdTurnCardIndex = randomCard;

                    inAttackRange = false;
                    inMagicAttackRange = false;
                    inLimitAttackRange = false;
                    availableCards.Clear();
                }
                else
                {
                    // 에너지업, 움직임 0 1 2 3 8
                    int[] numbers = { 0, 1, 2, 3, 8 };
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                    }
                    while (randomIndex == EfirstTurnCardIndex || randomIndex == EsecondTurnCardIndex);
                    EthirdTurnCardIndex = numbers[randomIndex];
                }
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("3턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정");
                int randomCard;
                int cardCost = 0;

                do
                {
                    randomCard = UnityEngine.Random.Range(0, 13);

                    // 선택된 카드에 해당하는 코스트를 확인
                    switch (randomCard)
                    {
                        case 5:
                            cardCost = attackCost; // attackCost
                            break;
                        case 6:
                            cardCost = magicAttackCost;  // magicAttackCost
                            break;
                        case 7:
                            cardCost = limitAttackCost; // limitAttackCost
                            break;
                        case 11:
                            cardCost = perfectGuardCost;
                            break;
                        default:
                            cardCost = 0;
                            break;

                    }
                }
                while (enemyPlayer.Energy <= cardCost);      // 적의 에너지 <= 코스트 면 다른게 뽑을 때까지 반복

                EthirdTurnCardIndex = randomCard;
                EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("3턴 플레이어가 공격, 이동을 안해서 진짜 랜덤");
            int randomCard;
            int cardCost = 0;

            do
            {
                randomCard = UnityEngine.Random.Range(0, 13);

                // 선택된 카드에 해당하는 코스트를 확인
                switch (randomCard)
                {
                    case 5:
                        cardCost = attackCost; // attackCost
                        break;
                    case 6:
                        cardCost = magicAttackCost;  // magicAttackCost
                        break;
                    case 7:
                        cardCost = limitAttackCost; // limitAttackCost
                        break;
                    case 11:
                        cardCost = perfectGuardCost;
                        break;
                    default:
                        cardCost = 0;
                        break;

                }
            }
            while ((randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex) && enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

            EthirdTurnCardIndex = randomCard;
            EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
        }

    }

    public int[] attackRange = null;          // 기본 공격 범위
    public int[] magicAttackRange = null;     // 마법 공격 범위
    public int[] limitAttackRange = null;     // 리미트 공격 범위

    /// <summary>
    /// 공격 범위 땜빵용
    /// </summary>
    /// <param name="CharacterIndex">해당 캐릭터의 번호</param>
    /// <param name="Section">현재 캐릭터의 위치</param>
    public void CharacterAttackRange(int CharacterIndex, int Section)
    {
        switch (CharacterIndex)
        {
            case 0:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 4 };                                   // 0 4
                        magicAttackRange = new int[] { Section, Section + 4, Section + 1 };     // 0 1 4
                        limitAttackRange = new int[] { Section                                                          // 0
                             };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section + 4 };                                   // 1 5
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,       // 1 0 5 2
                            Section + 4 };
                        limitAttackRange = new int[] { Section                                                          // 1
                             };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section + 4 };                                   // 2 6
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,       // 2 1 6 3
                            Section + 4 };
                        limitAttackRange = new int[] { Section                                                          // 2
                             };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section + 4 };                                   // 3 7
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4 };     // 3 2 7
                        limitAttackRange = new int[] { Section                                                          // 3
                             };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 4 0 8
                        magicAttackRange = new int[] { Section, Section - 4, Section + 1,        // 4 0 5 8
                            Section + 4 };
                        limitAttackRange = new int[] { Section, Section - 4, Section - 3,        // 4 0 1
                             };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 5 1 9
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 5 1 4 9 6
                            Section + 4, Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 5 0 1 2
                            Section - 3 };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 6 2 10
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 6 2 5 10 7
                            Section + 4, Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 6 1 2 3
                            Section - 3 };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 7 3 11
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 7 3 6 11
                            Section + 4 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 7 2 3
                             };
                        break;
                    case 8:
                        attackRange = new int[] { Section, Section - 4 };                                    // 8 4
                        magicAttackRange = new int[] { Section, Section - 4, Section + 1         // 8 4 9
                             };
                        limitAttackRange = new int[] { Section, Section - 4, Section - 3,        // 8 4 5
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section, Section - 4 };                                    // 9 5
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4,        // 9 8 5 10
                            Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 9 4 5 6
                            Section - 3 };
                        break;
                    case 10:
                        attackRange = new int[] { Section, Section - 4, };                                   // 10 6
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4,        // 10 9 6 11
                            Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 10 5 6 7
                            Section - 3 };
                        break;
                    case 11:
                        attackRange = new int[] { Section, Section - 4 };                                    // 11 7
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4         // 11 6 7
                             };
                        break;

                }
                break;

            case 1:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 4 };                                   // 0 4
                        magicAttackRange = new int[] { Section, Section + 5 };                              // 0 5
                        limitAttackRange = new int[] { Section, Section + 4, Section + 5        // 0 4 5
                            };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section + 4 };                                   // 1 5
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 1 4 6
                        limitAttackRange = new int[] { Section, Section + 3, Section + 4,       // 1 4 5 6
                            Section + 5 };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section + 4 };                                   // 2 6
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 2 5 7
                        limitAttackRange = new int[] { Section, Section +3 , Section + 4,       // 2 5 6 7
                            Section + 5 };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section + 4 };                                   // 3 7
                        magicAttackRange = new int[] { Section, Section + 3 };                              // 3 6
                        limitAttackRange = new int[] { Section, Section + 3, Section + 4        // 3 6 7
                             };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 4 0 8
                        magicAttackRange = new int[] { Section, Section + 5, Section - 3 };      // 4 1 9
                        limitAttackRange = new int[] { Section, Section - 4, Section - 3,        // 4 0 1 8 9
                            Section + 4, Section + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 5 1 9
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 5 0 8 2 10
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 5 0 1 2 8 9 10
                            Section - 3, Section + 3, Section + 4, Section + 5 };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 6 2 10
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 6 1 9 3 11
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 6 1 2 3 9 10 11
                            Section - 3, Section + 3, Section + 4, Section + 5 };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 7 3 11
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 7 2 3 10 11
                            Section + 3, Section + 4 };
                        break;
                    case 8:
                        attackRange = new int[] { Section, Section - 4 };                                    // 8 4
                        magicAttackRange = new int[] { Section, Section - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { Section, Section - 4, Section - 3,        // 8 4 5
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section, Section - 4 };                                    // 9 5
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 9 4 5 6
                            Section - 3 };
                        break;
                    case 10:
                        attackRange = new int[] { Section, Section - 4, };                                   // 10 6
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 10 5 6 7
                            Section - 3 };
                        break;
                    case 11:
                        attackRange = new int[] { Section, Section - 4 };                                    // 11 7
                        magicAttackRange = new int[] { Section, Section - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4         // 11 6 7
                             };
                        break;

                }
                break;

            case 2:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 1 };                                   // 0 1
                        magicAttackRange = new int[] { Section, Section + 4, Section + 1 };     // 0 1 4
                        limitAttackRange = new int[] { Section , Section + 1                                // 0 1
                             };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 1 0 2
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,       // 1 0 5 2
                            Section + 4 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 1        // 1 0 2
                             };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };          // 2 1 3
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,       // 2 1 6 3
                            Section + 4 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 1        // 2 1 3
                             };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section - 1 };                                   // 3 2
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4 };     // 3 2 7
                        limitAttackRange = new int[] { Section, Section - 1                                 // 3 2
                             };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 1 };                                    // 4 5
                        magicAttackRange = new int[] { Section, Section - 4, Section + 1,        // 4 0 5 8
                            Section + 4 };
                        limitAttackRange = new int[] { Section, Section - 4, Section - 3,        // 4 0 1 5
                            Section + 1 };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 5 4 6
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 5 1 4 9 6
                            Section + 4, Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 1, Section - 5,        // 5 4 0 1 2 6
                            Section - 4, Section - 3, Section + 1 };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 6 5 7
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 6 2 5 10 7
                            Section + 4, Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 1, Section - 5,        // 6 5 1 2 3 7
                            Section - 4, Section - 3, Section + 1 };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section - 1 };                                    // 7 6
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 7 3 6 11
                            Section + 4 };
                        limitAttackRange = new int[] { Section, Section - 1, Section - 5,        // 7 6 2 3
                            Section - 4 };
                        break;
                    case 8:
                        attackRange = new int[] { Section, Section + 1 };                                    // 8 9
                        magicAttackRange = new int[] { Section, Section - 4, Section + 1         // 8 4 9
                             };
                        limitAttackRange = new int[] { Section, Section - 4, Section - 3,        // 8 4 5 9
                            Section + 1 };
                        break;
                    case 9:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 9 8 10
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4,        // 9 8 5 10
                            Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 1, Section - 5,        // 9 4 5 6 10
                            Section - 4, Section - 3, Section + 1 };
                        break;
                    case 10:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 10 9 11
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4,        // 10 9 6 11
                            Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 1, Section - 5,        // 10 9 5 6 7 11
                            Section - 4, Section - 3, Section + 1 };
                        break;
                    case 11:
                        attackRange = new int[] { Section, Section - 1 };                                    // 11 10
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { Section, Section - 1, Section - 5,        // 11 10 6 7
                             Section - 4 };
                        break;

                }
                break;

            case 3:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 4, Section + 5 };          // 0 4 5
                        magicAttackRange = new int[] { Section, Section + 4, Section + 1 };     // 0 1 4
                        limitAttackRange = new int[] { Section                                                          // 0
                            };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section + 3, Section + 4,            // 1 4 5 6
                            Section + 5 };
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,       // 1 0 5 2
                            Section + 4 };
                        limitAttackRange = new int[] { Section                                                          // 1
                             };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section + 3, Section + 4,            // 2 5 6 7
                        Section + 5};
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,       // 2 1 6 3
                            Section + 4 };
                        limitAttackRange = new int[] { Section                                                          // 2
                             };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section + 3, Section + 4 };           // 3 6 7
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4 };      // 3 2 7
                        limitAttackRange = new int[] { Section                                                           // 3
                             };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 4, Section + 5 };           // 4 8 9
                        magicAttackRange = new int[] { Section, Section - 4, Section + 1,        // 4 0 5 8
                            Section + 4 };
                        limitAttackRange = new int[] { Section, Section - 4, Section - 3,        // 4 0 1
                             };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section + 3, Section + 4,             // 5 8 9 10
                            Section + 5};
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 5 1 4 9 6
                            Section + 4, Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 5 0 1 2
                            Section - 3 };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section + 3, Section + 4,             // 6 9 10 11
                            Section + 5 };
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 6 2 5 10 7
                            Section + 4, Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 6 1 2 3
                            Section - 3 };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section + 3, Section + 4 };           // 7 10 11
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 7 3 6 11
                            Section + 4 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 7 2 3
                             };
                        break;
                    case 8:
                        attackRange = new int[] { Section };                                                             // 8
                        magicAttackRange = new int[] { Section, Section - 4, Section + 1         // 8 4 9
                             };
                        limitAttackRange = new int[] { Section, Section - 4, Section - 3,        // 8 4 5
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section };                                                             // 9
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4,        // 9 8 5 10
                            Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 9 4 5 6
                            Section - 3 };
                        break;
                    case 10:
                        attackRange = new int[] { Section };                                                             // 10
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4,        // 10 9 6 11
                            Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4,        // 10 5 6 7
                            Section - 3 };
                        break;
                    case 11:
                        attackRange = new int[] { Section };                                                             // 11
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 4         // 11 6 7
                             };
                        break;

                }
                break;

            case 4:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 1 };                                   // 0 1
                        magicAttackRange = new int[] { Section, Section + 5 };                              // 0 5
                        limitAttackRange = new int[] { Section, Section + 4, Section + 5,       // 0 4 5 1
                            Section + 1 };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };          // 1 0 2
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 1 4 6
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,       // 1 0 4 5 6 2
                            Section + 4, Section + 5, Section + 1 };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };          // 2 1 3
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 1 4 6
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,       // 2 1 5 6 7 3
                            Section + 4, Section + 5, Section + 1 };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section - 1 };                                    // 3 2
                        magicAttackRange = new int[] { Section, Section + 3 };                               // 1 4 6
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,        // 3 2 6 7
                            Section + 4 };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 1 };                                    // 4 5
                        magicAttackRange = new int[] { Section, Section + 5, Section - 3 };      // 4 1 9
                        limitAttackRange = new int[] { Section, Section + 4, Section + 1,        // 4 8 5 9
                            Section + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 5 4 6
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 5 0 8 2 10
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,        // 5 4 8 9 10 6
                            Section + 4, Section + 5, Section + 1 };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 6 5 7
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 6 1 9 3 11
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,        // 6 5 9 10 11 7
                            Section + 4, Section + 5, Section + 1 };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section - 1 };                                    // 7 6
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,        // 7 6 10 11
                            Section + 4 };
                        break;
                    case 8:
                        attackRange = new int[] { Section, Section + 1 };                                    // 8 9
                        magicAttackRange = new int[] { Section, Section - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { Section, Section + 1,                                 // 8 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 9 8 10
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 1,        // 9 8 10
                             };
                        break;
                    case 10:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 10 9 11
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 1,        // 10 9 11
                             };
                        break;
                    case 11:
                        attackRange = new int[] { Section, Section - 1 };                                    // 11 10
                        magicAttackRange = new int[] { Section, Section - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { Section, Section - 1                                  // 11 10
                             };
                        break;

                }
                break;

            case 5:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 1, Section + 4 };          // 0 4 1
                        magicAttackRange = new int[] { Section, Section + 1, Section + 4 };     // 0 4 1
                        limitAttackRange = new int[] { Section, Section + 4, Section + 5,       // 0 4 5 1
                            Section + 1 };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section - 1, Section + 4,            // 1 0 5 2
                            Section + 1 };
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4,       // 1 0 5 2
                            Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,       // 1 0 4 5 6 2
                            Section + 4, Section + 5, Section + 1 };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section - 1, Section + 4,            // 2 1 6 3
                            Section + 1 };
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4,       // 2 1 6 3
                            Section + 1 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,       // 2 1 5 6 7 3
                            Section + 4, Section + 5, Section + 1 };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section - 1, Section + 4 };           // 3 2 7
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4 };      // 3 2 7
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,        // 3 2 6 7
                            Section + 4 };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 4, Section + 1 };           // 4 8 5
                        magicAttackRange = new int[] { Section, Section + 4, Section + 1,        // 4 8 5 0
                            Section - 4 };
                        limitAttackRange = new int[] { Section, Section + 4, Section + 1,        // 4 8 5 9
                            Section + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section - 1, Section + 4,             // 5 4 9 6
                            Section + 1 };
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4,        // 5 4 9 6 1
                            Section + 1, Section - 4 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,        // 5 4 8 9 10 6
                            Section + 4, Section + 5, Section + 1 };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section - 1, Section + 4,             // 6 5 10 7
                            Section + 1 };
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4,        // 6 5 10 7 2
                            Section + 1, Section - 4 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,        // 6 5 9 10 11 7
                            Section + 4, Section + 5, Section + 1 };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section - 1, Section + 4 };           // 7 6 11
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4,        // 7 6 11 3
                            Section - 4 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,        // 7 6 10 11
                            Section + 4 };
                        break;
                    case 8:
                        attackRange = new int[] { Section, Section + 1 };                                    // 8 9
                        magicAttackRange = new int[] { Section, Section + 1, Section - 4         // 8 9 4
                             };
                        limitAttackRange = new int[] { Section, Section + 1,                                 // 8 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 9 8 10
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,        // 9 8 10 5
                             Section - 4 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 1,        // 9 8 10
                             };
                        break;
                    case 10:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 10 9 11
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,        // 10 9 11 6
                             Section - 4 };
                        limitAttackRange = new int[] { Section, Section - 1, Section + 1,        // 10 9 11
                             };
                        break;
                    case 11:
                        attackRange = new int[] { Section, Section - 1 };                                    // 11 10
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { Section, Section - 1                                  // 11 10
                             };
                        break;

                }
                break;

            case 6:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 1 };                                   // 0 1
                        magicAttackRange = new int[] { Section, Section + 4, Section + 1 };     // 0 1 4
                        limitAttackRange = new int[] { Section + 1                                                      // 1
                             };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };          // 1 0 2
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,       // 1 0 5 2
                            Section + 4 };
                        limitAttackRange = new int[] { Section - 1, Section + 1                             // 0 2
                             };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };          // 2 1 3
                        magicAttackRange = new int[] { Section, Section - 1, Section + 1,       // 2 1 6 3
                            Section + 4 };
                        limitAttackRange = new int[] { Section - 1, Section + 1                             // 1 3
                             };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section - 1 };                                   // 3 2
                        magicAttackRange = new int[] { Section, Section - 1, Section + 4 };     // 3 2 7
                        limitAttackRange = new int[] { Section - 1                                                      // 2
                             };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 1 };                                    // 4 5
                        magicAttackRange = new int[] { Section, Section - 4, Section + 1,        // 4 0 5 8
                            Section + 4 };
                        limitAttackRange = new int[] { Section + 1                                                       // 5
                             };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 5 4 6
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 5 1 4 9 6
                            Section + 4, Section + 1 };
                        limitAttackRange = new int[] { Section - 1, Section + 1                              // 4 6
                             };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 6 5 7
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 6 2 5 10 7
                            Section + 4, Section + 1 };
                        limitAttackRange = new int[] { Section - 1, Section + 1                              // 5 7
                             };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section - 1 };                                    // 7 6
                        magicAttackRange = new int[] { Section, Section - 4, Section - 1,        // 7 3 6 11
                            Section + 4 };
                        limitAttackRange = new int[] { Section - 1                                                       // 6
                             };
                        break;
                    case 8:
                        attackRange = new int[] { Section, Section + 1 };                                    // 8 9
                        magicAttackRange = new int[] { Section, Section - 4, Section + 1         // 8 4 9
                             };
                        limitAttackRange = new int[] { Section + 1                                                       // 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 9 8 10
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4,        // 9 8 5 10
                            Section + 1 };
                        limitAttackRange = new int[] { Section - 1, Section + 1                              // 8 10
                             };
                        break;
                    case 10:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 10 9 11
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4,        // 10 9 6 11
                            Section + 1 };
                        limitAttackRange = new int[] { Section - 1, Section + 1                              // 9 11
                             };
                        break;
                    case 11:
                        attackRange = new int[] { Section, Section - 1 };                                    // 11 10
                        magicAttackRange = new int[] { Section, Section - 1, Section - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { Section - 1                                                       // 10
                             };
                        break;

                }
                break;

            case 7:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 1 };                                   // 0 1
                        magicAttackRange = new int[] { Section, Section + 5 };                              // 0 5
                        limitAttackRange = new int[] { Section, Section + 1, Section + 5        // 0 1 5
                            };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };          // 1 0 2
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 1 4 6
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,       // 1 0 4 2 6
                            Section + 1, Section + 5 };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };          // 2 1 3
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 1 4 6
                        limitAttackRange = new int[] { Section, Section -1, Section + 3,        // 2 1 5 3 7
                            Section + 1, Section + 5 };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section - 1 };                                    // 3 2
                        magicAttackRange = new int[] { Section, Section + 3 };                               // 1 4 6
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3        // 3 2 6
                             };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 1 };                                    // 4 5
                        magicAttackRange = new int[] { Section, Section + 5, Section - 3 };      // 4 1 9
                        limitAttackRange = new int[] { Section, Section + 1, Section - 3,        // 4 1 5 9
                            Section + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 5 4 6
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 5 0 8 2 10
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 5 0 4 8 2 6 10
                            Section + 3, Section - 3, Section + 1, Section + 5 };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 6 5 7
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 6 1 9 3 11
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 6 1 5 9 3 7 11
                            Section + 3, Section - 3, Section + 1, Section + 5 };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section - 1 };                                    // 7 6
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 7 2 6 10
                            Section + 3 };
                        break;
                    case 8:
                        attackRange = new int[] { Section, Section + 1 };                                    // 8 9
                        magicAttackRange = new int[] { Section, Section - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { Section, Section - 3, Section + 1,        // 8 5 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };          // 9 8 10
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 9 4 8 6 10
                            Section - 3, Section + 1 };
                        break;
                    case 10:
                        attackRange = new int[] { Section, Section - 1, Section + 1 };           // 10 9 11
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 10 5 9 7 11
                            Section - 3, Section + 1 };
                        break;
                    case 11:
                        attackRange = new int[] { Section, Section - 1 };                                    // 11 10
                        magicAttackRange = new int[] { Section, Section - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1         // 11 6 10
                             };
                        break;

                }
                break;

            case 8:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 4, Section + 5 };          // 0 4 5
                        magicAttackRange = new int[] { Section, Section + 5 };                              // 0 5
                        limitAttackRange = new int[] { Section, Section + 1, Section + 5        // 0 1 5
                            };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section + 3, Section + 4,            // 1 4 5 6
                            Section + 5 };
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 1 4 6
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3,       // 1 0 4 2 6
                            Section + 1, Section + 5 };
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section + 3, Section + 4,            // 2 5 6 7
                        Section + 5};
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 1 4 6
                        limitAttackRange = new int[] { Section, Section -1, Section + 3,        // 2 1 5 3 7
                            Section + 1, Section + 5 };
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section + 3, Section + 4 };           // 3 6 7
                        magicAttackRange = new int[] { Section, Section + 3 };                               // 1 4 6
                        limitAttackRange = new int[] { Section, Section - 1, Section + 3         // 3 2 6
                             };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 4, Section + 5 };           // 4 8 9
                        magicAttackRange = new int[] { Section, Section + 5, Section - 3 };      // 4 1 9
                        limitAttackRange = new int[] { Section, Section + 1, Section - 3,        // 4 1 5 9
                            Section + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section + 3, Section + 4,             // 5 8 9 10
                            Section + 5};
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 5 0 8 2 10
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 5 0 4 8 2 6 10
                            Section + 3, Section - 3, Section + 1, Section + 5 };
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section + 3, Section + 4,             // 6 9 10 11
                            Section + 5 };
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 6 1 9 3 11
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 6 1 5 9 3 7 11
                            Section + 3, Section - 3, Section + 1, Section + 5 };
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section + 3, Section + 4 };           // 7 10 11
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 7 2 6 10
                            Section + 3 };
                        break;
                    case 8:
                        attackRange = new int[] { Section };                                                             // 8
                        magicAttackRange = new int[] { Section, Section - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { Section, Section - 3, Section + 1,        // 8 5 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section };                                                             // 9
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 9 4 8 6 10
                            Section - 3, Section + 1 };
                        break;
                    case 10:
                        attackRange = new int[] { Section };                                                             // 10
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1,        // 10 5 9 7 11
                            Section - 3, Section + 1 };
                        break;
                    case 11:
                        attackRange = new int[] { Section };                                                             // 11
                        magicAttackRange = new int[] { Section, Section - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { Section, Section - 5, Section - 1         // 11 6 10
                             };
                        break;

                }
                break;

            case 9:

                // 공격 범위(위치에 따라 다름 )
                switch (Section)
                {
                    case 0:
                        attackRange = new int[] { Section, Section + 4 };                                   // 0 4
                        magicAttackRange = new int[] { Section, Section + 5 };                              // 0 5
                        limitAttackRange = new int[] { 0, 1, 2, 3                                                                   // 0 1 2 3
                             };
                        break;
                    case 1:
                        attackRange = new int[] { Section, Section + 4 };                                   // 1 5
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 1 4 6
                        limitAttackRange = new int[] { 0, 1, 2, 3 };                                                                // 0 1 2 3
                        break;
                    case 2:
                        attackRange = new int[] { Section, Section + 4 };                                   // 2 6
                        magicAttackRange = new int[] { Section, Section + 3, Section + 5 };     // 2 5 7
                        limitAttackRange = new int[] { 0, 1, 2, 3 };                                                                // 0 1 2 3
                        break;
                    case 3:
                        attackRange = new int[] { Section, Section + 4 };                                   // 3 7
                        magicAttackRange = new int[] { Section, Section + 3 };                              // 3 6
                        limitAttackRange = new int[] { 0, 1, 2, 3                                                                   // 0 1 2 3
                             };
                        break;
                    case 4:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 4 0 8
                        magicAttackRange = new int[] { Section, Section + 5, Section - 3 };      // 4 1 9
                        limitAttackRange = new int[] { 4, 5, 6, 7 };                                                                 // 4 5 6 7
                        break;
                    case 5:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 5 1 9
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 5 0 8 2 10
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { 4, 5, 6, 7 };                                                                 // 4 5 6 7
                        break;
                    case 6:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 6 2 10
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 6 1 9 3 11
                            Section + 5, Section - 3 };
                        limitAttackRange = new int[] { 4, 5, 6, 7 };                                                                 // 4 5 6 7
                        break;
                    case 7:
                        attackRange = new int[] { Section, Section + 4, Section - 4 };           // 7 3 11
                        magicAttackRange = new int[] { Section, Section - 5, Section + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { 4, 5, 6, 7 };                                                                 // 4 5 6 7
                        break;
                    case 8:
                        attackRange = new int[] { Section, Section - 4 };                                    // 8 4
                        magicAttackRange = new int[] { Section, Section - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { 8, 9, 10 ,11                                                                  // 8 9 10 11
                             };
                        break;
                    case 9:
                        attackRange = new int[] { Section, Section - 4 };                                    // 9 5
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { 8, 9, 10, 11 };                                                               // 8 9 10 11
                        break;
                    case 10:
                        attackRange = new int[] { Section, Section - 4, };                                   // 10 6
                        magicAttackRange = new int[] { Section, Section - 5, Section - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { 8, 9, 10, 11 };                                                               // 8 9 10 11
                        break;
                    case 11:
                        attackRange = new int[] { Section, Section - 4 };                                    // 11 7
                        magicAttackRange = new int[] { Section, Section - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { 8, 9, 10, 11 };                                                               // 8 9 10 11
                        break;

                }
                break;
        }
    }

    /// <summary>
    /// 이동 카드 선택시 현재 위치로부터 이동할 위치를 계산하는 함수
    /// </summary>
    /// <param name="cardIndex">현재 사용 할 카드 번호</param>
    /// <param name="Section">현재 위치</param>
    private void PlayerCharacterMove(int cardIndex, int Section)
    {
        switch (cardIndex)
        {
            // 아래로 이동
            case 0:
                if(Section > 3)        // 현재 위치가 4 5 6 7 일 때(윗줄)
                {
                    playerTargetSection = Section - 4;       // 아래로 이동할 때는 -4 만큼
                }
                break;

            // 위로 이동
            case 1:
                if (Section < 8)        // 현재 위치가 0 1 2 3 일 때(아래줄)  4 -> 8
                {
                    playerTargetSection = Section + 4;       // 위로 이동할 때는 +4 만큼 델리게이트로 전송
                }
                break;

            // 오른쪽으로 이동
            case 2:
                if (Section != 3 && Section != 7 && Section != 11)        // 현재 위치가 3, 7, 11 이 아닐 때(맨 오른쪽)
                {
                    playerTargetSection = Section++;          // 오른쪽으로 이동할 때는 +1 만큼
                }
                break;

            // 왼쪽으로 이동
            case 3:
                if (Section != 0 && Section != 4 && Section != 8)        // 현재 위치가 0, 4, 8 이 아닐 때(맨 왼쪽)
                {
                    playerTargetSection = Section--;          // 왼쪽으로 이동할 때는 -1 만큼
                }
                break;

            // 더블 오른쪽 이동
            case 9:
                if (Section == 2 || Section == 6 || Section == 10)
                {
                    playerTargetSection = Section++;
                }
                else if (Section != 2 && Section != 3 && Section != 6 && Section != 7 && Section != 10 && Section != 11)
                {
                    playerTargetSection = Section + 2;
                }
                break;

            // 더블 왼쪽 이동
            case 10:
                if (Section == 1 || Section == 5 || Section == 9)
                {
                    playerTargetSection = Section--;
                }
                else if (Section != 0 && Section != 1 && Section != 4 && Section != 5 && Section != 8 && Section != 9)
                {
                    playerTargetSection = Section - 2;
                }
                break;
        }
    }

    /// <summary>
    /// 이동 카드 선택시 현재 위치로부터 이동할 위치를 계산하는 함수
    /// </summary>
    /// <param name="cardIndex">현재 사용 할 카드 번호</param>
    /// <param name="Section">현재 위치</param>
    private void EnemyCharacterMove(int cardIndex, int Section)
    {
        switch (cardIndex)
        {
            // 아래로 이동
            case 0:
                if (Section > 3)        // 현재 위치가 4 5 6 7 일 때(윗줄)
                {
                    enemyTargetSection = Section - 4;       // 아래로 이동할 때는 -4 만큼
                }
                break;

            // 위로 이동
            case 1:
                if (Section < 8)        // 현재 위치가 0 1 2 3 일 때(아래줄)  4 -> 8
                {
                    enemyTargetSection = Section + 4;       // 위로 이동할 때는 +4 만큼 델리게이트로 전송
                }
                break;

            // 오른쪽으로 이동
            case 2:
                if (Section != 3 && Section != 7 && Section != 11)        // 현재 위치가 3, 7, 11 이 아닐 때(맨 오른쪽)
                {
                    enemyTargetSection = Section++;          // 오른쪽으로 이동할 때는 +1 만큼
                }
                break;

            // 왼쪽으로 이동
            case 3:
                if (Section != 0 && Section != 4 && Section != 8)        // 현재 위치가 0, 4, 8 이 아닐 때(맨 왼쪽)
                {
                    enemyTargetSection = Section--;          // 왼쪽으로 이동할 때는 -1 만큼
                }
                break;

            // 더블 오른쪽 이동
            case 9:
                if (Section == 2 || Section == 6 || Section == 10)
                {
                    enemyTargetSection = Section++;
                }
                else if (Section != 2 && Section != 3 && Section != 6 && Section != 7 && Section != 10 && Section != 11)
                {
                    enemyTargetSection = Section + 2;
                }
                break;

            // 더블 왼쪽 이동
            case 10:
                if (Section == 1 || Section == 5 || Section == 9)
                {
                    enemyTargetSection = Section--;
                }
                else if (Section != 0 && Section != 1 && Section != 4 && Section != 5 && Section != 8 && Section != 9)
                {
                    enemyTargetSection = Section - 2;
                }
                break;
            default:
                enemyTargetSection = Section;
                break;
        }
    }
}
