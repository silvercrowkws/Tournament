using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyPlayerMove
{
    None = 0,
    Up,
    Down,
    Left,
    Right,
    DoubleRight,
    DoubleLeft,
}

public enum EnemyPlayerAttack
{
    None = 0,
    Attack,
    MagicAttack,
    LimitAttack,
}

public enum EnemyPlayerProtect
{
    None = 0,
    Guard,
    PerfectGuard,
    EnergyUp,
    Heal,
}

public class EnemyPlayer : MonoBehaviour
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
    /// 적 플레이어가 어떻게 선택되고 그것을 받을지가 관건이겠네
    /// UI에서 선택을 하고 그걸 게임매니저가 받고, 다시 플레이어에게 알리고?
    /// </summary>
    public EnemyPlayerCharacter EselectedCharacter;       // 적 플레이어가 선택한 캐릭터 저장

    /// <summary>
    /// 적 플레이어가 어떻게 움직이는 상태인지
    /// </summary>
    public EnemyPlayerMove EselectedMove;

    /// <summary>
    /// 적 플레이어가 어떤 공격 상태인지
    /// </summary>
    public EnemyPlayerAttack EselectedAttack;

    /// <summary>
    /// 적 플레이어가 어떤 수비 상태인지
    /// </summary>
    public EnemyPlayerProtect EselectedProtect;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 보드
    /// </summary>
    Board board;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    /// <summary>
    /// 이동할 타겟 오브젝트
    /// </summary>
    GameObject targetObgect;

    /// <summary>
    /// 공격할 대상 바닥?
    /// </summary>
    GameObject targetAttack;

    // 체력 & 에너지 관련 시작 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// 적의 체력이 변경되었음을 알리는 델리게이트(UI 수정용)
    /// </summary>
    public Action<int> EhpChange;

    /// <summary>
    /// 적의 에너지가 변경되었음을 알리는 델리게이트(UI 수정용)
    /// </summary>
    public Action<int> EenergyChange;

    /// <summary>
    /// 적이 현재 가지고 있는 체력
    /// </summary>
    int currentHP = 100;

    /// <summary>
    /// 적이 현재 가지고 있는 에너지
    /// </summary>
    int currentEnergy = 100;

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
                currentHP = Mathf.Clamp(value, 0, 100);
                Debug.Log($"남은 체력 : {currentHP}");
                EhpChange?.Invoke(currentHP);
            }
        }
    }

    /// <summary>
    /// Energy 프로퍼티
    /// </summary>
    public int Energy
    {
        get => currentEnergy;
        set
        {
            Debug.Log($"Energy set 호출: {value}");
            if (currentEnergy != value)
            {
                //currentEnergy = value;
                currentEnergy = Mathf.Clamp(value, 0, 100);
                Debug.Log($"남은 에너지 : {currentEnergy}");
                EenergyChange?.Invoke(currentEnergy);
            }
        }
    }

    // 체력 & 에너지 관련 끝 ----------------------------------------------------------------------------------------------------

    // 애니메이터 관련 시작 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// 애니메이터
    /// </summary>
    Animator animator;

    /// <summary>
    /// 적의 행동이 끝났는지 확인하는 bool변수(true : 공격이 끝남, false : 공격 중)
    /// </summary>
    public bool enemyActiveEnd = false;

    // 애니메이터 관련 끝 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// 현재 위치 정보를 전달하는 델리게이트
    /// </summary>
    public Action<int> currentSection;

    /// <summary>
    /// 적의 현재 위치의 인덱스
    /// </summary>
    public int EcurrentSectionIndex = 0;

    /// <summary>
    /// 현재 위치가 달라지면 델리게이트를 보내는 프로퍼티
    /// </summary>
    public int CurrentSectionIndex
    {
        get => EcurrentSectionIndex;
        set
        {
            if (EcurrentSectionIndex != value)
            {
                currentSection?.Invoke(EcurrentSectionIndex);
            }
        }
    }

    private void Awake()
    {
        
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
            Debug.LogError("GameManager instance not found!");

        player = gameManager.Player;

        /*vsImage = FindAnyObjectByType<VSImage>();
        if (vsImage == null)
            Debug.LogError("VSImage object not found!");*/

        board = FindAnyObjectByType<Board>();
        if (board == null)
            Debug.LogError("Board is null!");

        EcurrentSectionIndex = 3;
        transform.position = board.player2_Position[EcurrentSectionIndex].transform.position;       // 플레이어의 현재 위치


        // 현재 위치 정보를 델리게이트로 전달 (여기서는 player1_Position[0]의 인덱스를 전달)
        SendSectionDelegateFC(EcurrentSectionIndex);


        EselectedCharacter = (EnemyPlayerCharacter)gameManager.enemyPlayerCharacterIndex;

        OnSelectedCharacter(EselectedCharacter);

        animator = gameObject.GetComponentInChildren<Animator>();                                 // 자식에서 애니메이터를 찾음 => 캐릭터를 만든 후 애니메이터를 찾도록 위치 변경

        if (animator == null)
            Debug.LogError("Animator is null!");
    }

    private void Update()
    {
        if (EselectedMove != EnemyPlayerMove.None)
        {
            //playerActiveEnd = false;                // 행동 중이라고 표시 => 여기서 걸어봤자 이미 턴은 시작되어서 ActivePlayer에서는 늦지?
            Move();                                 // 방향에 맞춰 이동 처리
            EselectedMove = EnemyPlayerMove.None;         // 이동 후 selectedMove를 None으로 설정하여 이동을 한 번만 처리
            //playerActiveEnd = true;                 // 행동이 끝났다고 표시 => 이쪽은 그래도 행동이 끝났다는거니까 의미가 있으려나(Move 다음에 바로 실행되는거면 의미 없는데)
        }

        if (EselectedAttack != EnemyPlayerAttack.None)
        {
            //playerActiveEnd = false;                                                // 행동 중이라고 표시
            Attack(EselectedCharacter, EselectedAttack, EcurrentSectionIndex);         // 누가 어디서 어떤 공격을 했는지에 따라 공격 처리
            EselectedAttack = EnemyPlayerAttack.None;                                     // 공격 후 selectedAttack을 None으로 설정하여 공격을 한 번만 처리
            //playerActiveEnd = true;                                                 // 행동이 끝났다고 표시
        }

        if (EselectedProtect != EnemyPlayerProtect.None)
        {
            //playerActiveEnd = false;                // 행동 중이라고 표시
            Protect();
            EselectedProtect = EnemyPlayerProtect.None;
            //playerActiveEnd = true;                 // 행동이 끝났다고 표시
        }
    }

    /// <summary>
    /// 토너먼트 리스트의 0번으로 선택된 캐릭터를 생성하는 함수
    /// </summary>
    /// <param name="character"></param>
    private void OnSelectedCharacter(EnemyPlayerCharacter character)
    {
        GameObject newCharacter = null;     // 생성된 캐릭터를 저장할 변수

        // 플레이어가 Character_Elle 를 선택했으면
        if (EselectedCharacter == EnemyPlayerCharacter.Character_Elle)
        {
            // Character_Elle_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Elle_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Akstar 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_Akstar)
        {
            // Character_Akstar_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Akstar_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Adel 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_Adel)
        {
            // Character_Adel_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Adel_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Amelia 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_Amelia)
        {
            // Character_Amelia_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Amelia_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Barbariccia 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_Barbariccia)
        {
            // Character_Barbariccia_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Barbariccia_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Jade 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_Jade)
        {
            // Character_Jade_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Jade_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Arngrim 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_Arngrim)
        {
            // Character_Arngrim_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Arngrim_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_BlackMage 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_BlackMage)
        {
            // Character_BlackMage_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_BlackMage_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Cloud 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_Cloud)
        {
            // Character_Cloud_Prefabs 을 플레이어 오브젝트의 자식으로 생성
            newCharacter = Instantiate(Character_Cloud_Prefabs, transform.position, transform.rotation, transform);
        }

        // 플레이어가 Character_Nalu 를 선택했으면
        else if (EselectedCharacter == EnemyPlayerCharacter.Character_Nalu)
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
        //Vector3 targetPosition = board.player2_Position[EcurrentSectionIndex].transform.position;
        Vector3 targetPosition;

        switch (EselectedMove)
        {
            case EnemyPlayerMove.Up:
                if (EcurrentSectionIndex < 8)         // 현재 위치가 0 1 2 3 일 때(아래줄)  4 -> 8
                {
                    EcurrentSectionIndex += 4;       // 위로 이동할 때는 +4 만큼 델리게이트로 전송
                }
                break;
            case EnemyPlayerMove.Down:
                if (EcurrentSectionIndex > 3)         // 현재 위치가 4 5 6 7 일 때(윗줄)
                {
                    EcurrentSectionIndex -= 4;       // 아래로 이동할 때는 -4 만큼
                }
                break;
            case EnemyPlayerMove.Left:
                if (EcurrentSectionIndex != 0 && EcurrentSectionIndex != 4 && EcurrentSectionIndex != 8)        // 현재 위치가 0, 4, 8 이 아닐 때(맨 왼쪽)
                {
                    EcurrentSectionIndex--;          // 왼쪽으로 이동할 때는 -1 만큼
                }
                break;
            case EnemyPlayerMove.Right:
                if (EcurrentSectionIndex != 3 && EcurrentSectionIndex != 7 && EcurrentSectionIndex != 11)        // 현재 위치가 3, 7, 11 이 아닐 때(맨 오른쪽)
                {
                    EcurrentSectionIndex++;          // 오른쪽으로 이동할 때는 +1 만큼
                }
                break;
            case EnemyPlayerMove.DoubleRight:            // 현재 위치가 2, 6, 10이면 ++ / 2, 3, 6, 7, 10, 11이 아니면 +2
                if (EcurrentSectionIndex == 2 || EcurrentSectionIndex == 6 || EcurrentSectionIndex == 10)
                {
                    EcurrentSectionIndex++;
                }
                else if (EcurrentSectionIndex != 2 && EcurrentSectionIndex != 3 && EcurrentSectionIndex != 6 && EcurrentSectionIndex != 7 && EcurrentSectionIndex != 10 && EcurrentSectionIndex != 11)
                {
                    EcurrentSectionIndex += 2;
                }
                break;
            case EnemyPlayerMove.DoubleLeft:             // 현재 위치가 1, 5, 9면 -- / 0, 1, 4, 5, 8, 9가 아니면 -2
                if (EcurrentSectionIndex == 1 || EcurrentSectionIndex == 5 || EcurrentSectionIndex == 9)
                {
                    EcurrentSectionIndex--;
                }
                else if (EcurrentSectionIndex != 0 && EcurrentSectionIndex != 1 && EcurrentSectionIndex != 4 && EcurrentSectionIndex != 5 && EcurrentSectionIndex != 8 && EcurrentSectionIndex != 9)
                {
                    EcurrentSectionIndex -= 2;
                }
                break;

        }

        // 배열 범위 검사를 추가하여 IndexOutOfRangeException을 방지
        EcurrentSectionIndex = Mathf.Clamp(EcurrentSectionIndex, 0, board.player2_Position.Length - 1);

        // 이동 후 플레이어의 위치 업데이트 -> 순간이동이 아니라 서서히 이동하도록 수정        
        //transform.position = board.player2_Position[EcurrentSectionIndex].transform.position;

        // 타겟 오브젝트
        targetObgect = board.player2_Position[EcurrentSectionIndex];

        if (targetObgect == null)
        {
            Debug.LogError("TargetObject is null! Cannot proceed with Move.");
            return; // 혹은 적절히 예외 처리
        }

        // 새로운 위치를 계산
        targetPosition = targetObgect.transform.position;

        // 이동하는 동안 목표 위치로 서서히 이동
        StartCoroutine(MoveToPosition(targetPosition));

        // 새로운 위치 정보를 델리게이트로 전송
        SendSectionDelegateFC(EcurrentSectionIndex);

        // 움직이는 모션 자리 여기 switch문 끝났을 때 한번만 넣으면 될듯?
        animator.SetTrigger("Move");
    }

    /// <summary>
    /// 포물선을 그리면서 플레이어를 이동시키는 코루틴(바닥 색 변화 포함)
    /// </summary>
    /// <param name="targetPosition">이동할 위치</param>
    /// <returns></returns>
    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float timeToMove = 0.5f;          // 총 이동 시간
        float maxHeight = 2f;             // 포물선의 최대 높이 (이동 경로의 정점 높이)

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;


        //Transform parent = board.player2_Position[EcurrentSectionIndex].transform.parent;

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
    /// <param name="where">어디서 공격을 했는지(EcurrentSectionIndex 기준)</param>
    public void Attack(EnemyPlayerCharacter EselectedCharacter, EnemyPlayerAttack EselectedAttack, int where)
    {
        // 내 위치 EcurrentSectionIndex 를 기준으로 +1, -1 이면 ㅡ 모양으로 범위가 결정되는 거고
        int[] attackRange = null;          // 기본 공격 범위
        int[] magicAttackRange = null;     // 마법 공격 범위
        int[] limitAttackRange = null;     // 리미트 공격 범위

        int attackCost = 0;         // 기본 공격 비용
        int magicAttackCost = 0;    // 마법 공격 비용
        int limitAttackCost = 0;    // 리미트 공격 비용

        int attackDamage = 0;       // 기본 공격 데미지
        int magicAttackDamage = 0;  // 마법 공격 데미지
        int limitAttackDamage = 0;  // 리미트 공격 데미지

        Debug.Log($"Attack 메서드 호출됨: {EselectedAttack}");

        switch (EselectedCharacter)
        {
            case EnemyPlayerCharacter.Character_Elle:

                // 코스트
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;

                // 공격력
                attackDamage = 25;          // ㅡ 모양
                magicAttackDamage = 35;     // x 모양
                limitAttackDamage = 45;     // H 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                   // 0 1
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5 };                              // 0 5
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1, EcurrentSectionIndex + 5        // 0 1 5
                            };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };          // 1 0 2
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,       // 1 0 4 2 6
                            EcurrentSectionIndex + 1, EcurrentSectionIndex + 5 };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };          // 2 1 3
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex -1, EcurrentSectionIndex + 3,        // 2 1 5 3 7
                            EcurrentSectionIndex + 1, EcurrentSectionIndex + 5 };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 3 2
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3 };                               // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3        // 3 2 6
                             };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 4 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };      // 4 1 9
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1, EcurrentSectionIndex - 3,        // 4 1 5 9
                            EcurrentSectionIndex + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 5 4 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 5 0 8 2 10
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 5 0 4 8 2 6 10
                            EcurrentSectionIndex + 3, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1, EcurrentSectionIndex + 5 };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 6 5 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 6 1 9 3 11
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 6 1 5 9 3 7 11
                            EcurrentSectionIndex + 3, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1, EcurrentSectionIndex + 5 };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 7 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 7 2 6 10
                            EcurrentSectionIndex + 3 };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 8 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1,        // 8 5 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };          // 9 8 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 9 4 8 6 10
                            EcurrentSectionIndex - 3, EcurrentSectionIndex + 1 };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 10 9 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 10 5 9 7 11
                            EcurrentSectionIndex - 3, EcurrentSectionIndex + 1 };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 11 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1         // 11 6 10
                             };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_Akstar:

                // 코스트
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;

                // 공격력
                attackDamage = 25;          // ㅣ 모양
                magicAttackDamage = 35;     // x 모양
                limitAttackDamage = 45;     // ㅜㅗ 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 0 4
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5 };                              // 0 5
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5        // 0 4 5
                            };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 1 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,       // 1 4 5 6
                            EcurrentSectionIndex + 5 };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 2 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 2 5 7
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex +3 , EcurrentSectionIndex + 4,       // 2 5 6 7
                            EcurrentSectionIndex + 5 };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 3 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3 };                              // 3 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4        // 3 6 7
                             };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 4 0 8
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };      // 4 1 9
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 3,        // 4 0 1 8 9
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 5 1 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 5 0 8 2 10
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 5 0 1 2 8 9 10
                            EcurrentSectionIndex - 3, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5 };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 6 2 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 6 1 9 3 11
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 6 1 2 3 9 10 11
                            EcurrentSectionIndex - 3, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5 };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 7 3 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 7 2 3 10 11
                            EcurrentSectionIndex + 3, EcurrentSectionIndex + 4 };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 8 4
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 3,        // 8 4 5
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 9 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 9 4 5 6
                            EcurrentSectionIndex - 3 };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, };                                   // 10 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 10 5 6 7
                            EcurrentSectionIndex - 3 };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 11 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4         // 11 6 7
                             };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_Adel:

                // 코스트
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 40;

                // 공격력
                attackDamage = 25;          // ㅣ 모양
                magicAttackDamage = 30;     // + 모양
                limitAttackDamage = 50;     // ㅗ 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 0 4
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };     // 0 1 4
                        limitAttackRange = new int[] { EcurrentSectionIndex                                                          // 0
                             };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 1 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,       // 1 0 5 2
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex                                                          // 1
                             };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 2 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,       // 2 1 6 3
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex                                                          // 2
                             };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 3 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4 };     // 3 2 7
                        limitAttackRange = new int[] { EcurrentSectionIndex                                                          // 3
                             };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 4 0 8
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex + 1,        // 4 0 5 8
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 3,        // 4 0 1
                             };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 5 1 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 5 1 4 9 6
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 5 0 1 2
                            EcurrentSectionIndex - 3 };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 6 2 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 6 2 5 10 7
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 6 1 2 3
                            EcurrentSectionIndex - 3 };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 7 3 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 7 3 6 11
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 7 2 3
                             };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 8 4
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex + 1         // 8 4 9
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 3,        // 8 4 5
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 9 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4,        // 9 8 5 10
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 9 4 5 6
                            EcurrentSectionIndex - 3 };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, };                                   // 10 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4,        // 10 9 6 11
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 10 5 6 7
                            EcurrentSectionIndex - 3 };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 11 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4         // 11 6 7
                             };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_Amelia:

                // 코스트
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;

                // 공격력
                attackDamage = 25;          // ㅡ 모양
                magicAttackDamage = 30;     // + 모양
                limitAttackDamage = 40;     // 아래 ㅁ 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                   // 0 1
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };     // 0 1 4
                        limitAttackRange = new int[] { EcurrentSectionIndex , EcurrentSectionIndex + 1                                // 0 1
                             };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 1 0 2
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,       // 1 0 5 2
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1        // 1 0 2
                             };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };          // 2 1 3
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,       // 2 1 6 3
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1        // 2 1 3
                             };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                   // 3 2
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4 };     // 3 2 7
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1                                 // 3 2
                             };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 4 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex + 1,        // 4 0 5 8
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 3,        // 4 0 1 5
                            EcurrentSectionIndex + 1 };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 5 4 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 5 1 4 9 6
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 5,        // 5 4 0 1 2 6
                            EcurrentSectionIndex - 4, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1 };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 6 5 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 6 2 5 10 7
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 5,        // 6 5 1 2 3 7
                            EcurrentSectionIndex - 4, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1 };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 7 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 7 3 6 11
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 5,        // 7 6 2 3
                            EcurrentSectionIndex - 4 };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 8 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex + 1         // 8 4 9
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 3,        // 8 4 5 9
                            EcurrentSectionIndex + 1 };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 9 8 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4,        // 9 8 5 10
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 5,        // 9 4 5 6 10
                            EcurrentSectionIndex - 4, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1 };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 10 9 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4,        // 10 9 6 11
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 5,        // 10 9 5 6 7 11
                            EcurrentSectionIndex - 4, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1 };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 11 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 5,        // 11 10 6 7
                             EcurrentSectionIndex - 4 };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_Barbariccia:

                // 코스트
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;

                // 공격력
                attackDamage = 25;          // ㅡ 모양
                magicAttackDamage = 35;     // x 모양
                limitAttackDamage = 40;     // 위 ㅁ 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                   // 0 1
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5 };                              // 0 5
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5,       // 0 4 5 1
                            EcurrentSectionIndex + 1 };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };          // 1 0 2
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,       // 1 0 4 5 6 2
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5, EcurrentSectionIndex + 1 };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };          // 2 1 3
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,       // 2 1 5 6 7 3
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5, EcurrentSectionIndex + 1 };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 3 2
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3 };                               // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,        // 3 2 6 7
                            EcurrentSectionIndex + 4 };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 4 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };      // 4 1 9
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 1,        // 4 8 5 9
                            EcurrentSectionIndex + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 5 4 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 5 0 8 2 10
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,        // 5 4 8 9 10 6
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5, EcurrentSectionIndex + 1 };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 6 5 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 6 1 9 3 11
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,        // 6 5 9 10 11 7
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5, EcurrentSectionIndex + 1 };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 7 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,        // 7 6 10 11
                            EcurrentSectionIndex + 4 };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 8 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1,                                 // 8 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 9 8 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,        // 9 8 10
                             };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 10 9 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,        // 10 9 11
                             };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 11 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1                                  // 11 10
                             };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_Jade:

                // 코스트
                attackCost = 25;
                magicAttackCost = 25;
                limitAttackCost = 45;

                // 공격력
                attackDamage = 20;          // ㅜ 모양
                magicAttackDamage = 35;     // x 모양
                limitAttackDamage = 45;     // H 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5 };          // 0 4 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5 };                              // 0 5
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1, EcurrentSectionIndex + 5        // 0 1 5
                            };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,            // 1 4 5 6
                            EcurrentSectionIndex + 5 };
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,       // 1 0 4 2 6
                            EcurrentSectionIndex + 1, EcurrentSectionIndex + 5 };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,            // 2 5 6 7
                        EcurrentSectionIndex + 5};
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex -1, EcurrentSectionIndex + 3,        // 2 1 5 3 7
                            EcurrentSectionIndex + 1, EcurrentSectionIndex + 5 };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4 };           // 3 6 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3 };                               // 1 4 6
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3         // 3 2 6
                             };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5 };           // 4 8 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };      // 4 1 9
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1, EcurrentSectionIndex - 3,        // 4 1 5 9
                            EcurrentSectionIndex + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,             // 5 8 9 10
                            EcurrentSectionIndex + 5};
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 5 0 8 2 10
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 5 0 4 8 2 6 10
                            EcurrentSectionIndex + 3, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1, EcurrentSectionIndex + 5 };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,             // 6 9 10 11
                            EcurrentSectionIndex + 5 };
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 6 1 9 3 11
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 6 1 5 9 3 7 11
                            EcurrentSectionIndex + 3, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1, EcurrentSectionIndex + 5 };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4 };           // 7 10 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 7 2 6 10
                            EcurrentSectionIndex + 3 };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex };                                                             // 8
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 3, EcurrentSectionIndex + 1,        // 8 5 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex };                                                             // 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 9 4 8 6 10
                            EcurrentSectionIndex - 3, EcurrentSectionIndex + 1 };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex };                                                             // 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1,        // 10 5 9 7 11
                            EcurrentSectionIndex - 3, EcurrentSectionIndex + 1 };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex };                                                             // 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 1         // 11 6 10
                             };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_Arngrim:

                // 코스트
                attackCost = 20;
                magicAttackCost = 25;
                limitAttackCost = 40;

                // 공격력
                attackDamage = 25;          // ㅜ 모양
                magicAttackDamage = 30;     // + 모양
                limitAttackDamage = 50;     // ㅗ 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5 };          // 0 4 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };     // 0 1 4
                        limitAttackRange = new int[] { EcurrentSectionIndex                                                          // 0
                            };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,            // 1 4 5 6
                            EcurrentSectionIndex + 5 };
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,       // 1 0 5 2
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex                                                          // 1
                             };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,            // 2 5 6 7
                        EcurrentSectionIndex + 5};
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,       // 2 1 6 3
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex                                                          // 2
                             };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4 };           // 3 6 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4 };      // 3 2 7
                        limitAttackRange = new int[] { EcurrentSectionIndex                                                           // 3
                             };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5 };           // 4 8 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex + 1,        // 4 0 5 8
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 3,        // 4 0 1
                             };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,             // 5 8 9 10
                            EcurrentSectionIndex + 5};
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 5 1 4 9 6
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 5 0 1 2
                            EcurrentSectionIndex - 3 };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4,             // 6 9 10 11
                            EcurrentSectionIndex + 5 };
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 6 2 5 10 7
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 6 1 2 3
                            EcurrentSectionIndex - 3 };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 4 };           // 7 10 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 7 3 6 11
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 7 2 3
                             };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex };                                                             // 8
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex + 1         // 8 4 9
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 3,        // 8 4 5
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex };                                                             // 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4,        // 9 8 5 10
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 9 4 5 6
                            EcurrentSectionIndex - 3 };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex };                                                             // 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4,        // 10 9 6 11
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4,        // 10 5 6 7
                            EcurrentSectionIndex - 3 };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex };                                                             // 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 4         // 11 6 7
                             };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_BlackMage:

                // 코스트
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 45;

                // 공격력
                attackDamage = 20;          // ㅗ 모양
                magicAttackDamage = 30;     // + 모양
                limitAttackDamage = 40;     // 위 ㅁ 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1, EcurrentSectionIndex + 4 };          // 0 4 1
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1, EcurrentSectionIndex + 4 };     // 0 4 1
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 5,       // 0 4 5 1
                            EcurrentSectionIndex + 1 };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,            // 1 0 5 2
                            EcurrentSectionIndex + 1 };
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,       // 1 0 5 2
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,       // 1 0 4 5 6 2
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5, EcurrentSectionIndex + 1 };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,            // 2 1 6 3
                            EcurrentSectionIndex + 1 };
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,       // 2 1 6 3
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,       // 2 1 5 6 7 3
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5, EcurrentSectionIndex + 1 };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4 };           // 3 2 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4 };      // 3 2 7
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,        // 3 2 6 7
                            EcurrentSectionIndex + 4 };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };           // 4 8 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 1,        // 4 8 5 0
                            EcurrentSectionIndex - 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 1,        // 4 8 5 9
                            EcurrentSectionIndex + 5 };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,             // 5 4 9 6
                            EcurrentSectionIndex + 1 };
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,        // 5 4 9 6 1
                            EcurrentSectionIndex + 1, EcurrentSectionIndex - 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,        // 5 4 8 9 10 6
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5, EcurrentSectionIndex + 1 };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,             // 6 5 10 7
                            EcurrentSectionIndex + 1 };
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,        // 6 5 10 7 2
                            EcurrentSectionIndex + 1, EcurrentSectionIndex - 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,        // 6 5 9 10 11 7
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 5, EcurrentSectionIndex + 1 };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4 };           // 7 6 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4,        // 7 6 11 3
                            EcurrentSectionIndex - 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 3,        // 7 6 10 11
                            EcurrentSectionIndex + 4 };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 8 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1, EcurrentSectionIndex - 4         // 8 9 4
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1,                                 // 8 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 9 8 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,        // 9 8 10 5
                             EcurrentSectionIndex - 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,        // 9 8 10
                             };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 10 9 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,        // 10 9 11 6
                             EcurrentSectionIndex - 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,        // 10 9 11
                             };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 11 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1                                  // 11 10
                             };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_Cloud:

                // 코스트
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 40;

                // 공격력
                attackDamage = 25;          // ㅡ 모양
                magicAttackDamage = 30;     // + 모양
                limitAttackDamage = 70;     // . . 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                   // 0 1
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };     // 0 1 4
                        limitAttackRange = new int[] { EcurrentSectionIndex + 1                                                      // 1
                             };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };          // 1 0 2
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,       // 1 0 5 2
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1, EcurrentSectionIndex + 1                             // 0 2
                             };
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };          // 2 1 3
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1,       // 2 1 6 3
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1, EcurrentSectionIndex + 1                             // 1 3
                             };
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                   // 3 2
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 4 };     // 3 2 7
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1                                                      // 2
                             };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 4 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex + 1,        // 4 0 5 8
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex + 1                                                       // 5
                             };
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 5 4 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 5 1 4 9 6
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1, EcurrentSectionIndex + 1                              // 4 6
                             };
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 6 5 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 6 2 5 10 7
                            EcurrentSectionIndex + 4, EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1, EcurrentSectionIndex + 1                              // 5 7
                             };
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 7 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex - 1,        // 7 3 6 11
                            EcurrentSectionIndex + 4 };
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1                                                       // 6
                             };
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 1 };                                    // 8 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, EcurrentSectionIndex + 1         // 8 4 9
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex + 1                                                       // 9
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 9 8 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4,        // 9 8 5 10
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1, EcurrentSectionIndex + 1                              // 8 10
                             };
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex + 1 };           // 10 9 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4,        // 10 9 6 11
                            EcurrentSectionIndex + 1 };
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1, EcurrentSectionIndex + 1                              // 9 11
                             };
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1 };                                    // 11 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 1, EcurrentSectionIndex - 4         // 11 10 7
                             };
                        limitAttackRange = new int[] { EcurrentSectionIndex - 1                                                       // 10
                             };
                        break;

                }
                break;

            case EnemyPlayerCharacter.Character_Nalu:

                // 코스트
                attackCost = 15;
                magicAttackCost = 25;
                limitAttackCost = 40;

                // 공격력
                attackDamage = 25;          // ㅣ 모양
                magicAttackDamage = 35;     // x 모양
                limitAttackDamage = 40;     // 자신 서있는 줄 ㅡ 모양

                // 공격 범위(위치에 따라 다름 )
                switch (where)
                {
                    case 0:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 0 4
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5 };                              // 0 5
                        limitAttackRange = new int[] { 0, 1, 2, 3                                                                   // 0 1 2 3
                             };
                        break;
                    case 1:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 1 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 1 4 6
                        limitAttackRange = new int[] { 0, 1, 2, 3 };                                                                // 0 1 2 3
                        break;
                    case 2:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 2 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3, EcurrentSectionIndex + 5 };     // 2 5 7
                        limitAttackRange = new int[] { 0, 1, 2, 3 };                                                                // 0 1 2 3
                        break;
                    case 3:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4 };                                   // 3 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 3 };                              // 3 6
                        limitAttackRange = new int[] { 0, 1, 2, 3                                                                   // 0 1 2 3
                             };
                        break;
                    case 4:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 4 0 8
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };      // 4 1 9
                        limitAttackRange = new int[] { 4, 5, 6, 7 };                                                                 // 4 5 6 7
                        break;
                    case 5:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 5 1 9
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 5 0 8 2 10
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { 4, 5, 6, 7 };                                                                 // 4 5 6 7
                        break;
                    case 6:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 6 2 10
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 6 1 9 3 11
                            EcurrentSectionIndex + 5, EcurrentSectionIndex - 3 };
                        limitAttackRange = new int[] { 4, 5, 6, 7 };                                                                 // 4 5 6 7
                        break;
                    case 7:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex + 4, EcurrentSectionIndex - 4 };           // 7 3 11
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex + 3,        // 7 2 10
                             };
                        limitAttackRange = new int[] { 4, 5, 6, 7 };                                                                 // 4 5 6 7
                        break;
                    case 8:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 8 4
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 3                                  // 8 5
                             };
                        limitAttackRange = new int[] { 8, 9, 10 ,11                                                                  // 8 9 10 11
                             };
                        break;
                    case 9:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 9 5
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 9 4 6
                             };
                        limitAttackRange = new int[] { 8, 9, 10, 11 };                                                               // 8 9 10 11
                        break;
                    case 10:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4, };                                   // 10 6
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5, EcurrentSectionIndex - 3         // 10 5 7
                             };
                        limitAttackRange = new int[] { 8, 9, 10, 11 };                                                               // 8 9 10 11
                        break;
                    case 11:
                        attackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 4 };                                    // 11 7
                        magicAttackRange = new int[] { EcurrentSectionIndex, EcurrentSectionIndex - 5                                  // 11 6
                             };
                        limitAttackRange = new int[] { 8, 9, 10, 11 };                                                               // 8 9 10 11
                        break;

                }
                break;

        }

        switch (EselectedAttack)
        {
            case EnemyPlayerAttack.Attack:

                // 공격 범위에 따라 공격하기
                // 공격 범위 내에 상대 플레이어가 있으면 HP 감소 시키기
                // 애니메이터 변경

                ResetTrigger();
                animator.SetTrigger("Attack");

                // 에너지 감소
                ReduceEnergy(attackCost);

                for (int i = 0; i < attackRange.Length; i++)
                {
                    // 만약 적 플레이어가 공격 범위에 있으면 데미지
                    if (player.currentSectionIndex == attackRange[i])
                    {
                        // 데미지
                        player.HP -= attackDamage;
                        Debug.Log($"적 플레이어의 남은 체력 : {player.HP}");
                    }
                    targetAttack = board.player1_Position[attackRange[i]];      // 바닥 빨갛게 보이기 위해
                    StartCoroutine(AttackRed());
                }
                break;

            case EnemyPlayerAttack.MagicAttack:

                ResetTrigger();
                animator.SetTrigger("MagicAttack");

                // 에너지 감소
                ReduceEnergy(magicAttackCost);

                for (int i = 0; i < magicAttackRange.Length; i++)
                {
                    // 만약 플레이어가 공격 범위에 있으면 데미지
                    if (player.currentSectionIndex == magicAttackRange[i])
                    {
                        // 데미지
                        player.HP -= magicAttackDamage;
                        Debug.Log($"적 플레이어의 남은 체력 : {player.HP}");
                    }
                    targetAttack = board.player1_Position[magicAttackRange[i]];
                    StartCoroutine(AttackRed());
                }
                break;

            case EnemyPlayerAttack.LimitAttack:

                ResetTrigger();
                animator.SetTrigger("LimitAttack");

                // 에너지 감소
                ReduceEnergy(limitAttackCost);

                for (int i = 0; i < limitAttackRange.Length; i++)
                {
                    // 만약 플레이어가 공격 범위에 있으면 데미지
                    if (player.currentSectionIndex == limitAttackRange[i])
                    {
                        // 데미지
                        player.HP -= limitAttackDamage;
                        Debug.Log($"적 플레이어의 남은 체력 : {player.HP}");
                    }
                    targetAttack = board.player1_Position[limitAttackRange[i]];
                    StartCoroutine(AttackRed());
                }
                break;
        }
    }

    /// <summary>
    /// 공격 범위를 보여주는 코루틴
    /// </summary>
    /// <returns></returns>
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
    /// 에너지를 감소시키는 함수
    /// </summary>
    /// <param name="cost">감소시키는 양</param>
    public void ReduceEnergy(int cost)
    {
        Debug.Log($"Energy 감소: {cost}");
        Energy -= cost;                         // 에너지 차감
        //energyChange?.Invoke(currentEnergy);    // UI 업데이트용 델리게이트 호출 => 에너지가 변경되면 프로퍼티에서 델리게이트 보냄
    }

    /// <summary>
    /// 플레이어가 수비적인 행동을 할 때 실행되는 함수
    /// </summary>
    private void Protect()
    {
        switch (EselectedProtect)
        {
            case EnemyPlayerProtect.Guard:
                StartCoroutine(GuardCoroutine());
                // 데미지 감소시키는 부분 필요 
                break;
            case EnemyPlayerProtect.PerfectGuard:
                StartCoroutine(GuardCoroutine());
                // 데미지 상쇄시키는 부분 필요
                break;
            case EnemyPlayerProtect.EnergyUp:
                StartCoroutine(EnergyHealCoroutine());
                // 에너지 회복하는 부분 필요
                break;
            case EnemyPlayerProtect.Heal:
                StartCoroutine(EnergyHealCoroutine());
                // HP 회복 시키는 부분 필요
                break;
        }
    }

    /// <summary>
    /// 플레이어가 가드 행동을 할 때 실행되는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator GuardCoroutine()
    {
        animator.SetTrigger("Protect");         // 가드, 에너지 업, 힐 애니메이터

        yield return new WaitForSeconds(1);     // 1초 대기 => 가드를 먼저 시작하고 후에 적이 공격을 시작하기 때문에(공격 시작 전까지는 enemyActiveEnd 가 false임)

        while (!player.playerActiveEnd)         // 플레이어의 공격이 끝날 때까지 반복
        {
            yield return null;
        }

        enemyActiveEnd = true;                 // 행동이 끝났음을 표시

        ResetTrigger();
        animator.SetTrigger("Idle");
    }

    /// <summary>
    /// 플레이어가 에너지, 체력을 회복할 때 실행되는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator EnergyHealCoroutine()
    {
        animator.SetTrigger("Protect");     // 가드, 에너지 업, 힐 애니메이터

        float elTime = 0;                   // 누적 시간

        while (elTime < 1.0f)                // 회복하는 동안 반복
        {
            elTime += Time.deltaTime;
            yield return null;
        }

        enemyActiveEnd = true;             // 행동이 끝났음을 표시

        ResetTrigger();
        animator.SetTrigger("Idle");
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
