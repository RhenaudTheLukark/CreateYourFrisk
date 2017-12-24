﻿using UnityEngine;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Utility class for the Unitale engine.
/// </summary>
public static class UnitaleUtil {
    internal static bool firstErrorShown = false; //Keeps track of whether an error already appeared, prevents subsequent errors from overriding the source.
    public static string printDebuggerBeforeInit = "";
    /*internal static string fileName = Application.dataPath + "/Logs/log-" + DateTime.Now.ToString().Replace('/', '-').Replace(':', '-') + ".txt";
    internal static StreamWriter sr;

    public static void createFile() {
        if (!Directory.Exists(Application.dataPath + "/Logs"))
            Directory.CreateDirectory(Application.dataPath + "/Logs");
        if (!File.Exists(fileName))
            sr = File.CreateText(fileName);
    }*/

    public static void WriteInLogAndDebugger(string mess) {
        try {
            /*sr.WriteLine("By DEBUG: " + mess.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t"));
            sr.Flush();*/
            UserDebugger.instance.UserWriteLine(mess);
        } catch /*(Exception e)*/ {
            //Debug.Log("Couldn't write on the log:\n" + e.Message + "\nMessage: " + mess);
            printDebuggerBeforeInit += (printDebuggerBeforeInit == "" ? "" : "\n") + mess;
        }
    }

    /*/// <summary>
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
    }*/

    public static bool IsOverworld {
        get {
            if (SceneManager.GetActiveScene().name == "TransitionOverworld")
                return true;
            if (GlobalControls.nonOWScenes.Contains(SceneManager.GetActiveScene().name))
                return false;
            return !GlobalControls.isInFight;
        }
    }

    /// <summary>
    /// Loads the Error scene with the Lua error that occurred.
    /// </summary>
    /// <param name="source">Name of the script that caused the error.</param>
    /// <param name="decoratedMessage">Error that was thrown. In MoonSharp's case, this is the DecoratedMessage property from its InterpreterExceptions.</param>
    public static void DisplayLuaError(string source, string decoratedMessage) {
        if (firstErrorShown)
            return;
        firstErrorShown = true;
        ErrorDisplay.Message = "error in script " + source + "\n\n" + decoratedMessage;
        if (Application.isEditor)
            SceneManager.LoadSceneAsync("Error"); // prevents editor from crashing
        else
            SceneManager.LoadScene("Error");
        Debug.Log("It's a Lua error! : " + ErrorDisplay.Message);
    }

    public static AudioSource GetCurrentOverworldAudio() {
        //if (GameObject.Find("Main Camera OW") && Camera.main != GameObject.Find("Main Camera OW")) return GameObject.Find("Main Camera OW").GetComponent<AudioSource>();
        return Camera.main.GetComponent<AudioSource>();
    }

    public static Vector3 VectToVector(GameState.Vect v)    { return new Vector3(v.x, v.y, v.z); }
    public static GameState.Vect VectorToVect(Vector3 vect) { return new GameState.Vect() { x = vect.x, y = vect.y, z = vect.z }; }

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
        return array;
    }

    public static Table DynValueArrayToTable(DynValue[] array) {
        Table table = new Table(null);
        for (int i = 1, l = array.Length; i <= l; i++)
            table.Set(i, array[i - 1]);
        return table;
    }

    public static float CalcTextWidth(TextManager txtmgr, int fromLetter = -1, int toLetter = -1) {
        float totalWidth = 0, totalWidthSpaceTest = 0, totalMaxWidth = 0, hSpacing = txtmgr.Charset.CharSpacing;
        if (fromLetter == -1)                                                                                       fromLetter = 0;
        if (txtmgr.textQueue == null)                                                                               return 0;
        if (txtmgr.textQueue[txtmgr.currentLine] == null)                                                           return 0;
        if (toLetter == -1)                                                                                         toLetter = txtmgr.textQueue[txtmgr.currentLine].Text.Length;
        if (fromLetter > toLetter || fromLetter < 0 || toLetter > txtmgr.textQueue[txtmgr.currentLine].Text.Length) return -1;
        if (fromLetter == toLetter)                                                                                 return 0;

        for (int i = fromLetter; i < toLetter; i++) {
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
                        hSpacing = ParseUtil.GetFloat(str.Split(':')[1]);
                    break;
                case '\r':
                case '\n':
                    if (totalMaxWidth < totalWidthSpaceTest - hSpacing)
                        totalMaxWidth = totalWidthSpaceTest - hSpacing;
                    totalWidth = 0;
                    totalWidthSpaceTest = 0;
                    break;
                default:
                    if (txtmgr.Charset.Letters.ContainsKey(txtmgr.textQueue[txtmgr.currentLine].Text[i])) {
                        totalWidth += txtmgr.Charset.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.x + hSpacing;
                        //Do not count end of line spaces
                        if (txtmgr.textQueue[txtmgr.currentLine].Text[i] != ' ')
                            totalWidthSpaceTest = totalWidth;
                    }
                    break;
            }
        }
        if (totalMaxWidth < totalWidthSpaceTest - hSpacing)
            totalMaxWidth = totalWidthSpaceTest - hSpacing;
        return totalMaxWidth;
    }

    public static float CalcTextHeight(TextManager txtmgr, int fromLetter = -1, int toLetter = -1) {
        float maxY = -999, minY = 999;
        if (fromLetter == -1) fromLetter = 0;
        if (toLetter == -1)   toLetter = txtmgr.textQueue[txtmgr.currentLine].Text.Length;
        if (fromLetter > toLetter || fromLetter < 0 || toLetter > txtmgr.textQueue[txtmgr.currentLine].Text.Length) return -1;
        if (fromLetter == toLetter)                                                                                 return 0;
        for (int i = fromLetter; i < toLetter; i++) {
            if (txtmgr.Charset.Letters.ContainsKey(txtmgr.textQueue[txtmgr.currentLine].Text[i])) {
                if (txtmgr.letterPositions[i].y < minY) {
                    //Debug.Log("minY change: going from " + minY + " to " + txtmgr.letterPositions[i].y);
                    minY = txtmgr.letterPositions[i].y;
                }
                if (txtmgr.letterPositions[i].y + txtmgr.Charset.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.y > maxY) {
                    //Debug.Log("maxY change: going from " + maxY + " to " + (txtmgr.letterPositions[i].y + txtmgr.Charset.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.y));
                    maxY = txtmgr.letterPositions[i].y + txtmgr.Charset.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.y;
                }
            }
        }
        //Debug.Log("The final height is " + (maxY - minY));
        return maxY - minY;
    }

    public static DynValue RebuildTableFromString(string text) {
        text.Trim();
        if (text[0] != '{' || text[text.Length-1] != '}') {
            Debug.LogError("RebuildTableFromString: The value given is not a reconstructible table!");
            return DynValue.Nil;
        }
        Table t = ConstructTable(text.TrimStart('{').TrimEnd('}'));
        return DynValue.NewTable(t);
    }

    private static Table ConstructTable(string text) {
        Table t = new Table(null);
        int inOtherTable = 0;
        string currentValue = "";
        DynValue valueName = null;

        bool inString = false, inSlashEffect1 = false, inSlashEffect2 = false;
        for (int i = 0; i < text.Length; i ++) {
            if (inSlashEffect1)      inSlashEffect1 = false;
            else if (inSlashEffect2) inSlashEffect2 = false;

            if (!inSlashEffect2) {
                if (text[i] == '{' && !inString) {
                    if (inOtherTable != 1)
                        currentValue += text[i];
                    inOtherTable++;
                } else if (text[i] == '}' && !inString) {
                    inOtherTable--;
                    if (inOtherTable == 0) {
                        if (valueName == null) t.Append(DynValue.NewTable(ConstructTable(currentValue)));
                        else                   t.Set(valueName, DynValue.NewTable(ConstructTable(currentValue)));
                        currentValue = "";
                        valueName = null;
                    } else
                        currentValue += text[i];
                } else if (text[i] == '"' && inOtherTable != 0)
                    inString = !inString;
                else if (text[i] == '\\') {
                    inSlashEffect1 = true;
                    inSlashEffect2 = true;
                } else if (text[i] == ',' && (!inString || inOtherTable != 0)) {
                    currentValue = currentValue.Trim();
                    Type type = CheckRealType(currentValue);
                    DynValue dv;
                    if (type == typeof(bool))       dv = DynValue.NewBoolean(currentValue == "true");
                    else if (type == typeof(float)) dv = DynValue.NewNumber(ParseUtil.GetFloat(currentValue));
                    else                            dv = DynValue.NewString(currentValue.Trim('"'));
                    if (valueName == null) t.Append(dv);
                    else                   t.Set(valueName, dv);
                    valueName = null;
                    currentValue = "";
                } else if (text[i] == '=' && (!inString || inOtherTable != 0)) {
                    currentValue = currentValue.Trim();
                    valueName = DynValue.NewString(currentValue);
                    Type type = CheckRealType(currentValue);
                    if (type == typeof(bool))       valueName = DynValue.NewBoolean(currentValue == "true");
                    else if (type == typeof(float)) valueName = DynValue.NewNumber(ParseUtil.GetFloat(currentValue));
                    else                            valueName = DynValue.NewString(currentValue.Trim('"'));
                } else
                    currentValue += text[i];
            } else
                currentValue += text[i];
        }

        return t;
    }

    public static string[] SpecialSplit(char c, string str, bool countTables = false) {
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

    /// <summary>
    /// Check DynValues parameter's value types. DON'T WORK WITH MULTIDIMENSIONNAL ARRAYS!
    /// (For now it doesn't work with arrays at all :c)
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static Type CheckRealType(string parameter) {
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

        //String
        //If all our attempts to check other types failed, this is really a string
        return typeof(string);
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

    public static bool TestPP(Color32[] playerMatrix, Color32[] bulletMatrix, float rotation, int playerHeight, int bulletHeight, Vector2 scale, Vector2 fromCenterProjectile, float spriteAlpha) {
        int bulletWidth = bulletMatrix.Length / bulletHeight, playerWidth = playerMatrix.Length / playerHeight;
        rotation *= Mathf.Deg2Rad;
        for (int currentHeight = 0; currentHeight < playerHeight && currentHeight >= 0; currentHeight ++)
            for (int currentWidth = 0; currentWidth < playerWidth && currentWidth >= 0; currentWidth ++) {
                if (ControlPanel.instance.MinimumAlpha == 0) {
                    if (playerMatrix[currentHeight * playerWidth + currentWidth].a * spriteAlpha == 0)
                        continue;
                } else if (playerMatrix[currentHeight * playerWidth + currentWidth].a * spriteAlpha < ControlPanel.instance.MinimumAlpha)
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
                    if (bulletMatrix[Mathf.RoundToInt(totalValY * bulletWidth + totalValX)].a != 0) {
                        //Debug.Log("pixel at (" + "(" + bulletWidth  / 2 + " + (" + Mathf.Cos(angle) + " * " + DFromCenter + ") / " + Mathf.Abs(scale.x) + ") = " + totalValX + ", " + 
                        //                         "(" + bulletHeight / 2 + " + (" + Mathf.Sin(angle) + " * " + DFromCenter + ") / " + Mathf.Abs(scale.y) + ") = " + totalValY + ") " +
                        //                         "is full pixel: PP detected for player pixel = (" + currentWidth + ", " + currentHeight + ")!");
                        return true;
                    }
            }
        return false;
    }

    /*//No updated
    public static bool TestPPEasy(Color32[] playerMatrix, Color32[] basisMatrix, float rotation, int playerHeight, int basisHeight, Vector2 scale, Vector2 fromCenterProjectile) {
        int basisWidth = basisMatrix.Length / basisHeight, playerWidth = playerMatrix.Length / playerHeight;
        float rotationVal = rotation, iVal = 0, jVal = 0, angleVal = 0, Dval = 0, totalVal = 0;
        rotation *= Mathf.Deg2Rad;

        float realX = playerWidth * Mathf.Abs(Mathf.Cos(rotation)) + playerHeight * Mathf.Abs(Mathf.Sin(rotation)),
              realY = playerHeight * Mathf.Abs(Mathf.Cos(rotation)) + playerWidth * Mathf.Abs(Mathf.Sin(rotation));
        int totalValX = 0, totalValY = 0, x = Mathf.FloorToInt(realX) + 2, y = Mathf.FloorToInt(realY) + 2;

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
                    } else if (playerMatrix[tempCurrentHeight * playerWidth + tempCurrentWidth].a < ControlPanel.instance.MinimumAlpha) {
                        Debug.Log("Not Enough Alpha : X = " + tempCurrentWidth + ", Y = " + tempCurrentHeight);
                        continue;
                    }

                    float DFromCenter = Mathf.Sqrt(Mathf.Pow(tempCurrentHeight + fromCenterProjectile.y - playerHeight / 2, 2) +
                                                   Mathf.Pow(tempCurrentWidth + fromCenterProjectile.x - playerWidth / 2, 2)),
                          oldangle = Mathf.Atan2(tempCurrentHeight + fromCenterProjectile.y - playerHeight / 2, tempCurrentWidth + fromCenterProjectile.x - playerWidth / 2),
                          angle = oldangle - rotation;
                    Dval = DFromCenter;
                    angleVal = angle;
                    totalValX = Mathf.RoundToInt(basisWidth / 2 + Mathf.Cos(angle) * DFromCenter);
                    totalValY = Mathf.RoundToInt(basisHeight / 2 + Mathf.Sin(angle) * DFromCenter);
                    int tempY = totalValY - (int)fromCenterRotated.y - basisHeight / 2, tempX = totalValX - (int)fromCenterRotated.x - basisWidth / 2;
                    totalVal = tempY * x + tempX;

                    Debug.Log("X = " + jVal + " Y = " + iVal + "Total = " + totalVal + " (" + tempY + "x" + x + " + " + tempX + ")");
                    if (totalValY >= 0 && totalValY < basisHeight && totalValX >= 0 && totalValX < basisWidth || tempY < 0 || tempY >= y || tempX < 0 || tempX >= x)
                        Debug.LogWarning("Out of bounds: X = " + currentWidth + " Y = " + currentHeight + "\ntotalRet = " + totalVal + " (" + (totalValY - (int)fromCenterRotated.y - basisHeight / 2) +
                                         "x" + x + " + " + (totalValX - (int)fromCenterRotated.x - basisWidth / 2) + ")          totalBasis = " +
                                         (totalValY * basisWidth + totalValX) + " (" + totalValY + "x" + basisWidth + " + " + totalValX + ") / " + basisMatrix.Length);
                    else if (basisMatrix[Mathf.RoundToInt(totalValY * basisWidth + totalValX)].a >= ControlPanel.instance.MinimumAlpha)
                        return true;
                }
        } catch {
            Debug.LogError("rotation = " + rotationVal + " D = " + Dval + " X = " + jVal + " Y = " + iVal +
                           " angle = " + (((angleVal + 4 * Mathf.PI) % 2 * Mathf.PI) / Mathf.PI) + "π" + "\ntotalRet = " + totalVal + " (" + 
                           (totalValY - (int)fromCenterRotated.y - basisHeight / 2) + "x" + x + " + " + (totalValX - (int)fromCenterRotated.x - basisWidth / 2) + 
                           ") / " + (x * y) + "          " + "totalBasis = " + (totalValY * basisWidth + totalValX) + " (" + totalValY + "x" +
                           basisWidth + " + " + totalValX + ") / " + basisMatrix.Length);
        }
        return false;
        //return ret;
    }*/

    /*public static Color32[] RotateMatrixOld(Color32[] matrix, float rotation, int height, Vector2 scale, out Vector2 sizeDelta) {
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
    }*/

    /*public static Rect GetFurthestCoordinates(Color32[] tex, int height, Transform tf) {
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
    }*/

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
        Transform[] children, firstChildren;
        int index = 0;
        if (parent != null) {
            children = parent.GetComponentsInChildren<Transform>(getInactive);
            firstChildren = new Transform[parent.childCount];
            foreach (Transform child in children)
                if (child.parent == parent) {
                    firstChildren[index] = child;
                    index++;
                }
        } else {
            List<Transform> tfs = new List<Transform>();
            foreach (Transform tf in Resources.FindObjectsOfTypeAll<Transform>().Where(o => o.hideFlags == HideFlags.None).ToList())
                if (tf.parent == null)
                    tfs.Add(tf);
            firstChildren = new Transform[tfs.Count];
            foreach (Transform root in tfs)
                if (getInactive || root.gameObject.activeInHierarchy) {
                    firstChildren[index] = root;
                    index++;
                }
        }
        return firstChildren;
    }

    public static Dictionary<string, string> MapCorrespondanceList = new Dictionary<string, string>();

    public static void AddKeysToMapCorrespondanceList() {
        MapCorrespondanceList.Add("test", "Snowdin - The test map");
        MapCorrespondanceList.Add("test2", "Hotland - The test map");
        MapCorrespondanceList.Add("test3", "The Core - The test map");
        MapCorrespondanceList.Add("test4", "The Core - Parallel universe");
        MapCorrespondanceList.Add("test5", "Snowdin - Parallax universe");
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

    // ******************************************************************************
    // *** Warning: Complex System.Reflexion stuff below! Enter at your own risk! *** 
    // ******************************************************************************

    /*/// <summary>
    /// Complex af
    /// Used like this: MethodInfo method = MethodOf(() => controller.Method3(default(int)));
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static MethodInfo MethodOf(Expression<Action> expression) {
        return ((MethodCallExpression)expression.Body).Method;
    }

    /// <summary>
    /// Checks if the expression given has the CYFEventFunction attribute I guess
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static bool MethodHasCYFEventFunctionAttribute(MethodInfo mf) {
        const bool includeInherited = false;
        return mf.GetCustomAttributes(typeof(CYFEventFunction), includeInherited).Any();
    }
    private static bool MethodHasCYFEventFunctionAttribute(Expression<Action> expression) {
        return MethodHasCYFEventFunctionAttribute(MethodOf(expression));
    }*/
}
