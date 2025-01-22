using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        nextButton = GetComponentInChildren<Button>();
        nextButton.onClick.AddListener(ChangeAlphaZero);
    }

    private void Start()
    {
        cardFrame = FindAnyObjectByType<Card_Frame>();

        controlZone = FindAnyObjectByType<ControlZone>();
        controlZone.onContinue += ChangeAlpha;

        gameManager = GameManager.Instance;
        gameManager.onBothPlayersDone += OnBothPlayersDone;

        // 상호작용 불가능하게 설정
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        nextButton.gameObject.SetActive(false);     // 버튼은 비활성화
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
        onFramSetActive?.Invoke();
    }

    /// <summary>
    /// 양쪽 플레이어 모두 턴을 마치고 NextRound 버튼을 활성화하는 함수
    /// </summary>
    private void OnBothPlayersDone()
    {
        nextButton.gameObject.SetActive(true);     // 버튼은 활성화
    }
}
