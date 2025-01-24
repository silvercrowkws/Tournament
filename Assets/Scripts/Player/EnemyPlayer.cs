using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayer : MonoBehaviour
{
    /// <summary>
    /// 적의 행동이 끝났음을 알리는 bool변수(true : 공격이 끝남, false : 공격 중)
    /// </summary>
    public bool enemyActiveEnd = false;

    // 체력 & 에너지 관련 시작 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// 체력이 변경되었음을 알리는 델리게이트(UI 수정용)
    /// </summary>
    public Action<int> EhpChange;

    /// <summary>
    /// 에너지가 변경되었음을 알리는 델리게이트(UI 수정용)
    /// </summary>
    public Action<int> EenergyChange;

    /// <summary>
    /// 현재 가지고 있는 체력
    /// </summary>
    int currentHP = 100;

    /// <summary>
    /// 현재 가지고 있는 에너지
    /// </summary>
    int currentEnergy = 100;

    /// <summary>
    /// HP 프로퍼티
    /// </summary>
    public int HP
    {
        get => currentHP;
        set
        {
            if (currentHP != value)
            {
                //currentHP = value;
                currentHP = Mathf.Clamp(value, 0, 100);
                Debug.Log($"남은 체력 : {currentHP}");
                EhpChange?.Invoke(currentHP);
            }
        }
    }

    /// <summary>
    /// Energy 프로퍼티
    /// </summary>
    public int Energy
    {
        get => currentEnergy;
        set
        {
            Debug.Log($"Energy set 호출: {value}");
            if (currentEnergy != value)
            {
                //currentEnergy = value;
                currentEnergy = Mathf.Clamp(value, 0, 100);
                Debug.Log($"남은 에너지 : {currentEnergy}");
                EenergyChange?.Invoke(currentEnergy);
            }
        }
    }

    // 체력 & 에너지 관련 끝 ----------------------------------------------------------------------------------------------------


}
