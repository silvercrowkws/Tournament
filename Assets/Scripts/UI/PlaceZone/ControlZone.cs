using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlZone : MonoBehaviour
{
    /// <summary>
    /// 컨티뉴 버튼
    /// </summary>
    Button continueButton;

    /// <summary>
    /// 클리어 버튼
    /// </summary>
    Button clearButton;

    /// <summary>
    /// 플레이스 카드 클래스
    /// </summary>
    PlaceCard placeCard;

    /// <summary>
    /// 플레이스 존을 비우라고 알리는 델리게이트
    /// </summary>
    public Action onClearPlace;

    private void Awake()
    {
        continueButton = transform.GetChild(1).GetComponent<Button>();
        clearButton = transform.GetChild(2).GetComponent<Button>();

        clearButton.onClick.AddListener(ClearPlace);
        continueButton.onClick.AddListener(Continue);
    }

    private void Continue()
    {
        // 컨티뉴 버튼이 할 일
        // 1. Fram 클래스 찾아서 끄기
        // 2. 턴 순서대로 게임 진행시키기
        // 이 버튼을 누르는 순간 Fram 클래스가 비활성화 되면 2번이 안되는데..?
    }

    /// <summary>
    /// 플레이스 존을 비우라고 알리는 함수
    /// </summary>
    private void ClearPlace()
    {
        onClearPlace?.Invoke();
    }

    private void Start()
    {
        placeCard = FindAnyObjectByType<PlaceCard>();
        placeCard.onSendCardNumber += OnRememberCard;
    }

    /// <summary>
    /// 델리게이트 연결으로 카드의 진행 순서를 기억하는 함수
    /// </summary>
    /// <param name="first">첫번째 턴에 사용할 카드의 인덱스</param>
    /// <param name="second">두번째 턴에 사용할 카드의 인덱스</param>
    /// <param name="third">세번째 턴에 사용할 카드의 인덱스</param>
    private void OnRememberCard(int first, int second, int third)
    {
        
    }
}
