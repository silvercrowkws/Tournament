using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPEN : MonoBehaviour
{
    // 플레이어와 적 플레이어의 HP, EN의 상태를 슬라이더로 표시하는 클래스
    // 턴 상황에 따라 가운데 Round Text의 숫자를 변경시켜야 함

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    /// <summary>
    /// 적 플레이어
    /// </summary>
    EnemyPlayer enemyPlayer;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 턴 매니저
    /// </summary>
    TurnManager turnManager;

    /// <summary>
    /// 텍스트 매쉬 프로
    /// </summary>
    TextMeshProUGUI roundText;

    /// <summary>
    /// 현재 턴 번호
    /// </summary>
    int currentTurnNumber = 0;

    /// <summary>
    /// 플레이어의 체력 슬라이더
    /// </summary>
    Slider playerHPSlider;

    /// <summary>
    /// 플레이어의 에너지 슬라이더
    /// </summary>
    Slider playerEnergySlider;

    /// <summary>
    /// 적의 체력 슬라이더
    /// </summary>
    Slider enemyHPSlider;

    /// <summary>
    /// 적의 에너지 슬라이더
    /// </summary>
    Slider enemyEnergySlider;

    // 최대 체력, 에너지
    int maxHp = 100;
    int maxEnergy = 100;

    private void Awake()
    {
        Transform child = transform.GetChild(3);        // 3번째 자식 CenterImage
        roundText = child.GetChild(1).GetComponent<TextMeshProUGUI>();

        child = transform.GetChild(1);                  // 1번째 자식 PlayerHE
        playerHPSlider = child.GetChild(0).GetComponent<Slider>();
        playerEnergySlider = child.GetChild(1).GetComponent<Slider>();
        playerHPSlider.value = 1;                       // 시작이 값을 1로 고정
        playerEnergySlider.value = 1;

        child = transform.GetChild(2);                  // 2번째 자식 EnermyPlayerHE
        enemyHPSlider = child.GetChild(0).GetComponent<Slider>();
        enemyEnergySlider = child.GetChild(1).GetComponent<Slider>();
        enemyHPSlider.value = 1;
        enemyEnergySlider.value = 1;

    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        player = gameManager.Player;
        player.hpChange += OnPlayerHPChange;
        player.energyChange += OnPlayerEnergyChange;

        enemyPlayer = gameManager.EnemyPlayer;
        enemyPlayer.EhpChange += OnEnemyHPChange;
        enemyPlayer.EenergyChange += OnEnemyEnergyChange;

        turnManager = TurnManager.Instance;
        turnManager.onTurnStart += OnRoundText;
        turnManager.onTurnEnd += OnEndTurnNumber;
    }

    /// <summary>
    /// 플레이어의 HP 변경으로 슬라이더를 조절하는 함수
    /// </summary>
    /// <param name="currentHP">현재 남은 체력</param>
    private void OnPlayerHPChange(int currentHP)
    {
        playerHPSlider.value = (float)currentHP /maxHp;
    }

    /// <summary>
    /// 플레이어의 에너지 변경으로 슬라이더를 조절하는 함수
    /// </summary>
    /// <param name="currentEnergy">현재 남은 에너지</param>
    private void OnPlayerEnergyChange(int currentEnergy)
    {
        playerEnergySlider.value = (float)currentEnergy / maxEnergy;
    }

    /// <summary>
    /// 적의 HP 변경으로 슬라이더를 조절하는 함수
    /// </summary>
    /// <param name="EcurrentHP">적의 현재 남은 체력</param>
    private void OnEnemyHPChange(int EcurrentHP)
    {
        enemyHPSlider.value = (float)EcurrentHP /maxHp;
    }

    /// <summary>
    /// 적의 에너지 변경으로 슬라이더를 조절하는 함수
    /// </summary>
    /// <param name="EcurrentEnergy">적의 현재 남은 에너지</param>
    private void OnEnemyEnergyChange(int EcurrentEnergy)
    {
        enemyEnergySlider.value = (float)EcurrentEnergy /maxEnergy;
    }


    /// <summary>
    /// 턴 숫자에 따라 라운드 텍스트를 변경하는 함수
    /// </summary>
    /// <param name="turnNumber">턴 숫자</param>
    private void OnRoundText(int turnNumber)
    {
        currentTurnNumber = turnNumber;
        if (turnNumber < 10)
        {
            roundText.text = $"0{turnNumber}";
        }
        else
        {
            roundText.text = turnNumber.ToString();
        }

        //roundText.text = turnNumber.ToString();
    }

    /// <summary>
    /// 턴이 종료되고 NextRound 버튼을 눌렀을 때도 상단의 라운드텍스트를 변경하기 위한 함수
    /// </summary>
    private void OnEndTurnNumber()
    {
        if(currentTurnNumber < 10)
        {
            roundText.text = $"0{currentTurnNumber + 1}";
        }
        else
        {
            roundText.text = currentTurnNumber + 1.ToString();
        }
    }
}
