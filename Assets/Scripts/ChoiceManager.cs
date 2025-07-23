using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChoiceManager : MonoBehaviour
{
    [Header("选项面板")]
    public GameObject choicePanel;

    [Header("问题文本")]
    public Text questionText;

    [Header("选项按钮预制体")]
    [Tooltip("必须带Text组件")]
    public Button choiceButtonPrefab;

    [Header("所有选项按钮父节点")]
    [Tooltip("可挂载GridLayoutGroup和ContentSizeFilter让选项自适应")]
    public Transform choicesContainer;
    
    private List<Button> currentChoiceButtons = new List<Button>();
    
    /** 显示选项 */
    public void ShowChoices(ChoiceNode node)
    {
        choicePanel.SetActive(true);

        // 清除旧的选择按钮
        foreach (var button in currentChoiceButtons)
        {
            Destroy(button);
        }
        currentChoiceButtons.Clear();

        // 设置问题文字
        questionText.text = node.questionText;

        // 创建新的选择按钮
        for (int i = 0; i < node.choices.Count; i++)
        {
            int index = i; // 闭包捕获
            var choice = node.choices[i];

            // 动态生成按钮物体
            var buttonObj = Instantiate<Button>(choiceButtonPrefab, choicesContainer);
            var button = buttonObj.GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = choice.choiceText;

            button.onClick.AddListener(() =>
            {
                GameManager.Instance.SelectChoice(index);
            });

            currentChoiceButtons.Add(buttonObj);
        }
    }
    
    /** 隐藏选项 */
    public void HideChoices()
    {
        choicePanel.SetActive(false);
    }
}