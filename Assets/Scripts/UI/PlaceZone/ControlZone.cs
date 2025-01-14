using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    /// 카드 프레임을 컨트롤 하는 클래스
    /// </summary>
    Card_Frame cardFrame;

    /// <summary>
    /// 플레이스 존을 비우라고 알리는 델리게이트
    /// </summary>
    public Action onClearPlace;

    /// <summary>
    /// 컨티뉴 버튼을 클릭했다고 알리는 델리게이트
    /// </summary>
    public Action onContinue;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 턴 매니저
    /// </summary>
    TurnManager turnManager;

    // 카드 진행 순서
    public int firstTurnCardIndex = 0;
    public int secondTurnCardIndex = 0;
    public int thirdTurnCardIndex = 0;

    /// <summary>
    /// 행동 순서를 알리는 델리게이트
    /// </summary>
    public Action<int, int, int> onActiveOrder;

    private void Awake()
    {
        continueButton = transform.GetChild(1).GetComponent<Button>();
        clearButton = transform.GetChild(2).GetComponent<Button>();

        clearButton.onClick.AddListener(ClearPlace);
        continueButton.onClick.AddListener(Continue);
    }
    private void Start()
    {
        gameManager = GameManager.Instance;
        turnManager = TurnManager.Instance;

        placeCard = FindAnyObjectByType<PlaceCard>();
        placeCard.onSendCardNumber += OnRememberCard;
    }

    private void Continue()
    {
        // 컨티뉴 버튼이 할 일
        // 1. Fram 클래스 찾아서 끄기 => 캔버스 그룹 조정하는 것으로 변경
        // 만약 카드 존에 카드가 3개 다 차있으면
        if (placeCard.pullCard)
        {
            onContinue?.Invoke();
            // 2. 턴 순서대로 게임 진행시키기 => 턴 매니저한테 턴 진행하라고 알림?
            turnManager.OnTurnStart2();
        }
    }

    /// <summary>
    /// 플레이스 존을 비우라고 알리는 함수
    /// </summary>
    private void ClearPlace()
    {
        onClearPlace?.Invoke();
    }

    /// <summary>
    /// 델리게이트 연결으로 카드의 진행 순서를 기억하는 함수
    /// </summary>
    /// <param name="first">첫번째 턴에 사용할 카드의 인덱스</param>
    /// <param name="second">두번째 턴에 사용할 카드의 인덱스</param>
    /// <param name="third">세번째 턴에 사용할 카드의 인덱스</param>
    private void OnRememberCard(int first, int second, int third)
    {
        firstTurnCardIndex = first;
        secondTurnCardIndex = second;
        thirdTurnCardIndex = third;
    }
}
