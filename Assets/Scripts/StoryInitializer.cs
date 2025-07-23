using System.IO;
using UnityEngine;

public class StoryInitializer
{
    public StoryTree InitializeStory(string storyFilePath)
    {
        // 从StreamingAssets加载CSV
        // TextAsset csvFile = Resources.Load<TextAsset>(storyFilePath);
        // if (csvFile == null)
        // {
        //     Debug.LogError($"CSV文件未找到！请检查路径：Assets/Resources/{storyFilePath}.csv");
        //     return null;
        // }
        string path = Path.Combine(Application.streamingAssetsPath, storyFilePath);
        string csvText = File.ReadAllText(path);
        return StoryCsvParser.ParseCsvToStory(csvText);
    }

    #region 废弃的代码化创建方式
    /**
    StoryTree CreateSampleStory()
    {
        // 节点1: 灰色背景开场
        var node1 = new DialogNode
        {
            nodeId = "start",
            backgroundId = "BG_Grey",
            nextNodeId = "main_choice",
            characters = new List<CharacterState>
            {
                new CharacterState { characterId = "Ch_usagi", position = new Vector2(-800, -100) },
                new CharacterState { characterId = "Ch_chiikawa", position = new Vector2(800, -100) },
                new CharacterState { characterId = "Ch_hachi", position = new Vector2(-600, -100)}
            },
            dialogs = new List<DialogLine>
            {
                new DialogLine { speakerDisplayName = "兔子", content = "今天天气不错呢！" },
                new DialogLine { speakerDisplayName = "小可爱", content = "是啊，要一起去冒险吗？" }
            }
        };

        // 节点2: 红色背景的选择支
        var node2 = new ChoiceNode
        {
            nodeId = "main_choice",
            backgroundId = "BG_Red",
            questionText = "去做什么好呢？",
            characters = new List<CharacterState>
            {
                new CharacterState { characterId = "Ch_usagi", position = new Vector2(-200, 0) },
                new CharacterState { characterId = "Ch_chiikawa", position = new Vector2(0, 0)},
                new CharacterState { characterId = "Ch_hachi", position = new Vector2(200, 0) }
            },
            choices = new List<Choice>
            {
                new Choice { choiceText = "去森林探险", nextNodeId = "forest" },
                new Choice { choiceText = "回家休息", nextNodeId = "home" }
            }
        };

        // 节点: 森林路线
        var nodeForest = CreateForestPath();
        // 节点: 回家路线
        var nodeHome = CreateHomePath();



        // 构建故事树
        var story = new StoryTree { rootNode = node1 };
        story.AddNode(node1);
        story.AddNode(node2);
        story.AddNode(nodeForest);
        story.AddNode(nodeHome);

        return story;
    }

    // 森林路线
    private Node CreateForestPath()
    {
        return new DialogNode
        {
            nodeId = "forest",
            backgroundId = "BG_Forest",
            characters = new List<CharacterState>
            {
                new CharacterState { characterId = "Ch_usagi", position = new Vector2(-150, -50) },
                new CharacterState { characterId = "Ch_hachi", position = new Vector2(150, -50) }
            },
            dialogs = new List<DialogLine>
            {
                new DialogLine { speakerDisplayName = "小八", content = "森林里有好多蘑菇！" },
                new DialogLine { speakerDisplayName = "兔子", content = "小心有毒的蘑菇哦！" },
                new DialogLine { speakerDisplayName = "小八", content = "我们还是快点回家吧。" },
            }
        };
    }

    // 回家路线
    private Node CreateHomePath()
    {
        return new DialogNode
        {
            nodeId = "home",
            backgroundId = "BG_Home",
            characters = new List<CharacterState>
            {
                new CharacterState { characterId = "Ch_chiikawa", position = new Vector2(0, 0) }
            },
            dialogs = new List<DialogLine>
            {
                new DialogLine { speakerDisplayName = "小可爱", content = "还是家里最舒服了~" },
                new DialogLine { speakerDisplayName = "小可爱", content = "不知道小八和兔子什么时候回来呢。" }
            }
        };
    }
    */
    #endregion
}