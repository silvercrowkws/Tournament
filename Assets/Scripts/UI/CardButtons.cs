using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardButtons : MonoBehaviour
{
    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// move 이미지의 배열(자리)
    /// </summary>
    Image[] moveImages;

    /// <summary>
    /// guard 이미지의 배열(자리)
    /// </summary>
    Image[] guardImages;

    /// <summary>
    /// attack 3가지 이미지의 배열(자리)
    /// </summary>
    Image[] attackImages;

    /// <summary>
    /// energy, heal 이미지의 배열(자리)
    /// </summary>
    Image[] energyImage;

    /// <summary>
    /// 공격 범위 이미지의 배열(자리)
    /// </summary>
    Image[] attackRange;

    /// <summary>
    /// 기본 공격 데미지, 에너지 텍스트
    /// </summary>
    TextMeshProUGUI[] attackTexts;

    /// <summary>
    /// 매직 어택 데미지, 에너지 텍스트
    /// </summary>
    TextMeshProUGUI[] magicAttackTexts;

    /// <summary>
    /// 리미트 어택 데미지, 에너지 텍스트
    /// </summary>
    TextMeshProUGUI[] limitAttackTexts;

    /// <summary>
    /// 각 카드의 버튼들의 배열
    /// </summary>
    Button[] buttons;

    /// <summary>
    /// 각 카드들이 가지고 있는 캔버스 그룹의 배열
    /// </summary>
    CanvasGroup[] canvasGroup;

    /// <summary>
    /// 카드를 클릭했다고 알리는 델리게이트
    /// </summary>
    public Action<int> onCardButton;

    /// <summary>
    /// PlaceCard 클래스
    /// </summary>
    PlaceCard placeCard;

    /// <summary>
    /// Adel의 스프라이트
    /// 0 : move(0,1,2,3,9,10)
    /// 1 : 가드, 퍼팩트 가드
    /// 2 : 어택
    /// 3 : 매직 어택
    /// 4 : 리미트 어택
    /// 5 : 에너지 업, 힐
    /// 6 : 어택 범위
    /// 7 : 매직 어택 범위
    /// 8 : 리미트 어택 범위
    /// </summary>
    public Sprite[] Adels;
    public Sprite[] Akstars;
    public Sprite[] Amelias;
    public Sprite[] Arngrims;
    public Sprite[] Barbariccias;
    public Sprite[] BlackMages;
    public Sprite[] Clouds;
    public Sprite[] Elles;
    public Sprite[] Jades;
    public Sprite[] Nalus;

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }

    private void Start()
    {
        placeCard = FindAnyObjectByType<PlaceCard>();
        placeCard.onCardDisable += OnCardDisable;
        placeCard.onCardEnable += OnCardEnable;

        // 배열 크기 초기화
        moveImages = new Image[6];
        guardImages = new Image[2];
        attackImages = new Image[3];
        energyImage = new Image[2];
        attackRange = new Image[3];
        attackTexts = new TextMeshProUGUI[2];
        magicAttackTexts = new TextMeshProUGUI[2];
        limitAttackTexts = new TextMeshProUGUI[2];
        buttons = new Button[13];
        canvasGroup = new CanvasGroup[13];

        Transform child = transform.GetChild(0);        // 0 번째 자식 MoveDown_Button

        // move 0, 1, 2, 3
        for( int i = 0; i < 4; i++)
        {
            child = transform.GetChild(i);
            moveImages[i] = child.GetChild(0).GetComponent<Image>();
        }

        // move 9, 10
        for( int i = 9; i < 11; i++)
        {
            child = transform.GetChild(i);
            moveImages[i-5] = child.GetChild(0).GetComponent<Image>();
        }

        // guard 4, 11
        child = transform.GetChild(4);
        guardImages[0] = child.GetChild(0).GetComponent <Image>();
        child = transform.GetChild(11);
        guardImages[1] = child.GetChild(0).GetComponent<Image>();

        // attack 3가지 5, 6, 7
        for(int i = 0; i< 3; i++)
        {
            child = transform.GetChild(i + 5);
            attackImages[i] = child.GetChild(0).GetComponent<Image>();

            child = child.GetChild(2);
            child = child.GetChild(2);

            attackRange[i] = child.GetComponent<Image>();
        }

        // energy, heal 8, 12
        child = transform.GetChild(8);
        energyImage[0] = child.GetChild(0).GetComponent<Image>();
        child = transform.GetChild(12);
        energyImage[1] = child.GetChild(0).GetComponent<Image>();

        /*child = transform.GetChild(5);          // Attack_Button
        child = child.transform.GetChild(2);    // Description
        child = child.transform.GetChild(1);    // Text 게임 오브젝트
        attackTexts[0] = child.GetChild(0).GetComponent<TextMeshProUGUI>();
        attackTexts[1] = child.GetChild(1).GetComponent<TextMeshProUGUI>();*/

        // 데미지, 에너지 텍스트 수정 용
        for(int i = 0;i< 3;i++)
        {
            child = transform.GetChild(i + 5);      // Attack_Button, MagicAttack_Button, LimitAttack_Button
            child = child.transform.GetChild(2);    // Description
            child = child.transform.GetChild(1);    // Text 게임 오브젝트

            if(i == 0)
            {
                attackTexts[0] = child.GetChild(0).GetComponent<TextMeshProUGUI>();
                attackTexts[1] = child.GetChild(1).GetComponent<TextMeshProUGUI>();
            }
            else if(i == 1)
            {
                magicAttackTexts[0] = child.GetChild(0).GetComponent<TextMeshProUGUI>();
                magicAttackTexts[1] = child.GetChild(1).GetComponent<TextMeshProUGUI>();
            }
            else if (i == 2)
            {
                limitAttackTexts[0] = child.GetChild(0).GetComponent<TextMeshProUGUI>();
                limitAttackTexts[1] = child.GetChild(1).GetComponent<TextMeshProUGUI>();
            }
        }

        for(int i = 0; i < buttons.Length; i++)
        {
            int index = i;                                              // 캡처할 변수 복사
            Transform buttonChild = transform.GetChild(i);
            buttons[i] = buttonChild.GetComponent<Button>();
            canvasGroup[i] = buttons[i].GetComponent<CanvasGroup>();
            buttons[i].onClick.AddListener(() => SendCard(index));      // 복사된 변수로 람다식으로 몇번째 버튼이 클릭되었는지 넘겨줌
        }

        ButtonsSpriteChange();
    }

    /// <summary>
    /// 해당하는 카드를 활성화 하기 위한 함수(PlaceCard 에서 확인 후 알파값 조절/선택이 취소된 카드용)
    /// </summary>
    /// <param name="cardIndex"></param>
    private void OnCardEnable(int cardIndex)
    {
        canvasGroup[cardIndex].alpha = 1;                   // 알파값 1로 조절
        canvasGroup[cardIndex].interactable = true;         // 상호작용 되게 변경
    }

    /// <summary>
    /// 해당하는 카드를 비활성화 하기 위한 함수(PlaceCard 에서 확인 후 알파값 조절/선택한 카드용)
    /// </summary>
    /// <param name="cardIndex"></param>
    private void OnCardDisable(int cardIndex)
    {
        canvasGroup[cardIndex].alpha = 0;                   // 알파값 0으로 조절
        canvasGroup[cardIndex].interactable = false;        // 상호작용 안되게 변경
    }

    /// <summary>
    /// 카드 버튼 클릭으로 실행되는 함수
    /// 0 : 아래, 1 : 위, 2 : 오른쪽, 3 : 왼쪽, 4 : 가드, 5 : 공격, 6 : 마법공격, 7 : 특수공격, 8 : 에너지 업, 9 : 더블 오른쪽, 10 : 더블 왼쪽, 11 : 퍼펙트 가드, 12 : 힐
    /// </summary>
    private void SendCard(int buttonNumber)
    {
        //Debug.Log($"{buttonNumber} 버튼 클릭");
        onCardButton?.Invoke(buttonNumber);
    }

    /// <summary>
    /// 내 캐릭터에 따라서 버튼의 이미지, 텍스트를 변경하는 함수
    /// </summary>
    private void ButtonsSpriteChange()
    {
        // 게임 매니저가 가지고 있는 내 캐릭터의 인덱스
        switch(gameManager.playerCharacterIndex)
        {
            // Adel
            case 0:
                for( int i = 0; i< moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Adels[0];
                }

                // guard 이미지 변경 4, 11
                for(int i = 0; i< guardImages.Length; i++)
                {
                    guardImages[i].sprite = Adels[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for(int i = 0; i< attackImages.Length; i++)
                {
                    attackImages[i].sprite = Adels[i + 2];
                }

                // energy, heal 8, 12
                for(int i = 0; i< energyImage.Length; i++)
                {
                    energyImage[i].sprite = Adels[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for(int i = 0; i< attackRange.Length; i++)
                {
                    attackRange[i].sprite = Adels[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "25";
                attackTexts[1].text = "15";
                magicAttackTexts[0].text = "30";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "50";
                limitAttackTexts[1].text = "40";

                break;

            // Akstar
            case 1:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Akstars[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = Akstars[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = Akstars[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = Akstars[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = Akstars[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "25";
                attackTexts[1].text = "15";
                magicAttackTexts[0].text = "35";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "45";
                limitAttackTexts[1].text = "45";

                break;

            //Amelia
            case 2:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Amelias[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = Amelias[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = Amelias[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = Amelias[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = Amelias[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "25";
                attackTexts[1].text = "15";
                magicAttackTexts[0].text = "30";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "40";
                limitAttackTexts[1].text = "45";

                break;

            // Arngrim
            case 3:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Arngrims[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = Arngrims[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = Arngrims[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = Arngrims[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = Arngrims[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "20";
                attackTexts[1].text = "25";
                magicAttackTexts[0].text = "35";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "50";
                limitAttackTexts[1].text = "40";

                break;

            // Barbariccia
            case 4:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Barbariccias[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = Barbariccias[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = Barbariccias[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = Barbariccias[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = Barbariccias[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "25";
                attackTexts[1].text = "15";
                magicAttackTexts[0].text = "35";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "40";
                limitAttackTexts[1].text = "45";

                break;

            // BlackMage
            case 5:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = BlackMages[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = BlackMages[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = BlackMages[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = BlackMages[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = BlackMages[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "20";
                attackTexts[1].text = "15";
                magicAttackTexts[0].text = "30";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "50";
                limitAttackTexts[1].text = "40";

                break;

            // Cloud
            case 6:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Clouds[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = Clouds[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = Clouds[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = Clouds[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = Clouds[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "25";
                attackTexts[1].text = "15";
                magicAttackTexts[0].text = "30";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "70";
                limitAttackTexts[1].text = "40";

                break;

            // Elle
            case 7:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Elles[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = Elles[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = Elles[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = Elles[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = Elles[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "25";
                attackTexts[1].text = "15";
                magicAttackTexts[0].text = "35";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "45";
                limitAttackTexts[1].text = "45";

                break;

            // Jade
            case 8:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Jades[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = Jades[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = Jades[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = Jades[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = Jades[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "20";
                attackTexts[1].text = "25";
                magicAttackTexts[0].text = "35";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "45";
                limitAttackTexts[1].text = "45";

                break;

            // Nalu
            case 9:
                for (int i = 0; i < moveImages.Length; i++)      // move 이미지 변경 0, 1, 2, 3, 9, 10
                {
                    moveImages[i].sprite = Nalus[0];
                }

                // guard 이미지 변경 4, 11
                for (int i = 0; i < guardImages.Length; i++)
                {
                    guardImages[i].sprite = Nalus[1];
                }

                // attack 3가지 이미지 변경 5, 6, 7(2,3,4 번 이미지)
                for (int i = 0; i < attackImages.Length; i++)
                {
                    attackImages[i].sprite = Nalus[i + 2];
                }

                // energy, heal 8, 12
                for (int i = 0; i < energyImage.Length; i++)
                {
                    energyImage[i].sprite = Nalus[5];
                }

                // 공격 범위 5, 6, 7(6,7,8 번 이미지)
                for (int i = 0; i < attackRange.Length; i++)
                {
                    attackRange[i].sprite = Nalus[i + 6];
                }

                // 공격력 & 에너지 텍스트 수정
                attackTexts[0].text = "25";
                attackTexts[1].text = "15";
                magicAttackTexts[0].text = "35";
                magicAttackTexts[1].text = "25";
                limitAttackTexts[0].text = "40";
                limitAttackTexts[1].text = "40";

                break;
        }
    }
}
