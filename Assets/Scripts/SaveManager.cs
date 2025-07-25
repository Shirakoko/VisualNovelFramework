using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogRecord
{
    public string Speaker;
    public string Content;

    public DialogRecord(string speaker, string content)
    {
        Speaker = speaker;
        Content = content;
    }
}

[System.Serializable]
public class SaveData
{
    public string nodeId; // 节点Id
    public int dialogIndex; // 对话索引
    public string saveTime; // 存档时间
    public string previewText; // 存档预览文本
    public List<DialogRecord> processedDialogs; // 已处理的对话历史
}


public class SaveManager : MonoBehaviour
{
    [Header("打开输出存档面板的按钮")]
    public Button showOutputSaveButton;

    [Header("打开读取存档面板的按钮")]
    public Button showInputSaveButton;

    [Header("关闭读取存档面板的按钮")]
    public Button hideInputSaveButton;

    [Header("存档面板")]
    public GameObject savePanel;

    private bool isInput; // 是否是读取模式

    private const int MAX_SAVE_COUNT = 5; // 存档槽个数

    [Header("存档槽按钮")]
    [SerializeField, Tooltip("不可修改大小")]
    private Button[] saveButtons = new Button[MAX_SAVE_COUNT]; // 存档按钮

    [Header("存档预览背景图")]
    [SerializeField]
    private Image[] previewImages = new Image[MAX_SAVE_COUNT]; // 存档预览背景图

    [Header("存档预览文字")]
    [SerializeField]
    private Text[] preViewTexts = new Text[MAX_SAVE_COUNT]; // 存档预览文字

    [Header("存档时间文字")]
    [SerializeField]
    private Text[] saveTimes = new Text[MAX_SAVE_COUNT]; // 存档时间

    [Header("截图尺寸设置")]
    [SerializeField]
    private int screenshotWidth = 256;
    [SerializeField]
    private int screenshotHeight = 144;

    private Texture2D pendingScreenshot; // 临时截图纹理

    // 固定截图路径和命名规则
    private string ScreenshotPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slotIndex}.png");
    }

    private const string NONESAVE_ID = "BG_NoneSave";

    private void Start()
    {
        Debug.Log("持久化数据路径: " + Application.persistentDataPath);

        showInputSaveButton.onClick.AddListener(() => { this.OnSaveButtonClicked(true); });
        showOutputSaveButton.onClick.AddListener(() => { this.OnSaveButtonClicked(false); });
        hideInputSaveButton.onClick.AddListener(this.HideSavePanel);

        for (int i = 0; i < MAX_SAVE_COUNT; i++)
        {
            int index = i; // 闭包问题
            saveButtons[i].onClick.AddListener(() => { this.OnSaveSlotClicked(index); });
        }

        UpdateSaveUI();
    }

    /** 显示存档面板 */
    private void ShowSavePanel()
    {
        savePanel.SetActive(true);
        UpdateSaveUI();
        //TODO 根据isInput高亮指定页签
    }

    /** 隐藏存档面板 */
    public void HideSavePanel()
    {
        // 删除临时资源
        Destroy(pendingScreenshot);
        pendingScreenshot = null;
        savePanel.SetActive(false);
    }

    #region 点击打开存档按钮截图
    public void OnSaveButtonClicked(bool isInput)
    {
        this.isInput = isInput;
        StartCoroutine(CaptureBeforeSavePanel());
    }

    private IEnumerator CaptureBeforeSavePanel()
    {
        // 等待一帧确保UI渲染完成
        yield return new WaitForEndOfFrame();

        // 截取当前游戏画面
        pendingScreenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        pendingScreenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        pendingScreenshot.Apply();

        // 打开存档界面
        ShowSavePanel();
    }
    #endregion

    /** 存档槽点击事件 */
    private void OnSaveSlotClicked(int index)
    {
        if (this.isInput)
        {
            LoadGame(index);
        }
        else
        {
            SaveGame(index);
            // 删除临时资源
            Destroy(pendingScreenshot);
            pendingScreenshot = null;
        }
    }

    /** 更新存档界面UI */
    private void UpdateSaveUI()
    {
        for (int i = 0; i < MAX_SAVE_COUNT; i++)
        {
            SaveData data = LoadSaveData(i);
            string screenshotPath = ScreenshotPath(i);

            if (data != null && File.Exists(screenshotPath))
            {
                // 有存档且截图文件存在
                byte[] fileData = File.ReadAllBytes(screenshotPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                previewImages[i].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

                preViewTexts[i].text = data.previewText;
                saveTimes[i].text = data.saveTime;
            }
            else if (data != null)
            {
                // 有存档但截图不存在（使用NONESAVE_ID图片）
                previewImages[i].sprite = GameManager.Instance.GetBGSpriteById(NONESAVE_ID);
                preViewTexts[i].text = data.previewText;
                saveTimes[i].text = data.saveTime;
            }
            else
            {
                // 无存档
                previewImages[i].sprite = GameManager.Instance.GetBGSpriteById(NONESAVE_ID);
                preViewTexts[i].text = "空存档";
                saveTimes[i].text = "";
            }
        }
    }

    /** 反序列化存档数据 */
    private SaveData LoadSaveData(int slotIndex)
    {
        string jsonData = PlayerPrefs.GetString($"Save_{slotIndex}", "");
        if (!string.IsNullOrEmpty(jsonData))
        {
            return JsonUtility.FromJson<SaveData>(jsonData);
        }
        return null;
    }


    /** 保存存档到指定槽位 */
    public void SaveGame(int slotIndex)
    {
        // 缩放截图
        Texture2D scaledScreenshot = ScaleTexture(pendingScreenshot, screenshotWidth, screenshotHeight);

        // 保存截图文件
        byte[] bytes = scaledScreenshot.EncodeToPNG();
        string screenshotPath = ScreenshotPath(slotIndex);
        File.WriteAllBytes(screenshotPath, bytes);

        var currentNode = GameManager.Instance.CurrentNode;
        var currentIndex = GameManager.Instance.CurrentDialogIndex;

        // 生成其他数据
        SaveData data = new SaveData
        {
            nodeId = currentNode.nodeId,
            dialogIndex = currentIndex,
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            processedDialogs = DialogDataConverter.ToDialogRecords(GameManager.Instance.processedDialogs)
        };

        // 生成预览文本
        if (currentNode is DialogNode dialogNode)
        {
            var dialog = dialogNode.dialogs[Mathf.Min(currentIndex, dialogNode.dialogs.Count - 1)];
            data.previewText = $"{dialog.speakerDisplayName}: {ShortenText(dialog.content, 22)}";
        }
        else if (currentNode is ChoiceNode choiceNode)
        {
            data.previewText = $"面临选择: {ShortenText(choiceNode.questionText, 22)}";
        }

        string jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString($"Save_{slotIndex}", jsonData);
        PlayerPrefs.Save();

        UpdateSaveUI();
    }

    /** 加载指定槽位的存档 */
    public void LoadGame(int slotIndex)
    {
        SaveData data = LoadSaveData(slotIndex);
        if (data != null)
        {
            GameManager.Instance.processedDialogs = DialogDataConverter.ToTuples(data.processedDialogs);

            var node = GameManager.Instance.CurrentStory.GetNodeById(data.nodeId);
            if (node != null)
            {
                GameManager.Instance.ProcessNode(node, data.dialogIndex);
            }

            HideSavePanel();
        }
    }

    /** 超过指定字数的文字将被截断，末尾加上... */
    private string ShortenText(string text, int maxLength)
    {
        if (text.Length <= maxLength) return text;
        return text.Substring(0, maxLength) + "...";
    }

    /** 缩放纹理 */
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);

        // 对结果的每个像素用双线性差值进行重采样
        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }

        result.Apply();
        return result;
    }
}

public static class DialogDataConverter
{
    /** 将元组列表转换为DialogRecord列表 */
    public static List<DialogRecord> ToDialogRecords(List<(string Speaker, string Content)> tuples)
    {
        return tuples.Select(t => new DialogRecord(t.Speaker, t.Content)).ToList();
    }

    /** 将DialogRecord列表转换为元组列表 */
    public static List<(string Speaker, string Content)> ToTuples(List<DialogRecord> records)
    {
        return records.Select(r => (r.Speaker, r.Content)).ToList();
    }
}
