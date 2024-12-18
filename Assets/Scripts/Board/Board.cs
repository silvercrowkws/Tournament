using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    GameObject[] line_0_Section;
    GameObject[] line_1_Section;
    GameObject[] line_2_Section;

    /// <summary>
    /// 플레이어1 이 움직일 수 있는 위치(0 ~ 11까지 12개 있음)
    /// </summary>
    public GameObject[] player1_Position;

    /// <summary>
    /// 플레이어2 이 움직일 수 있는 위치(0 ~ 11까지 12개 있음)
    /// </summary>
    public GameObject[] player2_Position;
    
    private void Awake()
    {
        Transform line_0 = transform.GetChild(0);
        Transform line_1 = transform.GetChild(1);
        Transform line_2 = transform.GetChild(2);

        line_0_Section = new GameObject[line_0.childCount];
        line_1_Section = new GameObject[line_1.childCount];
        line_2_Section = new GameObject[line_2.childCount];
        player1_Position = new GameObject[line_0.childCount + line_1.childCount + line_2.childCount];
        player2_Position = new GameObject[line_0.childCount + line_1.childCount + line_2.childCount];

        // 라인0 에서 Section 찾기
        for (int i = 0; i < line_0.childCount; i++)
        {
            line_0_Section[i] = line_0.GetChild(i).gameObject;
        }

        // 라인1 에서 Section 찾기
        for (int i = 0; i < line_1.childCount; i++)
        {
            line_1_Section[i] = line_1.GetChild(i).gameObject;
        }

        // 라인2 에서 Section 찾기
        for (int i = 0; i < line_2.childCount; i++)
        {
            line_2_Section[i] = line_2.GetChild(i).gameObject;
        }

        // 라인0 에서 찾은 Section들의 자식에서 player Position 찾기
        for (int i = 0; i< line_0.childCount; i++)
        {
            player1_Position[i] = line_0.GetChild(i).GetChild(0).gameObject;
            player2_Position[i] = line_0.GetChild(i).GetChild(1).gameObject;
        }

        // 라인1 에서 찾은 Section들의 자식에서 player Position 찾기
        for (int i = 0; i< line_1.childCount; i++)
        {
            player1_Position[i + 4] = line_1.GetChild(i).GetChild(0).gameObject;
            player2_Position[i + 4] = line_1.GetChild(i).GetChild(1).gameObject;
        }

        // 라인2 에서 찾은 Section들의 자식에서 player Position 찾기
        for (int i = 0; i < line_2.childCount; i++)
        {
            player1_Position[i + 8] = line_2.GetChild(i).GetChild(0).gameObject;
            player2_Position[i + 8] = line_2.GetChild(i).GetChild(1).gameObject;
        }
    }
}
