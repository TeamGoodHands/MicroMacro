using UnityEngine;
using UnityEngine.UI;

public class FlipbookAnimator : MonoBehaviour
{
    [SerializeField]
    private Image targetRenderer;

    [SerializeField]
    private Sprite[] animationFrames;

    [SerializeField]
    private float framesPerSecond = 12.0f;

    [SerializeField]
    private bool isLooping = true;

    [SerializeField]
    private bool playOnStart = true;

    // 現在の経過時間を計測する変数
    private float timer;

    // 現在のフレーム番号
    private int currentFrameIndex;

    // 再生中かどうかのフラグ
    private bool isPlaying;

    private void Start()
    {
        if (playOnStart)
            Play();
    }

    private void Update()
    {
        // 再生中でなければ処理しない
        if (!isPlaying)
            return;

        // レンダラーがない、または画像がない場合は処理しない
        if (targetRenderer == null)
            return;

        if (animationFrames.Length == 0)
            return;

        // 経過時間を加算
        timer += Time.unscaledDeltaTime;

        // 1フレームあたりの時間を計算 (1秒 / FPS)
        float secondsPerFrame = 1.0f / framesPerSecond;

        // 経過時間がフレーム切り替え時間を超えたかチェック
        if (timer >= secondsPerFrame)
        {
            // 時間をリセット（剰余を残すことでズレを防ぐ）
            timer -= secondsPerFrame;

            // 次のコマへ
            AdvanceFrame();
        }
    }

    private void AdvanceFrame()
    {
        currentFrameIndex++;

        // 最後のコマを超えた場合の処理
        if (currentFrameIndex >= animationFrames.Length)
        {
            if (isLooping)
            {
                // ループする場合は最初に戻る
                currentFrameIndex = 0;
            }
            else
            {
                // ループしない場合は最後のコマで止めて再生終了
                currentFrameIndex = animationFrames.Length - 1;
                isPlaying = false;
            }
        }

        // スプライトを更新
        targetRenderer.sprite = animationFrames[currentFrameIndex];
    }

    // 外部から再生を開始するためのメソッド
    public void Play()
    {
        isPlaying = true;
        currentFrameIndex = 0;
        timer = 0f;

        // 最初のフレームを即座に表示
        if (targetRenderer != null && animationFrames.Length > 0)
            targetRenderer.sprite = animationFrames[0];
    }

    // 外部から停止するためのメソッド
    public void Stop()
    {
        isPlaying = false;
    }

    // 画像リストを動的に入れ替える
    public void SetFrames(Sprite[] newFrames)
    {
        animationFrames = newFrames;
        // フレーム数が変わったのでインデックスを安全な範囲に戻す
        currentFrameIndex = 0;
    }
}