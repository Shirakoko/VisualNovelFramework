using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ProcessedManager : MonoBehaviour
{
    [Header("剧情回顾面板")]
    public GameObject processedPanel;

    [Header("打开剧情回顾的按钮")]
    public Button showProcessedButton;

    [Header("关闭剧情回顾的按钮")]
    public Button hideProcessedButton;

    [Header("单条对话预制体")]
    [Tooltip("必须带有两个Text，分别显示对话人和对话内容")]
    public SingleDialogItem singleDialogPrefab;

    [Header("所有对话的父节点")]
    [Tooltip("建议挂载GridLayoutGroup和ContentSizeFilter让对话条目自适应")]
    public Transform dialogsContainer;

    private List<SingleDialogItem> currentSingleDialogs = new List<SingleDialogItem>();

    private void Start()
    {
        showProcessedButton.onClick.AddListener(GameManager.Instance.ShowProcessedPanel);
        hideProcessedButton.onClick.AddListener(this.HideProcessedDialogs);
    }

    /** 显示剧情回顾 */
    public void ShowProcessedDialogs(List<(string Speaker, string Content)> processedDialogs)
    {
        processedPanel.SetActive(true);

        // 清除旧的对话条目
        foreach (var dialog in currentSingleDialogs)
        {
            Destroy(dialog.gameObject);
        }
        currentSingleDialogs.Clear();

        // 创建新的对话条目
        for (int i = 0; i < processedDialogs.Count; i++)
        {
            var dialog = processedDialogs[i];

            // 动态生成对话条目物体
            var singleDialogItem = Instantiate(singleDialogPrefab, dialogsContainer);
            singleDialogItem.SetSpeaker(dialog.Speaker);
            singleDialogItem.SetContent(dialog.Content);

            currentSingleDialogs.Add(singleDialogItem);
        }
    }

    /** 关闭剧情回顾 */
    public void HideProcessedDialogs()
    {
        processedPanel.SetActive(false);
    }
}