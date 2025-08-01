using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    [Header("面板提示文字")]
    [SerializeField]
    private Text informText;

    [Header("默认提示文字")]
    [SerializeField]
    private string defaultInformText;

    [Header("取消按钮")]
    [SerializeField]
    private Button cancelButton;
    public Button CancelButton => cancelButton;

    [Header("确认按钮")]
    [SerializeField]
    private Button confirmButton;
    public Button ConfirmButton => confirmButton;

    [Header("关闭按钮")]
    [SerializeField]
    private Button closeButton;

    void Awake()
    {
        this.informText.text = defaultInformText;

        this.closeButton.onClick.AddListener(this.HideConfirmPanel);
    }

    public void ShowConfirmPanel(string informText = null)
    {
        if (informText != null) {
            this.informText.text = informText;
        }
        this.gameObject.SetActive(true);
    }

    public void HideConfirmPanel()
    {
        this.gameObject.SetActive(false);
    }
}