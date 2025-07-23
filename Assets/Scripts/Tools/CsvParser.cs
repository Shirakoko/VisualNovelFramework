using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class StoryCsvParser
{
    public static StoryTree ParseCsvToStory(string csvText)
    {
        var lines = csvText.Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line.Trim()))
            .ToList();

        var nodes = new Dictionary<string, Node>();
        Node rootNode = null;

        // 按节点分组（以#开头的行开始一个新的节点）
        List<List<string>> nodeBlocks = new List<List<string>>();
        // 当前节点包含的Csv行文本的合集
        List<string> currentBlock = null;

        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("#"))
            {
                currentBlock = new List<string>();
                nodeBlocks.Add(currentBlock);
            }
            if (currentBlock != null)
            {
                currentBlock.Add(line);
            }
        }

        Debug.LogWarning($"当前CSV拥有{nodeBlocks.Count}个故事节点");

        // 解析每个节点块
        foreach (var block in nodeBlocks)
        {
            var firstLineFields = ParseCsvLine(block[0]);
            if (firstLineFields.Count <= 5) continue;

            // 所有节点共有信息
            string nodeType = firstLineFields[1]; // 节点类型
            string nodeId = firstLineFields[2]; // 节点Id
            string backgroundId = firstLineFields[4]; // 节点背景图片
            var allCharacterFields = new List<(string characterId, string position)>();

            string nextNodeId = "";
            var allDialogFields = new List<(string speaker, string content)>();
            var allChoiceFields = new List<(string choiceText, string nextNodeId)>();
            string questionText = "";

            foreach (var line in block)
            {
                var fields = ParseCsvLine(line);

                if (fields.Count <= 5) continue;

                // 解析角色信息（索引5、6）
                if (fields.Count > 6 && !string.IsNullOrEmpty(fields[5]))
                {
                    allCharacterFields.Add((fields[5], fields[6]));
                }

                // 解析下一个节点Id（索引3）和对话信息（索引7、8），仅DialogNode
                if (nodeType == "DialogNode" && fields.Count > 8 && !string.IsNullOrEmpty(fields[7]))
                {
                    nextNodeId = firstLineFields[3];
                    allDialogFields.Add((fields[7], fields[8]));
                }

                // 解析问题文本（索引9）和选项（索引10、11），仅ChoiceNode专用
                if (nodeType == "ChoiceNode")
                {
                    if (fields.Count > 9 && !string.IsNullOrEmpty(fields[9]))
                    {
                        questionText = fields[9];
                    }

                    if (fields.Count > 11 && !string.IsNullOrEmpty(fields[10]))
                    {
                        allChoiceFields.Add((fields[10], fields[11]));
                    }
                }
            }

            // 构建节点
            Node node = null;
            switch (nodeType)
            {
                case "DialogNode":
                    node = new DialogNode
                    {
                        nodeId = nodeId,
                        nextNodeId = nextNodeId,
                        backgroundId = backgroundId,
                        characters = allCharacterFields.Select(c => new CharacterState
                        {
                            characterId = c.characterId,
                            position = ParsePosition(c.position)
                        }).ToList(),
                        dialogs = allDialogFields.Select(d => new DialogLine
                        {
                            speakerDisplayName = d.speaker,
                            content = d.content
                        }).ToList()
                    };
                    break;
                case "ChoiceNode":
                    node = new ChoiceNode
                    {
                        nodeId = nodeId,
                        backgroundId = backgroundId,
                        questionText = questionText,
                        characters = allCharacterFields.Select(c => new CharacterState
                        {
                            characterId = c.characterId,
                            position = ParsePosition(c.position)
                        }).ToList(),
                        choices = allChoiceFields.Select(c => new Choice
                        {
                            choiceText = c.choiceText,
                            nextNodeId = c.nextNodeId
                        }).ToList()
                    };
                    break;
                default:
                    Debug.LogWarning($"存在无法识别的节点类型: {nodeType}, 请检查csv配置是否正确");
                    break;
            }

            if (node != null)
            {
                nodes.Add(nodeId, node);
                if (rootNode == null) rootNode = node;
            }
        }

        // 构建故事树
        var story = new StoryTree { rootNode = rootNode };
        foreach (var storyNode in nodes.Values)
        {
            story.AddNode(storyNode);
            Debug.LogWarning($"加入节点: {storyNode.nodeId}, 类型: {storyNode.GetType()}");
        }

        return story;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false; // 标志字段是否被引号包裹
        string currentFields = "";

        foreach (char c in line)
        {
            if (c == '"') { inQuotes = !inQuotes; } // 处理引号闭合
            else if (c == ',' && !inQuotes) { fields.Add(currentFields.Trim()); currentFields = ""; } // 碰到","且不在引号内，加入字段列表
            else { currentFields += c; }
        }

        fields.Add(currentFields.Trim()); // 加入最后一个字段
        return fields;
    }
    
    private static Vector2 ParsePosition(string positionStr)
    {
        if (string.IsNullOrEmpty(positionStr)) return Vector2.zero;
        
        var parts = positionStr.Split(',');
        if (parts.Length != 2) return Vector2.zero;

        if (int.TryParse(parts[0].Trim(), out int x) && 
            int.TryParse(parts[1].Trim(), out int y))
        {
            return new Vector2(x, y);
        }
        return Vector2.zero;
    }
}