using System;
using System.IO;

public class LuaFile {
    private string[] content;

    public int lineCount { get { return content.Length; } }
    public string openMode { get; private set; }
    public string filePath { get; private set; }

    public LuaFile(string path, string mode = "rw") {
        if (path == null)        throw new CYFException("Cannot open a file with a nil path.");
        if (path.Contains("..")) throw new CYFException("You cannot open a file outside of a mod folder. The use of \"..\" is forbidden.");

        path = (FileLoader.ModDataPath + "/" + path).Replace('\\', '/');

        if (mode != "r" && mode != "w" && mode != "rw" && mode != "wr") throw new CYFException("A file's open mode can only be \"r\" (read), \"w\" (write) or \"rw\" (read + write).");
        if (mode.Contains("r") && !File.Exists(path))                   throw new CYFException("You can't open a file that doesn't exist in read-only mode.");
        if (!Directory.Exists(path.Substring(0, path.Length - Path.GetFileName(path).Length))) throw new CYFException("Invalid path:\n\n\"" + path + "\"");

        filePath = path;
        openMode = mode;

        try { content = File.Exists(path) ? File.ReadAllText(path).Split('\n') : null; }
        catch (IOException e) { throw new CYFException(e.GetType() + " error:\n\n" + e.Message); }
    }

    public byte[] ReadBytes() {
        if (!openMode.Contains("r")) throw new CYFException("This file has been opened in write-only mode, you can't read anything from it.");
        if (!File.Exists(filePath))  throw new CYFException("The file at the path \"" + filePath + "\" doesn't exist, so you can't read from it.");
        return File.ReadAllBytes(filePath);
    }

    public void WriteBytes(byte[] data) {
        if (!openMode.Contains("w")) throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (data == null)            throw new CYFException("You can't write nil to a file! If you want to empty the file, use an empty table instead.");
        File.WriteAllBytes(filePath, data);
    }

    public string ReadLine(int line) {
        if (!openMode.Contains("r")) throw new CYFException("This file has been opened in write-only mode, you can't read anything from it.");
        if (!File.Exists(filePath))  throw new CYFException("The file at the path \"" + filePath + "\" doesn't exist, so you can't read from it.");
        if (line > content.Length || line < 1 || line % 1 != 0) throw new CYFException("Cannot read line #" + line + " of a file with " + content.Length + " lines.");
        return content[line - 1];
    }

    public string[] ReadLines() {
        if (!openMode.Contains("r")) throw new CYFException("This file has been opened in write-only mode, you can't read anything from it.");
        if (!File.Exists(filePath))  throw new CYFException("The file at the path \"" + filePath + "\" doesn't exist, so you can't read from it.");
        return content;
    }

    public void Write(string data, bool append = true) {
        if (!openMode.Contains("w")) throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (data == null)            throw new CYFException("You can't write nil to a file! If you want to empty the file, use an empty string with the append parameter set to false instead.");

        if (!File.Exists(filePath))
            File.Create(filePath).Close();

        try {
            if (append) File.WriteAllText(filePath, File.ReadAllText(filePath) + data);
            else        File.WriteAllText(filePath, data);
        }
        catch (UnauthorizedAccessException) { throw new CYFException("File.Write: Unauthorized access to file:\n\"" + filePath + "\"\n\nIt may be read-only, hidden or a folder."); }

        try { content = File.ReadAllText(filePath).Split('\n'); }
        catch (IOException e) { throw new CYFException(e.GetType() + " error:\n\n" + e.Message); }
    }

    public void ReplaceLine(int line, string data) {
        if (!openMode.Contains("w")) throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (!File.Exists(filePath))  throw new CYFException("The file at the path \"" + filePath + "\" doesn't exist, so you can't replace its lines.");
        if (line > content.Length || line < 1 || line % 1 != 0) throw new CYFException("Cannot replace line #" + line + " of a file with " + content.Length + " lines.");
        if (data == null)            throw new CYFException("You can't set a line to nil! If you want to remove the line, use the function DeleteLine().");

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

        File.WriteAllText(filePath, string.Join("\n", content));
    }

    public void DeleteLine(int line) {
        if (!openMode.Contains("w")) throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (!File.Exists(filePath))  throw new CYFException("The file at the path \"" + filePath + "\" doesn't exist, so you can't delete its lines.");
        if (line > content.Length || line < 1 || line % 1 != 0) throw new CYFException("The file only has " + content.Length + " lines yet you're trying to delete this file's line #" + line);

        string[] newContent = new string[lineCount - 1];
        Array.Copy(content, 0, newContent, 0, line - 1);
        Array.Copy(content, line, newContent, line - 1, lineCount - line);
        content = newContent;
    }

    public void Delete() {
        if (!openMode.Contains("w")) throw new CYFException("This file has been opened in read-only mode, you can't write anything to it.");
        if (!File.Exists(filePath)) return;
        try { File.Delete(filePath); }
        catch (UnauthorizedAccessException) { throw new CYFException("File.Delete: Unauthorized access to file:\n\"" + filePath + "\"\n\nIt may be read-only or hidden."); }
    }

    public void Move(string relativePath) {
        string newPath = (FileLoader.ModDataPath + "/" + relativePath).Replace('\\', '/');

        if (!File.Exists(filePath)) throw new CYFException("The file at the path \"" + filePath + "\" doesn't exist, so you can't move it.");
        if (newPath.Contains("..")) throw new CYFException("You cannot move a file outside of a mod folder. The use of \"..\" is forbidden.");
        if (File.Exists(newPath))   throw new CYFException("The file at the path \"" + newPath + "\" already exists.");

        try { File.Move(filePath, newPath); }
        catch (DirectoryNotFoundException) { throw new CYFException("File.Move: Could not find part or all of the path:\n\"" + newPath + "\"\n\nMake sure the path specified is valid, and its total length (" + newPath.Length + " characters) is not too long."); }
        catch (PathTooLongException) { throw new CYFException("File.Move: The destination path is too long:\n\"" + newPath + "\""); }

        filePath = newPath;
    }

    public void Copy(string relativePath, bool overwrite = false) {
        string newPath = (FileLoader.ModDataPath + "/" + relativePath).Replace('\\', '/');

        if (!File.Exists(filePath)) throw new CYFException("The file at the path \"" + filePath + "\" doesn't exist, so you can't move it.");
        if (newPath.Contains("..")) throw new CYFException("You cannot move a file outside of a mod folder. The use of \"..\" is forbidden.");
        if (File.Exists(newPath) && !overwrite) throw new CYFException("The file at the path \"" + newPath + "\" already exists.");

        try { File.Copy(filePath, newPath, overwrite); }
        catch (DirectoryNotFoundException) { throw new CYFException("File.Copy: Could not find part or all of the path:\n\"" + newPath + "\"\n\nMake sure the path specified is valid, and its total length (" + newPath.Length + " characters) is not too long."); }
        catch (PathTooLongException) { throw new CYFException("File.Copy: The destination path is too long:\n\"" + newPath + "\""); }
        catch (UnauthorizedAccessException) { throw new CYFException("File.Copy: Unauthorized access to file:\n\"" + newPath + "\"\n\nIt may be read-only or hidden."); }
    }
}