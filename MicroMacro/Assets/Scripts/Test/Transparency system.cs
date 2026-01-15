using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transparencysystem : MonoBehaviour
{
    [SerializeField] private Transform player; //プレイヤーの取得
    [SerializeField] private LayerMask FadeLayer;// 実行レイヤーを設定
    [SerializeField] private string fadeTag = "FadeObj";//隠すべきもののタグを設定
    [SerializeField] private float transparentAlpha = 0.1f; // 半透明にする際のアルファ値（0 = 完全透明, 1 = 不透明）
    [SerializeField] private float fadeDuration = 1.0f; // フェードにかける時間（秒）
    private HashSet<Renderer> fadingRenderers = new HashSet<Renderer>();//フェード中のレンダラーを追跡
    private Renderer currentRenderer = null; // 現在半透明にしているオブジェクトの格納場所
    private Material[] originalMaterials = null; // 元のマテリアルの保存場所

    void Update()
    {   //目標から始点を引いて目指す向きを決める
        Vector3 _Playerps = (player.transform.position - this.transform.position);
        //Reyの強さを数値化
        float ReyMG = _Playerps.magnitude;
        //Rayを可視化
        Debug.DrawRay(this.transform.position, _Playerps, Color.red, 1);
　　　  // Raycastでカメラとプレイヤーの間にあるオブジェクトを検出
        if (Physics.Raycast(transform.position, _Playerps.normalized, out RaycastHit hit, ReyMG, FadeLayer))
        {
            // タグが一致するオブジェクトのみ対象
            if (hit.collider.CompareTag(fadeTag))
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();//接触しているオブジェクトのコライダーを取得

                // 新しいオブジェクトに当たった場合のみ処理
                if (rend != null && rend != currentRenderer)
                {
                    Debug.Log("透かし実行");
                    if (currentRenderer != null) 
                    {
                        StartCoroutine(MakeTransparent(currentRenderer, 1.0f, resetAfter: true)); 
                    } 
                    // 前のオブジェクトを元に戻す 
                    Material[] mats = rend.materials;
                    originalMaterials = new Material[mats.Length];
                    for (int i = 0; i < mats.Length; i++)
                    {
                        originalMaterials[i] = new Material(mats[i]); // 新しいインスタンスを作成
                    }
                    currentRenderer = rend;
                    StartCoroutine(MakeTransparent(rend, transparentAlpha));//透明化処理
                }
                return;//ここでおわり
            }
        }

        //何もなかったら戻す。
        if (currentRenderer != null) 
        {
            StartCoroutine(MakeTransparent(currentRenderer, 1.0f, resetAfter: true)); 
            currentRenderer = null;
            originalMaterials = null; 
        }
        Debug.Log("感知ナシ");
    }

    IEnumerator MakeTransparent(Renderer rend, float targetAlpha, bool resetAfter = false)//透明化処理
    {
        // すでにフェード中ならスキップ
        if (fadingRenderers.Contains(rend)) yield break; 
        fadingRenderers.Add(rend); // 描画モードをTransparentに設定
        foreach (Material mat in rend.materials) 
        { 
            mat.SetFloat("_Mode", 3); 
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha); 
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); 
            mat.SetInt("_ZWrite", 0); mat.DisableKeyword("_ALPHATEST_ON"); 
            mat.EnableKeyword("_ALPHABLEND_ON"); 
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON"); 
            mat.renderQueue = 3000; 
        }
        float elapsed = 0f; float startAlpha = rend.material.color.a; 
        while (elapsed < fadeDuration) 
        {
            float t = elapsed / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t); 
            foreach (Material mat in rend.materials) 
            {
                Color color = mat.color; color.a = newAlpha; mat.color = color; 
            }
            elapsed += Time.deltaTime; yield return null; 
        } 
        // 最終的なアルファを設定
        foreach (Material mat in rend.materials) 
        {
            Color color = mat.color; color.a = targetAlpha; mat.color = color; 
        } 
        // 完全に元に戻した場合、マテリアルを復元
        if (resetAfter && originalMaterials != null) 
        {
            rend.materials = originalMaterials;
        }
        fadingRenderers.Remove(rend); 
    }
    
}
