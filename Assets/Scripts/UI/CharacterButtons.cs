using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtons : MonoBehaviour
{
    // 버튼을 누르면 selectedCharacter 를 플레이어에게 전달?

    /// <summary>
    /// UI에서 어떤 캐릭터를 선택했는지 시작버튼에게 알리는 델리게이트
    /// </summary>
    public Action<int> onPickCharacter;

    /// <summary>
    /// 캐릭터 버튼의 배열
    /// </summary>
    Button[] characterButtons;

    /// <summary>
    /// 캔버스 그룹
    /// </summary>
    CanvasGroup canvasGroup;

    /// <summary>
    /// 파이트컨트롤버튼
    /// </summary>
    FightControlButtons fightControlButtons;

    private void Awake()
    {
        // 자식의 개수
        int childCount = transform.childCount;

        // characterButtons 배열 초기화(자식의 개수 만큼)
        characterButtons = new Button[childCount];

        // 자식의 개수만큼 반복
        for (int i = 0; i < childCount; i++)
        {
            int index = i;
            Transform child = transform.GetChild(index);
            characterButtons[index] = child.GetComponent<Button>();
            
            characterButtons[index].onClick.AddListener(() => PickCharacter(index));
        }

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        fightControlButtons = FindAnyObjectByType<FightControlButtons>();
        fightControlButtons.onChangeFighter += OnInteractiveThis;
    }

    /// <summary>
    /// 캔버스 그룹으로 상호작용을 조절하는 함수
    /// </summary>
    private void OnInteractiveThis()
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
    }

    /// <summary>
    /// 버튼을 눌러서 캐릭터를 선택했을 때 실행될 함수
    /// </summary>
    /// <param name="index">어떤 캐릭터의 버튼인지 확인하기 위해</param>
    private void PickCharacter(int index)
    {
        onPickCharacter.Invoke(index);

        // 이 게임 오브젝트 비활성화 필요 => 상호작용 불가능으로 변경
        //gameObject.SetActive(false);

        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
    }
}
