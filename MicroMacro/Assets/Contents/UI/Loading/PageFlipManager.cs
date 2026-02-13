using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Module.Management; // SoundManager用

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PageFlipManager : MonoBehaviour
{
    [SerializeField] private Camera uiCamera; // UIを撮影する専用カメラ

    [SerializeField] private float distanceFromCamera = 10.0f; // カメラからの配置距離

    [Header("Audio Settings")] [SerializeField] [Tooltip("ページめくり時のSE名（SoundManagerに登録された名前）")]
    private string pageFlipSEName = "PageFlip";

    [SerializeField] [Tooltip("ページごとにSEを鳴らすか（falseなら最初の1回のみ）")]
    private bool playSoundPerPage = true;

    [SerializeField] [Tooltip("マスクディゾルブ時のSE名（SoundManagerに登録された名前）")]
    private string maskDissolveSEName = "MaskDissolve";

    [Header("Page Settings")] [SerializeField]
    private Texture[] intermediatePageTextures; // 中間ページ用テクスチャ（オプション）

    [Header("Rapid Flip Settings (パラパラ)")] [SerializeField] [Tooltip("パラパラめくり時の1ページあたりのアニメーション時間")]
    private float rapidFlipDuration = 0.1f;

    [SerializeField] [Tooltip("パラパラめくり時のページ数")]
    private int rapidFlipPageCount = 6;

    [SerializeField] [Tooltip("最初のページの速度倍率（大きいほど遅い）")] [Range(0.5f, 3f)]
    private float startSpeedMultiplier = 1.5f;

    [SerializeField] [Tooltip("最後のページの速度倍率（小さいほど速い）")] [Range(0.1f, 1f)]
    private float endSpeedMultiplier = 0.3f;

    [Header("Mesh Settings")] [SerializeField] [Range(10, 200)]
    private int meshResolutionX = 100;

    [SerializeField] [Range(2, 50)] private int meshResolutionY = 20;

    [Header("Mask Dissolve Settings")] [SerializeField] [Tooltip("マスクディゾルブ用テクスチャ")]
    private Texture2D maskTexture;

    [SerializeField] [Tooltip("マスクディゾルブの所要時間")]
    private float maskDissolveDuration = 0.3f;

    // 内部変数
    private Material pageMaterial;
    private RenderTexture uiRenderTexture;
    private MeshRenderer meshRenderer;
    private bool isFlipping = false;
    private Mesh sharedPageMesh; // 共有メッシュ

    // パラパラめくり用の複数ページ
    private GameObject[] rapidPageObjects;
    private Material[] rapidPageMaterials;
    private MeshRenderer[] rapidPageRenderers;

    // メインカメラは動的に取得（シーン遷移対応）
    private Camera MainCamera => Camera.main;

    // シェーダープロパティID
    private static readonly int CurlAmountProp = Shader.PropertyToID("_CurlAmount");
    private static readonly int MainTexProp = Shader.PropertyToID("_MainTex");
    private static readonly int MaskTexProp = Shader.PropertyToID("_MaskTex");
    private static readonly int MaskThresholdProp = Shader.PropertyToID("_MaskThreshold");
    private static readonly int UseMaskProp = Shader.PropertyToID("_UseMask");

    // イベント
    public event Action OnFlipComplete;
    public event Action OnAllFlipsComplete;

    // プロパティ
    public bool IsFlipping => isFlipping;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // 1. マテリアル確保
        if (meshRenderer != null)
        {
            pageMaterial = meshRenderer.material;
            // 初期状態は非表示
            meshRenderer.enabled = false;
        }

        // 2. メッシュ生成
        InitializeMesh();
    }

    private void Start()
    {
        // 3. RenderTexture初期化
        SetupRenderTexture();

        // 4. メッシュの位置合わせ
        FitToFrustum();
    }

    #region Public Methods

    /// <summary>
    /// 現在のカメラ映像をキャプチャしてテクスチャに設定
    /// </summary>
    public void CaptureCurrentScreen()
    {
        if (MainCamera == null)
        {
            Debug.LogError("メインカメラが見つかりません");
            return;
        }

        if (uiCamera == null)
        {
            Debug.LogError("UIカメラが見つかりません");
            return;
        }

        // 解像度チェック
        CheckAndRebuildRenderTexture();

        // 【WebGPU対策】MainCameraで直接レンダリングすると投影行列が歪むため、
        // uiCameraにMainCameraの設定をコピーして使用する

        // 1. すべてのCanvasを取得し、MainCameraを参照しているものをリストアップ
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        System.Collections.Generic.List<Canvas> canvasesToSwitch = new System.Collections.Generic.List<Canvas>();

        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == MainCamera)
            {
                canvasesToSwitch.Add(canvas);
            }
        }

        // 2. uiCameraの設定をMainCameraと完全に同期
        uiCamera.CopyFrom(MainCamera);

        // 3. CanvasのカメラをuiCameraに一時的に切り替え
        foreach (Canvas canvas in canvasesToSwitch)
        {
            canvas.worldCamera = uiCamera;
        }

        // 3-2. Canvas のレイアウトを強制的に再計算（WebGPU対策）
        Canvas.ForceUpdateCanvases();

        // 4. uiCameraでレンダリング
        RenderTexture currentRT = RenderTexture.active;

        uiCamera.targetTexture = uiRenderTexture;
        uiCamera.Render();
        uiCamera.targetTexture = null;

        RenderTexture.active = currentRT;

        // 5. Canvasを元に戻す
        foreach (Canvas canvas in canvasesToSwitch)
        {
            canvas.worldCamera = MainCamera;
        }

        // マテリアルに設定
        if (pageMaterial != null)
        {
            pageMaterial.SetTexture(MainTexProp, uiRenderTexture);
        }

        // 個別に生成されたページマテリアルにも反映
        if (rapidPageMaterials != null && rapidPageMaterials.Length > 0)
        {
            // 1枚目は必ずキャプチャ画像を使用するため更新
            if (rapidPageMaterials[0] != null)
            {
                rapidPageMaterials[0].SetTexture(MainTexProp, uiRenderTexture);
            }

            // 中間テクスチャが設定されていないページも更新（フォールバック用）
            for (int i = 1; i < rapidPageMaterials.Length; i++)
            {
                if (rapidPageMaterials[i] != null && (intermediatePageTextures == null || i - 1 >= intermediatePageTextures.Length))
                {
                    rapidPageMaterials[i].SetTexture(MainTexProp, uiRenderTexture);
                }
            }
        }
    }

    /// <summary>
    /// 外部からテクスチャを設定
    /// </summary>
    public void SetPageTexture(Texture texture)
    {
        if (pageMaterial != null && texture != null)
        {
            pageMaterial.SetTexture(MainTexProp, texture);
        }
    }

    /// <summary>
    /// カメラに合わせて位置を再設定（シーン遷移後に呼び出す）
    /// </summary>
    public void RefreshPosition()
    {
        FitToFrustum();

        // パラパラページがあれば遠近法補正も再計算
        if (rapidPageObjects != null)
        {
            int lastIndex = rapidPageObjects.Length - 1;

            for (int i = 0; i < rapidPageObjects.Length; i++)
            {
                if (rapidPageObjects[i] != null)
                {
                    // 2ページ目以降は少し大きめに表示（端が見えないように）
                    if (i > 0)
                    {
                        rapidPageObjects[i].transform.localPosition = Vector3.zero;
                        rapidPageObjects[i].transform.localScale = new Vector3(1.02f, 1.02f, 1f);
                    }
                    else
                    {
                        // 1ページ目は通常サイズ
                        rapidPageObjects[i].transform.localPosition = Vector3.zero;
                        rapidPageObjects[i].transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }

    /// <summary>
    /// メッシュの表示/非表示を設定
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = visible;
        }
    }


    /// <summary>
    /// パラパラめくり（高速で複数ページを重ねてめくる）
    /// </summary>
    /// <param name="pageCount">ページ数（-1でデフォルト）</param>
    /// <param name="keepLastPage">最後の1枚を残すか</param>
    /// <param name="token">キャンセレーショントークン</param>
    public async UniTask FlipRapidAsync(int pageCount = -1, bool keepLastPage = false, CancellationToken token = default)
    {
        if (pageCount < 0) pageCount = rapidFlipPageCount;
        if (isFlipping) return;

        isFlipping = true;

        try
        {
            // 元のメッシュは非表示
            if (meshRenderer != null) meshRenderer.enabled = false;

            // 先に親の位置・スケールを設定
            FitToFrustum();

            // 複数ページ用のオブジェクトを作成
            CreateRapidPageObjects(pageCount);

            // 各ページの開始時間と速度を事前に計算（加速あり）
            float[] pageStartTimes = new float[pageCount];
            float[] pageDurations = new float[pageCount];
            float currentStartTime = 0f;

            for (int i = 0; i < pageCount; i++)
            {
                // ページ位置に応じた加速（最初はゆっくり、後半は速く）
                float progress = (float)i / (pageCount - 1);
                float accelerationFactor = Mathf.Lerp(startSpeedMultiplier, endSpeedMultiplier, progress);

                pageDurations[i] = rapidFlipDuration * accelerationFactor;
                pageStartTimes[i] = currentStartTime;

                // 次のページの開始時間（オーバーラップも加速に合わせて調整）
                float overlapFactor = Mathf.Lerp(0.5f, 0.8f, progress); // 後半はよりオーバーラップ
                currentStartTime += pageDurations[i] * (1f - overlapFactor);
            }

            // 総アニメーション時間（最後の1枚を残す場合は最後の1ページのアニメは不要）
            int lastAnimatedPage = keepLastPage ? pageCount - 2 : pageCount - 1;
            float totalDuration = lastAnimatedPage >= 0
                ? pageStartTimes[lastAnimatedPage] + pageDurations[lastAnimatedPage]
                : 0f;
            float elapsedTime = 0f;

            // SE再生済みフラグ
            bool[] pageSoundPlayed = new bool[pageCount];

            while (elapsedTime < totalDuration)
            {
                elapsedTime += Time.deltaTime;

                // 各ページのカール量を個別に更新
                for (int i = 0; i < pageCount; i++)
                {
                    // 最後のページを残す場合、最後のページはアニメーションしない
                    if (keepLastPage && i == pageCount - 1) continue;

                    if (elapsedTime >= pageStartTimes[i])
                    {
                        // SE再生（まだ鳴らしていない場合）
                        if (!pageSoundPlayed[i])
                        {
                            // ページごとに鳴らすか、または最初の1回だけ鳴らすか
                            bool shouldPlay = playSoundPerPage || i == 0;
                            if (shouldPlay && SoundManager.instance != null)
                            {
                                SoundManager.instance.Play(pageFlipSEName);
                            }

                            pageSoundPlayed[i] = true;
                        }

                        float pageElapsed = elapsedTime - pageStartTimes[i];
                        float t = Mathf.Clamp01(pageElapsed / pageDurations[i]);

                        // 加速するイージング
                        float easedT = t * t;
                        float curlAmount = Mathf.Lerp(0f, 1f, easedT);

                        rapidPageMaterials[i].SetFloat(CurlAmountProp, curlAmount);

                        // ページが完全にめくれたら非表示（もう見えない）
                        if (curlAmount >= 0.95f)
                        {
                            rapidPageRenderers[i].enabled = false;
                        }
                    }
                }

                await UniTask.Yield(token);
            }

            // 最後のページを残さない場合は全ページを破棄
            if (!keepLastPage)
            {
                DestroyRapidPageObjects();
            }

            OnAllFlipsComplete?.Invoke();
        }
        finally
        {
            isFlipping = false;
        }
    }

    /// <summary>
    /// パラパラめくり用ページをクリーンアップ（外部から呼び出し用）
    /// </summary>
    public void CleanupRapidPages()
    {
        DestroyRapidPageObjects();
    }

    /// <summary>
    /// 最後のページをマスクディゾルブで消す（筆で塗られたような演出）
    /// </summary>
    public async UniTask MaskDissolveLastPageAsync(CancellationToken token = default)
    {
        // 最後のページがなければ何もしない
        if (rapidPageMaterials == null || rapidPageMaterials.Length == 0) return;

        int lastIndex = rapidPageMaterials.Length - 1;
        Material lastPageMat = rapidPageMaterials[lastIndex];
        if (lastPageMat == null) return;

        // SE再生
        if (!string.IsNullOrEmpty(maskDissolveSEName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(maskDissolveSEName);
        }

        // マスクテクスチャを設定
        if (maskTexture != null)
        {
            lastPageMat.SetTexture(MaskTexProp, maskTexture);
        }

        lastPageMat.SetFloat(UseMaskProp, 1f);
        lastPageMat.SetFloat(MaskThresholdProp, 1f);

        float elapsedTime = 0f;

        while (elapsedTime < maskDissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / maskDissolveDuration);

            // しきい値を1から0へ（黒い部分から徐々に消える）
            lastPageMat.SetFloat(MaskThresholdProp, 1f - t);


            await UniTask.Yield(token);
        }

        // 完全に消す
        lastPageMat.SetFloat(MaskThresholdProp, 0f);

        // クリーンアップ
        DestroyRapidPageObjects();
    }

    /// <summary>
    /// パラパラめくり用の複数ページオブジェクトを作成
    /// </summary>
    private void CreateRapidPageObjects(int pageCount)
    {
        DestroyRapidPageObjects(); // 既存を破棄

        rapidPageObjects = new GameObject[pageCount];
        rapidPageMaterials = new Material[pageCount];
        rapidPageRenderers = new MeshRenderer[pageCount];

        for (int i = 0; i < pageCount; i++)
        {
            // 新しいページオブジェクトを作成
            GameObject pageObj = new GameObject($"RapidPage_{i}");
            pageObj.transform.SetParent(transform, false);
            pageObj.transform.localPosition = Vector3.zero;
            pageObj.transform.localRotation = Quaternion.identity;
            pageObj.transform.localScale = Vector3.one;

            // メッシュフィルターを追加し、共有メッシュを設定
            MeshFilter filter = pageObj.AddComponent<MeshFilter>();
            filter.sharedMesh = sharedPageMesh;

            // メッシュレンダラーを追加し、個別のマテリアルインスタンスを作成
            MeshRenderer renderer = pageObj.AddComponent<MeshRenderer>();
            Material mat = new Material(pageMaterial);

            // テクスチャを設定（1枚目はキャプチャ画像、2枚目以降は中間テクスチャ）
            if (i == 0)
            {
                // 1枚目：キャプチャした画面
                mat.SetTexture(MainTexProp, uiRenderTexture);
            }
            else if (intermediatePageTextures != null && i - 1 < intermediatePageTextures.Length && intermediatePageTextures[i - 1] != null)
            {
                // 2枚目以降：中間ページテクスチャ
                mat.SetTexture(MainTexProp, intermediatePageTextures[i - 1]);
            }
            else
            {
                // テクスチャが足りない場合はキャプチャ画像を使用
                mat.SetTexture(MainTexProp, uiRenderTexture);
            }

            mat.SetFloat(CurlAmountProp, 0f);
            mat.SetFloat(UseMaskProp, 0f); // マスクを無効化（ディゾルブ時のみ有効にする）

            renderer.material = mat;
            renderer.enabled = true; // 最初から全ページを表示（重なって見える）

            // Z軸で少しずらして重なりを防止
            float zOffsetLocal = 0.01f * i;
            pageObj.transform.localPosition = new Vector3(0, 0, zOffsetLocal);

            // 遠近法補正：カメラの視野角と距離から正確に計算
            // ワールド空間でのZオフセット = ローカルオフセット * 親のZスケール
            float worldZOffset = zOffsetLocal * transform.lossyScale.z;

            // 遠近法補正: カメラからの距離に応じたスケール
            // 元の距離: distanceFromCamera
            // 新しい距離: distanceFromCamera + worldZOffset
            float perspectiveScale = (distanceFromCamera + worldZOffset) / distanceFromCamera;
            pageObj.transform.localScale = new Vector3(perspectiveScale, perspectiveScale, 1f);

            rapidPageObjects[i] = pageObj;
            rapidPageMaterials[i] = mat;
            rapidPageRenderers[i] = renderer;
        }
    }

    /// <summary>
    /// パラパラめくり用ページオブジェクトを破棄
    /// </summary>
    private void DestroyRapidPageObjects()
    {
        if (rapidPageObjects != null)
        {
            for (int i = 0; i < rapidPageObjects.Length; i++)
            {
                if (rapidPageMaterials != null && rapidPageMaterials[i] != null)
                {
                    Destroy(rapidPageMaterials[i]);
                }

                if (rapidPageObjects[i] != null)
                {
                    Destroy(rapidPageObjects[i]);
                }
            }

            rapidPageObjects = null;
            rapidPageMaterials = null;
            rapidPageRenderers = null;
        }
    }

    /// <summary>
    /// カメラのビューに合わせてメッシュサイズを調整
    /// </summary>
    public void FitToFrustum()
    {
        Camera cam = MainCamera;
        if (cam == null) return;

        // カメラの前に配置
        transform.position = cam.transform.position + cam.transform.forward * distanceFromCamera;
        transform.rotation = cam.transform.rotation;

        // 視推台（Frustum）に合わせてサイズ計算
        float frustumHeight = 2.0f * distanceFromCamera * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * cam.aspect;

        // XとZのスケールを合わせて円の歪みを防止
        transform.localScale = new Vector3(frustumWidth, frustumHeight, frustumWidth);
    }

    #endregion

    #region Private Methods

    private void SetupRenderTexture()
    {
        // 画面サイズに合わせて生成
        int w = Mathf.Max(Screen.width, 1);
        int h = Mathf.Max(Screen.height, 1);

        uiRenderTexture = new RenderTexture(w, h, 24);
        uiRenderTexture.useMipMap = false; // Mipmapは不要（ぼやけの原因になる）
        uiRenderTexture.filterMode = FilterMode.Bilinear;
        uiRenderTexture.name = "PageFlipTexture";
        uiRenderTexture.Create();

        // UIカメラにセット
        if (uiCamera != null)
        {
            uiCamera.targetTexture = uiRenderTexture;
            uiCamera.enabled = false; // Render()呼び出し時のみ動く
        }

        // マテリアルにセット
        if (pageMaterial != null)
        {
            pageMaterial.SetTexture(MainTexProp, uiRenderTexture);
        }
    }

    private void CheckAndRebuildRenderTexture()
    {
        if (uiRenderTexture == null || uiRenderTexture.width != Screen.width || uiRenderTexture.height != Screen.height)
        {
            if (uiRenderTexture != null)
            {
                uiRenderTexture.Release();
                Destroy(uiRenderTexture);
            }

            SetupRenderTexture();
        }
    }

    private void InitializeMesh()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.name = "DynamicPageMesh";

        int xRes = meshResolutionX;
        int yRes = meshResolutionY;

        // 頂点数チェック
        if ((xRes + 1) * (yRes + 1) > 65000)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        Vector3[] vertices = new Vector3[(xRes + 1) * (yRes + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[xRes * yRes * 6];

        for (int y = 0; y <= yRes; y++)
        {
            for (int x = 0; x <= xRes; x++)
            {
                int i = y * (xRes + 1) + x;
                float xPos = (float)x / xRes;
                float yPos = (float)y / yRes;

                vertices[i] = new Vector3(xPos - 0.5f, yPos - 0.5f, 0);
                uvs[i] = new Vector2(xPos, yPos);
            }
        }

        int triIndex = 0;
        for (int y = 0; y < yRes; y++)
        {
            for (int x = 0; x < xRes; x++)
            {
                int i = y * (xRes + 1) + x;

                triangles[triIndex] = i;
                triangles[triIndex + 1] = i + xRes + 1;
                triangles[triIndex + 2] = i + 1;

                triangles[triIndex + 3] = i + 1;
                triangles[triIndex + 4] = i + xRes + 1;
                triangles[triIndex + 5] = i + xRes + 2;

                triIndex += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.mesh = mesh;
        sharedPageMesh = mesh; // 共有メッシュとして保存
    }

    private void OnDestroy()
    {
        // クリーンアップ
        DestroyRapidPageObjects();

        if (uiRenderTexture != null)
        {
            if (uiCamera != null) uiCamera.targetTexture = null;
            uiRenderTexture.Release();
            Destroy(uiRenderTexture);
        }
    }

    #endregion
}