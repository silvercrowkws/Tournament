using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;      // 셔플을 위함

public class BackGround : MonoBehaviour
{
    /// <summary>
    /// 스프라이트 랜더러
    /// </summary>
    SpriteRenderer spriteRenderer;

    /// <summary>
    /// 뒷배경의 배열(10개 있음)
    /// </summary>
    public Sprite[] backgrounds;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    List<int> availableIndexes = new List<int>();

    private void Awake()
    {
        gameManager = GameManager.Instance;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 근데 이거 Start에서 하면 승리했을 때는 배경 안바뀔텐데?
        // 처음에 한번만 이렇게 하고
        // 다음 부터는 다음 라운드에 바뀌도록 어떻게 수정이 필요할 듯
        for(int i = 0; i < backgrounds.Length; i++)
        {
            availableIndexes.Add(i);
        }

        // 인덱스를 셔플하여 순서대로 사용할 수 있도록 준비
        availableIndexes = availableIndexes.OrderBy(x => Random.value).ToList();

        // 게임 상태가 카드 선택 상태이면
        if (gameManager.GameState == GameState.SelectCard)
        {
            // 셔플된 순서대로 배경을 설정
            int randomIndex = availableIndexes[0];              // 셔플된 첫 번째 인덱스를 선택

            spriteRenderer.sprite = backgrounds[randomIndex];

            // 이후 셔플된 리스트에서 첫 번째 인덱스를 제거(다음 0번을 위함)
            availableIndexes.RemoveAt(0);
        }

        // 게임 상태가 메인 상태이면
        else if(gameManager.GameState == GameState.Main)
        {
            spriteRenderer.sprite = backgrounds[0];
        }
    }
}
