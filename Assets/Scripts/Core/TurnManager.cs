using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>       // 나중에 리스타트 버튼 같은거 했을 때 문제 생기면 싱글톤 빼보자
{
    /// <summary>
    /// 현재 턴 진행상황 표시용 enum
    /// </summary>
    enum TurnProcessState
    {
        None = 0,
        Start,
        End,
    }

    /// <summary>
    /// 현재 턴 진행상황
    /// </summary>
    TurnProcessState turnState = TurnProcessState.None;

    /// <summary>
    /// 현재 턴 번호(몇번째 턴인지)
    /// </summary>
    public int turnNumber = 0;

    /// <summary>
    /// 턴이 진행될지 여부(true면 턴이 진행되고 false면 턴이 진행되지 않는다)
    /// </summary>
    bool isTurnEnable = true;

    /// <summary>
    /// 턴이 시작되었음을 알리는 델리게이트(int:시작된 턴 번호)
    /// </summary>
    public Action<int> onTurnStart;

    /// <summary>
    /// OnInitialize2가 실행되었음을 알리는 델리게이트
    /// </summary>
    public Action onInitialize2Start;

    /// <summary>
    /// 턴이 끝났음을 알리는 델리게이트
    /// </summary>
    public Action onTurnEnd;

    /// <summary>
    /// 턴 종료 처리 중인지 확인하는 변수
    /// </summary>
    bool isEndProcess = false;

    /// <summary>
    /// 마지막 턴이 끝났음을 알리는 델리게이트(UI 갱신용)
    /// </summary>
    public Action<int> onTurnOver;

    GameManager gameManager;
    private void Start()
    {
        gameManager = GameManager.Instance;
        turnNumber = 0;                         // OnTurnStart에서 turnNumber를 증가 시키기 때문에 0에서 시작

        OnInitialize2();
    }

    /// <summary>
    /// 씬이 시작될 때 초기화
    /// </summary>
    public void OnInitialize2()
    {
        /*if (turnNumber == 0)
        {
            turnNumber = 1;                     // 초기화시 0부터 시작하기 때문에
        }*/
        turnNumber = 0;                         // OnTurnStart에서 turnNumber를 증가 시키기 때문에 0에서 시작        

        turnState = TurnProcessState.None;      // 턴 진행 상태 초기화
        isTurnEnable = true;                    // 턴 켜기

        Debug.Log("턴 시작 준비 완료");

        onInitialize2Start?.Invoke();
        //OnTurnStart();                          // 턴 시작
    }


    /// <summary>
    /// 턴 시작 처리용 함수
    /// </summary>
    void OnTurnStart()
    {
        if (isTurnEnable)                           // 턴 매니저가 작동 중이면
        {
            turnNumber++;                           // 턴 숫자 증가
            gameManager.isPlayerDone = false;       // 플레이어의 턴 중임을 표시
            Debug.Log($"{turnNumber}턴 시작");
            turnState = TurnProcessState.Start;     // 턴 시작 상태

            //Debug.Log("onTurnStart 델리게이트 보냄");
            onTurnStart?.Invoke(turnNumber);        // 턴이 시작되었음을 알림(ActivePlayer 클래스에)
        }
    }

    /// <summary>
    /// 턴 종료 처리용 함수
    /// </summary>
    void OnTurnEnd()
    {
        /*if(turnNumber == 0)
        {
            turnNumber = 1;     // 가끔 현재 턴이 0인 상태가 있음
            Debug.Log("턴 꼬였음");
        }*/

        if (isTurnEnable)    // 턴 매니저가 작동 중이면
        {
            isEndProcess = true;    // 종료 처리 중이라고 표시
            onTurnEnd?.Invoke();    // 턴이 종료되었다고 알림
            Debug.Log($"{turnNumber}턴 종료");

            isEndProcess = false;   // 종료 처리가 끝났다고 표시
            OnTurnStart();          // 다음 턴 시작
        }
    }

    /*/// <summary>
    /// 마지막 웨이브가 끝나서 턴이 종료되는 함수
    /// </summary>
    /// <param name="doorArriveCount">문의 남은 체력</param>
    public void OnTurnOver(int doorArriveCount)
    {
        if(isTurnEnable)    // 턴 매니저가 작동 중이면
        {
            isEndProcess = true;    // 종료 처리 중이라고 표시
            onTurnEnd?.Invoke();    // 턴이 종료되었다고 알림
            Debug.Log($"{turnNumber}턴 종료");
            Debug.Log($"{turnNumber}턴은 마지막 턴이다.");

            isEndProcess = false;   // 종료 처리가 끝났다고 표시

            *//*GameObject gameOverPanel = GameObject.Find("GameOverPanel");
            gameOverPanel.SetActive(true);*//*
            onTurnOver?.Invoke(doorArriveCount);   // 마지막 턴이 끝났다고 알림
        }
    }*/

    /// <summary>
    /// OnTurnStart를 사용하기 위한 public 함수
    /// </summary>
    public void OnTurnStart2()
    {
        OnTurnStart();
    }

    /// <summary>
    /// OnTurnEnd를 사용하기 위한 public 함수
    /// </summary>
    public void OnTurnEnd2()
    {
        OnTurnEnd();
    }
}
