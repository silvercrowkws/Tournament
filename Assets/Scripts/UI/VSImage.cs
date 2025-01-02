using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;

/*public enum PlayerCharacter
{
    Character_Adel = 0,
    Character_Akstar,
    Character_Amelia,
    Character_Arngrim,
    Character_Barbariccia,
    Character_BlackMage,
    Character_Cloud,
    Character_Elle,
    Character_Jade,
    Character_Nalu,
}*/

public class VSImage : MonoBehaviour
{
    /// <summary>
    /// 캐릭터 선택 버튼
    /// </summary>
    CharacterButtons characterButtons;

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

    /// <summary>
    /// 대진표 번호(각 캐릭터의 번호)
    /// </summary>
    public List<int> tournamentList = new List<int>();

    // 각 캐릭터의 시작 위치
    Vector3 leftStartPos;
    Vector3 rightStartPos;

    /// <summary>
    /// 캐릭터를 다시 고를지, 게임 시작할지 결정하는 버튼
    /// </summary>
    FightControlButtons fightControlButtons;

    /// <summary>
    /// 캐릭터 선택시 잠깐 깜빡임에 사용할 이미지
    /// </summary>
    Image flashImage;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    private void Awake()
    {
        leftCharacter = transform.GetChild(0).GetComponent<Image>();
        rightCharacter = transform.GetChild(1).GetComponent<Image>();

        flashImage = transform.GetChild(3).GetComponent<Image>();
        flashImage.gameObject.SetActive(false);

        /*leftColor = leftCharacter.color;
        leftColor.a = 0f;                   // 알파값을 0으로 설정
        leftCharacter.color = leftColor;

        rightColor = rightCharacter.color;
        rightColor.a = 0f;                  // 알파값을 0으로 설정
        rightCharacter.color = rightColor;*/

        characterAlphaZero();

        Transform child = transform.GetChild(2);        // 2번째 자식 TournamentChart

        tournamentsCount = child.childCount;

        // tournaments, frameImages 배열 초기화
        tournamentImages = new Image[tournamentsCount];
        frameImages = new Image[tournamentsCount];

        for (int i = 0; i < tournamentsCount; i++)
        {
            tournamentImages[i] = child.GetChild(i).GetComponent<Image>();
            frameImages[i] = tournamentImages[i].GetComponentInChildren<Image>();

            // 알파값을 0으로 설정
            tournamentColor = tournamentImages[i].color;
            tournamentColor.a = 0f;
            tournamentImages[i].color = tournamentColor;
            frameImages[i].gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        characterButtons = FindAnyObjectByType<CharacterButtons>();
        characterButtons.onPickCharacter += OnPickCharacter;

        fightControlButtons = FindObjectOfType<FightControlButtons>();
        fightControlButtons.onChangeFighter += ClearListFC;
        fightControlButtons.onFight += SendListFC;

        // 비활성화 처리
        if (fightControlButtons != null)
        {
            fightControlButtons.gameObject.SetActive(false);  // 비활성화
        }
    }

    /// <summary>
    /// 캐릭터를 선택했을 때 vsImage 로 보여주는 함수
    /// </summary>
    /// <param name="index">내가 선택한 캐릭터의 인덱스</param>
    private void OnPickCharacter(int index)
    {
        leftCharacter.sprite = characters[index];

        /*// PlayerCharacter의 개수를 가져오기 위해 enum의 값을 사용
        int playerCharacterCount = Enum.GetValues(typeof(PlayerCharacter)).Length;

        int randomIndex = index;

        // 랜덤 인덱스를 index와 다르게 선택
        // 나중에는 제외하는 숫자를 리스트로 해서 해야할 듯?(이미 이긴 상대와는 다시 매칭 안되도록)
        while (randomIndex == index)
        {
            randomIndex = UnityEngine.Random.Range(0, playerCharacterCount);        // 0 ~ PlayerCharacter의 개수 - 1
        }

        rightCharacter.sprite = characters[randomIndex];*/

        leftColor.a = 1f;                   // 알파값을 1로 설정
        leftCharacter.color = leftColor;

        rightColor.a = 1f;                  // 알파값을 1로 설정
        rightCharacter.color = rightColor;

        ShuffleTournaments(index);
        StartCoroutine(MoveCharacters());
    }

    /// <summary>
    /// 대진표를 섞고 할당하는 함수
    /// </summary>
    /// <param name="excludeIndex">제외할 인덱스</param>
    private void ShuffleTournaments(int excludeIndex)
    {
        // tournaments 배열의 크기 (대진표에 있는 스프라이트 개수)
        int tournamentCount = tournaments.Length;

        // 제외할 인덱스를 제외한 리스트 생성
        //List<int> tournamentList = new List<int>();       // 전역 변수로 변경
        for (int i = 0; i < tournamentCount; i++)
        {
            if (i != excludeIndex)      // 제외 인덱스는 추가하지 않음
            {
                tournamentList.Add(i);
            }
        }

        // 리스트를 랜덤으로 섞기
        tournamentList = tournamentList.OrderBy(x => UnityEngine.Random.value).ToList();

        // 섞은 tournamentList에 해당하는 스프라이트를 tournamentImages 배열에 할당        
        for (int i = 0; i < tournamentList.Count; i++)
        {
            tournamentImages[i].sprite = tournaments[tournamentList[i]];
            tournamentColor.a = 1f;
            tournamentImages[i].color = tournamentColor;
            frameImages[i].gameObject.SetActive(true);
        }

        rightCharacter.sprite = characters[tournamentList[0]];      // 오른쪽 캐릭터 이미지 할당
        fightControlButtons.gameObject.SetActive(true);             // 버튼 활성화
    }

    /// <summary>
    /// 캐릭터 선택 이미지를 이동시키는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveCharacters()
    {
        float duration = 0.3f;        // 0.3초 동안
        float elapsedTime = 0f;

        leftStartPos = leftCharacter.rectTransform.localPosition;
        rightStartPos = rightCharacter.rectTransform.localPosition;

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

    /// <summary>
    /// fightControlButtons의 델리게이트를 받아 리스트를 초기화 하는 함수
    /// </summary>
    private void ClearListFC()
    {
        for(int i = 0; i < tournamentList.Count; i++)
        {
            tournamentColor.a = 0f;
            tournamentImages[i].color = tournamentColor;
            frameImages[i].gameObject.SetActive(false);
        }

        characterAlphaZero();                                           // 알파 0으로 변경
        leftCharacter.rectTransform.localPosition = leftStartPos;       // 왼쪽 캐릭터 시작위치로 이동
        rightCharacter.rectTransform.localPosition = rightStartPos;     // 오른족 캐릭터 시작위치로 이동

        tournamentList.Clear();
    }
    /// <summary>
    /// fightControlButtons의 델리게이트를 받아 리스트를 전달 하는 함수
    /// </summary>
    private void SendListFC()
    {
        gameManager.gameTournamentList.Clear();     // 기존에 있던 값을 초기화
        gameManager.gameTournamentList.AddRange(tournamentList);  // tournamentList의 내용을 gameTournamentList에 복사

        // gameTournamentList가 복사되었는지 확인 (디버깅 용)
        foreach (var item in gameManager.gameTournamentList)
        {
            Debug.Log("GameManager의 대진표: " + item);
        }
    }
}
