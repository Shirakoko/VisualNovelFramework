using System;
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

    private Action onConfirm; // 确认按钮回调的委托
    private Action onCancel; // 取消按钮回调的委托

    void Awake()
    {
        this.informText.text = defaultInformText;

        this.closeButton.onClick.AddListener(this.HideConfirmPanel);
        this.cancelButton.onClick.AddListener(OnCancel);
        this.confirmButton.onClick.AddListener(OnConfirm);
    }

    public void ShowConfirmPanel(Action confirmAction, Action cancelAction, string informText = null)
    {
        if (informText != null)
        {
            this.informText.text = informText;
        }
        this.onConfirm = confirmAction;
        this.onCancel = cancelAction;
        this.gameObject.SetActive(true);
    }

    public void HideConfirmPanel()
    {
        this.onConfirm = null;
        this.onCancel = null;
        this.gameObject.SetActive(false);
    }
    
    public void OnConfirm()
    {
        onConfirm?.Invoke();
        HideConfirmPanel();
    }

    public void OnCancel()
    {
        onCancel?.Invoke();
        HideConfirmPanel();
    }
}