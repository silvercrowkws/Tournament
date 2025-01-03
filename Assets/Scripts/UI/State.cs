using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class State : MonoBehaviour
{
    /// <summary>
    /// 스프라이트 배열
    /// </summary>
    public Sprite[] characters;

    /// <summary>
    /// 플레이어의 이미지
    /// </summary>
    Image playerImage;

    /// <summary>
    /// 적 플레이어의 이미지
    /// </summary>
    Image enemyPlayerImage;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    // 1. 이 씬이 불러졌을 때 플레이어와 적 플레이어의 이미지로 변경
    // 2. 턴이 진행될 때마다 RoundNumberText 숫자 늘리기
    // 3. 플레이어의 체력과 에너지를 보여줘야 함

    private void Awake()
    {
        Transform child = transform.GetChild(0);                    // 0번째 자식 PlayerPanelImage

        playerImage = child.GetChild(0).GetComponent<Image>();      // PlayerPanelImage의 0번째 자식 PlayerImage

        child = transform.GetChild(1);                              // 1번째 자식 EnemyPlayerPanelImage

        enemyPlayerImage = child.GetChild(0).GetComponent<Image>(); // EnemyPlayerPanelImage의 0번째 자식 EnemyPlayerImage
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        ImageChange();
    }

    /// <summary>
    /// 플레이어와 적의 이미지를 변경시키는 함수
    /// </summary>
    void ImageChange()
    {
        // 스테이지가 종료되었을 때 이 함수 사용하면 대진표에 맞게 적 플레이어의 이미지가 변경되어야 함
        playerImage.sprite = characters[gameManager.playerCharacterIndex];

        enemyPlayerImage.sprite = characters[gameManager.gameTournamentList[0]];        // 적 플레이어의 이미지 gameTournamentList의 0번으로 변경
        // 위에 코드 나중에 상대를 이겼으면 ++ 하면서 변경하던가
        // 아니면 대진표의 0번을 삭제하는 방법으로 가야할 듯
    }


    // 1. 전투로 넘어갈 때 Fram 게임 오브젝트를 컨트롤 해야 함(Card_Fram 클래스를 따로 만들어서 하는게 나을 듯)
    // 2. Card_Fram 클래스에는 선택 가능한 카드들이 포함되어야 함
}
