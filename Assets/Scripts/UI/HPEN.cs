using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HPEN : MonoBehaviour
{
    // 플레이어와 적 플레이어의 HP, EN의 상태를 슬라이더로 표시하는 클래스
    // 턴 상황에 따라 가운데 Round Text의 숫자를 변경시켜야 함

    /// <summary>
    /// 턴 매니저
    /// </summary>
    TurnManager turnManager;

    /// <summary>
    /// 텍스트 매쉬 프로
    /// </summary>
    TextMeshProUGUI roundText;

    /// <summary>
    /// 현재 턴 번호
    /// </summary>
    int currentTurnNumber = 0;

    private void Awake()
    {
        Transform child = transform.GetChild(3);
        roundText = child.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        turnManager = TurnManager.Instance;
        turnManager.onTurnStart += OnRoundText;
        turnManager.onTurnEnd += OnEndTurnNumber;
    }

    /// <summary>
    /// 턴 숫자에 따라 라운드 텍스트를 변경하는 함수
    /// </summary>
    /// <param name="turnNumber">턴 숫자</param>
    private void OnRoundText(int turnNumber)
    {
        currentTurnNumber = turnNumber;
        if (turnNumber < 10)
        {
            roundText.text = $"0{turnNumber}";
        }
        else
        {
            roundText.text = turnNumber.ToString();
        }

        //roundText.text = turnNumber.ToString();
    }

    /// <summary>
    /// 턴이 종료되고 NextRound 버튼을 눌렀을 때도 상단의 라운드텍스트를 변경하기 위한 함수
    /// </summary>
    private void OnEndTurnNumber()
    {
        if(currentTurnNumber < 10)
        {
            roundText.text = $"0{currentTurnNumber + 1}";
        }
        else
        {
            roundText.text = currentTurnNumber + 1.ToString();
        }
    }
}
