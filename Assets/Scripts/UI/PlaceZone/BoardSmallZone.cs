using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardSmallZone : MonoBehaviour
{
    /// <summary>
    /// 플레이어의 자리
    /// </summary>
    Image[] player1;

    /// <summary>
    /// 적 플레이어의 자리
    /// </summary>
    Image[] player2;

    /// <summary>
    /// 알파값 조절하기 위함
    /// </summary>
    Color color;

    /// <summary>
    /// 캐릭터들의 얼굴 스프라이트
    /// </summary>
    public Sprite[] characterSprites;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    /// <summary>
    /// 적 플레이어
    /// </summary>
    EnemyPlayer enemyPlayer;

    // 이전 플레이어가 위치했던 인덱스
    int oldPlayerSectionindex = 0;
    int oldEnemySectionindex = 0;

    private void Awake()
    {
        // 배열 초기화
        player1 = new Image[12];
        player2 = new Image[12];

        Transform child = transform.GetChild(0).GetChild(0);

        for(int i = 0; i< player1.Length; i++)
        {
            player1[i] = child.GetChild(i).GetComponent<Image>();
            //player1[i].gameObject.SetActive(false);       => 알파값 조절로 변경
            SetImageAlpha(player1[i], 0);
        }

        child = transform.GetChild(0).GetChild(1);

        for(int i = 0;i< player2.Length; i++)
        {
            player2[i] = child.GetChild(i).GetComponent<Image>();
            //player2[i].gameObject.SetActive(false);       => 알파값 조절로 변경
            SetImageAlpha(player2[i], 0);
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
        enemyPlayer = gameManager.EnemyPlayer;

        player.currentSection += PlayerUpdateBoardImage;
        enemyPlayer.currentSection += EnemyUpdateBoardImage;

        SetImageAlpha(player1[0], 1f);      // 플레이어 위치에 해당하는 이미지를 불투명하게
        player1[0].sprite = characterSprites[gameManager.playerCharacterIndex];

        SetImageAlpha(player2[3], 1f);      // 적 플레이어 위치에 해당하는 이미지를 불투명하게
        player2[3].sprite = characterSprites[gameManager.enemyPlayerCharacterIndex];
    }

    /// <summary>
    /// 플레이어의 위치 변경으로 작은 보드를 업데이트 하는 함수
    /// </summary>
    /// <param name="playerSection">=플레이어의 위치</param>
    private void EnemyUpdateBoardImage(int playerSection)
    {
        if (oldPlayerSectionindex != -1)
        {
            SetImageAlpha(player1[oldPlayerSectionindex], 0);   // 이전 위치의 알파값을 0으로 설정 (투명하게)
        }

        // 해당하는 이미지 활성화 및 캐릭터에 맞게 스프라이트 변경
        player1[playerSection].sprite = characterSprites[gameManager.playerCharacterIndex];
        SetImageAlpha(player1[playerSection], 1f);              // 새로운 위치를 불투명하게 설정

        // 새로운 위치를 이전 위치로 저장
        oldPlayerSectionindex = playerSection;
    }

    /// <summary>
    /// 적의 위치 변경으로 작은 보드를 업데이트 하는 함수
    /// </summary>
    /// <param name="enemySection">적 플레이어의 위치</param>
    private void PlayerUpdateBoardImage(int enemySection)
    {
        if (oldEnemySectionindex != -1)
        {
            SetImageAlpha(player1[oldEnemySectionindex], 0);        // 이전 위치의 알파값을 0으로 설정 (투명하게)
        }

        // 해당하는 이미지 활성화 및 캐릭터에 맞게 스프라이트 변경
        player1[enemySection].sprite = characterSprites[gameManager.playerCharacterIndex];
        SetImageAlpha(player1[enemySection], 1f);                   // 새로운 위치를 불투명하게 설정

        // 새로운 위치를 이전 위치로 저장
        oldEnemySectionindex = enemySection;
    }

    /// <summary>
    /// 이미지를 투명도에 맞게 설정하는 함수
    /// </summary>
    private void SetImageAlpha(Image image, float alpha)
    {
        color = image.color;  // 이미지의 현재 색상 값
        color.a = alpha;  // 알파값을 조정
        image.color = color;  // 색상 적용
    }
}
