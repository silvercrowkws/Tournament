using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 게임 난이도를 알리는 델리게이트(true : 노말모드, false : 하드모드)
    /// </summary>
    public Action<bool> onNormalMode;

    private void Awake()
    {
        Transform child = transform.GetChild(1);        // 1번째 자식 Buttons

        normal = child.GetChild(0).GetComponent<Button>();
        hard = child.GetChild(1).GetComponent<Button>();

        normal.onClick.AddListener(NormalMode);
        hard.onClick.AddListener(HardMode);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    /// <summary>
    /// 노말 버튼 클릭 시 실행되는 함수
    /// </summary>
    private void NormalMode()
    {
        // 게임 매니저에 노말모드로 플레이 한다고 알리기(져도 다시 플레이)
        Debug.Log("노말 모드");
        onNormalMode?.Invoke(true);

        // 캐릭터 선택으로 씬 이동하기
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// 하드 버튼 클릭 시 실행되는 함수
    /// </summary>
    private void HardMode()
    {
        // 게임 매니저에 하드모드로 플레이 한다고 알리기(한번 지면 끝)
        Debug.Log("하드 모드");
        onNormalMode?.Invoke(false);

        // 캐릭터 선택으로 씬 이동하기
        SceneManager.LoadScene(1);
    }
}
