using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : MonoBehaviour
{
    /// <summary>
    /// 스프라이트 배열
    /// </summary>
    public Sprite[] characters;

    // 1. 이 씬이 불러졌을 때 플레이어와 적 플레이어의 이미지로 변경
    // 2. 턴이 진행될 때마다 RoundNumberText 숫자 늘리기
    // 3. 플레이어의 체력과 에너지를 보여줘야 함


    // 1. 전투로 넘어갈 때 Fram 게임 오브젝트를 컨트롤 해야 함(Fram 클래스를 따로 만들어서 하는게 나을 듯)
    // 2. Fram 클래스에는 선택 가능한 카드들이 포함되어야 함
}
