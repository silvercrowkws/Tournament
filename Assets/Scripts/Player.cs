using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCharacter
{
    Character_Elle = 0,
    Character_Akstar,
    Character_Adel,
    Character_Amelia,
    Character_Barbariccia,
    Character_Jade,
    Character_Arngrim,
    Character_BlackMage,
    Character_Cloud,
    Character_Nalu,
}

public class Player : MonoBehaviour
{
    public GameObject Character_Elle_Prefabs;
    public GameObject Character_Akstar_Prefabs;
    public GameObject Character_Adel_Prefabs;
    public GameObject Character_Amelia_Prefabs;
    public GameObject Character_Barbariccia_Prefabs;
    public GameObject Character_Jade_Prefabs;
    public GameObject Character_Arngrim_Prefabs;
    public GameObject Character_BlackMage_Prefabs;
    public GameObject Character_Cloud_Prefabs;
    public GameObject Character_Nalu_Prefabs;

    /// <summary>
    /// 플레이어가 어떻게 선택되고 그것을 받을지가 관건이겠네
    /// UI에서 선택을 하고 그걸 게임매니저가 받고, 다시 플레이어에게 알리고?
    /// </summary>
    public PlayerCharacter selectedCharacter;       // 플레이어가 선택한 캐릭터 저장

    private void Awake()
    {
        // 플레이어가 Character_Elle 를 선택했으면
        if (selectedCharacter == PlayerCharacter.Character_Elle)
        {
            // Character_Elle_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Elle_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Akstar 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Akstar)
        {
            // Character_Akstar_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Akstar_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Adel 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Adel)
        {
            // Character_Adel_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Adel_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Amelia 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Amelia)
        {
            // Character_Amelia_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Amelia_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Barbariccia 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Barbariccia)
        {
            // Character_Barbariccia_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Barbariccia_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Jade 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Jade)
        {
            // Character_Jade_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Jade_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Arngrim 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Arngrim)
        {
            // Character_Arngrim_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Arngrim_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_BlackMage 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_BlackMage)
        {
            // Character_BlackMage_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_BlackMage_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Cloud 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Cloud)
        {
            // Character_Cloud_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Cloud_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Nalu 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Nalu)
        {
            // Character_Nalu_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            Instantiate(Character_Nalu_Prefabs, transform.position, transform.rotation, transform);
        }
    }
}
