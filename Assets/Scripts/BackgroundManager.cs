using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    [Header("背景图片")]
    public Image backgroundImage;

    [Header("背景图片文件夹")]
    public string backgroundsFolder = "Sprites/BGs"; // Resources下的背景文件夹路径
    
    public void SetBackground(string backgroundId)
    {
        string path = $"{backgroundsFolder}/{backgroundId}";

        Sprite bgSprite = Resources.Load<Sprite>(path);
        if (bgSprite != null) {
            backgroundImage.sprite = bgSprite;
        } else {
            Debug.LogError($"背景图片加载失败: {path}");
        }
    }
}