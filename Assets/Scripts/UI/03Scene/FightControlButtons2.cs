using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightControlButtons2 : MonoBehaviour
{
    /// <summary>
    /// 캐릭터 변경 버튼
    /// </summary>
    Button changeFighterButton;

    /// <summary>
    /// 전투 시작 버튼
    /// </summary>
    Button fightButton;

    private void Awake()
    {
        Transform child = transform.GetChild(1);

        changeFighterButton = child.GetChild(0).GetComponent<Button>();
        changeFighterButton.onClick.AddListener(ChangeFigherFC);

        fightButton = child.GetChild(1).GetComponent<Button>();
        fightButton.onClick.AddListener(FightFC);
    }

    /// <summary>
    /// 캐릭터 변경 버튼 클릭으로 실행되는 함수(씬 전환)
    /// </summary>
    private void ChangeFigherFC()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// 전투 시작 버튼 클릭으로 실행되는 함수
    /// </summary>
    private void FightFC()
    {
        SceneManager.LoadScene(2);
    }
}
