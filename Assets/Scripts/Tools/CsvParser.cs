using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StoryCsvParser
{
    /** 最少字段个数 */
    public const int MIN_FIELD_COUNT = 5;

    /** */

    public static StoryTree ParseCsvToStory(string csvText)
    {
        var lines = csvText.Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line.Trim()))
            .ToList();


        var headerFields = ParseCsvLine(lines[0]);
        var fieldIndices = new Dictionary<string, int>();
        for (int i = 0; i < headerFields.Count; i++) { fieldIndices[headerFields[i]] = i; }

        // 移除第一行（表头）
        lines.RemoveAt(0);

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
            if (firstLineFields.Count <= MIN_FIELD_COUNT) continue;

            // 与#同行的星系
            string nodeType = GetFieldValue(fieldIndices, firstLineFields, "type"); // 节点类型
            string nodeId = GetFieldValue(fieldIndices, firstLineFields, "nodeId"); // 节点Id
            string backgroundId = GetFieldValue(fieldIndices, firstLineFields, "backgroundId"); // 节点背景图片
            

            string nextNodeId = GetFieldValue(fieldIndices, firstLineFields, "nextNodeId");
            string questionText = GetFieldValue(fieldIndices, firstLineFields, "questionText");

            // 需要遍历多行读取的信息
            var allCharacterFields = new List<(string characterId, string position)>();
            var allDialogFields = new List<(string speaker, string content)>();
            var allChoiceFields = new List<(string choiceText, string nextNodeId)>();

            foreach (var line in block)
            {
                var fields = ParseCsvLine(line);

                if (fields.Count <= MIN_FIELD_COUNT) continue;

                // 解析角色信息
                var characterId = GetFieldValue(fieldIndices, fields, "character");
                var position = GetFieldValue(fieldIndices, fields, "position");
                if (!string.IsNullOrEmpty(characterId))
                {
                    if (string.IsNullOrEmpty(position)) { position = "0, 0"; }
                    allCharacterFields.Add((characterId, position));
                }

                // 解析对话信息，仅DialogNode
                if (nodeType == "DialogNode")
                {
                    var speaker = GetFieldValue(fieldIndices, fields, "speaker");
                    var content = GetFieldValue(fieldIndices, fields, "content");
                    if(!string.IsNullOrEmpty(speaker) && !string.IsNullOrEmpty(content)) { allDialogFields.Add((speaker, content)); }
                }

                // 解析选项，仅ChoiceNode专用
                if (nodeType == "ChoiceNode")
                {
                    var choice = GetFieldValue(fieldIndices, fields, "choices");
                    var choiceNextNodeId = GetFieldValue(fieldIndices, fields, "choiceNext");
                    if (!string.IsNullOrEmpty(choice) && !string.IsNullOrEmpty(choiceNextNodeId)){ allChoiceFields.Add((choice, choiceNextNodeId)); } 
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

    private static string GetFieldValue(Dictionary<string, int> fieldIndices, List<string> fields, string fieldName)
    {
        if (!fieldIndices.ContainsKey(fieldName))
        {
            Debug.LogError($"字段: {fieldName}在Csv首行中不存在，请检查csv格式是否正确");
        }
        if (fields.Count > fieldIndices[fieldName])
        {
            return fields[fieldIndices[fieldName]];
        }
        return "";
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