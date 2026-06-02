using System;
using System.Collections.Generic;
using UnityEngine;

public enum OptionAction
{
    Goto,
    Interrupt,
    Complete,
    AcceptQuest,
    OpenShop,
    GiveItem,
}

[Serializable]
public struct OptionEntry
{
    public string Content;
    public OptionAction Action;
    public int GotoIdx;
    public string ActionData;
    public OptionEntry(string content, OptionAction action, int gotoIdx, string actionData = "")
    {
        Content = content;
        Action = action;
        GotoIdx = gotoIdx;
        ActionData = actionData;
    }
}

[Serializable]
public struct DialogueNode
{
    public int Id;
    [TextArea] public string Content;
    // option -> nextDialogId
    public List<OptionEntry> Options;
}



[CreateAssetMenu(menuName = "CreateDialogue", fileName = "DialogueSystem")]
public class DialogueData : ScriptableObject
{
    public int InitializeIdx = 0;
    public List<DialogueNode> Nodes;
}
