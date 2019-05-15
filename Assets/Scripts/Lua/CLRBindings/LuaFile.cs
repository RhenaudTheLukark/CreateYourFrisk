using System;
using System.IO;
using UnityEngine;

public class LuaFile {
    private string path;
    private string mode;
    private string[] content;

    public int lineCount {
        get { return content.Length; }
    }

    public string openMode {
        get { return mode; }
    }

    public LuaFile(string path, string mode = "rw") {
        if (path == null)
            throw new CYFException("Cannot open a file with a nil path.");
        if (path.Contains(".."))
            throw new CYFException("You cannot open a file outside of a mod folder. The use of \"..\" is forbidden.");
        path = (FileLoader.ModDataPath + "/" + path).Replace('\\', '/');

        if (mode != "r" && mode != "w" && mode != "rw" && mode != "wr")
            throw new CYFException("A file's open mode can only be r (read), w (write) or rw (read + write).");
        if (mode == "r" && !File.Exists(path))
            throw new CYFException("You can't open a file that doesn't exist in read-only mode.");
        if (!Directory.Exists(path.Substring(0, path.Length - Path.GetFileName(path).Length)))
            throw new CYFException("Invalid path:\n\n\"" + path + "\"");

        this.path = path;
        this.mode = mode;

        content = File.Exists(path) ? File.ReadAllText(path).Split('\n') : null;
    }

    public string ReadLine(int line) {
        if (!mode.Contains("r"))
            throw new CYFException("This file has been opened in write-only mode, you can't read anything from it.");
        if (!File.Exists(path))
            throw new CYFException("The file at the path \"" + path + "\" doesn't exist, so you can't read from it.");
        if (line > content.Length || line < 1 || line % 1 != 0)
            throw new CYFException("Cannot read line #" + line + " of a file with " + content.Length + " lines.");
        return content[line - 1];
    }

    public string[] ReadLines() {
        if (!mode.Contains("r"))
            throw new CYFException("This file has been opened in write-only mode, you can't read anything from it.");
        if (!File.Exists(path))
            throw new CYFException("The file at the path \"" + path + "\" doesn't exist, so you can't read from it.");
        return content;
    }

    public void Write(string data, bool append = true) {
        if (!mode.Contains("w"))
            throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (data == null)
            throw new CYFException("You can't write nil to a file! If you want to empty the file, use an empty string with the append parameter set to false instead.");

        if (!File.Exists(path))
            File.Create(path).Close();

        if (append) File.WriteAllText(path, File.ReadAllText(path) + data);
        else File.WriteAllText(path, data);

        content = File.ReadAllText(path).Split('\n');
    }
    
    public void ReplaceLine(int line, string data) {
        if (!mode.Contains("w"))
            throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (line > content.Length || line < 1 || line % 1 != 0)
            throw new CYFException("Cannot replace line #" + line + " of a file with " + content.Length + " lines.");
        if (data == null)
            throw new CYFException("You can't set a line to nil! If you want to remove the line, use the function DeleteLine().");

        if (data.Contains("\n")) {
            string[] content1 = new string[line - 1],
                     content2 = data.Split('\n'),
                     content3 = new string[lineCount - line];
            Array.Copy(content, 0, content1, 0, line - 1);
            Array.Copy(content, line, content3, 0, lineCount - line);
            string[] newContent = new string[lineCount - 1 + content2.Length];
            content1.CopyTo(newContent, 0);
            content2.CopyTo(newContent, line - 1);
            content3.CopyTo(newContent, line - 1 + content2.Length);
            content = newContent;
        } else
            content[line - 1] = data;

        File.WriteAllText(path, string.Join("\n", content));
    }

    public void DeleteLine(int line) {
        if (!mode.Contains("w"))
            throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (line > content.Length || line < 1 || line % 1 != 0)
            throw new CYFException("The file only has " + content.Length + " lines yet you're trying to delete this file's line #" + line);

        string[] newContent = new string[lineCount - 1];
        Array.Copy(content, 0, newContent, 0, line - 1);
        Array.Copy(content, line, newContent, line - 1, lineCount - line);
        content = newContent;
    }

    public void Delete() {
        if (!mode.Contains("w"))
            throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (File.Exists(path))
            File.Delete(path);
    }
}