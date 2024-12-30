using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseDif : MonoBehaviour
{
    /// <summary>
    /// 노말 버튼
    /// </summary>
    Button normal;

    /// <summary>
    /// 하드 버튼
    /// </summary>
    Button hard;

    private void Awake()
    {
        Transform child = transform.GetChild(1);        // 1번째 자식 Buttons

        normal = child.GetChild(0).GetComponent<Button>();
        hard = child.GetChild(1).GetComponent<Button>();

        normal.onClick.AddListener(NormalMode);
        hard.onClick.AddListener(HardMode);
    }    

    /// <summary>
    /// 노말 버튼 클릭 시 실행되는 함수
    /// </summary>
    private void NormalMode()
    {
        // 게임 매니저에 노말모드로 플레이 한다고 알리기(져도 다시 플레이)
    }

    /// <summary>
    /// 하드 버튼 클릭 시 실행되는 함수
    /// </summary>
    private void HardMode()
    {
        // 게임 매니저에 하드모드로 플레이 한다고 알리기(한번 지면 끝)
    }
}
