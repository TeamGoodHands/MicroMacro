using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private Button titleButton;

    private void Start()
    {
        titleButton.onClick.AddListener(OnTitleButtonClicked);
    }

    private void OnTitleButtonClicked()
    {
        
    }
    
    private void OnDestroy()
    {
        titleButton.onClick.RemoveListener(OnTitleButtonClicked);
    }
}
