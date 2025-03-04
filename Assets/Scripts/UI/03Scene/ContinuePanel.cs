using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContinuePanel : MonoBehaviour
{
    /// <summary>
    /// 버튼
    /// </summary>
    Button button;

    /// <summary>
    /// 캔버스 그룹
    /// </summary>
    CanvasGroup canvasGroup;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 컨티뉴 버튼을 눌러서 패널이 안보이게 됬음을 알리는 델리게이트
    /// </summary>
    public Action onPanelDisable;

    private void Awake()
    {
        gameManager = GameManager.Instance;

        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;

        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(PanelDisable);
    }

    /// <summary>
    /// 버튼 클릭으로 패널을 안보이게 하는 함수(게임 매니저의 토너먼트 리스트의 0번 제거 포함)
    /// </summary>
    private void PanelDisable()
    {
        // 플레이어가 이긴 상황에서만 리스트 0번 제거
        if (gameManager.playerResult)
        {
            if(gameManager.gameTournamentList.Count > 0)
            {
                gameManager.gameTournamentList.RemoveAt(0);     // 리스트의 0번 제거
                gameManager.enemyPlayerCharacterIndex = gameManager.gameTournamentList[0];      // 적 플레이어의 번호 변경
            }
        }

        onPanelDisable?.Invoke();                       // VSImage2에 알림
        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
    }
}
