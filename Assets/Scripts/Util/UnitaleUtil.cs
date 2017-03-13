using UnityEngine;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Utility class for the Unitale engine.
/// </summary>
public static class UnitaleUtil {
    internal static bool firstErrorShown = false; //Keeps track of whether an error already appeared, prevents subsequent errors from overriding the source.
    internal static string fileName = Application.dataPath + "/Logs/log-" + DateTime.Now.ToString().Replace('/', '-').Replace(':', '-') + ".txt";
    internal static StreamWriter sr;

    public static void createFile() {
        if (!Directory.Exists(Application.dataPath + "/Logs"))
            Directory.CreateDirectory(Application.dataPath + "/Logs");
        if (!File.Exists(fileName))
            sr = File.CreateText(fileName);
    }

    public static void writeInLog(string mess) {
        try {
            sr.WriteLine(mess.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t"));
            sr.Flush();
            Debug.Log(mess);
        } catch (Exception e) { Debug.Log("Couldn't write on the log: " + e.Message + "\rMessage: " + mess); }
    }

    public static void writeInLogAndDebugger(string mess) {
        try {
            sr.WriteLine("By DEBUG: " + mess.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t"));
            sr.Flush();
            UserDebugger.instance.userWriteLine(mess);
            Debug.Log("Frame " + GlobalControls.frame + ": " + mess);
        } catch (Exception e) { Debug.Log("Couldn't write on the log:\n" + e.Message + "\nMessage: " + mess); }
    }

    /// <summary>
    /// This was previously used to create error messages for display in the UI controller, but is now obsolete as this is displayed in a separate scene.
    /// </summary>
    /// <param name="source">Name of the offending script</param>
    /// <param name="decoratedMessage">Decorated error messages as given by the InterpreterException thrown by the Lua script</param>
    /// <returns>TextMessage for display in a TextManager.</returns>
    public static TextMessage createLuaError(string source, string decoratedMessage) {
        string returnValue = "[font:monster][color:ffffff]error in script " + source + "\n";
        int lineLetterCount = 0;
        int maxChars = 50;
        for (int i = 0; i < decoratedMessage.Length; i++) {
            if (lineLetterCount >= maxChars && decoratedMessage[i] == ' ' || decoratedMessage[i] == '\n') {
                returnValue += "\n"; // linebreak on spaces after maxChars characters
                lineLetterCount = 0;
            } else {
                returnValue += decoratedMessage[i];
                lineLetterCount++;
            }
        }
        return new TextMessage(returnValue, false, true);
    }

    /// <summary>
    /// Loads the Error scene with the Lua error that occurred.
    /// </summary>
    /// <param name="source">Name of the script that caused the error.</param>
    /// <param name="decoratedMessage">Error that was thrown. In MoonSharp's case, this is the DecoratedMessage property from its InterpreterExceptions.</param>
    public static void displayLuaError(string source, string decoratedMessage) {
        if (firstErrorShown)
            return;

        /*if (UIController.instance == null)
            UIController.errorMsg = createLuaError(source, decoratedMessage);
        else
            UIController.instance.ShowError(createLuaError(source, decoratedMessage));*/
        Debug.Log(decoratedMessage);
        firstErrorShown = true;
        ErrorDisplay.Message = "error in script " + source + "\n\n" + decoratedMessage;
        if (Application.isEditor)
            SceneManager.LoadSceneAsync("Error"); // prevents editor from crashing
        else
            SceneManager.LoadScene("Error");
        writeInLog("It's a Lua error! : " + ErrorDisplay.Message);
    }

    public static bool isTouching(BoxCollider2D a, BoxCollider2D b) {
        Vector2 SizeA = new Vector2(a.size.x * a.gameObject.GetComponent<RectTransform>().localScale.x, a.size.y * a.gameObject.GetComponent<RectTransform>().localScale.y),
                centerA = new Vector2(a.offset.x + a.transform.position.x, a.offset.y + a.transform.position.y),
                SizeB = new Vector2(b.size.x * b.gameObject.GetComponent<RectTransform>().localScale.x, b.size.y * b.gameObject.GetComponent<RectTransform>().localScale.y),
                centerB = new Vector2(b.offset.x + b.transform.position.x, b.offset.y + b.transform.position.y);
        Rect AB = Intersect(new Rect(centerA, SizeA), new Rect(centerB, SizeB));
        if (AB.size.x < 0 || AB.size.y < 0)
            return false;
        return true;
    }

    public static Rect Intersect(Rect r1, Rect r2) {
        float xDif = r2.center.x - r1.center.x, yDif = r2.center.y - r1.center.y,
              width = Mathf.Min(r1.xMax, r2.xMax) - Mathf.Max(r1.xMin, r2.xMin),
              height = Mathf.Min(r1.yMax, r2.yMax) - Mathf.Max(r1.yMin, r2.yMin);
        return new Rect(xDif + ((Mathf.Sign(xDif) * (8 - width)) / 2) + r1.x, yDif + ((Mathf.Sign(yDif) * (8 - height)) / 2) + r1.y, Mathf.Ceil(width), Mathf.Ceil(height));
    }

    public static int fontStringWidth(UnderFont font, string s, int hSpacing = 3) {
        int width = 0;
        foreach (char c in s)
            if (font.Letters.ContainsKey(c))
                width += (int)font.Letters[c].rect.width + hSpacing;
        return width;
    }

    public static List<T> TableToList<T>(Table table, Func<DynValue, T> converter) {
        List<T> lst = new List<T>();

        for (int i = 1, l = table.Length; i <= l; i++) {
            DynValue v = table.Get(i);
            T o = converter(v);
            lst.Add(o);
        }

        return lst;
    }

    public static Array ListToArray<T>(List<T> lst) {
        T[] arr = new T[lst.Count];

        for (int i = 0; i < lst.Count; i++)
            arr[i] = lst[i];

        return arr;
    }

    public static List<T> ArrayToList<T>(T[] arr) {

        List<T> lst = new List<T>(arr.Length);

        for (int i = 0; i < arr.Length; i++)
            lst[i] = arr[i];

        return lst;
    }

    public static DynValue[] TableToDynValueArray(Table table) {
        DynValue[] array = new DynValue[table.Length];
        string test = "{ ";
        for (int i = 1; i <= table.Length; i++) {
            DynValue v = table.Get(i);
            array[i - 1] = v;
            if (v.Type == DataType.Boolean)     test += v.Boolean;
            else if (v.Type == DataType.Number) test += v.Number;
            else if (v.Type == DataType.String) test += v.String;
            if (i < table.Length)               test += ", ";
        }
        test += " }";
        Debug.Log(test);
        return array;
    }

    public static Table DynValueArrayToTable(DynValue[] array) {
        Table table = new Table(null);

        for (int i = 1, l = array.Length; i <= l; i++)
            table.Set(i, array[i - 1]);

        return table;
    }

    public static void CompleteTableFromArray(Array arr, ref Table dv, int basisRank, Type basisType) {
        int[] currentIndexes = new int[basisRank];
        for (int i = 0; i < basisRank; i++)
            currentIndexes[i] = 0;

        for (int i1 = 1; i1 <= currentIndexes[0]; i1 ++) {
            if (basisRank >= 2)
                for (int i2 = 1; i2 <= currentIndexes[1]; i2++) {
                    if (basisRank >= 3)
                        for (int i3 = 1; i3 <= currentIndexes[2]; i3++) {
                            if (basisRank >= 4)
                                for (int i4 = 1; i4 <= currentIndexes[3]; i4++) {
                                    if (basisRank >= 5)
                                        for (int i5 = 1; i5 <= currentIndexes[4]; i5++) {
                                            if (basisRank >= 6)
                                                for (int i6 = 1; i6 <= currentIndexes[5]; i6++) {
                                                    if (basisRank >= 7)
                                                        for (int i7 = 1; i7 <= currentIndexes[6]; i7++) {
                                                            if (basisType == typeof(float))
                                                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                                  .Get(DynValue.NewNumber(i4)).Table.Get(DynValue.NewNumber(i5)).Table.Get(DynValue.NewNumber(i6)).Table
                                                                  .Set(DynValue.NewNumber(i7), DynValue.NewNumber((float)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1, i6 + 1, i7 + 1)));
                                                            else if (basisType == typeof(bool))
                                                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                                  .Get(DynValue.NewNumber(i4)).Table.Get(DynValue.NewNumber(i5)).Table.Get(DynValue.NewNumber(i6)).Table
                                                                  .Set(DynValue.NewNumber(i7), DynValue.NewBoolean((bool)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1, i6 + 1, i7 + 1)));
                                                            else
                                                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                                  .Get(DynValue.NewNumber(i4)).Table.Get(DynValue.NewNumber(i5)).Table.Get(DynValue.NewNumber(i6)).Table
                                                                  .Set(DynValue.NewNumber(i7), DynValue.NewString((string)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1, i6 + 1, i7 + 1)));
                                                        } 
                                                    else if (basisType == typeof(float))
                                                        dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                          .Get(DynValue.NewNumber(i4)).Table.Get(DynValue.NewNumber(i5)).Table
                                                          .Set(DynValue.NewNumber(i6), DynValue.NewNumber((float)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1, i6 + 1)));
                                                    else if (basisType == typeof(bool))
                                                        dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                          .Get(DynValue.NewNumber(i4)).Table.Get(DynValue.NewNumber(i5)).Table
                                                          .Set(DynValue.NewNumber(i6), DynValue.NewBoolean((bool)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1, i6 + 1)));
                                                    else
                                                        dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                          .Get(DynValue.NewNumber(i4)).Table.Get(DynValue.NewNumber(i5)).Table
                                                          .Set(DynValue.NewNumber(i6), DynValue.NewString((string)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1, i6 + 1)));
                                                } 
                                            else if (basisType == typeof(float))
                                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                  .Get(DynValue.NewNumber(i4)).Table.Set(DynValue.NewNumber(i5), DynValue.NewNumber((float)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1)));
                                            else if (basisType == typeof(bool))
                                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                  .Get(DynValue.NewNumber(i4)).Table.Set(DynValue.NewNumber(i5), DynValue.NewBoolean((bool)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1)));
                                            else
                                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                                  .Get(DynValue.NewNumber(i4)).Table.Set(DynValue.NewNumber(i5), DynValue.NewString((string)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1, i5 + 1)));
                                        } 
                                    else if (basisType == typeof(float))
                                        dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                          .Set(DynValue.NewNumber(i4), DynValue.NewNumber((float)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1)));
                                    else if (basisType == typeof(bool))
                                        dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                          .Set(DynValue.NewNumber(i4), DynValue.NewBoolean((bool)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1)));
                                    else
                                        dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Get(DynValue.NewNumber(i3)).Table
                                          .Set(DynValue.NewNumber(i4), DynValue.NewString((string)arr.GetValue(i1 + 1, i2 + 1, i3 + 1, i4 + 1)));
                                } 
                            else if (basisType == typeof(float))
                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Set(DynValue.NewNumber(i3), DynValue.NewNumber((float)arr.GetValue(i1 + 1, i2 + 1, i3 + 1)));
                            else if (basisType == typeof(bool))
                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Set(DynValue.NewNumber(i3), DynValue.NewBoolean((bool)arr.GetValue(i1 + 1, i2 + 1, i3 + 1)));
                            else
                                dv.Get(DynValue.NewNumber(i1)).Table.Get(DynValue.NewNumber(i2)).Table.Set(DynValue.NewNumber(i3), DynValue.NewString((string)arr.GetValue(i1 + 1, i2 + 1, i3 + 1)));
                        } 
                    else if (basisType == typeof(float))
                        dv.Get(DynValue.NewNumber(i1)).Table.Set(DynValue.NewNumber(i2), DynValue.NewNumber((float)arr.GetValue(i1 + 1, i2 + 1)));
                    else if (basisType == typeof(bool))
                        dv.Get(DynValue.NewNumber(i1)).Table.Set(DynValue.NewNumber(i2), DynValue.NewBoolean((bool)arr.GetValue(i1 + 1, i2 + 1)));
                    else
                        dv.Get(DynValue.NewNumber(i1)).Table.Set(DynValue.NewNumber(i2), DynValue.NewString((string)arr.GetValue(i1 + 1, i2 + 1)));
                }
            else if (basisType == typeof(float))
                dv.Set(DynValue.NewNumber(i1), DynValue.NewNumber((float)arr.GetValue(i1 + 1)));
            else if (basisType == typeof(bool))
                dv.Set(DynValue.NewNumber(i1), DynValue.NewBoolean((bool)arr.GetValue(i1 + 1)));
            else
                dv.Set(DynValue.NewNumber(i1), DynValue.NewString((string)arr.GetValue(i1 + 1)));
        }
    }

    public static float calcTotalLength(TextManager txtmgr, int fromLetter = -1, int toLetter = -1) {
        float totalWidth = 0, totalMaxWidth = 0, hSpacing = txtmgr.Charset.CharSpacing;
        if (fromLetter == -1)
            fromLetter = 0;
        if (toLetter == -1)
            toLetter = txtmgr.textQueue[txtmgr.currentLine].Text.Length;
        //Debug.Log("From the letter " + fromLetter + " to the letter " + toLetter + ". The max is " + txtmgr.textQueue[txtmgr.currentLine].Text.Length);
        if (fromLetter > toLetter || fromLetter < 0 || toLetter > txtmgr.textQueue[txtmgr.currentLine].Text.Length) {
            //Debug.LogError("BAAAAAAAAAAAAAD : fromLetter = " + fromLetter + " and toLetter = " + toLetter);
            return -1;
        }
        if (fromLetter == toLetter)
            return 0;
        for (int i = fromLetter; i < toLetter; i++) {
            //Debug.Log(txtmgr.textQueue[txtmgr.currentLine].Text[i]);
            switch (txtmgr.textQueue[txtmgr.currentLine].Text[i]) {
                case '[':
                    string str = "";
                    for (int j = i + 1; j < txtmgr.textQueue[txtmgr.currentLine].Text.Length; j++) {
                        if (txtmgr.textQueue[txtmgr.currentLine].Text[j] == ']') {
                            i = j + 1;
                            break;
                        }
                        str += txtmgr.textQueue[txtmgr.currentLine].Text[j];
                    }
                    i--;
                    if (str.Split(':')[0] == "charspacing")
                        hSpacing = ParseUtil.getFloat(str.Split(':')[1]);
                    break;
                case '\r':
                case '\n':
                    if (totalMaxWidth < totalWidth - hSpacing)
                        totalMaxWidth = totalWidth - hSpacing;
                    totalWidth = 0;
                    break;
                default:
                    if (txtmgr.Charset.Letters.ContainsKey(txtmgr.textQueue[txtmgr.currentLine].Text[i]))
                        totalWidth += txtmgr.Charset.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.x + hSpacing;
                    break;
            }
            //Debug.Log(totalWidth + " ///// " + totalMaxWidth);
        }
        if (totalMaxWidth < totalWidth - hSpacing)
            totalMaxWidth = totalWidth - hSpacing;
        //Debug.Log(totalWidth);
        return totalMaxWidth;

        /*float lastY = 0, count = 0;

        for (int i = fromLetter; i < toLetter; i++) {
            if (rts[i].position.y != lastY) {
                totalWidth += txtmgr.hSpacing * (count - 1);
                if (totalWidth > totalMaxWidth)
                    totalMaxWidth = totalWidth;
                totalWidth = 0; count = 0;
                lastY = rts[i].position.y;
            }
            totalWidth += rts[i].sizeDelta.x;
            count++;
        }
        totalWidth += addNextValue;
        if (addNextValue != 0) count++;
        if (totalWidth != 0) totalWidth += txtmgr.hSpacing * (count - 1);
        if (totalWidth > totalMaxWidth) totalMaxWidth = totalWidth;
        return totalWidth;*/
    }

    public static float calcTotalHeight(TextManager txtmgr, int fromLetter = -1, int toLetter = -1) {
        float maxHeight = -999, minHeight = 999;
        if (fromLetter == -1)
            fromLetter = 0;
        if (toLetter == -1)
            toLetter = txtmgr.textQueue[txtmgr.currentLine].Text.Length;
        //Debug.Log("From the letter " + fromLetter + " to the letter " + toLetter + ". The max is " + txtmgr.textQueue[txtmgr.currentLine].Text.Length);
        if (fromLetter > toLetter || fromLetter < 0 || toLetter > txtmgr.textQueue[txtmgr.currentLine].Text.Length) return -1;
        if (fromLetter == toLetter)
            return 0;
        for (int i = fromLetter; i < toLetter; i++)
            if (txtmgr.Charset.Letters.ContainsKey(txtmgr.textQueue[txtmgr.currentLine].Text[i])) {
                if (txtmgr.letterPositions[i].y < minHeight)
                    minHeight = txtmgr.letterPositions[i].y;
                if (txtmgr.letterPositions[i].y + txtmgr.Charset.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.y > maxHeight)
                    maxHeight = txtmgr.letterPositions[i].y + txtmgr.Charset.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.y;
            }
        return maxHeight - minHeight;

        /*float lastY = 0, count = 0;

        for (int i = fromLetter; i < toLetter; i++) {
            if (rts[i].position.y != lastY) {
                totalWidth += txtmgr.hSpacing * (count - 1);
                if (totalWidth > totalMaxWidth)
                    totalMaxWidth = totalWidth;
                totalWidth = 0; count = 0;
                lastY = rts[i].position.y;
            }
            totalWidth += rts[i].sizeDelta.x;
            count++;
        }
        totalWidth += addNextValue;
        if (addNextValue != 0) count++;
        if (totalWidth != 0) totalWidth += txtmgr.hSpacing * (count - 1);
        if (totalWidth > totalMaxWidth) totalMaxWidth = totalWidth;
        return totalWidth;*/
    }

    public static string[] specialSplit(char c, string str, bool countTables = false) {
        bool inString = false; int lastIndex = 0, tableStack = 0;
        List<string> tempArray = new List<string>();
        for (int i = 0; i < str.Length; i++) {
            if (countTables)
                if (str[i] == '{')
                    tableStack++;
            if (str[i] == '"') {
                inString =!inString;
                continue;
            }
            if (str[i] == c &&!inString && ((tableStack == 0 && countTables) ||!countTables)) {
                tempArray.Add(str.Substring(lastIndex, i - lastIndex).Trim());
                lastIndex = i + 1;
            }
            if (countTables)
                if (str[i] == '}' && tableStack > 0)
                    tableStack--;
        }
        tempArray.Add(str.Substring(lastIndex, str.Length - lastIndex).Trim());
        return (string[])ListToArray(tempArray);
    }

    public static object stringToArray(string basisStr, out int[] lengths) {
        int arrayLevel = 0;
        while(basisStr[arrayLevel] == '{')
            arrayLevel++;
        lengths = new int[0];
        string[] tests = new string[0];
        Type arrayType = null;
        Array array = (Array)GetIndexArrayStackLevel(arrayLevel, basisStr, out lengths, out tests, out arrayType);
        Type testType = CheckRealType(tests[0], true);
        foreach (string str in tests) {
            if (CheckRealType(str, true) != testType)
                return null;
        }
        int[] currentIndexes = new int[lengths.Length];
        for (int i = 0; i < currentIndexes.Length; i++)
            currentIndexes[i] = 0;
        int currentStack = 0;
        FillBigArray(array, array, tests, 0, currentStack, arrayLevel, ref currentIndexes);
        return array;
    }

    public static void FillBigArray(Array array, Array currentArray, string[] items, int currentItem, int currentStack, int arrayLevel, ref int[] currentIndexes, bool tuple = false) {
        foreach (var obj in currentArray) {
            if (currentStack + 1 < arrayLevel)
                FillBigArray(array, (Array)obj, items, currentItem, currentStack + 1, arrayLevel, ref currentIndexes, tuple);
            else {
                Type t = CheckRealType(items[currentItem], true);
                object realItem;
                if (t == typeof(float)) {
                    if (tuple)                         realItem = DynValue.NewNumber(ParseUtil.getFloat(items[currentItem]));
                    else                               realItem = ParseUtil.getFloat(items[currentItem]);
                } else if (t == typeof(bool)) {
                    if (items[currentItem] == "true")  realItem = true;
                    else                               realItem = false;
                    if (tuple)                         realItem = DynValue.NewBoolean((bool)realItem);
                } else {
                    if (tuple)                         realItem = DynValue.NewString(items[currentItem]);
                    else                               realItem = items[currentItem];
                }
                array.SetValue(realItem, currentIndexes);
            }
            currentIndexes[currentStack - 1]++;
        }
    }

    public static Type GetBasisType(string str) {
        string strNoTable = str.Replace("{", "").Replace("}", "").Replace(",", "");
        if (strNoTable.Replace("true", "").Replace("false", "").Length == 0)
            return typeof(bool);
        else if (strNoTable.Replace("0", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "").Replace("5", "")
                           .Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "").Replace("-", "").Replace(".", "").Length == 0)
            return typeof(float);
        else
            return typeof(string);
    }

    public static object GetIndexArrayStackLevel(int stackLevel, string refString, out int[] lengths, out string[] oneDimensionnalData, out Type arrayType) {
        int currentStackLevel = 0, lastNumber = 1;
        Type basisType = GetBasisType(refString);
        object t;
        if (basisType == typeof(float)) t = 0;
        else if (basisType == typeof(bool)) t = false;
        else t = "";
        List<string> tempArray = new List<string>();
        List<List<string>> tempSuperArray = new List<List<string>>();
        lengths = new int[stackLevel];
        oneDimensionnalData = new string[] { refString };
        while (currentStackLevel < stackLevel) {
            if (currentStackLevel == 0)
                tempArray = ArrayToList(specialSplit(',', refString.Substring(1, refString.Length - 2), true));
            else {
                foreach (string str in tempArray)
                    tempSuperArray.Add(ArrayToList(specialSplit(',', str.Substring(1, str.Length - 2))));
                tempArray.Clear();
                foreach (List<string> strArray in tempSuperArray)
                    foreach (string str in strArray) {
                        Array.Resize(ref oneDimensionnalData, oneDimensionnalData.Length + 1);
                        oneDimensionnalData[oneDimensionnalData.Length - 1] = str.Trim('"', ' ');
                    }
            }
            lengths[currentStackLevel] = tempArray.Count / lastNumber;
            lastNumber = lengths[currentStackLevel];
            currentStackLevel++;
            tempSuperArray.Clear();
        }
        //for (int i = 0; i < stackLevel; i++)
        //    t = t[lengths[lengths.Length - i]];
        arrayType = t.GetType();
        return t;
    }

    /// <summary>
    /// Check DynValues parameter's value types. DON'T WORK WITH MULTIDIMENSIONNAL ARRAYS!
    /// (For now it doesn't work with arrays at all :c)
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static Type CheckRealType(string parameter, bool ignoreTables = false, int translateEmbeddedLevel = 0) {
        string parameterNoSpace = parameter.Replace(" ", "");

        //Boolean
        //If the string is equal to "true" or "false", this is a boolean
        if (parameterNoSpace == "false" || parameterNoSpace == "true")
            return typeof(bool);

        //Number (here float)
        bool isNumber = true;
        //If each parameter is a number, a dot, a space or a minus, this is a number
        foreach (char c in parameterNoSpace)
            if (c != '0' && c != '1' && c != '2' && c != '3' && c != '4' && c != '5' && c != '6' && c != '7' && c != '8' && c != '9' && c != '.' && c != ',' && c != ' ' && c != '-') {
                isNumber = false;
                break;
            }
        if (isNumber)
            return typeof(float);

        //Array
        /*int tempTestArray = 0;
        if (ignoreTables) {
            string parameter2 = parameter;
            while (parameter2[0] == '{') {
                int embeddedLevel = 1, charIndex = 1;
                while (embeddedLevel > 0) {
                    if (parameter2[charIndex] == '{')
                        embeddedLevel++;
                    embeddedLevel++;
                    if (parameter2[charIndex] == '}')
                        embeddedLevel--;
                    if (charIndex == parameter2.Length)
                        return typeof(string);
                }
                parameter2 = parameter2.Substring(1, charIndex - 1);
            }
            return CheckRealType(parameter2);
        } else if (parameter[0] == '{' && parameter[parameter.Length - 1] == '}') {
            tempTestArray++;
            while (parameter[tempTestArray] == '{')
                tempTestArray++;
            if (translateEmbeddedLevel == 0)
                translateEmbeddedLevel = tempTestArray;
            //We put out all arrays identifiers and split the values of the array
            string[] parameters = specialSplit(',', parameter.Substring(1, parameter.Length - 2), true);
            List<Type> list = new List<Type>();
            //We register all types of this array's values...
            foreach (string str in parameters)
                list.Add(CheckRealType(str, ignoreTables, translateEmbeddedLevel));
            Type testType = list[0];
            int temp = 0;
            //...and we test if these types are the same as the first type of the array
            foreach (Type type in list) {
                writeInLog(type.ToString() + " = " + temp++);
                //If these types aren't the same, this isn't an array
                if (testType != type)
                    return typeof(string);
            }
            if (tempTestArray == translateEmbeddedLevel)
                translateEmbeddedLevel = 0;
            Type endType = testType;
            while (endType.IsArray)
                endType = endType.GetElementType();
            Array tempArray;
            //If they are the same, we check what is the type of the first value, and return a unidimensionnal array type
            if (endType == typeof(bool))        tempArray = Array.CreateInstance(typeof(bool), new int[tempTestArray]);
            else if (endType == typeof(float))  tempArray = Array.CreateInstance(typeof(float), new int[tempTestArray]);
            else                                tempArray = Array.CreateInstance(typeof(string), new int[tempTestArray]);
            return tempArray.GetType();
        }*/

        //String
        //If all our attempts to check other types failed, this is really a string
        return typeof(string);
    }

    public static string noCommand(string str) {
        int inCommandStack = 0; string newstr = "";
        for (int i = 0; i < str.Length; i++) {
            if (str[i] == '[')                        inCommandStack ++;
            if (inCommandStack == 0)                  newstr += str[i];
            if (str[i] == ']' && inCommandStack > 0)  inCommandStack --;
        }
        return newstr;
    }

    public static float CropDecimal(float number) {
        float basisNumber = number;
        int dec = DecimalCount(number);
        return Mathf.Round(basisNumber * Mathf.Pow(10, dec)) / Mathf.Pow(10, dec);
    }

    public static int DecimalCount(float number) {
        int dec = 0, ninecount = 0, zerocount = 0;
        number %= 1;
        bool zeromode = false, ninemode = false, something = false;
        while (number != 0) {
            number *= 10;
            if (!something) {
                if (!zeromode &&!ninemode) {
                    if (Mathf.Floor(number) == 0)      zeromode = true;
                    else if (Mathf.Floor(number) == 9) ninemode = true;
                    else                               something = true;
                } else if ((Mathf.Floor(number) != 0 && zeromode) || (Mathf.Floor(number) != 9 && ninemode))
                    something = true;
            } else {
                if (Mathf.Floor(number) == 0 )      { zerocount++;   ninecount = 0; }
                else if (Mathf.Floor(number) == 9 ) { ninecount++;   zerocount = 0; } 
                else                                { ninecount = 0; zerocount = 0; }
            }
            dec++;
            number %= 1;
            if (ninecount == 3 || zerocount == 3)
                return dec - 3;
            if (dec == ControlPanel.instance.MaxDigitsAfterComma)
                return dec;
        }
        return dec;
    }

    public static void PlaySound(string basis, string sound, float volume = 0.65f) {
        sound = FileLoader.getRelativePathWithoutExtension(sound).Replace('\\', '/');
        for (int i = 1; i > 0; i++) {
            object audio = NewMusicManager.audiolist[basis + i];
            if (audio != null) {
                if (audio.ToString().ToLower() != "null") {
                    if (!NewMusicManager.isStopped(basis + i))
                        continue;
                } else {
                    NewMusicManager.audiolist.Remove(basis + i);
                    NewMusicManager.CreateChannel(basis + i);
                }
            } else
                NewMusicManager.CreateChannel(basis + i);
            NewMusicManager.SetVolume(basis + i, volume);
            NewMusicManager.PlaySound(basis + i, sound);
            break;
        }
    }

    public static void PlaySound(string basis, AudioClip sound, float volume = 0.65f) { PlaySound(basis, sound.name, volume); }

    public static bool TestPP(Color32[] playerMatrix, Color32[] bulletMatrix, float rotation, int playerHeight, int bulletHeight, Vector2 scale, Vector2 fromCenterProjectile) {
        int bulletWidth = bulletMatrix.Length / bulletHeight, playerWidth = playerMatrix.Length / playerHeight;
        rotation *= Mathf.Deg2Rad;

        for (int currentHeight = 0; currentHeight < playerHeight && currentHeight >= 0; currentHeight ++)
            for (int currentWidth = 0; currentWidth < playerWidth && currentWidth >= 0; currentWidth ++) {
                if (ControlPanel.instance.MinimumAlpha == 0) {
                    if (playerMatrix[currentHeight * playerWidth + currentWidth].a == 0)
                        continue;
                } else if (playerMatrix[currentHeight * playerWidth + currentWidth].a < ControlPanel.instance.MinimumAlpha)
                    continue;
                float dx = currentWidth + fromCenterProjectile.x,
                      dy = currentHeight + fromCenterProjectile.y;
                if (scale.x < 0) dx = -dx;
                if (scale.y < 0) dy = -dy;
                float DFromCenter = Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2)),
                      angle = Mathf.Atan2(dy, dx) - rotation,
                      totalValX = Mathf.RoundToInt(bulletWidth  / 2 + (Mathf.Cos(angle) * DFromCenter) / Mathf.Abs(scale.x)),
                      totalValY = Mathf.RoundToInt(bulletHeight / 2 + (Mathf.Sin(angle) * DFromCenter) / Mathf.Abs(scale.y));
                if (totalValY >= 0 && totalValY < bulletHeight && totalValX >= 0 && totalValX < bulletWidth)
                    if (bulletMatrix[Mathf.RoundToInt(totalValY * bulletWidth + totalValX)].a != 0)
                        return true;
            }
        return false;
    }

    //Non updated
    public static bool TestPPEasy(Color32[] playerMatrix, Color32[] basisMatrix, float rotation, int playerHeight, int basisHeight, Vector2 scale, Vector2 fromCenterProjectile) {
        int basisWidth = basisMatrix.Length / basisHeight, playerWidth = playerMatrix.Length / playerHeight;
        float rotationVal = rotation, iVal = 0, jVal = 0, angleVal = 0, Dval = 0;
        rotation *= Mathf.Deg2Rad;

        //if (rotation >= 135 && rotation <= 315)  shiftX--;
        //if (rotation >= 45 && rotation <= 235)   shiftY--;

        /*float realX = playerWidth * Mathf.Abs(Mathf.Cos(rotation)) + playerHeight * Mathf.Abs(Mathf.Sin(rotation)),
                realY = playerHeight * Mathf.Abs(Mathf.Cos(rotation)) + playerWidth * Mathf.Abs(Mathf.Sin(rotation));*/
        int totalValX = 0, totalValY = 0 /*, x = Mathf.FloorToInt(realX) + 2, y = Mathf.FloorToInt(realY) + 2*/;

        Vector2 fromCenterRotated = new Vector2();
        float playerD = Mathf.Sqrt(Mathf.Pow(fromCenterProjectile.x, 2) + Mathf.Pow(fromCenterProjectile.y, 2)),
                centerAngle = Mathf.Atan2(fromCenterProjectile.y, fromCenterProjectile.x),
                playerAngle = centerAngle - rotation;
        fromCenterRotated = new Vector2(Mathf.RoundToInt(Mathf.Cos(playerAngle) * playerD), Mathf.RoundToInt(Mathf.Cos(playerAngle) * playerD));

        try {
            for (float currentHeight = 0; currentHeight < playerHeight; currentHeight++)
                for (float currentWidth = 0; currentWidth < playerWidth; currentWidth++) {
                    totalValX = (int)fromCenterRotated.x; totalValY = (int)fromCenterRotated.y;
                    int tempCurrentHeight = Mathf.FloorToInt(currentHeight), tempCurrentWidth = Mathf.FloorToInt(currentWidth);
                    iVal = currentHeight; jVal = currentWidth;

                    if (ControlPanel.instance.MinimumAlpha == 0) {
                        if (playerMatrix[tempCurrentHeight * playerWidth + tempCurrentWidth].a == 0)
                            continue;
                    } else if (playerMatrix[tempCurrentHeight * playerWidth + tempCurrentWidth].a < ControlPanel.instance.MinimumAlpha)
                        //Debug.Log("Not Enough Alpha : X = " + tempCurrentWidth + ", Y = " + tempCurrentHeight);
                        continue;

                    float DFromCenter = Mathf.Sqrt(Mathf.Pow(tempCurrentHeight + fromCenterProjectile.y - playerHeight / 2, 2) +
                                                    Mathf.Pow(tempCurrentWidth + fromCenterProjectile.x - playerWidth / 2, 2)),
                            oldangle = Mathf.Atan2(tempCurrentHeight + fromCenterProjectile.y - playerHeight / 2, tempCurrentWidth + fromCenterProjectile.x - playerWidth / 2),
                            angle = oldangle - rotation;
                    Dval = DFromCenter;
                    angleVal = angle;
                    totalValX = Mathf.RoundToInt(basisWidth / 2 + Mathf.Cos(angle) * DFromCenter);
                    totalValY = Mathf.RoundToInt(basisHeight / 2 + Mathf.Sin(angle) * DFromCenter);
                    //int tempY = totalValY - (int)fromCenterRotated.y - basisHeight / 2, tempX = totalValX - (int)fromCenterRotated.x - basisWidth / 2;
                    //totalVal = tempY * x + tempX;
                    //Debug.Log("X = " + jVal + " Y = " + iVal + "Total = " + totalVal + " (" + tempY + "x" + x + " + " + tempX + ") / " + ret.Length);
                    if (totalValY >= 0 && totalValY < basisHeight && totalValX >= 0 && totalValX < basisWidth) /*|| tempY < 0 || tempY >= y || tempX < 0 || tempX >= x)
                    Debug.LogWarning("Out of bounds: X = " + currentWidth + " Y = " + currentHeight + "\ntotalRet = " + totalVal + " (" + (totalValY - (int)fromCenterRotated.y - basisHeight / 2) + "x" +
                                        x + " + " + (totalValX - (int)fromCenterRotated.x - basisWidth / 2) + ") / " + ret.Length + "          totalBasis = " +
                                        (totalValY * basisWidth + totalValX) + " (" + totalValY + "x" + basisWidth + " + " + totalValX + ") / " + basisMatrix.Length);
                else*/
                        if (basisMatrix[Mathf.RoundToInt(totalValY * basisWidth + totalValX)].a >= ControlPanel.instance.MinimumAlpha)
                            return true;
                }
        } catch {
            Debug.LogError("rotation = " + rotationVal + " D = " + Dval + " X = " + jVal + " Y = " + iVal +
                           " angle = " + (((angleVal + 4 * Mathf.PI) % 2 * Mathf.PI) / Mathf.PI) + "π" + /*"\ntotalRet = " + totalVal + " (" + 
                           (totalValY - (int)fromCenterRotated.y - basisHeight / 2) + "x" + x + " + " + (totalValX - (int)fromCenterRotated.x - basisWidth / 2) + 
                           ") / " + (x * y) + "          " + */"totalBasis = " + (totalValY * basisWidth + totalValX) + " (" + totalValY + "x" +
                           basisWidth + " + " + totalValX + ") / " + basisMatrix.Length);
        }
        return false;
        //return ret;
    }

    public static Color32[] RotateMatrixOld(Color32[] matrix, float rotation, int height, Vector2 scale, out Vector2 sizeDelta) {
        int width = matrix.Length / height, shiftX = 0, shiftY = 0;
        float rotationVal = rotation, iVal = 0, jVal = 0, angleVal = 0, Dval = 0;
        if (rotation >= 135 && rotation <= 315) shiftX--;
        if (rotation >= 45 && rotation <= 235) shiftY--;
        rotation *= Mathf.Deg2Rad;
        float realX = scale.x * (width * Mathf.Abs(Mathf.Cos(rotation)) + height * Mathf.Abs(Mathf.Sin(rotation))),
              realY = scale.y * (height * Mathf.Abs(Mathf.Cos(rotation)) + width * Mathf.Abs(Mathf.Sin(rotation)));
        int x = Mathf.FloorToInt(realX) + 2, y = Mathf.FloorToInt(realY) + 2, totalVal = 0, totalValX = 0, totalValY = 0;
        sizeDelta = new Vector2(x, y);
        Color32[] ret = new Color32[x * y];
        try {
            for (float currentHeight = 0; currentHeight < height; currentHeight += 1.0f / scale.x)
                for (float currentWidth = 0; currentWidth < width; currentWidth += 1.0f / scale.y) {
                    int tempCurrentHeight = Mathf.FloorToInt(currentHeight), tempCurrentWidth = Mathf.FloorToInt(currentWidth);
                    iVal = currentHeight + shiftY; jVal = currentWidth + shiftX;
                    if (ControlPanel.instance.MinimumAlpha == 0) {
                        if (matrix[tempCurrentHeight * width + tempCurrentWidth].a == 0)
                            continue;
                    } else if (matrix[tempCurrentHeight * width + tempCurrentWidth].a < ControlPanel.instance.MinimumAlpha)
                        continue;
                    float D = Mathf.Sqrt(Mathf.Pow(tempCurrentHeight - height / 2, 2) + Mathf.Pow(tempCurrentWidth - width / 2, 2)), oldangle;
                    Dval = D;
                    if (D == 0) oldangle = 0;
                    else oldangle = Mathf.Acos((tempCurrentWidth - width / 2) / D);
                    if (currentHeight <= height / 2f)
                        oldangle = -oldangle;
                    float angle = oldangle + rotation;
                    angleVal = angle;
                    totalValX = Mathf.RoundToInt(y / 2 + Mathf.Sin(angle) * D * scale.x);
                    totalValY = Mathf.RoundToInt(x / 2 + Mathf.Cos(angle) * D * scale.y);
                    totalVal = Mathf.RoundToInt((((y / 2 + Mathf.Sin(angle) * D) + shiftX) * x) + (x / 2 + Mathf.Cos(angle) * D) + shiftY);
                    ret[Mathf.RoundToInt((totalValX + shiftX) * x + (totalValY + shiftY))] = matrix[tempCurrentHeight * width + tempCurrentWidth];
                }
        } catch { Debug.LogError("Debug : rotation = " + rotationVal + " D = " + Dval + " currentWidth = " + jVal + " currentHeight = " + iVal + " angle(/Pi) = " + angleVal / Mathf.PI + " total = " + totalVal + " (" + totalValX + " / " + totalValY + ") / " + ret.Length); }
        return ret;
    }

    public static Rect GetFurthestCoordinates(Color32[] tex, int height, Transform tf) {
        int width = tex.Length / height;
        Vector4 coords = new Vector4(0, width, 0, height);
        for (int currentHeight = 0; currentHeight < height; currentHeight++)
            for (int currentWidth = 0; currentWidth < width; currentWidth++) {
                if (ControlPanel.instance.MinimumAlpha == 0) {
                    if (tex[currentHeight * width + currentWidth].a == 0)
                        continue;
                } else if (tex[currentHeight * width + currentWidth].a < ControlPanel.instance.MinimumAlpha)
                    continue;
                if (tex[currentHeight * width + currentWidth].a < ControlPanel.instance.MinimumAlpha)
                    continue;
                if (currentWidth > coords.x)  coords.x = currentWidth;  //maxHorz
                if (currentWidth < coords.y)  coords.y = currentWidth;  //minHorz
                if (currentHeight > coords.z) coords.z = currentHeight; //maxVert
                if (currentHeight < coords.w) coords.w = currentHeight; //minVert
            }
        Vector2 offset = new Vector2((coords.x + coords.y) / 2 - width / 2, (coords.z + coords.w) / 2 - height / 2);
        Rect maxDist = new Rect (tf.position.x - Mathf.Ceil((coords.x - coords.y + 1) / 2) + offset.x, 
                                 tf.position.y - Mathf.Ceil((coords.z - coords.w + 1) / 2) + offset.y, coords.x - coords.y + 1, coords.z - coords.w + 1);
        //Rect maxDist = new Rect (tf.position.x, tf.position.y, coords.y - coords.x, coords.w - coords.z);
        //Debug.Log(maxDist);
        return maxDist;
    }

    public static Transform GetChildPerName(Transform parent, string name, bool isInclusive = false, bool getInactive = false) {
        if (parent == null)
            throw new CYFException("If you want the parent to be null, search the object with GameObject.Find() directly.");

        Transform[] children = parent.GetComponentsInChildren<Transform>(getInactive);
        foreach (Transform go in children)
            if ((getInactive || (!getInactive && go.gameObject.activeInHierarchy)) && (go.name == name || (isInclusive && go.name.Contains(name))))
                return go.transform;
        return null;
    }

    public static Transform[] GetFirstChildren(Transform parent, bool getInactive = false) {
        Transform[] children, firstChildren, realFirstChildren;
        int index = 0;
        if (parent != null) {
            children = parent.GetComponentsInChildren<Transform>(getInactive);
            firstChildren = new Transform[parent.childCount];
            foreach (Transform child in children) {
                if (child.parent == parent) {
                    firstChildren[index] = child;
                    index++;
                }
            }
        } else {
            GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
            firstChildren = new Transform[gos.Length];
            foreach (GameObject root in gos)
                if (getInactive || root.activeInHierarchy) {
                    firstChildren[index] = root.transform;
                    index++;
                }
        }
        realFirstChildren = new Transform[index];
        for (int i = 0; i < index; i++)
            realFirstChildren[i] = firstChildren[i];

        return realFirstChildren;
    }

    public static Dictionary<string, string> MapCorrespondanceList = new Dictionary<string, string>();

    public static void AddKeysToMapCorrespondanceList() {
        MapCorrespondanceList.Add("test2", "Hotland - The test map");
        MapCorrespondanceList.Add("test-1", "How did you find this one?");
        MapCorrespondanceList.Add("Void", "The final map...?");
    }

    /*public static bool CheckAvailableDuster(out GameObject go) {
        go = null;
        ParticleSystem[] pss = GameObject.Find("psContainer").GetComponentsInChildren<ParticleSystem>(true);
        int a = pss.Length;
        for (int i = 0; i < a; i++) {
            if (!pss[i].gameObject.activeInHierarchy && pss[i].gameObject.name.Contains("MonsterDuster(Clone)")) {
                go = pss[i].gameObject;
                go.SetActive(true);
                go.transform.SetAsFirstSibling();
                return true;
            }
        }
        return false;
    }*/
}