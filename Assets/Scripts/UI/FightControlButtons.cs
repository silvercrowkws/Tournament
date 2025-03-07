using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FightControlButtons : MonoBehaviour
{
    /// <summary>
    /// 캐릭터 변경 버튼
    /// </summary>
    Button changeFighter;

    /// <summary>
    /// 전투 시작 버튼
    /// </summary>
    Button fight;

    /// <summary>
    /// 캐릭터 선택 버튼(클래스)
    /// </summary>
    CharacterButtons characterButtons;

    /// <summary>
    /// 캐릭터 변경 버튼을 클릭했다고 알리는 델리게이트
    /// </summary>
    public Action onChangeFighter;

    public Action onFight;

    /// <summary>
    /// 스크린샷을 찍은 후 Sprite로 변환하여 전달하는 델리게이트
    /// </summary>
    public Action<Sprite> onScreenshotCaptured;

    /// <summary>
    /// 캔버스 그룹
    /// </summary>
    CanvasGroup canvasGroup;

    /// <summary>
    /// 버서스 이미지 클래스
    /// </summary>
    VSImage vsImage;

    private void Awake()
    {
        Transform child = transform.GetChild(1);                        // 이 오브젝트의 1번째 자식 Buttons

        changeFighter = child.GetChild(0).GetComponent<Button>();       // Buttons의 0번째 자식 ChangeFighter
        changeFighter.onClick.AddListener(ChangeFigherFC);

        fight = child.GetChild(1).GetComponent<Button>();               // Buttons의 1번째 자식 Fight
        fight.onClick.AddListener(FightFC);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        characterButtons = FindAnyObjectByType<CharacterButtons>();
        vsImage = FindAnyObjectByType<VSImage>();
        vsImage.onInteract += OnInteractFalse;
    }

    /// <summary>
    /// 캐릭터 변경 버튼 클릭으로 실행되는 함수
    /// </summary>
    private void ChangeFigherFC()
    {
        // 1. CharacterButtons 활성화
        // 2. VSImage의 tournamentImages, frameImages 안보이게 변경(tournamentImages : 알파값 0으로, frameImages 비활성화)
        // 3. VSImage의 leftCharacter, rightCharacter 알파값 0으로 변경
        // 4. VSImage의 tournamentList 초기화
        // 5. 이 게임 오브젝트 비활성화

        //characterButtons.gameObject.SetActive(true);        // 1
        onChangeFighter?.Invoke();                          // 1, 2, 3, 4
        //this.gameObject.SetActive(false);                   // 5 => 캔버스 그룹 조절하는 것으로 변경
        OnInteractFalse(false);
    }

    private void OnInteractFalse(bool tf)
    {
        if (tf)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// 전투 시작 버튼 클릭으로 실행되는 함수
    /// </summary>
    private void FightFC()
    {
        // 1. 배틀 순서를 게임 매니저에 전달
        // 1.1 현재 화면 캡쳐해서 스프라이트로 게임매니저에 전달
        // 2. 씬 전환(카드 선택으로 작동하는 씬?)

        onFight?.Invoke();                                  // 1
        StartCoroutine(CaptureRoutine());

        TurnManager turnManager = TurnManager.Instance;
        turnManager.OnInitialize2();
        SceneManager.LoadScene(2);                          // 2
    }

    /// <summary>
    /// 화면을 캡쳐하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame(); // 프레임이 끝날 때까지 대기 (화면이 다 그려진 후 캡처하기 위함)

        int width = Screen.width;  // 현재 화면 너비 가져오기
        int height = Screen.height; // 현재 화면 높이 가져오기

        // RenderTexture 생성 (화면을 캡처할 임시 텍스처)
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        ScreenCapture.CaptureScreenshotIntoRenderTexture(renderTexture); // 화면을 RenderTexture에 캡처

        // Texture2D 생성 (RenderTexture에서 읽어오기 위해)
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture; // RenderTexture를 활성화하여 읽을 준비
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0); // 픽셀을 읽어서 Texture2D에 저장
        screenshot.Apply(); // Texture2D 적용

        // Y축 반전 함수 호출
        FlipTextureVertically(screenshot);

        // Texture2D를 Sprite로 변환
        Sprite sprite = TextureToSprite(screenshot);

        // 델리게이트 호출 (캡처된 Sprite를 이벤트 리스너들에게 전달)
        onScreenshotCaptured?.Invoke(sprite);

        // 메모리 해제 (RenderTexture 비활성화 후 삭제)
        RenderTexture.active = null;
        renderTexture.Release();
        Destroy(renderTexture);
    }

    /// <summary>
    /// Texture2D를 Sprite로 변환하는 함수
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Y축 반전 함수(캡쳐한 사진은 위아래가 뒤집힘)
    /// </summary>
    /// <param name="texture"></param>
    private void FlipTextureVertically(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;

        Color[] flippedPixels = new Color[pixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flippedPixels[x + y * width] = pixels[x + (height - y - 1) * width];
            }
        }

        texture.SetPixels(flippedPixels);
        texture.Apply();
    }
}
