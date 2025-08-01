using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaveSlot : MonoBehaviour
{
    [Header("存档预览背景图")]
    [SerializeField]
    public Image previewImage;

    [Header("存档预览文字")]
    [SerializeField]
    public Text previewText;

    [Header("存档时间文字")]
    [SerializeField]
    public Text timeText;

    private Button button;
    public Button Button => button;
    void Awake()
    {
        this.button = GetComponent<Button>();
    }
}