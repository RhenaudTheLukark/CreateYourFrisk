using UnityEngine;
using MoonSharp.Interpreter;

public class TextMessage {
    public TextMessage(string text, bool decorated, bool showImmediate, bool actualText = true, DynValue mugshot = null) {
        Setup(text, decorated, showImmediate, actualText, mugshot);
    }

    public TextMessage(string text, bool decorated, bool showImmediate, DynValue mugshot, bool actualText = true) {
        Setup(text, decorated, showImmediate, actualText, mugshot);
    }

    public string Text { get; set; }
    public bool Decorated { get; private set; }
    public bool ShowImmediate { get; private set; }
    public bool ActualText { get; private set; }
    public DynValue Mugshot { get; private set; }

    public void AddToText(string textToAdd) { Text += textToAdd; }

    protected void Setup(string text, bool decorated, bool showImmediate, bool actualText, DynValue mugshot) {
        text = Unescape(text); // compensate for unity inspector autoescaping control characters
        Text = decorated ? DecorateText(text) : text;
        Decorated = decorated;
        ShowImmediate = showImmediate;
        ActualText = actualText;
        Mugshot = mugshot;
    }

    protected void Setup(string text, bool formatted, bool showImmediate) { Setup(text, formatted, showImmediate, true, null); }

    public void SetText(string text) { this.Text = text; }

    private string DecorateText(string text) {
        string newText = "* ", textNew = "";
        if (text == null)
            return text;
        string[] lines = text.Split('\n');
        string[] linesCommands = new string[lines.Length];
        int index = 0;
        if (text.Length != 0)
            for (int i = 0; i < lines.Length; i++) {
                bool needExit = false;
                index = 0;
                if (lines[i].Length != 0)
                    while (lines[i][index] == '[') {
                        if (!(lines[i].Length >= 10 + index && (lines[i].Substring(index, 10) == "[starcolor" || lines[i].Substring(index, 8) == "[letters"))) {
                            if (lines[i][index] == '[') { // TODO: Somehow apply UnitaleUtil.ParseCommandInLine here maybe?
                                bool command = false;
                                for (int j = index; j < lines[i].Length; j++)
                                    if (lines[i][j] == ']') {
                                        command = true;
                                        linesCommands[i] += lines[i].Substring(index, j + 1);
                                        lines[i] = lines[i].Substring(index + j + 1, lines[i].Length - index - j - 1);
                                        break;
                                    }
                                if (!command || lines[i].Length == 0) break;
                            }
                        } else
                            while (lines[i][index] != ']')
                                index++;
                                if (index == lines[i].Length) {
                                    needExit = true;
                                    break;
                                }
                        if (needExit)
                            break;
                    }

                if (lines[i].Length != 0)
                    if (lines[i][0] == ' ')
                        lines[i] = lines[i].Substring(1, lines[i].Length - 1);

                if (i == lines.Length - 1) textNew += lines[i];
                else                       textNew += lines[i] + '\n';
            }
        int nCount = 0;
        newText = linesCommands[nCount++] + "* ";
        foreach (char c in textNew) {
            if (c == '\n')      newText += "\n" + linesCommands[nCount ++] + "* ";
            else if (c == '\r') newText += "\n  ";
            else                newText += c;
        }
        return newText;
    }

    private static string Unescape(string str) {
        try { return str.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"); }
        catch { return str; }
    }
}