using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerCharacter
{
    Character_Adel = 0,
    Character_Akstar,
    Character_Amelia,
    Character_Arngrim,
    Character_Barbariccia,
    Character_BlackMage,
    Character_Cloud,
    Character_Elle,
    Character_Jade,
    Character_Nalu,
}

public enum EnemyPlayerCharacter
{
    Character_Adel = 0,
    Character_Akstar,
    Character_Amelia,
    Character_Arngrim,
    Character_Barbariccia,
    Character_BlackMage,
    Character_Cloud,
    Character_Elle,
    Character_Jade,
    Character_Nalu,
}

/// <summary>
/// 게임상태
/// </summary>
public enum GameState
{
    Main = 0,                   // 기본 상태
    SelectCharacter,            // 캐릭터 선택 상태
    SelectCard,                 // 카드 선택 상태
    GameComplete,               // 게임이 완료된 상태(다음 판 있음)

    // 1. 처음에 캐릭터 고르고
    // 2. 어떻게 움직일지 카드 선택하고
    // 3. 선택한 대로 움직이는 GameStart 상태
    //      2,3 번 반복하면서 게임이 실행되다가
    // 4. 내가 이기면 GameWin 상태로 전환되고 다음 상대와 2번부터 시작
    // 5. 내가 지면 GameOver 상태로 전환되고 끝
}

/// <summary>
/// 게임 난이도
/// </summary>
public enum GameDifficulty
{
    Normal = 0,                 // 노말 난이도(져도 재시작)
    Hard,                       // 하드 난이도(지면 처음부터?)
}

public class GameManager : Singleton<GameManager>
{
    /// <summary>
    /// 플레이어의 캐릭터 인덱스
    /// </summary>
    public int playerCharacterIndex = 0;

    /// <summary>
    /// 적 플레이어의 캐릭터 인덱스
    /// </summary>
    public int enemyPlayerCharacterIndex = 0;

    /// <summary>
    /// 게임 매니저가 관리하는 게임 토너먼트 리스트
    /// </summary>
    public List<int> gameTournamentList = new List<int>();

    /// <summary>
    /// 위에 게임 토너먼트 리스트 복사본
    /// </summary>
    public List<int> tournamentList = new List<int>();

    /// <summary>
    /// FightControlButtons 클래스에서 캡쳐한 스프라이트
    /// </summary>
    public Sprite capturedSprite;

    /// <summary>
    /// 게임 완료에서 보여줄 뒷배경(캡쳐 스프라이트)
    /// </summary>
    GameCompleteBackGround gameCompleteBackGround;

    /// <summary>
    /// 현재 게임상태
    /// </summary>
    public GameState gameState = GameState.Main;

    /// <summary>
    /// 현재 게임 난이도
    /// </summary>
    public GameDifficulty gamediff = GameDifficulty.Normal;

    /// <summary>
    /// 현재 게임상태 변경시 알리는 프로퍼티
    /// </summary>
    public GameState GameState
    {
        get => gameState;
        set
        {
            if (gameState != value)
            {
                gameState = value;
                switch (gameState)
                {
                    case GameState.Main:
                        Debug.Log("메인 상태");
                        break;
                    case GameState.SelectCharacter:
                        Debug.Log("캐릭터 선택 상태");
                        onSelectCharacter?.Invoke();
                        break;
                    case GameState.SelectCard:
                        Debug.Log("카드 선택 상태");
                        onSelectCard?.Invoke();
                        break;
                    case GameState.GameComplete:
                        Debug.Log("게임 완료 상태");
                        onGameComplete?.Invoke();
                        break;
                }
            }
        }
    }


    // 게임상태 델리게이트
    public Action onSelectCharacter;
    public Action onSelectCard;
    public Action onGameComplete;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    public Player Player
    {
        get
        {
            if (player == null)
                player = FindAnyObjectByType<Player>();
            return player;
        }
    }

    /// <summary>
    /// 적 플레이어
    /// </summary>
    EnemyPlayer enemyPlayer;

    public EnemyPlayer EnemyPlayer
    {
        get
        {
            if (enemyPlayer == null)
                enemyPlayer = FindAnyObjectByType<EnemyPlayer>();
            return enemyPlayer;
        }
    }

    /// <summary>
    /// 턴 매니저
    /// </summary>
    //TurnManager turnManager;

    /// <summary>
    /// 난이도 선택 클래스
    /// </summary>
    ChooseDif chooseDif;

    /// <summary>
    /// 전투시작 버튼(전투 전에 화면 캡쳐한 스프라이트 받기 위해 있음)
    /// </summary>
    FightControlButtons fightControlButtons;

    /// <summary>
    /// 플레이어가 모든 행동을 완료했는지 확인하는 bool 변수(true : 행동을 완료했다, false : 행동을 완료하지 않았다.)
    /// </summary>
    public bool isPlayerDone = false;

    /// <summary>
    /// 적 플레이어가 모든 행동을 완료했는지 확인하는 bool 변수(true : 행동을 완료했다, false : 행동을 완료하지 않았다.)
    /// </summary>
    public bool isEnemyPlayerDone = false;

    /// <summary>
    /// 양쪽 플레이어 모두 행동을 완료했는지 확인하는 bool 변수
    /// </summary>
    public bool isBothPlayersDone = false;

    /// <summary>
    /// 양쪽 플레이어 모두 행동을 완료했다고 알리는 델리게이트
    /// </summary>
    public Action onBothPlayersDone;

    /// <summary>
    /// true : 승리, false : 패배
    /// </summary>
    public Action<bool> onPlayerResult;

    /// <summary>
    /// true : 승리, false : 패배
    /// </summary>
    public Action<bool> onEnemyResult;

    /// <summary>
    /// 플레이어의 승패 결과(true : 이김, false : 짐 / 토너먼트 리스트의 0번을 제거할지 말지 때문)
    /// </summary>
    public bool playerResult;

    /// <summary>
    /// 게임이 끝났으니 행동을 멈추라고 알리는 델리게이트(true : 게임 종료, false : 게임 진행 중)
    /// </summary>
    public bool gameOver = false;

    private void Start()
    {
        /*if (player != null)
        {
            player.onPlayerHPZero += OnGameOver;
        }
        else
        {
            Debug.Log("플레이어 없음");
        }

        if (enemyPlayer != null)
        {
            enemyPlayer.onEnemyHPZero += OnGameOver;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;*/
    }

    private void OnEnable()
    {
        chooseDif = FindAnyObjectByType<ChooseDif>();

        if(chooseDif != null)
        {
            chooseDif.onNormalMode += GameDif;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if(chooseDif != null)
        {
            chooseDif.onNormalMode -= GameDif;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        player = FindAnyObjectByType<Player>();        

        //turnManager = FindAnyObjectByType<TurnManager>();
        //turnManager.OnInitialize2();
    }

    private void Update()
    {
        // 플레이어와 적이 모두 턴을 끝냈으면
        if(!isBothPlayersDone && isPlayerDone && isEnemyPlayerDone)
        {
            isBothPlayersDone = true;
            onBothPlayersDone?.Invoke();
        }

        // 두 플레이어 중 하나라도 턴을 끝내지 않았으면
        else if(!isPlayerDone && !isEnemyPlayerDone)
        {
            isBothPlayersDone = false;
        }

        /*// 플레이어나 적이 죽은 상황에도 턴을 끝냈다고 판단 => 이게 필요한가..?
        else if (player.HP <= 0 || enemyPlayer.HP <= 0)
        {
            isBothPlayersDone = true;
        }*/
    }

    /// <summary>
    /// 델리게이트를 받아 게임 난이도를 설정하는 함수
    /// </summary>
    /// <param name="dif">게임 난이도(true : 노말, false : 하드)</param>
    private void GameDif(bool dif)
    {
        if (dif == true)
        {
            gamediff = GameDifficulty.Normal;
            Debug.Log("게임매니저에서 난이도 [노말]로 변경");
        }
        else if (dif == false)
        {
            gamediff = GameDifficulty.Hard;
            Debug.Log("게임매니저에서 난이도 [하드]로 변경");
        }
        else
        {
            Debug.LogWarning("게임매니저에서 난이도 변경 에러");
        }
    }

    /*/// <summary>
    /// 게임을 재시작 시킬때 초기화 시키는 함수
    /// </summary>
    public void GameRestart()
    {
        gameState = GameState.SelectCharacter;        // 게임 준비 상태로 전환
        
        //turnManager.turnNumber = 0;
        //turnManager.OnInitialize2();            // 씬이 시작될 때 
    }*/

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        switch(scene.buildIndex)
        {
            case 0:
                Debug.Log("메인 씬");
                gameState = GameState.Main;
                break;
            case 1:
                Debug.Log("캐릭터 선택 씬");
                gameState = GameState.SelectCharacter;

                fightControlButtons = FindAnyObjectByType<FightControlButtons>();
                if (fightControlButtons == null)
                {
                    Debug.LogError("1번 씬에서 fightControlButtons을 못찾음!");
                }
                else
                {
                    fightControlButtons.onScreenshotCaptured += OnScreenshotCaptured;
                    Debug.Log("onScreenshotCaptured 구독 완료!");
                }

                break;
            case 2:
                Debug.Log("카드 선택 씬");
                gameState = GameState.SelectCard;

                enemyPlayer = FindAnyObjectByType<EnemyPlayer>();
                if (enemyPlayer == null)
                {
                    Debug.LogError("2번 씬에서도 enemyPlayer를 못 찾음!");
                }
                else
                {
                    enemyPlayer.onEnemyHPZero += OnGameOver;
                    Debug.Log("onEnemyHPZero 이벤트 구독 완료!");
                }

                player = FindAnyObjectByType<Player>();
                if (player == null)
                {
                    Debug.LogError("2번 씬에서도 player를 못 찾음!");
                }
                else
                {
                    player.onPlayerHPZero += OnGameOver;
                    Debug.Log("onPlayerHPZero 이벤트 구독 완료!");
                }
                break;
            case 3:
                Debug.Log("전투 완료 씬");
                gameState = GameState.GameComplete;

                gameCompleteBackGround = FindAnyObjectByType<GameCompleteBackGround>();
                if(gameCompleteBackGround == null)
                {
                    Debug.Log("3번씬에서 gameCompleteBackGround 못찾음");
                }
                else
                {

                }
                break;
        }
    }

    /// <summary>
    /// 캡쳐된 스프라이트를 저장하는 함수
    /// </summary>
    /// <param name="sprite"></param>
    private void OnScreenshotCaptured(Sprite sprite)
    {
        capturedSprite = sprite;
    }

    /// <summary>
    /// 플레이어나 적의 HP가 0이 되었을 때 실행되는 함수
    /// </summary>
    private void OnGameOver()
    {
        gameOver = true;        // 나중에 이거 초기화 필요

        Debug.Log($"게임 오버 : {gameOver}");

        if (player.HP < 1 && enemyPlayer.HP < 1)
        {
            Debug.LogWarning("플레이어와 적이 동시에 패배!");
            onPlayerResult?.Invoke(false);  // 플레이어 패배
            onEnemyResult?.Invoke(false);   // 적도 패배
            playerResult = false;           // 플레이어 패배
        }
        else
        {
            if (player.HP < 1)
            {
                Debug.LogWarning("플레이어가 진 상황");
                // 플레이어가 진 상황
                onEnemyResult?.Invoke(true);        // 적이 이김
                onPlayerResult?.Invoke(false);      // 플레이어가 짐
                playerResult = false;               // 플레이어 패배
            }
            else if (enemyPlayer.HP < 1)
            {
                // 적이 진 상황
                Debug.LogWarning("적이 진 상황");
                onPlayerResult?.Invoke(true);       // 플레이어가 이김
                onEnemyResult?.Invoke(false);       // 적이 짐
                playerResult = true;                // 플레이어 승리
            }
        }
    }

#if UNITY_EDITOR


#endif
}
