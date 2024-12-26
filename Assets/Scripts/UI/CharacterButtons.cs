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
    }

    /// <summary>
    /// 버튼을 눌러서 캐릭터를 선택했을 때 실행될 함수
    /// </summary>
    /// <param name="index">어떤 캐릭터의 버튼인지 확인하기 위해</param>
    private void PickCharacter(int index)
    {
        onPickCharacter.Invoke(index);
    }
}
