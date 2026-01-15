using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transparencysystem : MonoBehaviour
{
    [SerializeField] private Transform player; //�v���C���[�̎擾
    [SerializeField] private LayerMask FadeLayer;// ���s���C���[��ݒ�
    [SerializeField] private string fadeTag = "FadeObj";//�B���ׂ����̂̃^�O��ݒ�
    [SerializeField] private float transparentAlpha = 0.1f; // �������ɂ���ۂ̃A���t�@�l�i0 = ���S����, 1 = �s�����j
    [SerializeField] private float fadeDuration = 1.0f; // �t�F�[�h�ɂ����鎞�ԁi�b�j
    private HashSet<Renderer> fadingRenderers = new HashSet<Renderer>();//�t�F�[�h���̃����_���[��ǐ�
    private Renderer currentRenderer = null; // ���ݔ������ɂ��Ă���I�u�W�F�N�g�̊i�[�ꏊ
    private Material[] originalMaterials = null; // ���̃}�e���A���̕ۑ��ꏊ

    void Update()
    {   //�ڕW����n�_�������Ėڎw�����������߂�
        Vector3 _Playerps = (player.transform.position - this.transform.position);
        //Rey�̋����𐔒l��
        float ReyMG = _Playerps.magnitude;
        //Ray������
        Debug.DrawRay(this.transform.position, _Playerps, Color.red, 1); 
        // Raycast�ŃJ�����ƃv���C���[�̊Ԃɂ���I�u�W�F�N�g�����o
        if (Physics.Raycast(transform.position, _Playerps.normalized, out RaycastHit hit, ReyMG, FadeLayer))
        {
            // �^�O����v����I�u�W�F�N�g�̂ݑΏ�
            if (hit.collider.CompareTag(fadeTag))
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();//�ڐG���Ă���I�u�W�F�N�g�̃R���C�_�[���擾

                // �V�����I�u�W�F�N�g�ɓ��������ꍇ�̂ݏ���
                if (rend != null && rend != currentRenderer)
                {
                    Debug.Log("���������s");
                    if (currentRenderer != null) 
                    {
                        StartCoroutine(MakeTransparent(currentRenderer, 1.0f, resetAfter: true)); 
                    } 
                    // �O�̃I�u�W�F�N�g�����ɖ߂� 
                    Material[] mats = rend.materials;
                    originalMaterials = new Material[mats.Length];
                    for (int i = 0; i < mats.Length; i++)
                    {
                        originalMaterials[i] = new Material(mats[i]); // �V�����C���X�^���X���쐬
                    }
                    currentRenderer = rend;
                    StartCoroutine(MakeTransparent(rend, transparentAlpha));//����������
                }
                return;//�����ł����
            }
        }

        //�����Ȃ�������߂��B
        if (currentRenderer != null) 
        {
            StartCoroutine(MakeTransparent(currentRenderer, 1.0f, resetAfter: true)); 
            currentRenderer = null;
            originalMaterials = null; 
        }
    }

    IEnumerator MakeTransparent(Renderer rend, float targetAlpha, bool resetAfter = false)//����������
    {
        // ���łɃt�F�[�h���Ȃ�X�L�b�v
        if (fadingRenderers.Contains(rend)) yield break; 
        fadingRenderers.Add(rend); // �`�惂�[�h��Transparent�ɐݒ�
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
        // �ŏI�I�ȃA���t�@��ݒ�
        foreach (Material mat in rend.materials) 
        {
            Color color = mat.color; color.a = targetAlpha; mat.color = color; 
        } 
        // ���S�Ɍ��ɖ߂����ꍇ�A�}�e���A���𕜌�
        if (resetAfter && originalMaterials != null) 
        {
            rend.materials = originalMaterials;
        }
        fadingRenderers.Remove(rend); 
    }
    
}
