using UnityEngine;

// ユーザーが付けたファイル名に合わせてクラス名を変更
public class SampleHrtfProcessor : MonoBehaviour
{
    [Tooltip("音を鳴らすオブジェクト")]
    public Transform soundSource;

    [Tooltip("HRIRのWAVファイル（1250個）。順番が超重要！")]
    public AudioClip[] hrirClips;

    // --- 定数定義 (変更なし) ---
    private readonly float[] azimuths = new float[]
    {
        -80, -65, -55, -45, -40, -35, -30, -25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 55, 65, 80
    };
    private readonly float[] elevations = new float[50];
    private const int ELEVATION_COUNT = 50;
    private const int HRIR_LENGTH = 200;

    // --- 内部変数 (変更なし) ---
    private float[][] hrirLeftSamples;
    private float[][] hrirRightSamples;
    private float[] convolutionBufferLeft;
    private float[] convolutionBufferRight;
    private bool isReady = false;
    private float currentAzimuth = 0f;
    private float currentElevation = 0f;

    void Awake()
    {
        for (int i = 0; i < ELEVATION_COUNT; i++)
        {
            elevations[i] = -45f + 5.625f * i;
        }

        if (hrirClips.Length != azimuths.Length * ELEVATION_COUNT)
        {
            Debug.LogError($"HRIRクリップの数が正しくありません！期待値: {azimuths.Length * ELEVATION_COUNT}, 現在値: {hrirClips.Length}");
            return;
        }

        // ここで関数を呼び出している
        LoadAllHrirData();

        convolutionBufferLeft = new float[HRIR_LENGTH];
        convolutionBufferRight = new float[HRIR_LENGTH];

        isReady = true;
        Debug.Log("HRIRデータの準備が完了しました。");
    }

    // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
    // この関数が丸ごと抜けているはず！ここに追加しよう！
    // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
    void LoadAllHrirData()
    {
        int totalClips = hrirClips.Length;
        hrirLeftSamples = new float[totalClips][];
        hrirRightSamples = new float[totalClips][];

        for (int i = 0; i < totalClips; i++)
        {
            AudioClip clip = hrirClips[i];
            hrirLeftSamples[i] = new float[HRIR_LENGTH];
            hrirRightSamples[i] = new float[HRIR_LENGTH];

            if (clip == null)
            {
                Debug.LogError($"hrirClipsのインデックス {i} が空です！");
                continue;
            }

            float[] allSamples = new float[clip.samples * clip.channels];
            clip.GetData(allSamples, 0);

            for (int j = 0; j < HRIR_LENGTH; j++)
            {
                if (j < clip.samples)
                {
                    if (clip.channels == 2)
                    {
                        hrirLeftSamples[i][j] = allSamples[j * 2];
                        hrirRightSamples[i][j] = allSamples[j * 2 + 1];
                    }
                    else
                    {
                        hrirLeftSamples[i][j] = allSamples[j];
                        hrirRightSamples[i][j] = allSamples[j];
                    }
                }
            }
        }
    }

    void Update()
    {
        if (!isReady || soundSource == null) return;
        
        Vector3 direction = soundSource.position - transform.position;
        CalculateAngles(direction, out currentAzimuth, out currentElevation);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isReady) return;

        int azIndex, elIndex;
        FindNearestHrirIndices(currentAzimuth, currentElevation, out azIndex, out elIndex);

        int clipIndex = azIndex * ELEVATION_COUNT + elIndex;

        if (clipIndex < 0 || clipIndex >= hrirLeftSamples.Length) return;

        float[] hrirL = hrirLeftSamples[clipIndex];
        float[] hrirR = hrirRightSamples[clipIndex];

        for (int i = 0; i < data.Length; i += channels)
        {
            float monoInput = data[i];
            float resultL = Convolve(monoInput, hrirL, convolutionBufferLeft);
            float resultR = Convolve(monoInput, hrirR, convolutionBufferRight);
            data[i] = resultL;
            if (channels > 1) data[i + 1] = resultR;
        }
    }
    
    // --- ヘルパー関数 (変更なし) ---
#region Helper Functions
    void CalculateAngles(Vector3 direction, out float az, out float el)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            az = 0; el = 0; return;
        }
        direction.Normalize();
        Vector3 right = transform.right;
        Vector3 fwd = transform.forward;
        Vector3 up = transform.up;
        az = Mathf.Atan2(Vector3.Dot(direction, right), Vector3.Dot(direction, fwd)) * Mathf.Rad2Deg;
        el = Mathf.Asin(Vector3.Dot(direction, up)) * Mathf.Rad2Deg;
    }

    void FindNearestHrirIndices(float targetAz, float targetEl, out int azIndex, out int elIndex)
    {
        float minAzDiff = float.MaxValue; azIndex = 0;
        for (int i = 0; i < azimuths.Length; i++)
        {
            float diff = Mathf.Abs(targetAz - azimuths[i]);
            if (diff < minAzDiff) { minAzDiff = diff; azIndex = i; }
        }

        float minElDiff = float.MaxValue; elIndex = 0;
        for (int i = 0; i < elevations.Length; i++)
        {
            float diff = Mathf.Abs(targetEl - elevations[i]);
            if (diff < minElDiff) { minElDiff = diff; elIndex = i; }
        }
    }

    float Convolve(float inputSample, float[] hrir, float[] buffer)
    {
        for (int i = buffer.Length - 1; i > 0; i--) buffer[i] = buffer[i - 1];
        buffer[0] = inputSample;
        float result = 0f;
        for (int i = 0; i < hrir.Length; i++) result += buffer[i] * hrir[i];
        return result;
    }
#endregion
}