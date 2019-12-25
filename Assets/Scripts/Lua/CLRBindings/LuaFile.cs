using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class LuaFile {
    private string path;
    private string mode;
    private string[] content;
    private System.Text.Encoding _encoding = System.Text.Encoding.Unicode;

    public int lineCount { get { return content.Length; } }
    public string openMode { get { return mode; } }
    public string filePath { get { return path; } }

    public string encoding {
        get {
            if (!mode.Contains("b"))
                throw new CYFException("Cannot access file.encoding unless the file is opened in byte mode.");
            var list = new System.Collections.Generic.Dictionary<System.Text.Encoding, string>() {
                {System.Text.Encoding.Default,             "Default"},
                {System.Text.Encoding.Unicode,             "Unicode"},
                {System.Text.Encoding.BigEndianUnicode,    "BigEndianUnicode"},
                {System.Text.Encoding.ASCII,               "ASCII"},
                {System.Text.Encoding.UTF7,                "UTF7"},
                {System.Text.Encoding.UTF8,                "UTF8"},
                {System.Text.Encoding.UTF32,               "UTF32"}
            };

            string outValue;
            list.TryGetValue(_encoding, out outValue);
            return outValue;
        }
        set {
            if (!mode.Contains("b"))
                throw new CYFException("Cannot access file.encoding unless the file is opened in byte mode.");

            var list = new System.Collections.Generic.Dictionary<string, System.Text.Encoding>() {
                {"Default",             System.Text.Encoding.Default},
                {"Unicode",             System.Text.Encoding.Unicode},
                {"BigEndianUnicode",    System.Text.Encoding.BigEndianUnicode},
                {"ASCII",               System.Text.Encoding.ASCII},
                {"UTF7",                System.Text.Encoding.UTF7},
                {"UTF8",                System.Text.Encoding.UTF8},
                {"UTF32",               System.Text.Encoding.UTF32}
            };

            System.Text.Encoding newValue = null;
            list.TryGetValue(value, out newValue);

            if (newValue != null) {
                _encoding = newValue;
                content = File.Exists(path) ? _encoding.GetString(File.ReadAllBytes(path)).Split('\n') : null;
            } else
                throw new CYFException("file.encoding: \"" + value.ToString() + "\" is not a valid encoding type.");
        }
    }

    public LuaFile(string path, string mode = "rw") {
        if (path == null)
            throw new CYFException("Cannot open a file with a nil path.");
        if (path.Contains(".."))
            throw new CYFException("You cannot open a file outside of a mod folder. The use of \"..\" is forbidden.");
        path = (FileLoader.ModDataPath + "/" + path).Replace('\\', '/');

        Regex validator = new Regex(@"^[rwb*]+$");
        if (!validator.IsMatch(mode))
            throw new CYFException("A file's open mode must have one or both of the characters \"r\" (read) and \"w\" (write), optionally followed by \"b\" (byte mode).");
        if (mode.Contains("r") && !File.Exists(path))
            throw new CYFException("You can't open a file that doesn't exist in read-only mode.");
        if (!Directory.Exists(path.Substring(0, path.Length - Path.GetFileName(path).Length)))
            throw new CYFException("Invalid path:\n\n\"" + path + "\"");

        this.path = path;
        this.mode = mode;

        content = File.Exists(path) ? (mode.Contains("b") ? _encoding.GetString(File.ReadAllBytes(path)).Split('\n') : File.ReadAllText(path).Split('\n')) : null;
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

        try {
            if (append) {
                if (mode.Contains("b")) {
                    byte[] fileContents = File.ReadAllBytes(path);
                    byte[] dataContents = _encoding.GetBytes(data);
                    byte[] writeBytes   = new byte[fileContents.Length + dataContents.Length];

                    fileContents.CopyTo(writeBytes, 0);
                    dataContents.CopyTo(writeBytes, fileContents.Length);
                    File.WriteAllBytes(path, writeBytes);
                } else
                    File.WriteAllText(path, File.ReadAllText(path) + data);
            } else {
                if (mode.Contains("b"))
                    File.WriteAllBytes(path, _encoding.GetBytes(data));
                else
                    File.WriteAllText(path, data);
            }
        } catch (UnauthorizedAccessException) {
            throw new CYFException("File.Write: Unauthorized access to file:\n\"" + path + "\"\n\nIt may be read-only, hidden or a folder.");
        }

        content = (mode.Contains("b") ? _encoding.GetString(File.ReadAllBytes(path)).Split('\n') : File.ReadAllText(path).Split('\n'));
    }

    public void ReplaceLine(int line, string data) {
        if (!mode.Contains("w"))
            throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (!File.Exists(path))
            throw new CYFException("The file at the path \"" + path + "\" doesn't exist, so you can't replace its lines.");
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

        if (mode.Contains("b")) {
            File.WriteAllBytes(path, _encoding.GetBytes(string.Join("\n", content)));
        } else
            File.WriteAllText(path, string.Join("\n", content));
    }

    public void DeleteLine(int line) {
        if (!mode.Contains("w"))
            throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (!File.Exists(path))
            throw new CYFException("The file at the path \"" + path + "\" doesn't exist, so you can't delete its lines.");
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
            try {
                File.Delete(path);
            } catch (UnauthorizedAccessException) {
                throw new CYFException("File.Delete: Unauthorized access to file:\n\"" + path + "\"\n\nIt may be read-only or hidden.");
            }
    }

    public void Move(string relativePath) {
        string newPath = (FileLoader.ModDataPath + "/" + relativePath).Replace('\\', '/');

        if (!File.Exists(path))
            throw new CYFException("The file at the path \"" + path + "\" doesn't exist, so you can't move it.");
        if (newPath.Contains(".."))
            throw new CYFException("You cannot move a file outside of a mod folder. The use of \"..\" is forbidden.");
        if (File.Exists(newPath))
            throw new CYFException("The file at the path \"" + newPath + "\" already exists.");

        try {
            File.Move(path, newPath);
        } catch (DirectoryNotFoundException) {
            throw new CYFException("File.Move: Could not find part or all of the path:\n\"" + newPath + "\"\n\nMake sure the path specified is valid, and its total length (" + newPath.Length + " characters) is not too long.");
        } catch (PathTooLongException) {
            throw new CYFException("File.Move: The destination path is too long:\n\"" + newPath + "\"");
        }

        path = newPath;
    }

    public void Copy(string relativePath, bool overwrite = false) {
        string newPath = (FileLoader.ModDataPath + "/" + relativePath).Replace('\\', '/');

        if (!File.Exists(path))
            throw new CYFException("The file at the path \"" + path + "\" doesn't exist, so you can't move it.");
        if (newPath.Contains(".."))
            throw new CYFException("You cannot move a file outside of a mod folder. The use of \"..\" is forbidden.");
        if (File.Exists(newPath) && !overwrite)
            throw new CYFException("The file at the path \"" + newPath + "\" already exists.");

        try {
            File.Copy(path, newPath, overwrite);
        } catch (DirectoryNotFoundException) {
            throw new CYFException("File.Copy: Could not find part or all of the path:\n\"" + newPath + "\"\n\nMake sure the path specified is valid, and its total length (" + newPath.Length + " characters) is not too long.");
        } catch (PathTooLongException) {
            throw new CYFException("File.Copy: The destination path is too long:\n\"" + newPath + "\"");
        } catch (UnauthorizedAccessException) {
            throw new CYFException("File.Copy: Unauthorized access to file:\n\"" + newPath + "\"\n\nIt may be read-only or hidden.");
        }
    }
}