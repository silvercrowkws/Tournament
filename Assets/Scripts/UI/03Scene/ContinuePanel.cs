using System;
using TMPro;
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

    /// <summary>
    /// 이겼을 때 활성화 됨
    /// </summary>
    GameObject win;

    /// <summary>
    /// 졌을 때 활성화 됨
    /// </summary>
    GameObject lose;

    /// <summary>
    /// 몇 번 이겼는지 텍스트
    /// </summary>
    TextMeshProUGUI numberText;

    private void Awake()
    {
        gameManager = GameManager.Instance;

        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;

        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(PanelDisable);

        Transform child = transform.GetChild(1);
        win = child.gameObject;
        win.SetActive(false);

        child = child.GetChild(1);
        numberText = child.GetComponent<TextMeshProUGUI>();

        child = transform.GetChild(2);
        lose = child.gameObject;
        lose.SetActive(false);

        if (gameManager.playerResult)
        {
            // 전체 대전 수 - 현재 남아있는 적 수 = 현재까지 이긴 횟수
            // +1 은 이겼을 때 PanelDisable를 눌러야 리스트에서 0번이 빠지기 때문에 따로 계산
            numberText.text = $"({(gameManager.tournamentList.Count - gameManager.gameTournamentList.Count) + 1 } / 10)";
            //Debug.Log($"gameManager.tournamentList.Count : {gameManager.tournamentList.Count}");
            //Debug.Log($"gameManager.gameTournamentList.Count : {gameManager.gameTournamentList.Count}");
            //Debug.Log($"gameManager.gameTournamentList.Count + 1 : {gameManager.gameTournamentList.Count + 1}");
            win.SetActive(true);
            lose.SetActive(false);
        }
        else
        {
            lose?.SetActive(true);
            win.SetActive(false);
        }
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

                if(gameManager.gameTournamentList.Count > 0)    // 마지막 적이었을 경우 리스트의 0번을 제거하면 리스트에는 아무것도 없기 때문에
                {
                    gameManager.enemyPlayerCharacterIndex = gameManager.gameTournamentList[0];      // 적 플레이어의 번호 변경
                }
                else
                {
                    Debug.Log("모든 적을 처치했습니다! 더 이상 적이 없습니다.");
                    gameManager.enemyPlayerCharacterIndex = -1;     // 혹은 적이 없는 상태를 의미하는 값 설정
                }
            }
        }

        // 다음 라운드가 남아있을 때만 처리
        if(gameManager.gameTournamentList.Count > 0)
        {
            onPanelDisable?.Invoke();                       // VSImage2에 알림
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
        }
        else
        {
            // 히든 보스 추가하고 싶으면 여기서 뭔가를?
            // gameManager.gameTournamentList의 0번에 히든 보스를 넣고?
            // gameManager.enemyPlayerCharacterIndex 이것도 설정하고?
            // 그리고 이 else 문은 한번만 실행되게 뭔가 해야 함 => 안그러면 계속 반복될 듯?
        }
    }
}
