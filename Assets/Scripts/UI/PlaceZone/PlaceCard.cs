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

    /// <summary>
    /// 카드 공간이 전부 차있는지 확인하기 위함(0,1,2)
    /// </summary>
    int fullPlace;

    /// <summary>
    /// 버튼에 할당할 카드 이미지들의 배열
    /// </summary>
    public Sprite[] AdelCards;

    /// <summary>
    /// 선택된 카드를 비활성화 시키기 위한 델리게이트
    /// </summary>
    public Action<int> onCardDisable;

    /// <summary>
    /// 선택이 취소된 카드를 활성화 시키기 위한 델리게이트
    /// </summary>
    public Action<int> onCardEnable;

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

    /// <summary>
    /// 첫번째 공간이 비었는지(true : 비어있다, false : 차있다)
    /// </summary>
    bool emptyFirstCard = true;
    bool emptySecondCard = true;
    bool emptyThirdCard = true;

    private void Start()
    {
        fullPlace = 0;
        cardButtons = FindAnyObjectByType<CardButtons>();
        cardButtons.onCardButton += OnCardPlace;
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
                emptyFirstCard = true;                          // 배치할 수 있게 bool 변수 초기화
                break;
            case 1:
                buttonImages[1].sprite = defaultCard[1];
                onCardEnable?.Invoke(secondCardNumber);
                secondCardNumber = 0;
                emptySecondCard = true;
                break;
            case 2:
                buttonImages[2].sprite = defaultCard[2];
                onCardEnable?.Invoke(thirdCardNumber);
                thirdCardNumber = 0;
                emptyThirdCard = true;
                break;
        }

        UpdatePlaceImages();
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
    /// 델리게이트를 받아 버튼의 이미지를 변경하는 함수
    /// </summary>
    /// <param name="cardIndex">CardButtons에서 넘겨받은 카드의 인덱스</param>
    private void OnCardPlace(int cardIndex)
    {
        // 여기 델리게이트도 수정해서 캐릭터의 인덱스도 받아야 할 듯

        // 만약 카드 공간이 3개 전부 차있지 않으면
        if(emptyFirstCard || emptySecondCard || emptyThirdCard)
        {
            // 첫번째 카드 공간이 비어있으면
            if(emptyFirstCard)
            {
                // 버튼에 이미지 할당
                buttonImages[0].sprite = AdelCards[cardIndex];
                firstCardNumber = cardIndex;
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
                buttonImages[1].sprite = AdelCards[cardIndex];
                secondCardNumber = cardIndex;
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
                buttonImages[2].sprite = AdelCards[cardIndex];
                thirdCardNumber = cardIndex;
                emptyThirdCard = false;
            }

            // CardButtons 에서 해당하는 카드 비활성화 시키는 부분 필요
            onCardDisable?.Invoke(cardIndex);







            // 버튼에 이미지 할당
            //buttons[fullPlace].GetComponent<Image>().sprite = AdelCards[cardIndex];

            // CardButtons 에서 해당하는 카드 비활성화 시키는 부분 필요
            //onCardDisable?.Invoke(cardIndex);

            // Place 쪽 버튼을 눌렀을 때 다시 CardButtons 쪽 버튼을 활성화 시키기 위해 기억해 놓음
            /*switch (fullPlace)
            {
                case 0:
                    firstCardNumber = cardIndex;
                    break;
                case 1:
                    secondCardNumber = cardIndex;
                    break;
                case 2:
                    thirdCardNumber = cardIndex;
                    break;
            }*/

            //fullPlace++;
        }
        else
        {
            Debug.LogWarning("모든 카드 공간이 이미 차 있다.");
        }
    }
}
