using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCompleteBackGround : MonoBehaviour
{
    /// <summary>
    /// 뒷배경 이미지(자리)
    /// </summary>
    Image image;

    /// <summary>
    /// X 표시를 위한 스프라이트
    /// </summary>
    public Sprite xSptire;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    private void Awake()
    {
        gameManager = GameManager.Instance;

        image = GetComponent<Image>();
    }

    private void Start()
    {
        image.sprite = gameManager.capturedSprite;
    }
}
