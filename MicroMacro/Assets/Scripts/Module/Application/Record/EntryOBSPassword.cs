using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Application.Recoed
{
    public class EntryOBSPassword : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [Header("パスワードが確定している場合ここに入力")]
        [SerializeField] private string password;

        private void Awake()
        {
            if (inputField == null)
            {
                Debug.LogError("InputFieldがnullです。");
            }
        }

        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(password))
            {
                RecordManager.Instance.SetPasswordAsync(password);
            }
        }

        public void InputAndSend()
        {
            if (!string.IsNullOrWhiteSpace(inputField.text))
            {
                password = inputField.text;
                RecordManager.Instance.SetPasswordAsync(password);
                inputField.text = "";
                return;
            }
            Debug.LogWarning("パスワードの登録に失敗しました。");
        }
    }
}