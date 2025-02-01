using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlaceCard : MonoBehaviour
{
    /// <summary>
    /// 카드 버튼들을 관리하는 클래스
    /// </summary>
    CardButtons cardButtons;

    /// <summary>
    /// 카드가 세팅되는 곳
    /// </summary>
    Button[] buttons;

    /// <summary>
    /// 버튼의 이미지들
    /// </summary>
    Image[] buttonImages;

    /// <summary>
    /// 버튼의 기본 스프라이트
    /// </summary>
    public Sprite[] defaultCard;

    /// <summary>
    /// 카드를 세팅할 곳을 알려주는 스프라이트
    /// </summary>
    public Sprite[] placeHereCard;

    int firstCardNumber = 0;
    int secondCardNumber = 0;
    int thirdCardNumber = 0;

    int firstCardCost = 0;
    int secondCardCost = 0;
    int thirdCardCost = 0;

    /// <summary>
    /// 카드 공간이 전부 차있는지 확인하기 위함(0,1,2)
    /// </summary>
    int fullPlace;

    /// <summary>
    /// 버튼에 할당할 카드 이미지들의 배열
    /// </summary>
    public Sprite[] AdelCards;
    public Sprite[] AkstarCards;
    public Sprite[] AmeliaCards;
    public Sprite[] ArngrimCards;
    public Sprite[] BarbaricciaCards;
    public Sprite[] BlackMageCards;
    public Sprite[] CloudCards;
    public Sprite[] ElleCards;
    public Sprite[] JadeCards;
    public Sprite[] NaluCards;

    /// <summary>
    /// 캐릭터에 따라 내용이 달라지는 배열
    /// </summary>
    public Sprite[] ChangeCards;

    /// <summary>
    /// 선택된 카드를 비활성화 시키기 위한 델리게이트
    /// </summary>
    public Action<int> onCardDisable;

    /// <summary>
    /// 선택이 취소된 카드를 활성화 시키기 위한 델리게이트
    /// </summary>
    public Action<int> onCardEnable;

    /// <summary>
    /// 턴이 진행될 카드의 순서를 알리는 델리게이트
    /// </summary>
    public Action<int, int, int> onSendCardNumber;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 컨트롤 존
    /// </summary>
    ControlZone controlZone;

    /// <summary>
    /// 넥스트 존
    /// </summary>
    NextZone nextZone;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    /// <summary>
    /// 첫번째 공간이 비었는지(true : 비어있다, false : 차있다)
    /// </summary>
    bool emptyFirstCard = true;
    bool emptySecondCard = true;
    bool emptyThirdCard = true;

    /// <summary>
    /// 카드 공간이 전부 차있는지 확인하는 bool 변수(true : 가득찼다, false : 비어있다)
    /// </summary>
    public bool pullCard = false;

    /// <summary>
    /// 세팅된 카드들의 코스트 총 합
    /// </summary>
    public int totalCardCost = 0;

    /// <summary>
    /// 누적 코스트 변경을 알리는 델리게이트
    /// </summary>
    public Action<int> onTotalCostChange;

    private void Awake()
    {
        // 배열 크기 초기화
        buttons = new Button[3];
        buttonImages = new Image[3];

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i] = transform.GetChild(i).GetComponent<Button>();
            buttons[i].onClick.AddListener(() => OnPlaceEmpty(index));
            buttonImages[i] = buttons[i].GetComponent<Image>();
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = GameManager.Instance.Player;
        controlZone = FindAnyObjectByType<ControlZone>();
        controlZone.onClearPlace += OnClearPlace;

        // 배열 초기화
        ChangeCards = new Sprite[AdelCards.Length];     // 카드들의 개수는 같으니 AdelCards.Length 로 초기화
        
        OnChangeCard();                                 // 배열 복사

        fullPlace = 0;
        cardButtons = FindAnyObjectByType<CardButtons>();
        cardButtons.onCardButton += OnCardPlace;

        nextZone = FindAnyObjectByType<NextZone>();
        nextZone.onFramSetActive += OnClearPlace;
    }

    /// <summary>
    /// 플레이어 인덱스에 따라서 배열을 복사시키는 함수
    /// </summary>
    void OnChangeCard()
    {
        // 플레이어 인덱스에 따라서
        switch (gameManager.playerCharacterIndex)
        {
            case 0:
                for (int i = 0; i < AdelCards.Length; i++)
                {
                    ChangeCards[i] = AdelCards[i];
                }
                break;
            case 1:
                for (int i = 0; i < AkstarCards.Length; i++)
                {
                    ChangeCards[i] = AkstarCards[i];
                }
                break;
            case 2:
                for (int i = 0; i < AmeliaCards.Length; i++)
                {
                    ChangeCards[i] = AmeliaCards[i];
                }
                break;
            case 3:
                for (int i = 0; i < ArngrimCards.Length; i++)
                {
                    ChangeCards[i] = ArngrimCards[i];
                }
                break;
            case 4:
                for (int i = 0; i < BarbaricciaCards.Length; i++)
                {
                    ChangeCards[i] = BarbaricciaCards[i];
                }
                break;
            case 5:
                for (int i = 0; i < BlackMageCards.Length; i++)
                {
                    ChangeCards[i] = BlackMageCards[i];
                }
                break;
            case 6:
                for (int i = 0; i < CloudCards.Length; i++)
                {
                    ChangeCards[i] = CloudCards[i];
                }
                break;
            case 7:
                for (int i = 0; i < ElleCards.Length; i++)
                {
                    ChangeCards[i] = ElleCards[i];
                }
                break;
            case 8:
                for (int i = 0; i < JadeCards.Length; i++)
                {
                    ChangeCards[i] = JadeCards[i];
                }
                break;
            case 9:
                for (int i = 0; i < NaluCards.Length; i++)
                {
                    ChangeCards[i] = NaluCards[i];
                }
                break;
        }
    }

    /// <summary>
    /// 카드 공간 클릭으로 그 공간을 비우는 함수
    /// </summary>
    /// <param name="placeIndex">비워야 될 공간의 인덱스</param>
    private void OnPlaceEmpty(int placeIndex)
    {
        switch (placeIndex)
        {
            case 0:
                buttonImages[0].sprite = defaultCard[0];        // 버튼의 이미지 초기화
                onCardEnable?.Invoke(firstCardNumber);          // 카드 다시 활성화 하라고 델리게이트 전달
                firstCardNumber = 0;                            // 기록 초기화
                totalCardCost -= firstCardCost;                 // 총 합에서 차감
                firstCardCost = 0;                              // 코스트 초기화
                //SendCardNumber();                               // 카드 순서 넘겨줌
                emptyFirstCard = true;                          // 배치할 수 있게 bool 변수 초기화
                break;
            case 1:
                buttonImages[1].sprite = defaultCard[1];
                onCardEnable?.Invoke(secondCardNumber);
                secondCardNumber = 0;
                totalCardCost -= secondCardCost;
                secondCardCost = 0;
                //SendCardNumber();
                emptySecondCard = true;
                break;
            case 2:
                buttonImages[2].sprite = defaultCard[2];
                onCardEnable?.Invoke(thirdCardNumber);
                thirdCardNumber = 0;
                totalCardCost -= thirdCardCost;
                thirdCardCost = 0;
                //SendCardNumber();
                emptyThirdCard = true;
                break;
        }
        onTotalCostChange?.Invoke(totalCardCost);
        pullCard = false;
        SendCardNumber();

        UpdatePlaceImages();
    }

    /// <summary>
    /// 클리어 버튼 클릭으로 세팅을 전부 초기화 하는 함수
    /// </summary>
    private void OnClearPlace()
    {
        buttonImages[0].sprite = placeHereCard[0];      // 버튼 이미지 초기화
        buttonImages[1].sprite = defaultCard[1];
        buttonImages[2].sprite = defaultCard[2];
        onCardEnable?.Invoke(firstCardNumber);          // 카드 다시 활성화 하라고 델리게이트 전달
        onCardEnable?.Invoke(secondCardNumber);
        onCardEnable?.Invoke(thirdCardNumber);

        firstCardNumber = 0;                            // 기록 초기화
        totalCardCost -= firstCardCost;                 // 총합 차감
        firstCardCost = 0;

        secondCardNumber = 0;
        totalCardCost -= secondCardCost;
        secondCardCost = 0;

        thirdCardNumber = 0;
        totalCardCost -= thirdCardCost;
        thirdCardCost = 0;

        onTotalCostChange?.Invoke(totalCardCost);

        emptyFirstCard = true;                          // 배치할 수 있게 bool 변수 초기화
        emptySecondCard = true;
        emptyThirdCard = true;
        pullCard = false;                               // 카드 공간이 비어있게 bool 변수 false로
        SendCardNumber();                               // 카드 순서 넘겨줌
    }

    /// <summary>
    /// 현재 카드 공간 상태에 따라 이미지를 업데이트하는 함수
    /// </summary>
    private void UpdatePlaceImages()
    {
        // 가장 작은 비어 있는 공간에 placeHereCard 이미지를 설정
        if (emptyFirstCard)
        {
            buttonImages[0].sprite = placeHereCard[0];                  // 버튼 0 번 placeHereCard로 이미지 변경

            if (buttonImages[1].sprite == placeHereCard[1])             // 만약 버튼 1번 이미지가 placeHereCard면
            {
                buttonImages[1].sprite = defaultCard[1];                // 버튼 1번 default 이미지로 변경
            }
            else if(buttonImages[2].sprite == placeHereCard[2])         // 만약 버튼 2번 이미지가 placeHereCard면
            {
                buttonImages[2].sprite = defaultCard[2];                // 버튼 2번 default 이미지로 변경
            }
        }
        else if (emptySecondCard)
        {
            buttonImages[1].sprite = placeHereCard[1];                  // 버튼 1번 placeHereCard로 이미지 변경

            if (buttonImages[2].sprite == placeHereCard[2])             // 만약 버튼 2번 이미지가 placeHereCard면
            {
                buttonImages[2].sprite = defaultCard[2];                // 버튼 2번 default 이미지로 변경
            }
        }
        else if (emptyThirdCard)
        {
            buttonImages[2].sprite = placeHereCard[2];
        }
    }


    /// <summary>
    /// 델리게이트를 받아 플레이스 존 버튼의 이미지를 변경하는 함수
    /// </summary>
    /// <param name="cardIndex">CardButtons에서 넘겨받은 카드의 인덱스</param>
    /// <param name="cardCost">CardButtons에서 넘겨받은 카드의 코스트</param>
    private void OnCardPlace(int cardIndex, int cardCost)
    {
        // 누적된 코스트 + 카드 코스트 < 현재 에너지
        if (totalCardCost + cardCost < player.Energy)
        {
            // 만약 카드 공간이 3개 전부 차있지 않으면
            if(emptyFirstCard || emptySecondCard || emptyThirdCard)
            {
                // 첫번째 카드 공간이 비어있으면
                if(emptyFirstCard)
                {
                    // 버튼에 이미지 할당
                    buttonImages[0].sprite = ChangeCards[cardIndex];
                    firstCardNumber = cardIndex;
                    firstCardCost = cardCost;
                    totalCardCost += firstCardCost;
                    //onTotalCostChange?.Invoke(totalCardCost);
                    SendCardNumber();
                    emptyFirstCard = false;

                    if (buttonImages[1].sprite == defaultCard[1])       // 버튼 1번의 이미지가 디폴트면
                    {
                        buttonImages[1].sprite = placeHereCard[1];
                    }
                    // 버튼 1번의 이미지가 플레이스가 아니고, 버튼 2번의 이미지가 기본 상태이면
                    else if(buttonImages[1].sprite != placeHereCard[1] && buttonImages[2].sprite == defaultCard[2])
                    {
                        buttonImages[2].sprite = placeHereCard[2];
                    }
                }

                // 두번째 카드 공간이 비어있으면
                else if (emptySecondCard)
                {
                    // 버튼에 이미지 할당
                    buttonImages[1].sprite = ChangeCards[cardIndex];
                    secondCardNumber = cardIndex;
                    secondCardCost = cardCost;
                    totalCardCost += secondCardCost;
                    SendCardNumber();
                    emptySecondCard = false;

                    if (buttonImages[2].sprite == defaultCard[2])       // 버튼 2번의 이미지가 디폴트면
                    {
                        buttonImages[2].sprite = placeHereCard[2];
                    }
                }

                // 세번째 카드 공간이 비어있으면
                else if (emptyThirdCard)
                {
                    // 버튼에 이미지 할당
                    buttonImages[2].sprite = ChangeCards[cardIndex];
                    thirdCardNumber = cardIndex;
                    thirdCardCost = cardCost;
                    totalCardCost += thirdCardCost;
                    SendCardNumber();
                    emptyThirdCard = false;
                }

                onTotalCostChange?.Invoke(totalCardCost);
                pullCard = false;

                // CardButtons 에서 해당하는 카드 비활성화 시키는 부분
                onCardDisable?.Invoke(cardIndex);

                // 모든 카드가 배치되었는지 확인
                if (!emptyFirstCard && !emptySecondCard && !emptyThirdCard)
                {
                    pullCard = true;        // 카드가 3개 전부 배치되었을 때 pullCard를 true로 설정
                }
            }
            else
            {
                Debug.LogWarning("모든 카드 공간이 이미 차 있다.");
                pullCard = true;
            }

        }        
    }

    /// <summary>
    /// 델리게이트로 카드 순서를 넘겨주는 함수
    /// </summary>
    void SendCardNumber()
    {
        onSendCardNumber?.Invoke(firstCardNumber, secondCardNumber, thirdCardNumber);
    }
}
