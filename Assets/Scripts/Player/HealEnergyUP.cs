using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealEnergyUP : MonoBehaviour
{
    /// <summary>
    /// 회복과 에너지 업 애니메이션
    /// </summary>
    Animator heAnimator;

    private void Awake()
    {
        heAnimator = GetComponent<Animator>();
        this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        heAnimator.Play("Heal");
    }

    /// <summary>
    /// 애니메이션이 끝났을 때 실행되는 함수(애니메이션 이벤트로 실행)
    /// </summary>
    private void AnimationEnd()
    {
        this.gameObject.SetActive(false);
    }
}
