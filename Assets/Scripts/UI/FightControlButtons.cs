using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FightControlButtons : MonoBehaviour
{
    /// <summary>
    /// 캐릭터 변경 버튼
    /// </summary>
    Button changeFighter;

    /// <summary>
    /// 전투 시작 버튼
    /// </summary>
    Button fight;

    /// <summary>
    /// 캐릭터 선택 버튼(클래스)
    /// </summary>
    CharacterButtons characterButtons;

    /// <summary>
    /// 캐릭터 변경 버튼을 클릭했다고 알리는 델리게이트
    /// </summary>
    public Action onChangeFighter;

    public Action onFight;

    /// <summary>
    /// 캔버스 그룹
    /// </summary>
    CanvasGroup canvasGroup;

    /// <summary>
    /// 버서스 이미지 클래스
    /// </summary>
    VSImage vsImage;

    private void Awake()
    {
        Transform child = transform.GetChild(1);                        // 이 오브젝트의 1번째 자식 Buttons

        changeFighter = child.GetChild(0).GetComponent<Button>();       // Buttons의 0번째 자식 ChangeFighter
        changeFighter.onClick.AddListener(ChangeFigherFC);

        fight = child.GetChild(1).GetComponent<Button>();               // Buttons의 1번째 자식 Fight
        fight.onClick.AddListener(FightFC);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        characterButtons = FindAnyObjectByType<CharacterButtons>();
        vsImage = FindAnyObjectByType<VSImage>();
        vsImage.onInteract += OnInteractFalse;
    }

    /// <summary>
    /// 캐릭터 변경 버튼 클릭으로 실행되는 함수
    /// </summary>
    private void ChangeFigherFC()
    {
        // 1. CharacterButtons 활성화
        // 2. VSImage의 tournamentImages, frameImages 안보이게 변경(tournamentImages : 알파값 0으로, frameImages 비활성화)
        // 3. VSImage의 leftCharacter, rightCharacter 알파값 0으로 변경
        // 4. VSImage의 tournamentList 초기화
        // 5. 이 게임 오브젝트 비활성화

        //characterButtons.gameObject.SetActive(true);        // 1
        onChangeFighter?.Invoke();                          // 1, 2, 3, 4
        //this.gameObject.SetActive(false);                   // 5 => 캔버스 그룹 조절하는 것으로 변경
        OnInteractFalse(false);
    }

    private void OnInteractFalse(bool tf)
    {
        if (tf)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// 전투 시작 버튼 클릭으로 실행되는 함수
    /// </summary>
    private void FightFC()
    {
        // 1. 배틀 순서를 게임 매니저에 전달
        // 2. 씬 전환(카드 선택으로 작동하는 씬?)

        onFight?.Invoke();                                  // 1
        SceneManager.LoadScene(2);                          // 2
    }
}
