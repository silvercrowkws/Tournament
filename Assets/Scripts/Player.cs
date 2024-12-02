using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCharacter
{
    Player_9S = 0,
    Sango,
}

public class Player : MonoBehaviour
{
    public GameObject Player_9S_Prefabs;

    /// <summary>
    /// 플레이어가 어떻게 선택되고 그것을 받을지가 관건이겠네
    /// UI에서 선택을 하고 그걸 게임매니저가 받고, 다시 플레이어에게 알리고?
    /// </summary>
    public PlayerCharacter selectedCharacter;       // 플레이어가 선택한 캐릭터 저장

    private void Awake()
    {
        // 플레이어가 이누야샤를 선택했으면
        if (selectedCharacter == PlayerCharacter.Player_9S)
        {
            // InuyashaPrefab을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Player_9S_Prefabs, transform.position, transform.rotation, transform);
        }
    }
}
