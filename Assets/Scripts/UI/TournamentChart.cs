using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentChart : MonoBehaviour
{
    /// <summary>
    /// vs 스프라이트 배열
    /// </summary>
    public Sprite[] characters;

    CharacterButtons characterButtons;

    private void Awake()
    {
        
    }

    private void Start()
    {
        characterButtons = FindAnyObjectByType<CharacterButtons>();
        characterButtons.onPickCharacter += OnChart;
    }

    private void OnChart(int index)
    {
        
    }
}
