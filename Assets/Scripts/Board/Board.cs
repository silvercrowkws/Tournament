using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public GameObject[] line_0_Section;
    public GameObject[] line_1_Section;

    public GameObject[] player1_Section;
    public GameObject[] player2_Section;
    
    private void Awake()
    {
        Transform line_0 = transform.GetChild(0);
        Transform line_1 = transform.GetChild(1);

        line_0_Section = new GameObject[line_0.childCount];
        line_1_Section = new GameObject[line_1.childCount];
        player1_Section = new GameObject[line_0.childCount + line_1.childCount];
        player2_Section = new GameObject[line_0.childCount + line_1.childCount];

        for (int i = 0; i < line_0.childCount; i++)
        {
            line_0_Section[i] = line_0.GetChild(i).gameObject;
        }


        for (int i = 0; i < line_1.childCount; i++)
        {
            line_1_Section[i] = line_1.GetChild(i).gameObject;
        }

        for(int i = 0; i< line_0.childCount; i++)
        {
            player1_Section[i] = line_0.GetChild(i).GetChild(0).gameObject;
            player2_Section[i] = line_0.GetChild(i).GetChild(1).gameObject;
        }

        for(int i = 0; i< line_1.childCount; i++)
        {
            player1_Section[i+4] = line_1.GetChild(i).GetChild(0).gameObject;
            player2_Section[i+4] = line_1.GetChild(i).GetChild(1).gameObject;
        }
    }
}
