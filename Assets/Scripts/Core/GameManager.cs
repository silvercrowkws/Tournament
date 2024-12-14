using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임상태
/// </summary>
public enum GameState
{
    SelectCharacter = 0,        // 캐릭터 선택 상태
    SelectCard,                 // 카드 선택 상태
    GameStart,                  // 내가 카드 선택한대로 움직이는 상태
    GameWin,                    // 내가 그 판을 이긴 상태
    GameOver                    // 내가 그 판을 진 상태

    // 1. 처음에 캐릭터 고르고
    // 2. 어떻게 움직일지 카드 선택하고
    // 3. 선택한 대로 움직이는 GameStart 상태
    //      2,3 번 반복하면서 게임이 실행되다가
    // 4. 내가 이기면 GameWin 상태로 전환되고 다음 상대와 2번부터 시작
    // 5. 내가 지면 GameOver 상태로 전환되고 끝
}

public class GameManager : Singleton<GameManager>
{
    /// <summary>
    /// 현재 게임상태
    /// </summary>
    public GameState gameState = GameState.SelectCharacter;

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
                    case GameState.SelectCharacter:
                        Debug.Log("캐릭터 선택 상태");
                        onSelectCharacter?.Invoke();
                        break;
                    case GameState.SelectCard:
                        Debug.Log("카드 선택 상태");
                        onSelectCard?.Invoke();
                        break;
                    case GameState.GameStart:
                        Debug.Log("게임스타트");
                        onGameStart?.Invoke();
                        break;
                    case GameState.GameWin:
                        Debug.Log("게임 승리");
                        onGameWin?.Invoke();
                        break;
                    case GameState.GameOver:
                        Debug.Log("게임 패배");
                        onGameOver?.Invoke();
                        break;
                }
            }
        }
    }


    // 게임상태 델리게이트
    public Action onSelectCharacter;
    public Action onSelectCard;
    public Action onGameStart;
    public Action onGameWin;
    public Action onGameOver;

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
    /// 턴 매니저
    /// </summary>
    //TurnManager turnManager;


    private void Start()
    {
        
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        player = FindAnyObjectByType<Player>();

        //turnManager = FindAnyObjectByType<TurnManager>();
        //turnManager.OnInitialize2();
    }

    /// <summary>
    /// 게임을 재시작 시킬때 초기화 시키는 함수
    /// </summary>
    public void GameRestart()
    {
        gameState = GameState.SelectCharacter;        // 게임 준비 상태로 전환
        
        //turnManager.turnNumber = 0;
        //turnManager.OnInitialize2();            // 씬이 시작될 때 
    }





#if UNITY_EDITOR


#endif
}
