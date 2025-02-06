using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NextZone : MonoBehaviour
{
    // Fram 오브젝트가 꺼질때 => 컨티뉴 버튼이 눌러졌을 때 알파값과 상호작용 가능하게 변경
    // 플레이어와 적 플레이어의 턴이 모두 끝나면 버튼의 알파값을 올리고 상호작용 가능하게 변경
    // 버튼은 다시 Fram 클래스를 활성화 시켜야 함

    /// <summary>
    /// 캔버스 그룹
    /// </summary>
    CanvasGroup canvasGroup;

    /// <summary>
    /// 컨트롤 존 클래스
    /// </summary>
    ControlZone controlZone;

    /// <summary>
    /// 플레이어를 움직이는 클래스
    /// </summary>
    ActivePlayer activePlayer;

    /// <summary>
    /// 플레이스 카드 클래스
    /// </summary>
    PlaceCard placeCard;

    /// <summary>
    /// NextRound 버튼
    /// </summary>
    Button nextButton;

    /// <summary>
    /// 카드프레임 클래스
    /// </summary>
    Card_Frame cardFrame;

    /// <summary>
    /// 카드 프레임의 알파값을 켜라고 알리는 델리게이트(PlaceCard 클래스의 OnClearPlace와도 연결)
    /// </summary>
    public Action onFramSetActive;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 턴 매니저
    /// </summary>
    TurnManager turnManager;

    /// <summary>
    /// 카드 뒷면
    /// </summary>
    public Sprite cardBack;

    /// <summary>
    /// 플레이어의 카드 3장
    /// </summary>
    Image[] playerNext = new Image[3];

    /// <summary>
    /// 적 플레이어의 카드 3장
    /// </summary>
    Image[] enemyPlayerNext = new Image[3];

    /// <summary>
    /// 각 캐릭터의 카드를 복사하는 배열
    /// </summary>
    public Sprite[] characterCardNext;

    /// <summary>
    /// 적 플레이어의 카드를 복사하는 배열
    /// </summary>
    public Sprite[] enemyCharacterCardNext;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        nextButton = GetComponentInChildren<Button>();
        nextButton.onClick.AddListener(ChangeAlphaZero);
        
        for(int i = 0; i< 3; i++)
        {
            Transform playerChild = transform.GetChild(0);
            Transform enermyChild = transform.GetChild(1);
            playerNext[i] = playerChild.GetChild(i).GetComponent<Image>();
            enemyPlayerNext[i] = enermyChild.GetChild(i).GetComponent<Image>();
        }
    }

    private void Start()
    {
        player = GameManager.Instance.Player;

        cardFrame = FindAnyObjectByType<Card_Frame>();

        activePlayer = FindAnyObjectByType<ActivePlayer>();
        activePlayer.onNextCard += OnNextCard;

        controlZone = FindAnyObjectByType<ControlZone>();
        controlZone.onContinue += ChangeAlpha;

        gameManager = GameManager.Instance;
        gameManager.onBothPlayersDone += OnBothPlayersDone;

        turnManager = TurnManager.Instance;

        placeCard = FindAnyObjectByType<PlaceCard>();

        // 스프라이트 배열 복사
        switch (gameManager.playerCharacterIndex)
        {
            case 0:
                characterCardNext = placeCard.AdelCards.ToArray();
                break;
            case 1:
                characterCardNext = placeCard.AkstarCards.ToArray();
                break;
            case 2:
                characterCardNext = placeCard.AmeliaCards.ToArray();
                break;
            case 3:
                characterCardNext = placeCard.ArngrimCards.ToArray();
                break;
            case 4:
                characterCardNext = placeCard.BarbaricciaCards.ToArray();
                break;
            case 5:
                characterCardNext = placeCard.BlackMageCards.ToArray();
                break;
            case 6:
                characterCardNext = placeCard.CloudCards.ToArray();
                break;
            case 7:
                characterCardNext = placeCard.ElleCards.ToArray();
                break;
            case 8:
                characterCardNext = placeCard.JadeCards.ToArray();
                break;
            case 9:
                characterCardNext = placeCard.NaluCards.ToArray();
                break;
        }

        // 적이 쓸 스프라이트 배열 복사
        switch (gameManager.enemyPlayerCharacterIndex)
        {
            case 0:
                enemyCharacterCardNext = placeCard.AdelCards.ToArray();
                break;
            case 1:
                enemyCharacterCardNext = placeCard.AkstarCards.ToArray();
                break;
            case 2:
                enemyCharacterCardNext = placeCard.AmeliaCards.ToArray();
                break;
            case 3:
                enemyCharacterCardNext = placeCard.ArngrimCards.ToArray();
                break;
            case 4:
                enemyCharacterCardNext = placeCard.BarbaricciaCards.ToArray();
                break;
            case 5:
                enemyCharacterCardNext = placeCard.BlackMageCards.ToArray();
                break;
            case 6:
                enemyCharacterCardNext = placeCard.CloudCards.ToArray();
                break;
            case 7:
                enemyCharacterCardNext = placeCard.ElleCards.ToArray();
                break;
            case 8:
                enemyCharacterCardNext = placeCard.JadeCards.ToArray();
                break;
            case 9:
                enemyCharacterCardNext = placeCard.NaluCards.ToArray();
                break;
        }

        // 상호작용 불가능하게 설정
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        nextButton.gameObject.SetActive(false);     // 버튼은 비활성화
    }


    /// <summary>
    /// 행동에 맞게 카드를 보여주는 함수
    /// </summary>
    /// <param name="activeNumber">몇번째 행동인지(0부터 시작)</param>
    private void OnNextCard(int activeNumber)
    {
        // activeNumber가 0, 1, 2 범위 내에 있는지 확인
        if (activeNumber >= 0 && activeNumber < 3)
        {
            if (activeNumber == 0)
            {
                playerNext[activeNumber].sprite = characterCardNext[controlZone.firstTurnCardIndex];
                enemyPlayerNext[2 - activeNumber].sprite = enemyCharacterCardNext[activePlayer.EfirstTurnCardIndex];
            }
            else if (activeNumber == 1)
            {
                playerNext[activeNumber].sprite = characterCardNext[controlZone.secondTurnCardIndex];
                enemyPlayerNext[2 - activeNumber].sprite = enemyCharacterCardNext[activePlayer.EsecondTurnCardIndex];
            }
            else if (activeNumber == 2)
            {
                playerNext[activeNumber].sprite = characterCardNext[controlZone.thirdTurnCardIndex];
                enemyPlayerNext[2 - activeNumber].sprite = enemyCharacterCardNext[activePlayer.EthirdTurnCardIndex];
            }
        }
        else
        {
            Debug.LogError("activeNumber 값이 배열 인덱스를 초과했습니다. activeNumber: " + activeNumber);
        }
    }

    /// <summary>
    /// 컨티뉴 버튼 클릭으로 알파값과 상호작용 가능하게 변경하는 함수
    /// </summary>
    private void ChangeAlpha()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;

        //nextButton.gameObject.SetActive(false);     // 버튼은 비활성화
    }

    /// <summary>
    /// NextRound 버튼 클릭으로 알파값과 상호작용을 불가능하게 변경하는 함수
    /// 이 버튼 클릭하면 Clear 알아서 들어가야 함
    /// </summary>
    private void ChangeAlphaZero()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        nextButton.gameObject.SetActive(false);     // 버튼은 비활성화
        gameManager.isPlayerDone = false;
        gameManager.isEnemyPlayerDone = false;

        for(int i = 0; i< 3; i++)
        {
            playerNext[i].sprite = cardBack;
            enemyPlayerNext[i].sprite = cardBack;
            
        }
        turnManager.OnTurnEnd2();                    // 턴 종료
        onFramSetActive?.Invoke();
        player.Energy += 15;                        // 에너지 15 회복(나중에 적 플레이어도 같은 것 필요할 지 모름)
    }

    /// <summary>
    /// 양쪽 플레이어 모두 턴을 마치고 NextRound 버튼을 활성화하는 함수
    /// </summary>
    private void OnBothPlayersDone()
    {
        nextButton.gameObject.SetActive(true);     // 버튼은 활성화
    }
}
