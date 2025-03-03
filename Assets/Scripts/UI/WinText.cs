using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinText : MonoBehaviour
{
    /// <summary>
    /// 이겼을 때 텍스트
    /// </summary>
    TextMeshProUGUI winText;

    /// <summary>
    /// 졌을 때 텍스트
    /// </summary>
    TextMeshProUGUI loseText;

    /// <summary>
    /// 비겼을 때 텍스트
    /// </summary>
    TextMeshProUGUI tieText;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    /// <summary>
    /// 적 플레이어
    /// </summary>
    EnemyPlayer enemyPlayer;

    private void Awake()
    {
        winText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        loseText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        tieText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        tieText.gameObject.SetActive(false);
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        player.onPlayerResult += OnPlayerResult;

        enemyPlayer = GameManager.Instance.EnemyPlayer;
    }

    /// <summary>
    /// 플레이어의 승패 결과를 받아 텍스트를 변경하는 함수
    /// </summary>
    /// <param name="result">true : 플레이어가 이김, false : 플레이어가 짐</param>
    private void OnPlayerResult(bool result)
    {
        if (player.HP < 1 && enemyPlayer.HP < 1)
        {
            //비김
            tieText.gameObject.SetActive(true);
        }
        else
        {
            if (result)
            {
                // 플레이어가 이김
                winText.gameObject.SetActive(true);
            }
            else
            {
                // 플레이어가 짐
                loseText.gameObject.SetActive(true);
            }
        }

        StartCoroutine(Scene03());      // 씬전환
    }

    /// <summary>
    /// 승패 결과에 상관없이 씬 전환하는 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator Scene03()
    {
        yield return new WaitForSeconds(5);     // 5초 정도 기다리고

        SceneManager.LoadScene(3);
    }
}
