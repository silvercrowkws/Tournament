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
    /// 플레이어
    /// </summary>
    Player player;

    /// <summary>
    /// 보드(플레이어의 위치를 표시하기 위함)
    /// </summary>
    Board board;

    /// <summary>
    /// 행동에 맞는 카드를 보이라고 알리는 델리게이트
    /// </summary>
    public Action<int> onNextCard;

    // 공격시 들어가는 코스트
    int attackCost = 0;
    int magicAttackCost = 0;
    int limitAttackCost = 0;
    int perfectGuardCost = 0;

    // 행동들
    int EfirstTurnCardIndex = 0;
    int EsecondTurnCardIndex = 0;
    int EthirdTurnCardIndex = 0;

    private void Start()
    {
        gameManager = GameManager.Instance;
        turnManager = TurnManager.Instance;
        enemyPlayer = gameManager.EnemyPlayer;
        player = gameManager.Player;
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

        enemyPlayer.currentSection += PlayerSction;
    }

    void OnPlay()
    {
        StartCoroutine(OnPlayerActive());
    }

    /// <summary>
    /// 적 플레이어를 카드에 맞게 행동시키는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnPlayerActive()
    {
        Debug.Log("적의 OnPlayerActive 함수");
        Debug.Log($"적이 본 플레이어의 1번째 카드 인덱스 : {controlZone.firstTurnCardIndex}");
        Debug.Log($"적이 본 플레이어의 2번째 카드 인덱스 : {controlZone.secondTurnCardIndex}");
        Debug.Log($"적이 본 플레이어의 3번째 카드 인덱스 : {controlZone.thirdTurnCardIndex}");

        // 게임 난이도에 따라
        if(gameManager.gamediff == GameDifficulty.Hard)
        {
            Debug.Log("하드모드 들어옴");
            HardMode();
        }
        else
        {
            // NormalMode();
        }

        int activeNumber = 0;                               // 몇번째 행동인지 확인하는 변수

        // 턴 시작시 턴매니저에서 플레이어가 턴 중임을 표시 isEnemyPlayerDone = false;
        yield return new WaitForSeconds(1);                 // 1초 대기

        // 첫번째 행동
        enemyPlayer.enemyActiveEnd = false;                 // 적 플레이어가 행동 중임을 표시
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기
        ActiveCard(EfirstTurnCardIndex);                    // 첫 번째 카드 행동 실행
        yield return StartCoroutine(WaitForEnemyPlayerAction()); // 행동 완료 대기
        yield return StartCoroutine(WaitForPlayerAction());
        Debug.Log("적의 첫 번째 행동 완료");
        yield return new WaitForSeconds(2);                 // 2초 대기

        // 두번째 행동
        activeNumber++;
        enemyPlayer.enemyActiveEnd = false;
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기
        ActiveCard(EsecondTurnCardIndex);
        yield return StartCoroutine(WaitForEnemyPlayerAction());
        yield return StartCoroutine(WaitForPlayerAction());
        Debug.Log("적의 두 번째 행동 완료");
        yield return new WaitForSeconds(2);                 // 2초 대기

        // 세번째 행동
        activeNumber++;
        enemyPlayer.enemyActiveEnd = false;
        onNextCard?.Invoke(activeNumber);                   // 카드를 보이라고 알림
        yield return new WaitForSeconds(1);                 // 1초 대기
        ActiveCard(EthirdTurnCardIndex);
        yield return StartCoroutine(WaitForEnemyPlayerAction());
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
    /// 적 플레이어의 행동이 끝날 때까지 기다리는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForEnemyPlayerAction()
    {
        Debug.Log("적 대기 코루틴 시작");
        while (!enemyPlayer.enemyActiveEnd)
        {
            yield return null;
        }
        Debug.Log("적 대기 코루틴 끝");
    }

    /// <summary>
    /// 플레이어의 행동이 끝날 때까지 기다리는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForPlayerAction()
    {
        Debug.Log("플레이어의 행동 끝을 기다리는 코루틴");
        while (!player.playerActiveEnd)
        {
            yield return null;
        }
        Debug.Log("플레이어의 행동 끝을 기다리는 코루틴 끝");
    }

    private void PlayerSction(int section)
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
                    while(randomCard == EfirstTurnCardIndex);

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
