using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

public enum PlayerMove
{
    None = 0,
    Up,
    Down,
    Left,
    Right,
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

    /// <summary>
    /// 플레이어가 어떻게 움직이는 상태인지
    /// </summary>
    public PlayerMove selectedMove;

    /// <summary>
    /// 보드
    /// </summary>
    Board board;

    // 체력 관련 시작 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// 체력이 변경되었음을 알리는 델리게이트(UI 수정용)
    /// </summary>
    public Action<int> hpChange;

    /// <summary>
    /// 현재 가지고 있는 체력
    /// </summary>
    int currentHP = 100;

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
                currentHP = Mathf.Clamp(value, 0, 999);
                Debug.Log($"남은 체력 : {currentHP}");
                hpChange?.Invoke(currentHP);
            }
        }
    }

    // 체력 관련 끝 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// 현재 위치 정보를 전달하는 델리게이트
    /// </summary>
    public Action<int> currentSection;

    /// <summary>
    /// 현재 위치의 인덱스
    /// </summary>
    int currentSectionIndex = 0;

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

    private void Start()
    {
        board = FindAnyObjectByType<Board>();
        currentSectionIndex = 0;
        transform.position = board.player1_Section[currentSectionIndex].transform.position;       // 플레이어의 현재 위치

        // 현재 위치 정보를 델리게이트로 전달 (여기서는 player1_Section[0]의 인덱스를 전달)
        SendSectionDelegateFC(currentSectionIndex);
    }

    private void Update()
    {
        if (selectedMove != PlayerMove.None)
        {
            Move();  // 방향에 맞춰 이동 처리
            selectedMove = PlayerMove.None;  // 이동 후 selectedMove를 None으로 설정하여 이동을 한 번만 처리
        }
    }

    /// <summary>
    /// 현재 위치 정보를 델리게이트로 전달하는 함수
    /// </summary>
    /// <param name="section">현재 위치의 인덱스</param>
    void SendSectionDelegateFC(int section)
    {
        currentSection?.Invoke(section);
    }

    /// <summary>
    /// 위아래 좌우로 움직이는 함수
    /// 나중에 한턴에 행동 하나씩 할 때 이 함수 실행되는 위치를 조절해야 함
    /// </summary>
    void Move()
    {
        switch (selectedMove)
        {
            case PlayerMove.Up:
                if(currentSectionIndex < 4)         // 현재 위치가 0 1 2 3 일 때(아래줄)
                {
                    currentSectionIndex += 4;       // 위로 이동할 때는 +4 만큼 델리게이트로 전송
                }
                break;
            case PlayerMove.Down:
                if(currentSectionIndex > 3)         // 현재 위치가 4 5 6 7 일 때(윗줄)
                {
                    currentSectionIndex -= 4;       // 아래로 이동할 때는 -4 만큼
                }
                break;
            case PlayerMove.Left:
                if(currentSectionIndex != 0 && currentSectionIndex != 4)        // 현재 위치가 0 또는 4 가 아닐 때(맨 왼쪽)
                {
                    currentSectionIndex--;          // 왼쪽으로 이동할 때는 -1 만큼
                }
                break;
            case PlayerMove.Right:
                if(currentSectionIndex != 3 && currentSectionIndex != 7)        // 현재 위치가 3 또는 7 가 아닐 때(맨 오른쪽)
                {
                    currentSectionIndex++;          // 오른쪽으로 이동할 때는 +1 만큼
                }
                break;

        }

        // 배열 범위 검사를 추가하여 IndexOutOfRangeException을 방지
        currentSectionIndex = Mathf.Clamp(currentSectionIndex, 0, board.player1_Section.Length - 1);

        // 이동 후 플레이어의 위치 업데이트
        transform.position = board.player1_Section[currentSectionIndex].transform.position;

        // 새로운 위치 정보를 델리게이트로 전송
        SendSectionDelegateFC(currentSectionIndex);

        // 움직이는 모션 자리 여기 switch문 끝났을 때 한번만 넣으면 될듯?

    }
}
