using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    [Header("背景图片")]
    public Image backgroundImage;

    [Header("背景图片文件夹")]
    public string backgroundsFolder = "Sprites/BGs"; // Resources下的背景文件夹路径

    public Sprite GetBGSpriteById(string backgroundId)
    {
        string path = $"{backgroundsFolder}/{backgroundId}";
        Sprite bgSprite = Resources.Load<Sprite>(path);
        if (bgSprite == null)
        {
            Debug.LogError($"背景图片加载失败: {path}");
        }

        return bgSprite;
    }
    
    public void SetBackground(string backgroundId)
    {
        Sprite bgSprite = GetBGSpriteById(backgroundId);
        if (bgSprite != null)
        {
            backgroundImage.sprite = bgSprite;
        }
    }
}