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
    int oldPlayerSectionindex = 4;
    int oldEnemySectionindex = 7;

    /// <summary>
    /// 넥스트 존 클래스
    /// </summary>
    NextZone nextZone;

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
        nextZone = FindAnyObjectByType<NextZone>();
        nextZone.onFramSetActive += OnRotateImage;

        player.currentSection += PlayerUpdateBoardImage;
        enemyPlayer.currentSection += EnemyUpdateBoardImage;

        SetImageAlpha(player1[4], 1f);                                                  // 플레이어 위치에 해당하는 이미지를 불투명하게
        player1[4].sprite = characterSprites[gameManager.playerCharacterIndex];         // 플레이어의 초기 위치

        SetImageAlpha(player2[7], 1f);                                                  // 적 플레이어 위치에 해당하는 이미지를 불투명하게
        player2[7].sprite = characterSprites[gameManager.enemyPlayerCharacterIndex];    // 적 플레이어의 초기 위치

        // 시작 시 플레이어와 적의 위치에 맞게 회전 설정
        RotateImagesToFace(player1[4], player.gameObject.transform.position, enemyPlayer.gameObject.transform.position);
        RotateImagesToFace(player2[7], enemyPlayer.gameObject.transform.position, player.gameObject.transform.position);
    }

    /// <summary>
    /// 플레이어의 위치 변경으로 작은 보드를 업데이트 하는 함수
    /// </summary>
    /// <param name="playerSection">=플레이어의 위치</param>
    private void PlayerUpdateBoardImage(int playerSection)
    {
        if (oldPlayerSectionindex != -1)
        {
            SetImageAlpha(player1[oldPlayerSectionindex], 0);   // 이전 위치의 알파값을 0으로 설정 (투명하게)
        }

        // 해당하는 이미지 활성화 및 캐릭터에 맞게 스프라이트 변경
        player1[playerSection].sprite = characterSprites[gameManager.playerCharacterIndex];
        SetImageAlpha(player1[playerSection], 1f);              // 새로운 위치를 불투명하게 설정
        
        // 플레이어의 위치를 기준으로 적의 위치와 비교하여 회전 (마주보도록)
        RotateImagesToFace(player1[playerSection], player.gameObject.transform.position, enemyPlayer.gameObject.transform.position);

        // 새로운 위치를 이전 위치로 저장
        oldPlayerSectionindex = playerSection;
    }

    /// <summary>
    /// 적의 위치 변경으로 작은 보드를 업데이트 하는 함수
    /// </summary>
    /// <param name="enemySection">적 플레이어의 위치</param>
    private void EnemyUpdateBoardImage(int enemySection)
    {
        if (oldEnemySectionindex != -1)
        {
            SetImageAlpha(player2[oldEnemySectionindex], 0);        // 이전 위치의 알파값을 0으로 설정 (투명하게)
        }

        // 해당하는 이미지 활성화 및 캐릭터에 맞게 스프라이트 변경
        player2[enemySection].sprite = characterSprites[gameManager.enemyPlayerCharacterIndex];
        SetImageAlpha(player2[enemySection], 1f);                   // 새로운 위치를 불투명하게 설정

        // 적의 위치를 기준으로 플레이어의 위치와 비교하여 회전 (마주보도록)
        RotateImagesToFace(player2[enemySection], enemyPlayer.gameObject.transform.position, player.gameObject.transform.position);

        // 새로운 위치를 이전 위치로 저장
        oldEnemySectionindex = enemySection;
    }

    /// <summary>
    /// 이미지를 투명도에 맞게 설정하는 함수
    /// </summary>
    private void SetImageAlpha(Image image, float alpha)
    {
        color = image.color;    // 이미지의 현재 색상 값
        color.a = alpha;        // 알파값을 조정
        image.color = color;    // 색상 적용
    }

    /// <summary>
    /// 두 객체가 마주보도록 이미지를 회전시키는 함수
    /// </summary>
    private void RotateImagesToFace(Image image, Vector3 playerPosition, Vector3 enemyPosition)
    {
        // RectTransform을 사용할 수 있게 이미지의 RectTransform 접근
        RectTransform rectTransform = image.GetComponent<RectTransform>();

        // X값을 비교하여 회전 결정
        float directionX = playerPosition.x - enemyPosition.x;

        // 적과 플레이어의 상대 위치를 기준으로 0도 또는 180도로 회전
        if (directionX > 0)
        {
            // 플레이어가 적의 오른쪽에 있으면 0도 (정면)
            rectTransform.localRotation = Quaternion.Euler(0, 0, 0);  // Y축 기준으로 0도 회전
        }
        else if (directionX < 0)
        {
            // 플레이어가 적의 왼쪽에 있으면 180도 (뒤로)
            rectTransform.localRotation = Quaternion.Euler(0, 180, 0);  // Y축 기준으로 180도 회전
        }
        else
        {
            // X값이 같으면 정면(0도)으로 설정 (완전 일치하는 경우)
            rectTransform.localRotation = Quaternion.Euler(0, 0, 0);  // Y축 기준으로 0도 회전
        }
    }

    private void OnRotateImage()
    {
        RotateImagesToFace(player1[oldPlayerSectionindex], player.gameObject.transform.position, enemyPlayer.gameObject.transform.position);
        RotateImagesToFace(player2[oldEnemySectionindex], enemyPlayer.gameObject.transform.position, player.gameObject.transform.position);
    }
}
