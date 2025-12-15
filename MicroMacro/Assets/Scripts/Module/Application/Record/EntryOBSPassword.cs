using TMPro;
using UnityEngine;

namespace Module.Application.Record
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
                Debug.Log("パスワードを自動登録しました。");
            }
        }

        public void InputAndSend()
        {
            if (RecordController.IsConnected() || RecordController.IsInitialized)
            {
                Debug.LogWarning("既にOBSに接続されているため再度接続を試みます。");
                RecordController.OBSDisconnect();
                RecordController.PasswordReset();
            }
            
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