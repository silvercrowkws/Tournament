using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VSImage2 : MonoBehaviour
{
    /// <summary>
    /// vs 스프라이트 배열
    /// </summary>
    public Sprite[] characters;

    /// <summary>
    /// 왼쪽 캐릭터 이미지
    /// </summary>
    Image leftCharacter;

    /// <summary>
    /// 오른쪽 캐릭터 이미지
    /// </summary>
    Image rightCharacter;

    /// <summary>
    /// 왼쪽 캐릭터의 color
    /// </summary>
    Color leftColor;

    /// <summary>
    /// 오른쪽 캐릭터의 color
    /// </summary>
    Color rightColor;

    // 각 캐릭터의 시작 위치
    Vector3 leftStartPos;
    Vector3 rightStartPos;
    
    FightControlButtons2 fightControlButtons2;

    /// <summary>
    /// 캐릭터 선택시 잠깐 깜빡임에 사용할 이미지
    /// </summary>
    Image flashImage;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 컨티뉴 패널
    /// </summary>
    ContinuePanel continuePanel;

    private void Awake()
    {
        leftCharacter = transform.GetChild(0).GetComponent<Image>();
        rightCharacter = transform.GetChild(1).GetComponent<Image>();

        flashImage = transform.GetChild(2).GetComponent<Image>();
        flashImage.gameObject.SetActive(false);

        characterAlphaZero();

        leftStartPos = leftCharacter.rectTransform.localPosition;
        rightStartPos = rightCharacter.rectTransform.localPosition;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        continuePanel = FindAnyObjectByType<ContinuePanel>();
        continuePanel.onPanelDisable += OnPickCharacter;
    }

    /// <summary>
    /// 캐릭터를 선택했을 때 vsImage 로 보여주는 함수
    /// </summary>
    private void OnPickCharacter()
    {
        leftCharacter.sprite = characters[gameManager.playerCharacterIndex];
        rightCharacter.sprite = characters[gameManager.gameTournamentList[0]];

        leftColor.a = 1f;                   // 알파값을 1로 설정
        leftCharacter.color = leftColor;

        rightColor.a = 1f;                  // 알파값을 1로 설정
        rightCharacter.color = rightColor;

        StartCoroutine(MoveCharacters());
    }

    /// <summary>
    /// 캐릭터 선택 이미지를 이동시키는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveCharacters()
    {
        float duration = 0.3f;        // 0.3초 동안
        float elapsedTime = 0f;

        //leftStartPos = leftCharacter.rectTransform.localPosition;
        //rightStartPos = rightCharacter.rectTransform.localPosition;

        // 상대적인 이동 거리 (기존 위치에서 450만큼 이동)
        Vector3 leftEndPos = new Vector3(leftStartPos.x + 450f, leftStartPos.y, leftStartPos.z);
        Vector3 rightEndPos = new Vector3(rightStartPos.x - 450f, rightStartPos.y, rightStartPos.z);


        // flashImage 활성화
        flashImage.gameObject.SetActive(true);

        // 0.1초 후에 flashImage 비활성화
        yield return new WaitForSeconds(0.1f);
        flashImage.gameObject.SetActive(false);

        while (elapsedTime < duration)
        {
            // 비율을 계산하여 위치를 보간
            float t = elapsedTime / duration;

            leftCharacter.rectTransform.localPosition = Vector3.Lerp(leftStartPos, leftEndPos, t);
            rightCharacter.rectTransform.localPosition = Vector3.Lerp(rightStartPos, rightEndPos, t);

            elapsedTime += Time.deltaTime; // 경과 시간 증가
            yield return null; // 매 프레임 대기
        }

        // 종료 시 정확한 위치로 설정 (혹시 오차가 있을 수 있기 때문에)
        leftCharacter.rectTransform.localPosition = leftEndPos;
        rightCharacter.rectTransform.localPosition = rightEndPos;
    }

    /// <summary>
    /// 왼쪽 오른쪽 캐릭터의 알파값을 0으로 변경하는 함수
    /// </summary>
    void characterAlphaZero()
    {
        leftColor = leftCharacter.color;
        leftColor.a = 0f;                   // 알파값을 0으로 설정
        leftCharacter.color = leftColor;

        rightColor = rightCharacter.color;
        rightColor.a = 0f;                  // 알파값을 0으로 설정
        rightCharacter.color = rightColor;
    }
}
