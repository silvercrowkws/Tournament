using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterName2
{
    Adel = 0,
    Akstar,
    Amelia,
    Arngrim,
    Barbariccia,
    BlackMage,
    Cloud,
    Elle,
    Jade,
    Nalu,
}

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
    /// 토너먼트 차트 이미지 배열
    /// </summary>
    Image[] tournamentImages;

    /// <summary>
    /// 토너먼트 차트 프레임 이미지 배열
    /// </summary>
    Image[] frameImages;

    /// <summary>
    /// 이겼을 때 X 표시해줄 이미지 배열
    /// </summary>
    Image[] X_Images;

    /// <summary>
    /// 왼쪽 캐릭터의 color
    /// </summary>
    Color leftColor;

    /// <summary>
    /// 오른쪽 캐릭터의 color
    /// </summary>
    Color rightColor;

    /// <summary>
    /// 토너먼트 이미지의 color
    /// </summary>
    Color tournamentColor;

    /// <summary>
    /// 대결 순서 스프라이트 배열
    /// </summary>
    public Sprite[] tournaments;

    /// <summary>
    /// 상대의 수
    /// </summary>
    int tournamentsCount;

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

    /// <summary>
    /// 왼쪽 캐릭터 이름
    /// </summary>
    TextMeshProUGUI leftCharacterName;

    /// <summary>
    /// 오른쪽 캐릭터 이름
    /// </summary>
    TextMeshProUGUI rightCharacterName;

    /// <summary>
    /// 승리 횟수
    /// </summary>
    int winCount;

    private void Awake()
    {
        leftCharacter = transform.GetChild(0).GetComponent<Image>();
        rightCharacter = transform.GetChild(1).GetComponent<Image>();

        flashImage = transform.GetChild(3).GetComponent<Image>();
        flashImage.gameObject.SetActive(false);

        Transform child = transform.GetChild(4);
        leftCharacterName = child.GetComponent<TextMeshProUGUI>();

        child = transform.GetChild(5);
        rightCharacterName = child.GetComponent<TextMeshProUGUI>();

        //characterAlphaZero();

        child = transform.GetChild(2);        // 2번째 자식 TournamentChart

        tournamentsCount = child.childCount;

        // tournaments, frameImages 배열 초기화
        tournamentImages = new Image[tournamentsCount];
        frameImages = new Image[tournamentsCount];
        X_Images = new Image[tournamentsCount];

        for (int i = 0; i < tournamentsCount; i++)
        {
            tournamentImages[i] = child.GetChild(i).GetComponent<Image>();
            //frameImages[i] = tournamentImages[i].GetComponentInChildren<Image>();
            frameImages[i] = tournamentImages[i].transform.GetChild(0).GetComponent<Image>();
            X_Images[i] = tournamentImages[i].transform.GetChild(1).GetComponent<Image>();

            // 알파값을 0으로 설정
            tournamentColor = tournamentImages[i].color;
            tournamentColor.a = 0f;
            tournamentImages[i].color = tournamentColor;
            frameImages[i].gameObject.SetActive(false);
            X_Images[i].gameObject.SetActive(false);
        }

        leftStartPos = leftCharacter.rectTransform.localPosition;
        rightStartPos = rightCharacter.rectTransform.localPosition;

        characterAlphaZero();
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
        rightCharacter.sprite = characters[gameManager.gameTournamentList[0]];      // 여긴 다음 대상을 보여야 하니까 기존 리스트

        leftColor.a = 1f;                   // 알파값을 1로 설정
        leftCharacter.color = leftColor;

        rightColor.a = 1f;                  // 알파값을 1로 설정
        rightCharacter.color = rightColor;

        // 왼쪽 플레이어 이름 수정
        if (Enum.IsDefined(typeof(CharacterName2), gameManager.playerCharacterIndex))
        {
            CharacterName2 character = (CharacterName2)gameManager.playerCharacterIndex;
            string characterName = character.ToString().ToUpper();

            // 각 글자마다 \n을 추가하여 세로로 표시
            leftCharacterName.text = string.Join("\n", characterName.ToCharArray());
        }
        else
        {
            leftCharacterName.text = "Invalid Index";
        }

        // 오른쪽 플레이어(적) 이름 수정
        if (Enum.IsDefined(typeof(CharacterName2), gameManager.gameTournamentList[0]))
        {
            CharacterName2 character = (CharacterName2)gameManager.gameTournamentList[0];
            string characterName = character.ToString().ToUpper();

            // 각 글자마다 \n을 추가하여 세로로 표시
            rightCharacterName.text = string.Join("\n", characterName.ToCharArray());
        }
        else
        {
            rightCharacterName.text = "Invalid Index";
        }

        for (int i = 0; i < tournamentsCount; i++)
        {
            tournamentImages[i].sprite = tournaments[gameManager.tournamentList[i]];        // 여긴 원래 대진표를 보이는 곳이니까 복사한 리스트
            tournamentColor.a = 1f;
            tournamentImages[i].color = tournamentColor;
            frameImages[i].gameObject.SetActive(true);
        }

        // 전체 대전 수 - 현재 남아있는 상대 수 = 이긴 전적 수
        winCount = 9 - gameManager.gameTournamentList.Count;

        if (winCount > 0)
        {
            for(int i = 0; i < winCount; i++)
            {
                X_Images[i].gameObject.SetActive(true);
            }

            // 마지막 X_Images 애니메이션 적용
            StartCoroutine(AnimateLastXImage(winCount - 1));
        }

        StartCoroutine(MoveCharacters());
    }

    IEnumerator AnimateLastXImage(int index)
    {
        if (index < 0 || index >= X_Images.Length) yield break;

        Image xImage = X_Images[index];
        RectTransform xRect = xImage.rectTransform;

        // 초기 상태 설정
        xImage.gameObject.SetActive(true);
        xRect.localScale = new Vector3(3, 3, 1);
        xRect.offsetMin = new Vector2(xRect.offsetMin.x, 270);
        xRect.offsetMax = new Vector2(xRect.offsetMax.x, 270);

        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 startScale = new Vector3(3, 3, 1);
        Vector3 targetScale = Vector3.one;
        Vector2 startMin = new Vector2(xRect.offsetMin.x, 270);
        Vector2 targetMin = new Vector2(xRect.offsetMin.x, 0);
        Vector2 startMax = new Vector2(xRect.offsetMax.x, 270);
        Vector2 targetMax = new Vector2(xRect.offsetMax.x, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            xRect.localScale = Vector3.Lerp(startScale, targetScale, t);
            xRect.offsetMin = Vector2.Lerp(startMin, targetMin, t);
            xRect.offsetMax = Vector2.Lerp(startMax, targetMax, t);

            yield return null;
        }

        // 최종 값 보정
        xRect.localScale = targetScale;
        xRect.offsetMin = targetMin;
        xRect.offsetMax = targetMax;
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

        leftCharacterName.text = "";
        rightCharacterName.text = "";

        for (int i = 0; i < 9; i++)
        {
            tournamentColor.a = 0f;
            tournamentImages[i].color = tournamentColor;
            frameImages[i].gameObject.SetActive(false);
        }
    }
}
