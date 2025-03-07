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
    /// 캐릭터 변경 버튼 클릭으로 실행되는 함수(씬 전환) => 게임 종료로 변경
    /// </summary>
    private void ChangeFigherFC()
    {
        SceneManager.LoadScene(1);
        //Application.Quit();
    }

    /// <summary>
    /// 전투 시작 버튼 클릭으로 실행되는 함수
    /// </summary>
    private void FightFC()
    {
        TurnManager turnManager = TurnManager.Instance;        
        turnManager.OnInitialize2();
        SceneManager.LoadScene(2);
    }

    /*void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 2)      // 2번 씬에서
        {
            NextZone nextZone = GameObject.FindObjectOfType<NextZone>();
            if (nextZone != null)
            {
                // nextZone을 새로 할당하거나 재설정
                Debug.Log("NextZone 오브젝트가 씬에 있습니다.");
            }
            else
            {
                Debug.LogWarning("NextZone 오브젝트가 씬에 없습니다.");
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }*/
}
