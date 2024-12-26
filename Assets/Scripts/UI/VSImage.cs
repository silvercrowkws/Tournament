using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum PlayerCharacter
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
}

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
    /// 왼쪽 캐릭터의 color
    /// </summary>
    Color leftColor;

    /// <summary>
    /// 오른쪽 캐릭터의 color
    /// </summary>
    Color rightColor;

    /// <summary>
    /// 대결 순서 스프라이트 배열
    /// </summary>
    public Sprite[] tournaments;

    Image[] images;

    /// <summary>
    /// 상대의 수
    /// </summary>
    int tournamentsCount;

    private void Awake()
    {
        leftCharacter = transform.GetChild(0).GetComponent<Image>();
        rightCharacter = transform.GetChild(1).GetComponent<Image>();

        leftColor = leftCharacter.color;
        leftColor.a = 0f;                   // 알파값을 0으로 설정
        leftCharacter.color = leftColor;

        rightColor = rightCharacter.color;
        rightColor.a = 0f;                  // 알파값을 0으로 설정
        rightCharacter.color = rightColor;

        Transform child = transform.GetChild(2);        // 2번째 자식 TournamentChart

        tournamentsCount = child.childCount;

        // tournaments 배열 초기화
        images = new Image[tournamentsCount];

        for (int i = 0; i < tournamentsCount; i++)
        {
            images[i] = child.GetChild(i).GetComponent<Image>();
        }
    }

    private void Start()
    {
        characterButtons = FindAnyObjectByType<CharacterButtons>();
        characterButtons.onPickCharacter += OnPickCharacter;
    }

    /// <summary>
    /// 캐릭터를 선택했을 때 vsImage 로 보여주는 함수
    /// </summary>
    /// <param name="index">내가 선택한 캐릭터의 인덱스</param>
    private void OnPickCharacter(int index)
    {
        leftCharacter.sprite = characters[index];

        // PlayerCharacter의 개수를 가져오기 위해 enum의 값을 사용
        int playerCharacterCount = Enum.GetValues(typeof(PlayerCharacter)).Length;

        int randomIndex = index;

        // 랜덤 인덱스를 index와 다르게 선택
        // 나중에는 제외하는 숫자를 리스트로 해서 해야할 듯?(이미 이긴 상대와는 다시 매칭 안되도록)
        while (randomIndex == index)
        {
            randomIndex = UnityEngine.Random.Range(0, playerCharacterCount);        // 0 ~ PlayerCharacter의 개수 - 1
        }

        rightCharacter.sprite = characters[randomIndex];

        leftColor.a = 1f;                   // 알파값을 1로 설정
        leftCharacter.color = leftColor;

        rightColor.a = 1f;                  // 알파값을 1로 설정
        rightCharacter.color = rightColor;

        ShuffleTournaments(index);
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
        List<int> tournamentList = new List<int>();
        for (int i = 0; i < tournamentCount; i++)
        {
            if (i != excludeIndex)      // 제외 인덱스는 추가하지 않음
            {
                tournamentList.Add(i);
            }
        }

        // 리스트를 랜덤으로 섞기
        tournamentList = tournamentList.OrderBy(x => UnityEngine.Random.value).ToList();

        // 섞은 tournamentList에 해당하는 스프라이트를 images 배열에 할당        
        for (int i = 0; i < tournamentList.Count; i++)
        {
            images[i].sprite = tournaments[tournamentList[i]];
        }
    }
}
