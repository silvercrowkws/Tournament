using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Collections.Specialized.BitVector32;

public class ActivePlayer : Singleton<ActivePlayer>
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
    /// 플레이어가 움직일 위치
    /// </summary>
    int playerTargetSection = 0;

    /// <summary>
    /// 적이 움직일 위치
    /// </summary>
    int enemyTargetSection = 0;

    /// <summary>
    /// 현재 진행된 라운드
    /// </summary>
    int round = 0;

    private void Start()
    {
        gameManager = GameManager.Instance;
        turnManager = TurnManager.Instance;
        player = gameManager.Player;
        enemyPlayer = gameManager.EnemyPlayer;
        controlZone = FindAnyObjectByType<ControlZone>();
        board = FindAnyObjectByType<Board>();

        SceneManager.sceneLoaded += OnSceneLoaded;


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
        //turnManager.onTurnStart += (_) => OnPlay();
        turnManager.onTurnStart += OnRoundPlay;

        player.currentSection += PlayerSction;
        enemyPlayer.currentSection += EnemyPlayerSction;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if(scene.buildIndex == 2)
        {
            player = gameManager.Player;
            enemyPlayer = gameManager.EnemyPlayer;
            controlZone = FindAnyObjectByType<ControlZone>();
            Debug.Log("ControlZone 새로 찾음!");
            Debug.LogWarning("플레이어 새로 찾음!");
            Debug.LogWarning("적 플레이어 새로 찾음!");
        }
    }

    /// <summary>
    /// 라운드를 시작시키는 함수
    /// </summary>
    /// <param name="roundNumber">라운드 숫자</param>
    private void OnRoundPlay(int roundNumber)
    {
        round = roundNumber;
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
            Debug.Log("노말모드 들어옴");
            NormalMode();
        }

        int activeNumber = 0;                               // 몇번째 행동인지 확인하는 변수
        //int enemyActiveNumver = 0;                          // 적의 몇번째 행동인지 확인하는 변수

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
                yield return StartCoroutine(WaitForSecond(1.5f));               // 적의 행동이 끝나고 잠시 기다림
                PlayerActiveCard(controlZone.firstTurnCardIndex);               // 적의 행동이 끝나고 플레이어의 행동 시작(수비면 행동이 끝나는 상황이 아님)
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
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EfirstTurnCardIndex);                           // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.firstTurnCardIndex);
                //yield return StartCoroutine(WaitForPlayerAction());
                yield return StartCoroutine(WaitForSecond(1.5f));               // 모두의 행동을 잠시 지속
                EnemyActiveCard(EfirstTurnCardIndex);
                //yield return StartCoroutine(WaitForEnemyPlayerAction());
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("첫 번째 행동 완료");
        yield return StartCoroutine(WaitForSecond(2));                 // 2초 대기


        // 만약 첫번째 행동에서 게임이 종료된 상황이면
        if (gameManager.gameOver)
        {
            Debug.Log("게임 오버");
            player.playerActiveEnd = true;      // 가끔 무한 루프 이유가 이거인것 같아서 해봄
            enemyPlayer.enemyActiveEnd = true;
            yield break;        // 코루틴을 종료
        }


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
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EsecondTurnCardIndex);                          // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.secondTurnCardIndex);
                yield return StartCoroutine(WaitForSecond(1.5f));               // 모두의 행동을 잠시 지속
                EnemyActiveCard(EsecondTurnCardIndex);
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("두 번째 행동 완료");
        yield return StartCoroutine(WaitForSecond(2));                 // 2초 대기


        // 만약 두번째 행동에서 게임이 종료된 상황이면
        if (gameManager.gameOver)
        {
            Debug.Log("게임 오버");
            player.playerActiveEnd = true;      // 가끔 무한 루프 이유가 이거인것 같아서 해봄
            enemyPlayer.enemyActiveEnd = true;
            yield break;        // 코루틴을 종료
        }


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
                yield return StartCoroutine(WaitForSecond(1.5f));               // 플레이어의 행동이 끝나고 잠시 기다림
                EnemyActiveCard(EthirdTurnCardIndex);                           // 적의 행동 실행
                yield return StartCoroutine(WaitForEnemyPlayerAction());
            }
            // 적의 행동이 수비면 플레이어 먼저 행동 / 둘 다 수비인 경우에는 행동이 끝났음을 표시해 줘야 함
            else
            {
                PlayerActiveCard(controlZone.thirdTurnCardIndex);
                yield return StartCoroutine(WaitForSecond(1.5f));               // 모두의 행동을 잠시 지속
                EnemyActiveCard(EthirdTurnCardIndex);
                player.playerActiveEnd = true;          // 플레이어의 행동이 끝났음을 표시
                enemyPlayer.enemyActiveEnd = true;      // 적의 행동이 끝났음을 표시
            }
        }

        // 플레이어와 적의 행동이 끝나기를 기다리고
        yield return StartCoroutine(WaitForPlayerAction());
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        Debug.Log("세 번째 행동 완료");
        yield return StartCoroutine(WaitForSecond(2));                 // 2초 대기

        // 만약 세번째 행동에서 게임이 종료된 상황이면
        if (gameManager.gameOver)
        {
            Debug.Log("게임 오버");
            player.playerActiveEnd = true;      // 가끔 무한 루프 이유가 이거인것 같아서 해봄
            enemyPlayer.enemyActiveEnd = true;
            yield break;        // 코루틴을 종료
        }

        gameManager.isPlayerDone = true;                    // 행동을 완료했다고 표시
        gameManager.isEnemyPlayerDone = true;               // 행동을 완료했다고 표시
    }

    /// <summary>
    /// 카드의 인덱스에 따라 행동을 실행하는 함수
    /// </summary>
    /// <param name="cardIndex">실행시킬 행동(카드)의 인덱스</param>
    private void PlayerActiveCard(int cardIndex)
    {
        Debug.LogWarning("PlayerActiveCard 호출됨");
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
    /// 노말 모드로 플레이 할 경우의 적 플레이어의 행동
    /// </summary>
    private void NormalMode()
    {
        Debug.Log("NormalMode 함수 실행");
        // 노말 모드일 경우
        EfirstTurnCardIndex = 0;
        EsecondTurnCardIndex = 0;
        EthirdTurnCardIndex = 0;

        playerTargetSection = player.currentSectionIndex;           // 움직일 위치 초기값을 현재 위치와 동기화
        enemyTargetSection = enemyPlayer.EcurrentSectionIndex;

        /*Debug.Log($"노말모드로 초기화된 플레이어와 적의 위치");
        Debug.Log($"플레이어 : {playerTargetSection}");
        Debug.Log($"플레이어 : {player.currentSectionIndex}");

        Debug.Log($"적 : {enemyTargetSection}");
        Debug.Log($"적 : {enemyPlayer.EcurrentSectionIndex}");*/

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

                    int[] numbers = { 0, 1, 2, 3, 9, 10 };
                    int randomCard;
                    int randomIndex;

                    randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                    randomCard = numbers[randomIndex];      // 0 1 2 3 9 10 중에서 선택

                    EfirstTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
                    EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                }
                else
                {
                    // 움직이지 않기로 함
                    Debug.LogWarning("30% 확률로 움직이지 않기로 함");
                    // 움직이지 않기로 했는데 플레이어가 적의 공격 범위에 있으면 공격하고
                    // 공격 범위에 없으면 딴거하는게?

                    // 적의 현재 위치에서 공격 범위 계산
                    CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

                    // 공격 범위에 포함되는 카드들만 선택
                    List<int> availableCards = new List<int>();

                    // 움직이지 않기로 했는데 플레이어가 적의 공격 범위에 있으면
                    if (attackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("1턴 플레이어가 공격할건데 적이 기본 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 기본 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (enemyPlayer.Energy >= attackCost)
                        {
                            availableCards.Add(5);
                            Debug.Log("에너지가 기본 공격 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("하지만 에너지가 기본 공격 코스트 미만이라 할 수 없다");
                        }
                    }

                    if (magicAttackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("1턴 플레이어가 공격할건데 적이 마법 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 마법 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (enemyPlayer.Energy >= magicAttackCost)
                        {
                            availableCards.Add(6);
                            Debug.Log("에너지가 마법 공격 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("하지만 에너지가 마법 공격 코스트 미만이라 할 수 없다");
                        }
                    }

                    if (limitAttackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("1턴 플레이어가 공격할건데 적이 특수 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 특수 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (enemyPlayer.Energy >= limitAttackCost)
                        {
                            availableCards.Add(7);
                            Debug.Log("에너지가 특수 공격 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("하지만 에너지가 특수 공격 코스트 미만이라 할 수 없다");
                        }
                    }

                    // 적의 공격 범위에 하나라도 포함되면
                    if (availableCards.Count > 0)
                    {
                        int randomIndex;
                        int randomCard;
                        int cardCost = 0;

                        // 여기 들어왔다는 것은 availableCards[0]은 최소 5가 보장된다는 의미임(기본 공격 코스트 이상이기 때문에?)

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
                        availableCards.Clear();
                    }
                    // 적의 공격 범위에 하나도 포함되지 않으면
                    else
                    {
                        // 에너지업, 움직임 0 1 2 3 8
                        if (UnityEngine.Random.value < 0.3f)     // 30% 확률로
                        {
                            EfirstTurnCardIndex = 8;            // 에너지 회복
                        }
                        else
                        {
                            int[] numbers = { 0, 1, 2, 3, 8 };
                            int randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                            EfirstTurnCardIndex = numbers[randomIndex];
                            EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                        }
                    }
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                // 여기에 에너지 회복하는 부분을 우선적으로 하는게?
                Debug.LogWarning("1턴 플레이어가 공격할건데, 적이 공격 범위에 없음");

                if (UnityEngine.Random.value < 0.3f)     // 30% 확률로 에너지 회복 먼저
                {
                    Debug.Log("30% 확률로 에너지 회복 하기로 함");
                    EfirstTurnCardIndex = 8;            // 8이 에너지 회복
                }
                else
                {
                    Debug.Log("70% 확률로 움직이기로 함");
                    int randomCard = UnityEngine.Random.Range(0, 4);        // 0 1 2 3 중 1개 선택
                    EfirstTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
                    EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                }
            }
        }
        // 플레이어가 첫턴에 움직일 예정이면
        else if (controlZone.firstTurnCardIndex == 0 || controlZone.firstTurnCardIndex == 1 || controlZone.firstTurnCardIndex == 2 || controlZone.firstTurnCardIndex == 3 ||
            controlZone.firstTurnCardIndex == 9 || controlZone.firstTurnCardIndex == 10)
        {
            Debug.Log($"첫번째로 움직이기 전 플레이어의 위치 {playerTargetSection}");
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.firstTurnCardIndex, playerTargetSection);
            Debug.Log($"첫번째로 플레이어의 움직일 위치 {playerTargetSection}");

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (attackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(5);
                    Debug.Log("에너지가 기본 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("하지만 에너지가 기본 공격 코스트 미만이라 할 수 없다");
                }
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (enemyPlayer.Energy >= magicAttackCost)
                {
                    availableCards.Add(6);
                    Debug.Log("에너지가 마법 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("하지만 에너지가 마법 공격 코스트 미만이라 할 수 없다");
                }
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (enemyPlayer.Energy >= limitAttackCost)
                {
                    availableCards.Add(7);
                    Debug.Log("에너지가 특수 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("하지만 에너지가 특수 공격 코스트 미만이라 할 수 없다");
                }
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;
                int cardCost = 0;

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
                availableCards.Clear();
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("1턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정(에너지 회복 30% 추가)");
                int randomCard;
                int randomIndex;
                int cardCost = 0;

                // 첫 라운드이면 왼쪽, 위쪽, 아래쪽으로 움직임
                if (round == 1)
                {
                    Debug.LogWarning("첫 라운드에서는 무조건 왼쪽 or 위쪽 or 아래쪽으로 움직임");
                    int[] numbers = { 0, 1, 3, 10 };
                    randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                    randomCard = numbers[randomIndex];      // 0 1 3 10 중에서 선택
                    EfirstTurnCardIndex = randomCard;
                    EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                }
                // 첫 라운드가 아니면
                else
                {
                    if (UnityEngine.Random.value < 0.3f)     // 30% 확률로
                    {
                        EfirstTurnCardIndex = 8;             // 에너지 회복
                    }
                    else
                    {
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
                        while (enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                        EfirstTurnCardIndex = randomCard;
                        EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                    }
                }
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤 => 에너지 회복 확률 추가
            Debug.Log("1턴 플레이어가 공격, 이동을 안해서 진짜 랜덤 => 에너지 회복 확률 추가");

            if (UnityEngine.Random.value < 0.3f)
            {
                EfirstTurnCardIndex = 8;
            }
            else
            {
                int randomCard = UnityEngine.Random.Range(0, 13);
                EfirstTurnCardIndex = randomCard;
                EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
            }
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

                    int[] numbers = { 0, 1, 2, 3, 9, 10 };
                    int randomCard;
                    int randomIndex;
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                        randomCard = numbers[randomIndex];      // 0 1 2 3 9 10 중에서 선택
                    }
                    while (randomCard == EfirstTurnCardIndex);


                    EsecondTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
                    EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                }
                else
                {
                    // 움직이지 않기로 함
                    Debug.LogWarning("30% 확률로 움직이지 않기로 함");
                    // 움직이지 않기로 했는데 플레이어가 적의 공격 범위에 있으면 공격하고
                    // 공격 범위에 없으면 딴거하는게?

                    // 적의 현재 위치에서 공격 범위 계산
                    CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

                    // 공격 범위에 포함되는 카드들만 선택
                    List<int> availableCards = new List<int>();

                    // 움직이지 않기로 했는데 플레이어가 적의 공격 범위에 있으면
                    if (attackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("2턴 플레이어가 공격할건데 적이 기본 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 기본 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (EfirstTurnCardIndex != 5 && enemyPlayer.Energy >= attackCost)
                        {
                            availableCards.Add(5);
                            Debug.Log("첫 턴에 기본 공격을 안했고, 에너지가 기본 공격 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("첫 턴에 기본 공격을 했거나, 에너지가 기본 공격 코스트 미만");
                        }
                    }

                    if (magicAttackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("2턴 플레이어가 공격할건데 적이 마법 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 마법 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (EfirstTurnCardIndex != 6 && enemyPlayer.Energy >= magicAttackCost)
                        {
                            availableCards.Add(6);
                            Debug.Log("첫 턴에 마법 공격을 안했고, 에너지가 기본 마법 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("첫 턴에 마법 공격을 했거나, 에너지가 마법 공격 코스트 미만");
                        }
                    }

                    if (limitAttackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("2턴 플레이어가 공격할건데 적이 특수 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 특수 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (EfirstTurnCardIndex != 7 && enemyPlayer.Energy >= limitAttackCost)
                        {
                            availableCards.Add(7);
                            Debug.Log("첫 턴에 특수 공격을 안했고, 에너지가 특수 공격 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("첫 턴에 특수 공격을 했거나, 에너지가 특수 공격 코스트 미만");
                        }
                    }

                    // 적의 공격 범위에 하나라도 포함되면
                    if (availableCards.Count > 0)
                    {
                        int randomIndex;
                        int randomCard;

                        randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                        randomCard = availableCards[randomIndex];

                        EsecondTurnCardIndex = randomCard;

                        availableCards.Clear();
                    }
                    // 적의 공격 범위에 하나도 포함되지 않으면
                    else
                    {
                        // 에너지업, 움직임 0 1 2 3 8

                        if (EfirstTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)     // 30% 확률로
                        {
                            EsecondTurnCardIndex = 8;            // 에너지 회복
                        }
                        else
                        {
                            int[] numbers = { 0, 1, 2, 3, 8 };
                            int randomIndex;
                            do
                            {
                                randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                            }
                            while (randomIndex == EfirstTurnCardIndex);
                            EsecondTurnCardIndex = numbers[randomIndex];
                            EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                        }
                    }
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.LogWarning("2턴 플레이어가 공격할건데, 적이 공격 범위에 없음");

                if (EfirstTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)     // 30% 확률로 에너지 회복 먼저
                {
                    Debug.Log("30% 확률로 에너지 회복 하기로 함");
                    EsecondTurnCardIndex = 8;            // 8이 에너지 회복
                }
                else
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
            }
        }
        // 플레이어가 두번째 턴에 움직일 예정이면
        else if (controlZone.secondTurnCardIndex == 0 || controlZone.secondTurnCardIndex == 1 || controlZone.secondTurnCardIndex == 2 || controlZone.secondTurnCardIndex == 3 ||
            controlZone.secondTurnCardIndex == 9 || controlZone.secondTurnCardIndex == 10)
        {
            Debug.Log($"두번째로 움직이기 전 플레이어의 위치 {playerTargetSection}");
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.secondTurnCardIndex, playerTargetSection);
            Debug.Log($"두번째로 플레이어의 움직일 위치 {playerTargetSection}");

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (attackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 5 && enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(5);
                    Debug.Log("첫 턴에 기본 공격을 안했고, 에너지가 기본 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("첫 턴에 기본 공격을 했거나, 에너지가 기본 공격 코스트 미만");
                }
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 6 && enemyPlayer.Energy >= magicAttackCost)
                {
                    availableCards.Add(6);
                    Debug.Log("첫 턴에 마법 공격을 안했고, 에너지가 마법 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("첫 턴에 마법 공격을 했거나, 에너지가 마법 공격 코스트 미만");
                }
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 7 && enemyPlayer.Energy >= limitAttackCost)
                {
                    availableCards.Add(7);
                    Debug.Log("첫 턴에 특수 공격을 안했고, 에너지가 특수 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("첫 턴에 특수 공격을 했거나, 에너지가 특수 공격 코스트 미만");
                }
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;

                randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                randomCard = availableCards[randomIndex];

                EsecondTurnCardIndex = randomCard;

                availableCards.Clear();
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("2턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정(에너지 회복 30% 추가)");
                int randomCard;
                int randomIndex;
                int cardCost = 0;

                if (round == 1)
                {
                    Debug.LogWarning("첫 라운드에서는 무조건 왼쪽 or 위쪽 or 아래쪽으로 움직임");
                    int[] numbers = { 0, 1, 3, 10 };
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                        randomCard = numbers[randomIndex];      // 1 3 10 중에서 선택
                    }
                    while (randomCard == EfirstTurnCardIndex);
                    EsecondTurnCardIndex = randomCard;
                    EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                }
                // 첫 라운드가 아니면
                else
                {
                    if (EfirstTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)
                    {
                        EsecondTurnCardIndex = 8;
                    }
                    else
                    {
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
                        while (randomCard == EfirstTurnCardIndex || enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                        EsecondTurnCardIndex = randomCard;
                        EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                    }
                }
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("2턴 플레이어가 공격, 이동을 안해서 진짜 랜덤 => 에너지 회복 확률 추가");
            int randomCard;
            int cardCost = 0;

            if (EfirstTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)
            {
                EsecondTurnCardIndex = 8;
            }
            else
            {
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
                while (randomCard == EfirstTurnCardIndex || enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                EsecondTurnCardIndex = randomCard;
                EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
            }
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
                    int[] numbers = { 0, 1, 2, 3, 9, 10 };
                    int randomCard;
                    int randomIndex;
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                        randomCard = numbers[randomIndex];      // 0 1 2 3 9 10 중에서 선택
                    }
                    while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);              // 같은 값이면 다시 뽑기

                    EthirdTurnCardIndex = randomCard;                      // 중복되지 않는 값 저장
                    EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                }
                else
                {
                    // 움직이지 않기로 함
                    Debug.LogWarning("30% 확률로 움직이지 않기로 함");
                    // 적의 현재 위치에서 공격 범위 계산
                    CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

                    // 공격 범위에 포함되는 카드들만 선택
                    List<int> availableCards = new List<int>();

                    // 움직이지 않기로 했는데 플레이어가 적의 공격 범위에 있으면
                    if (attackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("3턴 플레이어가 공격할건데 적이 기본 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 기본 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (EfirstTurnCardIndex != 5 && EsecondTurnCardIndex != 5 && enemyPlayer.Energy >= attackCost)
                        {
                            availableCards.Add(5);
                            Debug.Log("1,2 턴에 기본 공격을 안했고, 에너지가 기본 공격 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("1,2 턴에 기본 공격을 했거나, 에너지가 기본 공격 코스트 미만");
                        }
                    }

                    if (magicAttackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("3턴 플레이어가 공격할건데 적이 마법 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 마법 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (EfirstTurnCardIndex != 6 && EsecondTurnCardIndex != 6 && enemyPlayer.Energy >= magicAttackCost)
                        {
                            availableCards.Add(6);
                            Debug.Log("1,2 턴에 마법 공격을 안했고, 에너지가 기본 마법 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("1,2 턴에 마법 공격을 했거나, 에너지가 마법 공격 코스트 미만");
                        }
                    }

                    if (limitAttackRange.Contains(playerTargetSection))
                    {
                        Debug.Log("3턴 플레이어가 공격할건데 적이 특수 공격 범위에 있고, 30% 확률로 움직이지 않기로 함");
                        Debug.Log("적의 특수 공격 범위에 포함됨");
                        Debug.Log($"{playerTargetSection}이 포함");
                        if (EfirstTurnCardIndex != 7 && EsecondTurnCardIndex != 7 && enemyPlayer.Energy >= limitAttackCost)
                        {
                            availableCards.Add(7);
                            Debug.Log("1,2 턴에 특수 공격을 안했고, 에너지가 특수 공격 코스트 이상");
                        }
                        else
                        {
                            Debug.Log("1,2 턴에 특수 공격을 했거나, 에너지가 특수 공격 코스트 미만");
                        }
                    }

                    // 적의 공격 범위에 하나라도 포함되면
                    if (availableCards.Count > 0)
                    {
                        int randomIndex;
                        int randomCard;

                        randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                        randomCard = availableCards[randomIndex];

                        EthirdTurnCardIndex = randomCard;

                        availableCards.Clear();
                    }
                    // 적의 공격 범위에 하나도 포함되지 않으면
                    else
                    {
                        // 에너지업, 움직임 0 1 2 3 8
                        if (EfirstTurnCardIndex != 8 && EsecondTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)     // 30% 확률로
                        {
                            EthirdTurnCardIndex = 8;            // 에너지 회복
                        }
                        else
                        {
                            int[] numbers = { 0, 1, 2, 3, 8 };
                            int randomIndex;
                            do
                            {
                                randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                            }
                            while (randomIndex == EfirstTurnCardIndex || randomIndex == EsecondTurnCardIndex);

                            EthirdTurnCardIndex = numbers[randomIndex];
                            EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                        }
                    }
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.LogWarning("3턴 플레이어가 공격할건데, 적이 공격 범위에 없음");

                if (EfirstTurnCardIndex != 8 && EsecondTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)     // 30% 확률로 에너지 회복 먼저
                {
                    Debug.Log("30% 확률로 에너지 회복 하기로 함");
                    EthirdTurnCardIndex = 8;            // 8이 에너지 회복
                }
                else
                {
                    Debug.Log("70% 확률로 움직이기로 함");
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
        }
        // 플레이어가 세번째 턴에 움직일 예정이면
        else if (controlZone.thirdTurnCardIndex == 0 || controlZone.thirdTurnCardIndex == 1 || controlZone.thirdTurnCardIndex == 2 || controlZone.thirdTurnCardIndex == 3 ||
            controlZone.thirdTurnCardIndex == 9 || controlZone.thirdTurnCardIndex == 10)
        {
            Debug.Log($"세번째로 움직이기 전 플레이어의 위치 {playerTargetSection}");
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.thirdTurnCardIndex, playerTargetSection);
            Debug.Log($"세번째로 플레이어의 움직일 위치 {playerTargetSection}");

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (attackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 5 && EsecondTurnCardIndex != 5 && enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(5);
                    Debug.Log("1,2 턴에 기본 공격을 안했고, 에너지가 기본 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("1,2 턴에 기본 공격을 했거나, 에너지가 기본 공격 코스트 미만");
                }
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 6 && EsecondTurnCardIndex != 6 && enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(6);
                    Debug.Log("1,2 턴에 마법 공격을 안했고, 에너지가 마법 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("1,2 턴에 마법 공격을 했거나, 에너지가 마법 공격 코스트 미만");
                }
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 7 && EsecondTurnCardIndex != 7 && enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(7);
                    Debug.Log("1,2 턴에 특수 공격을 안했고, 에너지가 특수 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("1,2 턴에 특수 공격을 했거나, 에너지가 특수 공격 코스트 미만");
                }
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;

                randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                randomCard = availableCards[randomIndex];

                EthirdTurnCardIndex = randomCard;

                availableCards.Clear();
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("3턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정(에너지 회복 30% 추가)");
                int randomCard;
                int randomIndex;
                int cardCost = 0;

                if (round == 1)
                {
                    Debug.LogWarning("첫 라운드에서는 무조건 왼쪽 or 위쪽 or 아래쪽으로 움직임");
                    int[] numbers = { 0, 1, 3, 10 };
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                        randomCard = numbers[randomIndex];      // 1 3 10 중에서 선택
                    }
                    while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);
                    EthirdTurnCardIndex = randomCard;
                    EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                }
                else
                {
                    if (EfirstTurnCardIndex != 8 && EsecondTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)
                    {
                        EthirdTurnCardIndex = 8;
                    }
                    else
                    {
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
                        while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex || enemyPlayer.Energy <= cardCost);      // 적의 에너지 <= 코스트 면 다른게 뽑을 때까지 반복

                        EthirdTurnCardIndex = randomCard;
                        EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                    }
                }
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("3턴 플레이어가 공격, 이동을 안해서 진짜 랜덤 => 에너지 회복 확률 추가");
            int randomCard;
            int cardCost = 0;

            if (EfirstTurnCardIndex != 8 && EsecondTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)
            {
                EthirdTurnCardIndex = 8;
            }
            else
            {
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
                while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex || enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                EthirdTurnCardIndex = randomCard;
                EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
            }
        }
    }


    /// <summary>
    /// 적이 특정 방향으로 이동했을 때 도착할 위치를 계산 (실제 이동 X)
    /// </summary>
    private int SimulateEnemyMove(int move, int currentPosition)
    {
        switch (move)
        {
            case 0: // 아래로 이동
                if (currentPosition > 3)  // 현재 위치가 4, 5, 6, 7 일 때(윗줄)
                {
                    return currentPosition - 4; // 아래로 이동할 때는 -4 만큼
                }
                break;

            case 1: // 위로 이동
                if (currentPosition < 8)  // 현재 위치가 0, 1, 2, 3 일 때(아래줄)  4 -> 8
                {
                    return currentPosition + 4; // 위로 이동할 때는 +4 만큼
                }
                break;

            case 2: // 오른쪽으로 이동
                if (currentPosition != 3 && currentPosition != 7 && currentPosition != 11)  // 현재 위치가 3, 7, 11 이 아닐 때(맨 오른쪽)
                {
                    return currentPosition + 1; // 오른쪽으로 이동할 때는 +1 만큼
                }
                break;

            case 3: // 왼쪽으로 이동
                if (currentPosition != 0 && currentPosition != 4 && currentPosition != 8)  // 현재 위치가 0, 4, 8 이 아닐 때(맨 왼쪽)
                {
                    return currentPosition - 1; // 왼쪽으로 이동할 때는 -1 만큼
                }
                break;

            case 9: // 더블 오른쪽 이동
                if (currentPosition == 2 || currentPosition == 6 || currentPosition == 10)
                {
                    return currentPosition + 1;
                }
                else if (currentPosition != 2 && currentPosition != 3 && currentPosition != 6 && currentPosition != 7 && currentPosition != 10 && currentPosition != 11)
                {
                    return currentPosition + 2;
                }
                break;

            case 10: // 더블 왼쪽 이동
                if (currentPosition == 1 || currentPosition == 5 || currentPosition == 9)
                {
                    return currentPosition + 1;
                }
                else if (currentPosition != 0 && currentPosition != 1 && currentPosition != 4 && currentPosition != 5 && currentPosition != 8 && currentPosition != 9)
                {
                    return currentPosition - 2;
                }
                break;

            default:
                break;
        }

        // 이동할 수 없다면, 현재 위치를 그대로 반환
        return currentPosition;
    }

    /// <summary>
    /// 플레이어 공격 시 적이 공격 범위에 없을 때 사용되는 함수
    /// </summary>
    /// <param name="turnCardIndex"></param>
    void HandleEnemyOutOfRange(ref int turnCardIndex)
    {
        // 적의 현재 위치에서 공격 범위 계산
        CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

        // 공격 범위에 포함되는 카드들만 선택
        List<int> availableCards = new List<int>();

        if (attackRange.Contains(playerTargetSection))
        {
            Debug.Log("적의 기본 공격 범위에 포함됨");
            Debug.Log($"{playerTargetSection}이 포함");
            if (enemyPlayer.Energy >= attackCost)
            {
                availableCards.Add(5);
                Debug.Log("에너지가 기본 공격 코스트 이상");
            }
            else
            {
                Debug.Log("하지만 에너지가 기본 공격 코스트 미만이라 할 수 없다");
            }
        }

        if (magicAttackRange.Contains(playerTargetSection))
        {
            Debug.Log("적의 마법 공격 범위에 포함됨");
            Debug.Log($"{playerTargetSection}이 포함");
            if (enemyPlayer.Energy >= magicAttackCost)
            {
                availableCards.Add(6);
                Debug.Log("에너지가 마법 공격 코스트 이상");
            }
            else
            {
                Debug.Log("하지만 에너지가 마법 공격 코스트 미만이라 할 수 없다");
            }
        }

        if (limitAttackRange.Contains(playerTargetSection))
        {
            Debug.Log("적의 특수 공격 범위에 포함됨");
            Debug.Log($"{playerTargetSection}이 포함");
            if (enemyPlayer.Energy >= limitAttackCost)
            {
                availableCards.Add(7);
                Debug.Log("에너지가 특수 공격 코스트 이상");
            }
            else
            {
                Debug.Log("하지만 에너지가 특수 공격 코스트 미만이라 할 수 없다");
            }
        }

        // 적의 공격 범위에 하나라도 포함되면
        if (availableCards.Count > 0)
        {
            int randomIndex;
            int randomCard;
            int cardCost = 0;

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

            // 선택된 카드 값을 turnCardIndex에 할당
            turnCardIndex = randomCard;
            availableCards.Clear();
        }
        // 적의 공격 범위에 하나도 포함되지 않으면
        else
        {
            // 에너지업, 움직임 0 1 2 3 8
            if (UnityEngine.Random.value < 0.3f)     // 30% 확률로
            {
                turnCardIndex = 8;            // 에너지 회복
            }
            else
            {
                int[] numbers = { 0, 1, 2, 3, 8 };
                int randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                turnCardIndex = numbers[randomIndex];
                EnemyCharacterMove(turnCardIndex, enemyTargetSection);
            }
        }
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

        playerTargetSection = player.currentSectionIndex;           // 움직일 위치 초기값을 현재 위치와 동기화
        enemyTargetSection = enemyPlayer.EcurrentSectionIndex;

        /*Debug.Log($"하드모드로 초기화된 플레이어와 적의 위치");
        Debug.Log($"플레이어 : {playerTargetSection}");
        Debug.Log($"플레이어 : {player.currentSectionIndex}");
        
        Debug.Log($"적 : {enemyTargetSection}");
        Debug.Log($"적 : {enemyPlayer.EcurrentSectionIndex}");*/

        List<int> safeMoves = new List<int>();

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
                //Debug.LogWarning("1턴 플레이어가 공격 할건데, 적이 공격 범위에 있음");

                // 플레이어가 기본 공격을 했을 때
                if (controlZone.firstTurnCardIndex == 5)
                {
                    // 적이 플레이어의 기본 공격 범위에 있으면
                    if (attackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 기본 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!attackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            EfirstTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EfirstTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default:
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (enemyPlayer.Energy < cardCost);

                            EfirstTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 기본 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EfirstTurnCardIndex);
                    }
                }

                // 플레이어가 마법 공격을 했을 때
                else if(controlZone.firstTurnCardIndex == 6)
                {
                    // 적이 플레이어의 마법 공격 범위에 있으면
                    if (magicAttackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 마법 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!magicAttackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            EfirstTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EfirstTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default:
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (enemyPlayer.Energy < cardCost);

                            EfirstTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 마법 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EfirstTurnCardIndex);
                    }
                }

                // 플레이어가 특수 공격을 했을 때
                else if (controlZone.firstTurnCardIndex == 7)
                {
                    // 적이 플레이어의 특수 공격 범위에 있으면
                    if (limitAttackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 특수 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!limitAttackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            //Debug.Log(safeMoves.Count);
                            EfirstTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EfirstTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default :
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (enemyPlayer.Energy < cardCost);

                            EfirstTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 특수 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EfirstTurnCardIndex);
                    }
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                // 여기에 에너지 회복하는 부분을 우선적으로 하는게?
                Debug.LogWarning("1턴 플레이어가 공격할건데, 적이 공격 범위에 없음");

                if(UnityEngine.Random.value < 0.3f)     // 30% 확률로 에너지 회복 먼저
                {
                    Debug.Log("30% 확률로 에너지 회복 하기로 함");
                    EfirstTurnCardIndex = 8;            // 8이 에너지 회복
                }
                else
                {
                    Debug.Log("70% 확률로 움직이기로 함");
                    //공격 범위에 없는데 그냥 움직이는게 맞나?
                    int randomCard = UnityEngine.Random.Range(0, 4);        // 0 1 2 3 중 1개 선택
                    EfirstTurnCardIndex = randomCard;                       // 나중에 썼던 카드 또 쓰면 안됨
                    EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                }
            }
        }
        // 플레이어가 첫턴에 움직일 예정이면
        else if (controlZone.firstTurnCardIndex == 0 || controlZone.firstTurnCardIndex == 1 || controlZone.firstTurnCardIndex == 2 || controlZone.firstTurnCardIndex == 3 ||
            controlZone.firstTurnCardIndex == 9 || controlZone.firstTurnCardIndex == 10)
        {
            Debug.Log($"첫번째로 움직이기 전 플레이어의 위치 {playerTargetSection}");
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.firstTurnCardIndex, playerTargetSection);
            Debug.Log($"첫번째로 플레이어의 움직일 위치 {playerTargetSection}");

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (attackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(5);
                    Debug.Log("에너지가 기본 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("하지만 에너지가 기본 공격 코스트 미만이라 할 수 없다");
                }
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (enemyPlayer.Energy >= magicAttackCost)
                {
                    availableCards.Add(6);
                    Debug.Log("에너지가 마법 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("하지만 에너지가 마법 공격 코스트 미만이라 할 수 없다");
                }
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("1턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (enemyPlayer.Energy >= limitAttackCost)
                {
                    availableCards.Add(7);
                    Debug.Log("에너지가 특수 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("하지만 에너지가 특수 공격 코스트 미만이라 할 수 없다");
                }
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;
                int cardCost = 0;

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
                availableCards.Clear();
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("1턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정(에너지 회복 30% 추가)");
                int randomCard;
                int randomIndex;
                int cardCost = 0;

                // 첫 라운드이면 왼쪽, 위쪽, 아래쪽으로 움직임
                if (round == 1)
                {
                    Debug.LogWarning("첫 라운드에서는 무조건 왼쪽 or 위쪽 or 아래쪽으로 움직임");
                    int[] numbers = { 0, 1, 3, 10 };
                    randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                    randomCard = numbers[randomIndex];      // 0 1 3 10 중에서 선택
                    EfirstTurnCardIndex = randomCard;
                    EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                }
                // 첫 라운드가 아니면
                else
                {
                    if (UnityEngine.Random.value < 0.3f)     // 30% 확률로
                    {
                        EfirstTurnCardIndex = 8;             // 에너지 회복
                    }
                    else
                    {
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
                        while (enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                        EfirstTurnCardIndex = randomCard;
                        EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
                    }
                }
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤 => 에너지 회복 확률 추가
            Debug.Log("1턴 플레이어가 공격, 이동을 안해서 진짜 랜덤 => 에너지 회복 확률 추가");

            if (UnityEngine.Random.value < 0.3f)
            {
                EfirstTurnCardIndex = 8;
            }
            else
            {
                int randomCard = UnityEngine.Random.Range(0, 13);
                EfirstTurnCardIndex = randomCard;
                EnemyCharacterMove(EfirstTurnCardIndex, enemyTargetSection);
            }
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
                //Debug.LogWarning("1턴 플레이어가 공격 할건데, 적이 공격 범위에 있음");

                // 플레이어가 기본 공격을 했을 때
                if (controlZone.secondTurnCardIndex == 5)
                {
                    // 적이 플레이어의 기본 공격 범위에 있으면
                    if (attackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 기본 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            if (move == EfirstTurnCardIndex)        // 첫번째 번호와 같은 것은 아래 코드 실행하지 않음
                            {
                                continue;
                            }

                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!attackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            EsecondTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EsecondTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default:
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (randomIndex == EfirstTurnCardIndex || enemyPlayer.Energy < cardCost);

                            EsecondTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 기본 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EsecondTurnCardIndex);
                    }
                }

                // 플레이어가 마법 공격을 했을 때
                else if (controlZone.secondTurnCardIndex == 6)
                {
                    // 적이 플레이어의 마법 공격 범위에 있으면
                    if (magicAttackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 마법 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            if (move == EfirstTurnCardIndex)        // 첫번째 번호와 같은 것은 아래 코드 실행하지 않음
                            {
                                continue;
                            }

                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!magicAttackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            EsecondTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EsecondTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default:
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (randomIndex == EfirstTurnCardIndex || enemyPlayer.Energy < cardCost);

                            EsecondTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 마법 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EsecondTurnCardIndex);
                    }
                }

                // 플레이어가 특수 공격을 했을 때
                else if (controlZone.secondTurnCardIndex == 7)
                {
                    // 적이 플레이어의 특수 공격 범위에 있으면
                    if (limitAttackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 특수 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            if (move == EfirstTurnCardIndex)        // 첫번째 번호와 같은 것은 아래 코드 실행하지 않음
                            {
                                continue;
                            }

                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!limitAttackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            //Debug.Log(safeMoves.Count);
                            EsecondTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EsecondTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default:
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (randomIndex == EfirstTurnCardIndex || enemyPlayer.Energy < cardCost);

                            EsecondTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 특수 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EsecondTurnCardIndex);
                    }
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.LogWarning("2턴 플레이어가 공격할건데, 적이 공격 범위에 없음");

                if (EfirstTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)     // 30% 확률로 에너지 회복 먼저
                {
                    Debug.Log("30% 확률로 에너지 회복 하기로 함");
                    EsecondTurnCardIndex = 8;            // 8이 에너지 회복
                }
                else
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
            }
        }
        // 플레이어가 두번째 턴에 움직일 예정이면
        else if (controlZone.secondTurnCardIndex == 0 || controlZone.secondTurnCardIndex == 1 || controlZone.secondTurnCardIndex == 2 || controlZone.secondTurnCardIndex == 3 ||
            controlZone.secondTurnCardIndex == 9 || controlZone.secondTurnCardIndex == 10)
        {
            Debug.Log($"두번째로 움직이기 전 플레이어의 위치 {playerTargetSection}");
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.secondTurnCardIndex, playerTargetSection);
            Debug.Log($"두번째로 플레이어의 움직일 위치 {playerTargetSection}");

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (attackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 5 && enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(5);
                    Debug.Log("첫 턴에 기본 공격을 안했고, 에너지가 기본 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("첫 턴에 기본 공격을 했거나, 에너지가 기본 공격 코스트 미만");
                }
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 6 && enemyPlayer.Energy >= magicAttackCost)
                {
                    availableCards.Add(6);
                    Debug.Log("첫 턴에 마법 공격을 안했고, 에너지가 마법 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("첫 턴에 마법 공격을 했거나, 에너지가 마법 공격 코스트 미만");
                }
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("2턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 7 && enemyPlayer.Energy >= limitAttackCost)
                {
                    availableCards.Add(7);
                    Debug.Log("첫 턴에 특수 공격을 안했고, 에너지가 특수 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("첫 턴에 특수 공격을 했거나, 에너지가 특수 공격 코스트 미만");
                }
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;

                randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                randomCard = availableCards[randomIndex];

                EsecondTurnCardIndex = randomCard;
                
                availableCards.Clear();
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("2턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정(에너지 회복 30% 추가)");
                int randomCard;
                int randomIndex;
                int cardCost = 0;

                if (round == 1)
                {
                    Debug.LogWarning("첫 라운드에서는 무조건 왼쪽 or 위쪽 or 아래쪽으로 움직임");
                    int[] numbers = { 0, 1, 3, 10 };
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                        randomCard = numbers[randomIndex];      // 1 3 10 중에서 선택
                    }
                    while (randomCard == EfirstTurnCardIndex);
                    EsecondTurnCardIndex = randomCard;
                    EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                }
                // 첫 라운드가 아니면
                else
                {
                    if (EfirstTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)
                    {
                        EsecondTurnCardIndex = 8; 
                    }
                    else
                    {
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
                        while (randomCard == EfirstTurnCardIndex || enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                        EsecondTurnCardIndex = randomCard;
                        EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
                    }
                }
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("2턴 플레이어가 공격, 이동을 안해서 진짜 랜덤 => 에너지 회복 확률 추가");
            int randomCard;
            int cardCost = 0;

            if (EfirstTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)
            {
                EsecondTurnCardIndex = 8;
            }
            else
            {
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
                while (randomCard == EfirstTurnCardIndex || enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                EsecondTurnCardIndex = randomCard;
                EnemyCharacterMove(EsecondTurnCardIndex, enemyTargetSection);
            }
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
                //Debug.LogWarning("1턴 플레이어가 공격 할건데, 적이 공격 범위에 있음");

                // 플레이어가 기본 공격을 했을 때
                if (controlZone.thirdTurnCardIndex == 5)
                {
                    // 적이 플레이어의 기본 공격 범위에 있으면
                    if (attackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 기본 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            if (move == EfirstTurnCardIndex || move == EsecondTurnCardIndex)        // 첫, 두번째 번호와 같은 것은 아래 코드 실행하지 않음
                            {
                                continue;
                            }

                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!attackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            EthirdTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EthirdTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default:
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (randomIndex == EfirstTurnCardIndex || randomIndex == EsecondTurnCardIndex || enemyPlayer.Energy < cardCost);

                            EthirdTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 기본 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EthirdTurnCardIndex);
                    }
                }

                // 플레이어가 마법 공격을 했을 때
                else if (controlZone.thirdTurnCardIndex == 6)
                {
                    // 적이 플레이어의 마법 공격 범위에 있으면
                    if (magicAttackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 마법 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            if (move == EfirstTurnCardIndex || move == EsecondTurnCardIndex)       // 첫, 두번째 번호와 같은 것은 아래 코드 실행하지 않음
                            {
                                continue;
                            }

                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!magicAttackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            EthirdTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EthirdTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default:
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (randomIndex == EfirstTurnCardIndex || randomIndex == EsecondTurnCardIndex || enemyPlayer.Energy < cardCost);

                            EthirdTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 마법 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EthirdTurnCardIndex);
                    }
                }

                // 플레이어가 특수 공격을 했을 때
                else if (controlZone.thirdTurnCardIndex == 7)
                {
                    // 적이 플레이어의 특수 공격 범위에 있으면
                    if (limitAttackRange.Contains(enemyPlayer.EcurrentSectionIndex))
                    {
                        Debug.LogWarning("적이 특수 공격 범위에 있음, 피할 수 있는지 확인");

                        int[] moveOptions = { 0, 1, 2, 3, 9, 10 };

                        // 이동 가능한 방향 체크
                        foreach (int move in moveOptions)
                        {
                            if (move == EfirstTurnCardIndex || move == EsecondTurnCardIndex)        // 첫번째 번호와 같은 것은 아래 코드 실행하지 않음
                            {
                                continue;
                            }

                            int newEnemyPosition = SimulateEnemyMove(move, enemyPlayer.EcurrentSectionIndex);

                            if (!limitAttackRange.Contains(newEnemyPosition))
                            {
                                safeMoves.Add(move);
                            }
                        }

                        // 피할 수 있는 방향이 있다면 랜덤하게 선택
                        if (safeMoves.Count > 0)
                        {
                            //Debug.Log(safeMoves.Count);
                            EthirdTurnCardIndex = safeMoves[UnityEngine.Random.Range(0, safeMoves.Count)];
                            EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                            //Debug.Log($"적이 {EthirdTurnCardIndex} 방향으로 이동하여 공격을 피함!");
                            safeMoves.Clear();
                        }
                        else
                        {
                            Debug.Log("적이 어떤 방향으로 이동해도 공격을 피할 수 없음!");

                            int[] randomNumber = { 4, 11, 12 };
                            int randomIndex = 0;
                            int cardCost = 0;
                            do
                            {
                                int num = 0;
                                num = UnityEngine.Random.Range(0, randomNumber.Length);

                                randomIndex = randomNumber[num];

                                switch (randomIndex)
                                {
                                    case 11:
                                        cardCost = 25;
                                        break;
                                    default:
                                        cardCost = 0;
                                        break;

                                }
                            }
                            while (randomIndex == EfirstTurnCardIndex || randomIndex == EsecondTurnCardIndex || enemyPlayer.Energy < cardCost);

                            EthirdTurnCardIndex = randomIndex;
                        }
                    }
                    // 적이 특수 공격 범위에 없음
                    else
                    {
                        HandleEnemyOutOfRange(ref EthirdTurnCardIndex);
                    }
                }
            }
            // 플레이어가 공격을 했으나 적이 공격 범위에 없음
            else
            {
                Debug.LogWarning("3턴 플레이어가 공격할건데, 적이 공격 범위에 없음");

                if (EfirstTurnCardIndex != 8 && EsecondTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)     // 30% 확률로 에너지 회복 먼저
                {
                    Debug.Log("30% 확률로 에너지 회복 하기로 함");
                    EthirdTurnCardIndex = 8;            // 8이 에너지 회복
                }
                else
                {
                    Debug.Log("70% 확률로 움직이기로 함");
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
        }
        // 플레이어가 세번째 턴에 움직일 예정이면
        else if (controlZone.thirdTurnCardIndex == 0 || controlZone.thirdTurnCardIndex == 1 || controlZone.thirdTurnCardIndex == 2 || controlZone.thirdTurnCardIndex == 3 ||
            controlZone.thirdTurnCardIndex == 9 || controlZone.thirdTurnCardIndex == 10)
        {
            Debug.Log($"세번째로 움직이기 전 플레이어의 위치 {playerTargetSection}");
            // 현재 위치로부터 움직일 위치 계산하고
            PlayerCharacterMove(controlZone.thirdTurnCardIndex, playerTargetSection);
            Debug.Log($"세번째로 플레이어의 움직일 위치 {playerTargetSection}");

            // 적의 공격 범위 확인하고
            CharacterAttackRange(gameManager.enemyPlayerCharacterIndex, enemyTargetSection);

            // 공격 범위에 포함되는 카드들만 선택
            List<int> availableCards = new List<int>();

            // 적의 기본 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (attackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 기본 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if(EfirstTurnCardIndex != 5 && EsecondTurnCardIndex != 5 && enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(5);
                    Debug.Log("1,2 턴에 기본 공격을 안했고, 에너지가 기본 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("1,2 턴에 기본 공격을 했거나, 에너지가 기본 공격 코스트 미만");
                }
            }

            // 적의 마법 공격 범위에 플레이어가(움직일 위치에) 있다면
            if (magicAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 마법 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 6 && EsecondTurnCardIndex != 6 && enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(6);
                    Debug.Log("1,2 턴에 마법 공격을 안했고, 에너지가 마법 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("1,2 턴에 마법 공격을 했거나, 에너지가 마법 공격 코스트 미만");
                }
            }

            if (limitAttackRange.Contains(playerTargetSection))
            {
                Debug.Log("3턴 플레이어가 움직였는데 적의 특수 공격 범위에 포함됨");
                Debug.Log($"{playerTargetSection}이 포함");
                if (EfirstTurnCardIndex != 7 && EsecondTurnCardIndex != 7 && enemyPlayer.Energy >= attackCost)
                {
                    availableCards.Add(7);
                    Debug.Log("1,2 턴에 특수 공격을 안했고, 에너지가 특수 공격 코스트 이상");
                }
                else
                {
                    Debug.Log("1,2 턴에 특수 공격을 했거나, 에너지가 특수 공격 코스트 미만");
                }
            }

            // 적의 특수 공격 범위에 플레이어가(움직일 위치에) 있다면
            //if (enemyPlayer.limitAttackRange != null && Array.Exists(enemyPlayer.limitAttackRange, element => element == player.Section))

            // 적의 공격 범위에 하나라도 포함되면
            if (availableCards.Count > 0)
            {
                int randomIndex;
                int randomCard;

                randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                randomCard = availableCards[randomIndex];

                EthirdTurnCardIndex = randomCard;

                availableCards.Clear();
            }
            // 적의 공격 범위에 하나라도 포함되지 않으면
            else
            {
                Debug.LogWarning("3턴 플레이어가 움직였는데 적의 공격 범위에 하나도 포함되지 않음 => 랜덤으로 결정(에너지 회복 30% 추가)");
                int randomCard;
                int randomIndex;
                int cardCost = 0;

                if (round == 1)
                {
                    Debug.LogWarning("첫 라운드에서는 무조건 왼쪽 or 위쪽 or 아래쪽으로 움직임");
                    int[] numbers = { 0, 1, 3, 10 };
                    do
                    {
                        randomIndex = UnityEngine.Random.Range(0, numbers.Length);
                        randomCard = numbers[randomIndex];      // 1 3 10 중에서 선택
                    }
                    while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex);
                    EthirdTurnCardIndex = randomCard;
                    EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                }
                else
                {
                    if (EfirstTurnCardIndex != 8 && EsecondTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)
                    {
                        EthirdTurnCardIndex = 8;
                    }
                    else
                    {
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
                        while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex || enemyPlayer.Energy <= cardCost);      // 적의 에너지 <= 코스트 면 다른게 뽑을 때까지 반복

                        EthirdTurnCardIndex = randomCard;
                        EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
                    }
                }
            }
        }
        // 플레이어가 공격, 이동을 안했으면(가드, 에너지 업, 퍼펙트 가드, 힐)
        else
        {
            // 진짜 랜덤
            Debug.Log("3턴 플레이어가 공격, 이동을 안해서 진짜 랜덤 => 에너지 회복 확률 추가");
            int randomCard;
            int cardCost = 0;

            if (EfirstTurnCardIndex != 8 && EsecondTurnCardIndex != 8 && UnityEngine.Random.value < 0.3f)
            {
                EthirdTurnCardIndex = 8;
            }
            else
            {
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
                while (randomCard == EfirstTurnCardIndex || randomCard == EsecondTurnCardIndex || enemyPlayer.Energy < cardCost);      // 적의 에너지 < 코스트 면 다른게 뽑을 때까지 반복

                EthirdTurnCardIndex = randomCard;
                EnemyCharacterMove(EthirdTurnCardIndex, enemyTargetSection);
            }
        }

    }

    public int[] attackRange = null;          // 기본 공격 범위
    public int[] magicAttackRange = null;     // 마법 공격 범위
    public int[] limitAttackRange = null;     // 리미트 공격 범위

    /// <summary>
    /// 현재 위치에서 공격 범위 계산하는 함수
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
        Debug.Log($"PlayerCharacterMove 실행 전 - Section: {Section}, playerTargetSection: {playerTargetSection}");

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
                    playerTargetSection = Section + 1;          // 오른쪽으로 이동할 때는 +1 만큼
                }
                break;

            // 왼쪽으로 이동
            case 3:
                if (Section != 0 && Section != 4 && Section != 8)        // 현재 위치가 0, 4, 8 이 아닐 때(맨 왼쪽)
                {
                    playerTargetSection = Section - 1;          // 왼쪽으로 이동할 때는 -1 만큼
                }
                break;

            // 더블 오른쪽 이동
            case 9:
                if (Section == 2 || Section == 6 || Section == 10)
                {
                    playerTargetSection = Section + 1;
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
                    playerTargetSection = Section - 1;
                }
                else if (Section != 0 && Section != 1 && Section != 4 && Section != 5 && Section != 8 && Section != 9)
                {
                    playerTargetSection = Section - 2;
                }
                break;
        }
        Debug.Log($"PlayerCharacterMove 실행 후 - playerTargetSection: {playerTargetSection}");
    }

    /// <summary>
    /// 이동 카드 선택시 현재 위치로부터 이동할 위치를 계산하는 함수
    /// </summary>
    /// <param name="cardIndex">현재 사용 할 카드 번호</param>
    /// <param name="Section">현재 위치</param>
    private void EnemyCharacterMove(int cardIndex, int Section)
    {
        Debug.Log($"EnemyCharacterMove 실행 전 - Section: {Section}, enemyTargetSection: {enemyTargetSection}");
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
                    enemyTargetSection = Section + 1;          // 오른쪽으로 이동할 때는 +1 만큼
                }
                break;

            // 왼쪽으로 이동
            case 3:
                if (Section != 0 && Section != 4 && Section != 8)        // 현재 위치가 0, 4, 8 이 아닐 때(맨 왼쪽)
                {
                    enemyTargetSection = Section - 1;          // 왼쪽으로 이동할 때는 -1 만큼     후위 연산자라 계산에 적용 안되는 경우가 있었음
                }
                break;

            // 더블 오른쪽 이동
            case 9:
                if (Section == 2 || Section == 6 || Section == 10)
                {
                    enemyTargetSection = Section + 1;
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
                    enemyTargetSection = Section + 1;
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
        Debug.Log($"EnemyCharacterMove 실행 후 - enemyTargetSection: {enemyTargetSection}");
    }
}
