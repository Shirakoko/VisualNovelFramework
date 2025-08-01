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

    [Header("删除存档按钮")]
    [SerializeField]
    private Button deleteButton;
    public Button DeleteButton => deleteButton;

    private Button button;
    public Button Button => button;

    void Awake()
    {
        this.button = GetComponent<Button>();
    }

    /** 设置删除存档的按钮是否可交互 */
    public void SetDeleteButtonInteractable(bool hasSave)
    {
        deleteButton.interactable = hasSave;
    }
}