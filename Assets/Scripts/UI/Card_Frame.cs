using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_Frame : MonoBehaviour
{
    /// <summary>
    /// 컨트롤 존 클래스
    /// </summary>
    ControlZone controlZone;

    /// <summary>
    /// 캔버스 그룹
    /// </summary>
    CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
    }

    private void Start()
    {
        controlZone = FindAnyObjectByType<ControlZone>();
        controlZone.onContinue += OnContinue;
    }

    /// <summary>
    /// 컨티뉴 버튼 클릭으로 알파값을 조절하고 상호작용을 끄는 함수
    /// </summary>
    private void OnContinue()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
    }
}
