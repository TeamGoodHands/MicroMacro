using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class SplineHandleAnimator : MonoBehaviour
{
    [SerializeField] private SplineContainer targetContainer;

    [SerializeField, HideInInspector] private Transform handlesRoot;

    [SerializeField, HideInInspector] private List<Transform> knotHandles = new List<Transform>();

    // 無限ループ（ハンドル動かす→スプライン更新→検知→ハンドル動かす...）防止用フラグ
    private bool _isUpdatingHandles = false;
    private bool _isUpdatingSpline = false;

    private void OnEnable()
    {
        if (targetContainer == null)
            targetContainer = GetComponent<SplineContainer>();

        // Spline側の変更（ツールでの操作など）を監視します
        Spline.Changed += OnSplineChanged;
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    private void Update()
    {
        // アニメーション（ハンドル） -> Spline への同期
        SyncHandlesToSpline();
    }

    // Splineツールで操作した時に呼ばれます
    private void OnSplineChanged(Spline spline, int index, SplineModification modification)
    {
        // 自分がSplineに書き込んでいる最中なら無視（自作自演を防ぐ）
        if (_isUpdatingSpline) return;

        if (targetContainer == null || spline != targetContainer.Spline)
        {
            return;
        }

        // Spline -> ハンドル への同期を実行
        SyncSplineToHandles();
    }

    private void SyncHandlesToSpline()
    {
        if (targetContainer == null || knotHandles == null) return;
        Spline spline = targetContainer.Spline;
        if (spline.Count != knotHandles.Count) return;

        // Spline側からの更新中は書き込まない
        if (_isUpdatingHandles) return;

        _isUpdatingSpline = true;
        try
        {
            for (int i = 0; i < spline.Count; i++)
            {
                Transform handle = knotHandles[i];
                if (handle == null) continue;

                BezierKnot knot = spline[i];
                Vector3 localPos = handle.localPosition;

                // 位置がズレている場合のみ更新
                // ここで「Animationで動いたハンドル」の位置をSplineに適用します
                if (!knot.Position.Equals((Unity.Mathematics.float3)localPos))
                {
                    knot.Position = localPos;
                    spline[i] = knot;
                }
            }
        }
        finally
        {
            _isUpdatingSpline = false;
        }
    }

    private void SyncSplineToHandles()
    {
        if (targetContainer == null || knotHandles == null) return;
        Spline spline = targetContainer.Spline;

        // 数が合わない時は同期できません（Setupし直してください）
        if (spline.Count != knotHandles.Count) return;

        _isUpdatingHandles = true;
        try
        {
            for (int i = 0; i < spline.Count; i++)
            {
                Transform handle = knotHandles[i];
                if (handle == null) continue;

                BezierKnot knot = spline[i];

                // Splineツールで動かした位置を、ハンドルに反映させます
                if (handle.localPosition != (Vector3)knot.Position)
                {
                    handle.localPosition = knot.Position;
                }
            }
        }
        finally
        {
            _isUpdatingHandles = false;
        }
    }

    [ContextMenu("Setup Handles (Clean Mode)")]
    public void SetupHandles()
    {
        if (targetContainer == null)
            targetContainer = GetComponent<SplineContainer>();

        if (targetContainer == null)
        {
            Debug.LogError("SplineContainerが見つかりません。");
            return;
        }

        if (handlesRoot != null)
        {
            var children = new List<GameObject>();
            foreach (Transform child in handlesRoot) children.Add(child.gameObject);
            children.ForEach(c => DestroyImmediate(c));
        }
        else
        {
            GameObject rootObj = new GameObject("_SplineHandles_ (Don't Touch)");
            rootObj.transform.SetParent(this.transform, false);
            handlesRoot = rootObj.transform;
        }

        knotHandles.Clear();
        Spline spline = targetContainer.Spline;

        for (int i = 0; i < spline.Count; i++)
        {
            BezierKnot knot = spline[i];
            GameObject handle = new GameObject($"Knot_{i}");
            handle.transform.SetParent(handlesRoot, false);
            handle.transform.localPosition = knot.Position;
            knotHandles.Add(handle.transform);
        }

        Debug.Log("ハンドルを再生成しました。");
    }
}