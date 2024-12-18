using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

public enum PlayerAttack
{
    None = 0,
    Attack,
    MagicAttack,
    LimitAttack,
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
    /// 플레이어가 어떤 공격 상태인지
    /// </summary>
    public PlayerAttack selectedAttack;


    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 보드
    /// </summary>
    Board board;

    /// <summary>
    /// 적 플레이어
    /// </summary>
    EnemyPlayer enemyPlayer;

    /// <summary>
    /// 이동할 타겟 오브젝트
    /// </summary>
    GameObject targetObgect;

    /// <summary>
    /// 공격할 대상 바닥?
    /// </summary>
    GameObject targetAttack;

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

    // 애니메이터 관련 시작 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// 애니메이터
    /// </summary>
    Animator animator;

    // 애니메이터 관련 끝 ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 현재 위치 정보를 전달하는 델리게이트
    /// </summary>
    public Action<int> currentSection;

    /// <summary>
    /// 현재 위치의 인덱스
    /// </summary>
    public int currentSectionIndex = 0;

    private void Awake()
    {
        GameObject newCharacter = null;     // 생성된 캐릭터를 저장할 변수

        // 플레이어가 Character_Elle 를 선택했으면
        if (selectedCharacter == PlayerCharacter.Character_Elle)
        {
            // Character_Elle_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Elle_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Akstar 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Akstar)
        {
            // Character_Akstar_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Akstar_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Adel 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Adel)
        {
            // Character_Adel_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Adel_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Amelia 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Amelia)
        {
            // Character_Amelia_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Amelia_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Barbariccia 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Barbariccia)
        {
            // Character_Barbariccia_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Barbariccia_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Jade 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Jade)
        {
            // Character_Jade_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Jade_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Arngrim 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Arngrim)
        {
            // Character_Arngrim_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Arngrim_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_BlackMage 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_BlackMage)
        {
            // Character_BlackMage_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_BlackMage_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Cloud 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Cloud)
        {
            // Character_Cloud_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Cloud_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Nalu 를 선택했으면
        else if (selectedCharacter == PlayerCharacter.Character_Nalu)
        {
            // Character_Nalu_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Nalu_Prefabs, transform.position, transform.rotation, transform);
        }

        if (newCharacter != null)
        {
            Vector3 flippedScale = newCharacter.transform.localScale;
            flippedScale.x *= -1;               // X축 반전
            newCharacter.transform.localScale = flippedScale;
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        enemyPlayer = gameManager.EnemyPlayer;

        board = FindAnyObjectByType<Board>();
        currentSectionIndex = 0;
        transform.position = board.player1_Position[currentSectionIndex].transform.position;       // 플레이어의 현재 위치

        // 현재 위치 정보를 델리게이트로 전달 (여기서는 player1_Position[0]의 인덱스를 전달)
        SendSectionDelegateFC(currentSectionIndex);

        animator = gameObject.GetComponentInChildren<Animator>();                                 // 자식에서 애니메이터를 찾음
    }

    private void Update()
    {
        if (selectedMove != PlayerMove.None)
        {
            Move();                                 // 방향에 맞춰 이동 처리
            selectedMove = PlayerMove.None;         // 이동 후 selectedMove를 None으로 설정하여 이동을 한 번만 처리
        }

        if(selectedAttack != PlayerAttack.None)
        {
            Attack(selectedCharacter, selectedAttack, currentSectionIndex);          // 누가 어디서 어떤 공격을 했는지에 따라 공격 처리
            selectedAttack = PlayerAttack.None;                 // 공격 후 selectedAttack을 None으로 설정하여 공격을 한 번만 처리
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
        // 목표 위치
        //Vector3 targetPosition = board.player1_Position[currentSectionIndex].transform.position;
        Vector3 targetPosition;

        switch (selectedMove)
        {
            case PlayerMove.Up:
                if(currentSectionIndex < 8)         // 현재 위치가 0 1 2 3 일 때(아래줄)  4 -> 8
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
                if(currentSectionIndex != 0 && currentSectionIndex != 4 && currentSectionIndex != 8)        // 현재 위치가 0, 4, 8 이 아닐 때(맨 왼쪽)
                {
                    currentSectionIndex--;          // 왼쪽으로 이동할 때는 -1 만큼
                }
                break;
            case PlayerMove.Right:
                if(currentSectionIndex != 3 && currentSectionIndex != 7 && currentSectionIndex != 11)        // 현재 위치가 3, 7, 11 이 아닐 때(맨 오른쪽)
                {
                    currentSectionIndex++;          // 오른쪽으로 이동할 때는 +1 만큼
                }
                break;

        }

        // 배열 범위 검사를 추가하여 IndexOutOfRangeException을 방지
        currentSectionIndex = Mathf.Clamp(currentSectionIndex, 0, board.player1_Position.Length - 1);

        // 이동 후 플레이어의 위치 업데이트 -> 순간이동이 아니라 서서히 이동하도록 수정        
        //transform.position = board.player1_Position[currentSectionIndex].transform.position;

        // 타겟 오브젝트
        targetObgect = board.player1_Position[currentSectionIndex];

        // 새로운 위치를 계산
        targetPosition = targetObgect.transform.position;

        // 이동하는 동안 목표 위치로 서서히 이동
        StartCoroutine(MoveToPosition(targetPosition));

        // 새로운 위치 정보를 델리게이트로 전송
        SendSectionDelegateFC(currentSectionIndex);

        // 움직이는 모션 자리 여기 switch문 끝났을 때 한번만 넣으면 될듯?
        animator.SetTrigger("Move");
    }

    /// <summary>
    /// 포물선을 그리면서 플레이어를 이동시키는 코루틴
    /// </summary>
    /// <param name="targetPosition">이동할 위치</param>
    /// <returns></returns>
    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float timeToMove = 0.5f;          // 총 이동 시간
        float maxHeight = 2f;             // 포물선의 최대 높이 (이동 경로의 정점 높이)

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;


        //Transform parent = board.player1_Position[currentSectionIndex].transform.parent;

        // targetObject의 부모 색상을 초록색으로 변경
        Transform parent = targetObgect.transform.parent;
        Renderer parentRenderer = null;

        if (parent != null)
        {
            parentRenderer = parent.GetComponent<Renderer>();
            if (parentRenderer != null)
            {
                parentRenderer.material.color = Color.green;        // 부모의 색상을 초록색으로 변경
            }
        }


        while (elapsedTime < timeToMove)
        {
            // 수평 이동 비율 (0 ~ 1)
            float t = elapsedTime / timeToMove;

            // 수평 위치를 보간
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, t);

            // 수직 위치 계산: 포물선 높이
            float height = Mathf.Sin(t * Mathf.PI) * maxHeight;

            // 최종 위치 = 수평 위치 + 수직 위치
            transform.position = new Vector3(horizontalPosition.x, horizontalPosition.y + height, horizontalPosition.z);

            // 시간 증가
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 위치를 정확히 설정
        transform.position = targetPosition;


        // 부모 색상을 다시 흰색으로 변경
        if (parentRenderer != null)
        {
            parentRenderer.material.color = Color.white;        // 부모의 색상을 흰색으로 변경
        }


        ResetTrigger();                 // 트리거 리셋
        animator.SetTrigger("Idle");    // 대기 상태로 전환
    }

    /// <summary>
    /// 공격 함수(어떤 캐릭터가 어떤 공격을 했다)
    /// </summary>
    /// <param name="selectedCharacter">어떤 캐릭터가 공격했는지(공격 범위, 공격력이 캐릭터마다 다름)</param>
    /// <param name="selectedAttack">어떤 공격을 했는지</param>
    /// <param name="where">어디서 공격을 했는지(currentSectionIndex 기준)</param>
    public void Attack(PlayerCharacter selectedCharacter, PlayerAttack selectedAttack, int where)
    {
        // 내 위치 currentSectionIndex 를 기준으로 +1, -1 이면 ㅡ 모양으로 범위가 결정되는 거고
        int[] attackRange = null;          // 기본 공격 범위
        int[] magicAttackRange = null;     // 마법 공격 범위
        int[] limitAttackRange = null;     // 리미트 공격 범위

        int attackCost = 0;         // 기본 공격 비용
        int magicAttackCost = 0;    // 마법 공격 비용
        int limitAttackCost = 0;    // 리미트 공격 비용

        int attackDamage = 0;       // 기본 공격 데미지
        int magicAttackDamage = 0;  // 마법 공격 데미지
        int limitAttackDamage = 0;  // 리미트 공격 데미지


        switch (selectedCharacter)
        {
            case PlayerCharacter.Character_Elle:

                // 코스트
                attackCost = 25;
                magicAttackCost = 35;
                limitAttackCost = 50;

                // 공격력
                attackDamage = 35;
                magicAttackDamage = 25;
                limitAttackDamage = 50;

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex + 1 };                                   // 0 1
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex + 5 };                              // 0 5
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex + 1, currentSectionIndex + 5        // 0 1 5
                            };
                        break;
                    case 1:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1, currentSectionIndex + 1 };          // 1 0 2
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex + 3, currentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 1, currentSectionIndex + 3,       // 1 0 4 2 6
                            currentSectionIndex + 1, currentSectionIndex + 5 };
                        break;
                    case 2:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1, currentSectionIndex + 1 };          // 2 1 3
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex + 3, currentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex -1, currentSectionIndex + 3,        // 2 1 5 3 7
                            currentSectionIndex + 1, currentSectionIndex + 5 };
                        break;
                    case 3:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1};                                    // 3 2
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex + 3};                               // 1 4 6
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 1, currentSectionIndex + 3        // 3 2 6
                             };
                        break;
                    case 4:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex + 1 };                                    // 4 5
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex + 5, currentSectionIndex - 3 };      // 4 1 9
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex + 1, currentSectionIndex - 3,        // 4 1 5 9
                            currentSectionIndex + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1, currentSectionIndex + 1 };           // 5 4 6
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex + 3,        // 5 0 8 2 10
                            currentSectionIndex + 5, currentSectionIndex - 3 };
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex - 1,        // 5 0 4 8 2 6 10
                            currentSectionIndex + 3, currentSectionIndex - 3, currentSectionIndex + 1, currentSectionIndex + 5 };
                        break;
                    case 6:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1, currentSectionIndex + 1 };           // 6 5 7
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex + 3,        // 6 1 9 3 11
                            currentSectionIndex + 5, currentSectionIndex - 3 };
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex - 1,        // 6 1 5 9 3 7 11
                            currentSectionIndex + 3, currentSectionIndex - 3, currentSectionIndex + 1, currentSectionIndex + 5 };
                        break;
                    case 7:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1 };                                    // 7 6
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex - 1,        // 7 2 6 10
                            currentSectionIndex + 3 };
                        break;
                    case 8:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex + 1 };                                    // 8 9
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 3, currentSectionIndex + 1,        // 8 5 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1,  currentSectionIndex + 1 };          // 9 8 10
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex - 1,        // 9 4 8 6 10
                            currentSectionIndex - 3, currentSectionIndex + 1 };
                        break;
                    case 10:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1, currentSectionIndex + 1 };           // 10 9 11
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex - 1,        // 10 5 9 7 11
                            currentSectionIndex - 3, currentSectionIndex + 1 };
                        break;
                    case 11:
                        attackRange = new int[] { currentSectionIndex, currentSectionIndex - 1 };                                    // 11 10
                        magicAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { currentSectionIndex, currentSectionIndex - 5, currentSectionIndex - 1         // 11 6 10
                             };
                        break;

                }

                // MagicAttack 의 공격 범위 & 코스트 & 공격력 지정
                // LimitAttack 의 공격 범위 & 코스트 & 공격력 지정
                break;
            case PlayerCharacter.Character_Akstar:
                break;
            case PlayerCharacter.Character_Adel:
                break;
            case PlayerCharacter.Character_Amelia:
                break;
            case PlayerCharacter.Character_Barbariccia:
                break;
            case PlayerCharacter.Character_Jade:
                break;
            case PlayerCharacter.Character_Arngrim:
                break;
            case PlayerCharacter.Character_BlackMage:
                break;
            case PlayerCharacter.Character_Cloud:
                break;
            case PlayerCharacter.Character_Nalu:
                break;

        }

        switch(selectedAttack)
        {
            case PlayerAttack.Attack:

                // 공격 범위에 따라 공격하기
                // 공격 범위 내에 상대 플레이어가 있으면 HP 감소 시키기
                // 애니메이터 변경
                
                ResetTrigger();
                animator.SetTrigger("Attack");
                
                for(int i = 0; i< attackRange.Length; i++)
                {

                    // 만약 적 플레이어가 공격 범위에 있으면 데미지
                    if (enemyPlayer.transform.position == board.player2_Position[attackRange[i]].transform.transform.position)
                    {
                        // 데미지
                    }
                    targetAttack = board.player2_Position[attackRange[i]];
                    StartCoroutine(AttackRed());
                }
                break;

            case PlayerAttack.MagicAttack:

                ResetTrigger();
                animator.SetTrigger("MagicAttack");

                for (int i = 0; i < magicAttackRange.Length; i++)
                {

                    // 만약 적 플레이어가 공격 범위에 있으면 데미지
                    if (enemyPlayer.transform.position == board.player2_Position[magicAttackRange[i]].transform.transform.position)
                    {
                        // 데미지
                    }
                    targetAttack = board.player2_Position[magicAttackRange[i]];
                    StartCoroutine(AttackRed());
                }
                break;

            case PlayerAttack.LimitAttack:

                ResetTrigger();
                animator.SetTrigger("LimitAttack");

                for (int i = 0; i < limitAttackRange.Length; i++)
                {

                    // 만약 적 플레이어가 공격 범위에 있으면 데미지
                    if (enemyPlayer.transform.position == board.player2_Position[limitAttackRange[i]].transform.transform.position)
                    {
                        // 데미지
                    }
                    targetAttack = board.player2_Position[limitAttackRange[i]];
                    StartCoroutine(AttackRed());
                }
                break;
        }

        // 공격 범위에 해당하는 보드가 빨간색으로 바뀌어야 함
    }

    IEnumerator AttackRed()
    {
        float elapsedTime = 0;
        float duration = 1;

        Transform parent = targetAttack.transform.parent;
        Renderer parentRenderer = null;

        if (parent != null)
        {
            parentRenderer = parent.GetComponent<Renderer>();
            if (parentRenderer != null)
            {
                parentRenderer.material.color = Color.red;        // 부모의 색상을 빨간색으로 변경
            }
        }

        while (elapsedTime < duration)
        {
            // 시간 증가
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 부모 색상을 다시 흰색으로 변경
        if (parentRenderer != null)
        {
            parentRenderer.material.color = Color.white;        // 부모의 색상을 흰색으로 변경
        }
    }

    /// <summary>
    /// 트리거를 리셋하는 함수
    /// </summary>
    private void ResetTrigger()
    {
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Die");
        animator.ResetTrigger("Move");
    }
}
