using MoonSharp.Interpreter;

public class TextMessage {
    public TextMessage(string text, bool decorated, bool showImmediate, bool actualText = true, DynValue mugshot = null) {
        Setup(text, decorated, showImmediate, actualText, false, mugshot);
    }

    public TextMessage(string text, bool decorated, bool showImmediate, DynValue mugshot, bool actualText = true) {
        Setup(text, decorated, showImmediate, actualText, false, mugshot);
    }

    public string Text { get; set; }
    public bool Decorated { get; private set; }
    public bool ShowImmediate { get; private set; }
    public bool ActualText { get; private set; }
    public bool ForceNoAutoLineBreak { get; private set; }
    public DynValue Mugshot { get; private set; }

    public void AddToText(string textToAdd) { Text += textToAdd; }

    protected void Setup(string text, bool decorated, bool showImmediate, bool actualText = true, bool forceNoAutoLineBreak = false, DynValue mugshot = null) {
        text = Unescape(text); // compensate for unity inspector autoescaping control characters
        text = text.Replace("[name]", PlayerCharacter.instance.Name);
        Text = decorated ? DecorateText(text) : text;
        Decorated = decorated;
        ShowImmediate = showImmediate;
        ActualText = actualText;
        ForceNoAutoLineBreak = forceNoAutoLineBreak;
        Mugshot = mugshot;
    }

    public void SetText(string text) { Text = text; }

    private static string DecorateText(string text) {
        string textNew = "";
        if (text == null)
            return null;
        string[] lines = text.Split('\n');
        string[] linesCommands = new string[lines.Length];
        if (text.Length != 0)
            for (int i = 0; i < lines.Length; i++) {
                bool needExit = false;
                int index = 0;
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
                            while (lines[i][index] != ']') {
                                index++;
                                if (index != lines[i].Length) continue;
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
        string newText = linesCommands[nCount++] + "* ";
        foreach (char c in textNew) {
            switch (c) {
                case '\n': newText += "\n" + linesCommands[nCount ++] + "* "; break;
                case '\r': newText += "\n  ";                                 break;
                default:   newText += c;                                      break;
            }
        }
        return newText;
    }

    private static string Unescape(string str) {
        try { return str.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"); }
        catch { return str; }
    }
}