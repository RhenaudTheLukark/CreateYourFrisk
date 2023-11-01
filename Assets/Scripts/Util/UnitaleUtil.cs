using UnityEngine;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Utility class for the Unitale engine.
/// </summary>
public static class UnitaleUtil {
    internal static bool firstErrorShown; //Keeps track of whether an error already appeared, prevents subsequent errors from overriding the source.
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

    public static void Warn(string line, bool show = true) {
        line = "[WARN]" + line;
        if (!GlobalControls.retroMode && show) {
            WriteInLogAndDebugger(line);
            return;
        }
        try { UserDebugger.instance.Warn(line); } catch { printDebuggerBeforeInit += (printDebuggerBeforeInit == "" ? "" : "\n") + line; }
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
    /// <param name="DoNotDecorateMessage">Set to true to hide "error in script x" at the top. This arg is true when using error(..., 0).</param>
    public static void DisplayLuaError(string source, string decoratedMessage, bool DoNotDecorateMessage = false) {
        if (firstErrorShown)
            return;
        firstErrorShown = true;
        ScreenResolution.ResetAfterBattle();
        ErrorDisplay.Message = (!DoNotDecorateMessage ? "error in script " + source + "\n\n" : "") + decoratedMessage;
        if (Application.isEditor) SceneManager.LoadSceneAsync("Error"); // prevents editor from crashing
        else                      SceneManager.LoadScene("Error");
        Debug.Log("It's a Lua error! : " + ErrorDisplay.Message);
        ScreenResolution.wideFullscreen = true;
    }

    /// <summary>
    /// Handles most CYF errors related to script execution
    /// </summary>
    /// <param name="scriptname">Name of the script, used for the error message</param>
    /// <param name="function">Name of the function, used for the error message</param>
    /// <param name="e">Exception to handle</param>
    public static void HandleError(string scriptname, string function, Exception e) {
        if (e as InterpreterException != null) {
            InterpreterException ie = e as InterpreterException;
            DisplayLuaError(scriptname, ie.DecoratedMessage == null ? ie.Message : FormatErrorSource(ie.DecoratedMessage, ie.Message) + ie.Message, ie.DoNotDecorateMessage);
        } else if (GlobalControls.retroMode)
            return;
        else if (e.GetType().ToString() == "System.IndexOutOfRangeException" && e.StackTrace.Contains("at MoonSharp.Interpreter.DataStructs.FastStack`1[MoonSharp.Interpreter.DynValue].Push"))
            DisplayLuaError(scriptname + ", calling the function " + function, "<b>Possible infinite loop</b>\n\nThis is a " + e.GetType() + " error."
                                                                             + "\n\nYou almost definitely have an infinite loop in your code. A function tried to call itself infinitely. It could be a normal function or a metatable function."
                                                                             + "\n\nFull stracktrace (see CYF output log at <b>" + Application.persistentDataPath + "/output_log.txt</b>):"
                                                                             + "\n" + e.StackTrace);
        else
            DisplayLuaError(scriptname + ", calling the function " + function, "This is a " + e.GetType() + " error. Contact a dev and show them this screen, this must be an engine-side error."
                                                                             + "\n\n" + e.Message
                                                                             + "\n\nFull stracktrace (see CYF output log at <b>" + Application.persistentDataPath + "/output_log.txt</b>):"
                                                                             + "\n" + e.StackTrace + "\n");
    }

    public static string FormatErrorSource(string DecoratedMessage, string message) {
        string source = DecoratedMessage.Substring(0, DecoratedMessage.Length - message.Length);
        Regex validator = new Regex(@"\(\d+,\d+(-[\d,]+)?\)"); //Finds `(13,9-16)` or `(13,9-14,10)` or `(20,0)`
        Match scanned = validator.Match(source);
        if (!scanned.Success) return source;
        string stacktrace = scanned.Value;
        validator = new Regex(@"(\d+),(\d+)"); //Finds `13,9`
        MatchCollection matches = validator.Matches(stacktrace);

        //Add "line " and "char " before some numbers
        return matches.Cast<Match>().Aggregate(source, (current, match) => current.Replace(match.Value, "line " + match.Groups[1].Value + ", char " + match.Groups[2].Value));
    }

    public static AudioSource GetCurrentOverworldAudio() {
        //if (GameObject.Find("Main Camera OW") && Camera.main != GameObject.Find("Main Camera OW")) return GameObject.Find("Main Camera OW").GetComponent<AudioSource>();
        return GameObject.Find("Background").GetComponent<MapInfos>().isMusicKeptBetweenBattles ? PlayerOverworld.audioKept : Camera.main.GetComponent<AudioSource>();
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
        for (int i = 1; i <= table.Length; i++) {
            DynValue v = table.Get(i);
            array[i - 1] = v;
        }
        return array;
    }

    public static Table DynValueArrayToTable(DynValue[] array) {
        Table table = new Table(null);
        for (int i = 1, l = array.Length; i <= l; i++)
            table.Set(i, array[i - 1]);
        return table;
    }

    public static string ParseCommandInline(string input, ref int currentChar, bool onlyAcceptExistingCommands = true) {
        int start = currentChar;
        currentChar++;
        string control = ""; int count = 1;
        for (; currentChar < input.Length; currentChar++) {
            if (input[currentChar] == '[')
                count++;
            else if (input[currentChar] == ']') {
                count--;
                if (count == 0) {
                    if (onlyAcceptExistingCommands && !TextManager.commandList.Contains(control.Split(':')[0]))
                        break;
                    return control;
                }
            }
            control += input[currentChar];
        }
        currentChar = start;
        return null;
    }

    /// <summary>
    /// Predicts the length of the text using the letters' size and various tags.
    /// </summary>
    /// <param name="txtmgr">Text object</param>
    /// <param name="fromLetter">Letter of the current line of text to count from</param>
    /// <param name="toLetter">Letter of the current line of text to count to</param>
    /// <param name="countEOLSpace">True if we count spaces (spaces are usually skipped)</param>
    /// <param name="getLastSpace">True if we count the letter spacing after the last letter of the text</param>
    /// <returns>The length of the text in pixels</returns>
    public static float PredictTextWidth(TextManager txtmgr, int fromLetter = -1, int toLetter = -1, bool countEOLSpace = false) {
        float totalWidth = 0, totalWidthSpaceTest = 0, totalMaxWidth = 0, hSpacing = txtmgr.font.CharSpacing, columns = 0;
        List<float> columnsMaxWidth = new List<float>();
        if (fromLetter == -1)                                                                                       fromLetter = 0;
        if (txtmgr.textQueue == null)                                                                               return 0;
        if (txtmgr.textQueue[txtmgr.currentLine] == null)                                                           return 0;
        if (toLetter == -1)                                                                                         toLetter = txtmgr.textQueue[txtmgr.currentLine].Text.Length - 1;
        if (fromLetter > toLetter || fromLetter < 0 || toLetter > txtmgr.textQueue[txtmgr.currentLine].Text.Length) return -1;

        for (int i = fromLetter; i <= toLetter; i++) {
            switch (txtmgr.textQueue[txtmgr.currentLine].Text[i]) {
                case '[':
                    string str = ParseCommandInline(txtmgr.textQueue[txtmgr.currentLine].Text, ref i);
                    if (str == null) {
                        if (txtmgr.font.Letters.ContainsKey(txtmgr.textQueue[txtmgr.currentLine].Text[i]))
                            totalWidth += txtmgr.font.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.x + hSpacing;
                    } else if (str.Split(':')[0] == "charspacing")
                        hSpacing = str.Split(':')[1].ToLower() == "default" ? txtmgr.font.CharSpacing : ParseUtil.GetFloat(str.Split(':')[1]);
                    break;
                case '\t':
                    // Add columns if they're not empty or filled with spaces
                    if (totalWidthSpaceTest == totalWidth)
                        columnsMaxWidth.Add(totalWidth);
                    totalWidth = txtmgr.columnShift * ++columns;
                    if (countEOLSpace)
                        totalWidthSpaceTest = totalWidth;
                    break;
                case '\r':
                case '\n':
                    columns = 0;
                    columnsMaxWidth.Add(totalWidthSpaceTest);
                    totalWidthSpaceTest = columnsMaxWidth.Max(w => w);
                    totalMaxWidth = Mathf.Max(totalMaxWidth, totalWidthSpaceTest - hSpacing);
                    totalWidth = 0;
                    totalWidthSpaceTest = 0;
                    columnsMaxWidth.Clear();
                    break;
                default:
                    if (txtmgr.font.Letters.ContainsKey(txtmgr.textQueue[txtmgr.currentLine].Text[i])) {
                        totalWidth += txtmgr.font.Letters[txtmgr.textQueue[txtmgr.currentLine].Text[i]].textureRect.size.x + hSpacing;
                        // Do not count spaces
                        if (txtmgr.textQueue[txtmgr.currentLine].Text[i] != ' ' || countEOLSpace)
                            totalWidthSpaceTest = totalWidth;
                    }
                    break;
            }
        }
        totalMaxWidth = Mathf.Max(totalMaxWidth, totalWidthSpaceTest - hSpacing);
        return Mathf.Max(totalMaxWidth, 0);
    }

    /// <summary>
    /// Computes the text's width using the x position of all of the text's letters.
    /// This function assumes the text's letters have been created.
    /// </summary>
    /// <param name="txtmgr">Text object</param>
    /// <param name="fromLetter">Letter of the current line of text to count from</param>
    /// <param name="toLetter">Letter of the current line of text to count to</param>
    /// <param name="countEOLSpace">True if we count spaces (spaces are usually skipped)</param>
    /// <returns>The length of the text in pixels</returns>
    public static float CalcTextWidth(TextManager txtmgr, int fromLetter = -1, int toLetter = -1, bool countEOLSpace = false) {
        if (txtmgr.textQueue == null || txtmgr.textQueue[txtmgr.currentLine] == null)                                return 0;
        if (fromLetter > toLetter || fromLetter < -1 || toLetter > txtmgr.textQueue[txtmgr.currentLine].Text.Length) return 0;
        if (fromLetter == -1)                                                                                        fromLetter = 0;
        if (toLetter == -1)                                                                                          toLetter = txtmgr.textQueue[txtmgr.currentLine].Text.Length - 1;

        float maxX = Mathf.NegativeInfinity, minX = Mathf.Infinity;
        LuaTextManager ltm = txtmgr as LuaTextManager;

        // Add text pos in case of tab
        if (fromLetter == 0) {
            minX = Mathf.Min(minX, ltm.absx);
            maxX = Mathf.Max(maxX, ltm.absx);
        }

        for (int i = fromLetter; i <= toLetter; i++) {
            if (!txtmgr.letters.Any(l => l.index == i))
                continue;
            if (txtmgr.textQueue[txtmgr.currentLine].Text[i] == ' ' && !countEOLSpace)
                continue;

            TextManager.LetterData letter = txtmgr.letters.Find(l => l.index == i);
            float letterPosMin = letter.image.rectTransform.position.x,
                  letterPosMax = letter.image.rectTransform.position.x + letter.image.rectTransform.rect.width * letter.image.rectTransform.localScale.x;
            minX = Mathf.Min(minX, letterPosMin, letterPosMax);
            maxX = Mathf.Max(maxX, letterPosMin, letterPosMax);
        }
        return Mathf.Max(maxX - minX, 0) / (ltm ? ltm.xscale : 1);
    }

    public static float CalcTextHeight(TextManager txtmgr, int fromLetter = -1, int toLetter = -1, bool countEOLSpace = false) {
        if (txtmgr.textQueue == null || txtmgr.textQueue[txtmgr.currentLine] == null)                               return 0;
        if (fromLetter == -1)                                                                                       fromLetter = 0;
        if (toLetter == -1)                                                                                         toLetter = txtmgr.textQueue[txtmgr.currentLine].Text.Length - 1;
        if (fromLetter > toLetter || fromLetter < 0 || toLetter > txtmgr.textQueue[txtmgr.currentLine].Text.Length) return 0;

        float maxY = Mathf.NegativeInfinity, minY = Mathf.Infinity;
        LuaTextManager ltm = txtmgr as LuaTextManager;

        for (int i = fromLetter; i <= toLetter; i++) {
            if (!txtmgr.letters.Any(l => l.index == i))
                continue;
            if (txtmgr.textQueue[txtmgr.currentLine].Text[i] == ' ' && !countEOLSpace)
                continue;

            TextManager.LetterData letter = txtmgr.letters.Find(l => l.index == i);
            float letterPosMin = letter.image.rectTransform.position.y,
                  letterPosMax = letter.image.rectTransform.position.y + letter.image.rectTransform.rect.height * letter.image.rectTransform.localScale.y;
            minY = Mathf.Min(minY, letterPosMin, letterPosMax);
            maxY = Mathf.Max(maxY, letterPosMin, letterPosMax);
        }
        return Mathf.Max(maxY - minY, 0) / (ltm ? ltm.yscale : 1);
    }

    public static DynValue RebuildTableFromString(string text) {
        text = text.Trim();
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
        foreach (char c in text) {
            if (inSlashEffect1)      inSlashEffect1 = false;
            else if (inSlashEffect2) inSlashEffect2 = false;

            if (!inSlashEffect2) {
                if (c == '{' && !inString) {
                    if (inOtherTable != 1)
                        currentValue += c;
                    inOtherTable++;
                } else if (c == '}' && !inString) {
                    inOtherTable--;
                    if (inOtherTable == 0) {
                        if (valueName == null) t.Append(DynValue.NewTable(ConstructTable(currentValue)));
                        else                   t.Set(valueName, DynValue.NewTable(ConstructTable(currentValue)));
                        currentValue = "";
                        valueName    = null;
                    } else
                        currentValue += c;
                } else if (c == '"' && inOtherTable != 0)
                    inString = !inString;
                else if (c == '\\') {
                    inSlashEffect1 = true;
                    inSlashEffect2 = true;
                } else if (c == ',' && (!inString || inOtherTable != 0)) {
                    currentValue = currentValue.Trim();
                    Type     type = CheckRealType(currentValue);
                    DynValue dv;
                    if (type      == typeof(bool))       dv = DynValue.NewBoolean(currentValue == "true");
                    else if (type == typeof(float)) dv      = DynValue.NewNumber(ParseUtil.GetFloat(currentValue));
                    else                            dv      = DynValue.NewString(currentValue.Trim('"'));
                    if (valueName == null) t.Append(dv);
                    else                   t.Set(valueName, dv);
                    valueName    = null;
                    currentValue = "";
                } else if (c == '=' && (!inString || inOtherTable != 0)) {
                    currentValue = currentValue.Trim();
                    Type type = CheckRealType(currentValue);
                    if (type == typeof(bool))       valueName = DynValue.NewBoolean(currentValue == "true");
                    else if (type == typeof(float)) valueName = DynValue.NewNumber(ParseUtil.GetFloat(currentValue));
                    else                            valueName = DynValue.NewString(currentValue.Trim('"'));
                } else
                    currentValue += c;
            } else
                currentValue += c;
        }

        // Parse one final time for the end of the string
        Type typey = CheckRealType(currentValue);
        DynValue dynv;
        if (typey == typeof(bool))       dynv = DynValue.NewBoolean(currentValue == "true");
        else if (typey == typeof(float)) dynv = DynValue.NewNumber(ParseUtil.GetFloat(currentValue));
        else                            dynv = DynValue.NewString(currentValue.Trim('"'));
        if (valueName == null) t.Append(dynv);
        else                   t.Set(valueName, dynv);

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
            if (!countTables) continue;
            if (str[i] == '}' && tableStack > 0)
                tableStack--;
        }
        tempArray.Add(str.Substring(lastIndex, str.Length - lastIndex).Trim());
        return (string[])ListToArray(tempArray);
    }

    public static void Dust(GameObject go, LuaSpriteController spr) {
        if (go.GetComponent<ParticleDuplicator>() == null)
            go.AddComponent<ParticleDuplicator>();

        // Move it to the nearest remanent object
        while (go.transform.parent.GetComponent<CYFSprite>() && go.transform.parent.GetComponent<CYFSprite>().ctrl.limbo)
            go.transform.SetParent(go.transform.parent.parent);

        go.GetComponent<ParticleDuplicator>().Activate(spr);
    }

    /// <summary>
    /// Check DynValues parameter's value types. DOESN'T WORK WITH MULTIDIMENSIONNAL ARRAYS!
    /// (For now it doesn't work with arrays at all :c)
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static Type CheckRealType(string parameter) {
        string parameterNoSpace = parameter.Replace(" ", "");

        //Boolean
        //If the string is equal to "true" or "false", this is a boolean
        if (parameterNoSpace == "false" || parameterNoSpace == "true")
            return typeof(bool);

        //Number (here float)
        bool isNumber = parameterNoSpace.All(c => c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9' || c == '.' || c == ',' || c == ' ' || c == '-');
        //If each parameter is a number, a dot, a space or a minus, this is a number
        return isNumber ? typeof(float) : typeof(string);

        //String
        //If all our attempts to check other types failed, this is really a string
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
                if (Mathf.Floor(number) == 0)      { zerocount++;   ninecount = 0; }
                else if (Mathf.Floor(number) == 9) { ninecount++;   zerocount = 0; }
                else                               { ninecount = 0; zerocount = 0; }
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

    public static bool TestContainsListVector2(List<Vector2> list, int testValue) {
        return list.Any(v => v.x == testValue);
    }

    public static void PlaySound(string basis, string sound, float volume = 0.65f) {
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
            NewMusicManager.PlaySound(basis + i, sound);
            NewMusicManager.SetVolume(basis + i, volume);
            break;
        }
    }

    public static void PlayVoice(string basis, string voice, float volume = 0.65f) { PlaySound(basis, "Voices/" + voice, volume); }

    /// <summary>
    /// Checks if the Player and a given bullet collide.
    /// TODO: Extend this function so it can handle two different random objects, instead of forcing one of them to be the Player.
    /// </summary>
    /// <param name="playerMatrix">List of pixels depicting the Player's hitbox. Currently is 8x8 with no transparent pixel.</param>
    /// <param name="bulletMatrix">List of pixels depicting the bullet's sprite.</param>
    /// <param name="rotation">Difference between the bullet's rotation and the Player's rotation.</param>
    /// <param name="playerHeight">Height of the Player's hitbox. Currently always 8.</param>
    /// <param name="bulletHeight">Height of the bullet's sprite.</param>
    /// <param name="scale">Scale difference between the bullet and the Player.</param>
    /// <param name="fromCenterProjectile">Position difference between the two objects.</param>
    /// <returns>True if the two objects collide, false otherwise.</returns>
    public static bool TestPP(Color32[] playerMatrix, Color32[] bulletMatrix, float rotation, int playerHeight, int bulletHeight, Vector2 scale, Vector2 fromCenterProjectile) {
        // Get the bullet's and Player's widths
        int bulletWidth = bulletMatrix.Length / bulletHeight, playerWidth = playerMatrix.Length / playerHeight;
        // As rotation is given in degrees, transform it into a value in radians
        rotation *= Mathf.Deg2Rad;
        // Setup vectors to store the starting point and vertical and horizontal distance between the bullet's pixels
        Vector2 start = new Vector2(), xDiff = new Vector2(), yDiff = new Vector2();

        // For each pixel in the Player's hitbox...
        for (int currentHeight = 0; currentHeight < playerHeight && currentHeight >= 0; currentHeight ++)
            for (int currentWidth = 0; currentWidth < playerWidth && currentWidth >= 0; currentWidth ++) {
                int roundedValX, roundedValY;

                // Three times: (0, 0), (1, 0) and (0, 1)
                if ((currentWidth == 0 && currentHeight < 2) || (currentWidth < 2 && currentHeight == 0)) {
                    // Compute the horizontal and vertical distance between the two objects
                    float dx = currentWidth + fromCenterProjectile.x,
                          dy = currentHeight + fromCenterProjectile.y;
                    if (scale.x < 0) dx = -dx;
                    if (scale.y < 0) dy = -dy;

                    // Compute the distance between the center of the two objects
                    float DFromCenter = Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2)),
                    // Compute the angle between the two objects
                          angle = Mathf.Atan2(dy, dx) - rotation,
                    // Compute the real coordinates of the bullet's pixel relative to the Player's hitbox
                          fullValX = bulletWidth  / 2f + Mathf.Cos(angle) * DFromCenter / Mathf.Abs(scale.x),
                          fullValY = bulletHeight / 2f + Mathf.Sin(angle) * DFromCenter / Mathf.Abs(scale.y);
                    // Get a rounded value for table checks
                    roundedValX = Mathf.FloorToInt(fullValX);
                    roundedValY = Mathf.FloorToInt(fullValY);

                    // Compute the starting point for (0, 0)
                    if (currentWidth == 0 && currentHeight == 0) start = new Vector2(fullValX, fullValY);
                    // Compute the distance between two horizontal pixels for (1, 0)
                    else if (currentHeight == 0)                 xDiff = new Vector2(fullValX - start.x, fullValY - start.y);
                    // Compute the distance between two vertical pixels for (0, 1)
                    else                                         yDiff = new Vector2(fullValX - start.x, fullValY - start.y);
                // Use the distance and starting point we computed above to compute where the current pixel is
                } else {
                    roundedValX = Mathf.FloorToInt(start.x + xDiff.x * currentWidth + yDiff.x * currentHeight);
                    roundedValY = Mathf.FloorToInt(start.y + xDiff.y * currentWidth + yDiff.y * currentHeight);
                }

                // Don't check the computed bullet's pixel if it doesn't exist, duh
                int pixelIndex = Mathf.FloorToInt(roundedValY * bulletWidth + roundedValX);
                if (pixelIndex < 0 || pixelIndex >= bulletMatrix.Length)
                    continue;

                // If the bullet's pixel's alpha is 0, continue to the next pixel
                if (bulletMatrix[pixelIndex].a == 0) continue;
                if (roundedValY >= 0 && roundedValY < bulletHeight && roundedValX >= 0 && roundedValX < bulletWidth)
                    // All checks are passed: the two objects collide
                    return true;
            }
        // After checking all pixels in the Player's hitbox, if we haven't found a colliding pixel, then the two objects don't collide
        return false;
    }

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
            throw new CYFException("If you want the parent to be null, find the object with GameObject.Find() directly.");

        Transform[] children = parent.GetComponentsInChildren<Transform>(getInactive);
        return (from go in children where (getInactive || go.gameObject.activeInHierarchy) && (go.name == name || isInclusive && go.name.Contains(name)) select go.transform).FirstOrDefault();
    }

    public static List<Transform> GetFirstChildren(Transform parent, bool getInactive = false) {
        List<Transform> firstChildren = new List<Transform>();
        firstChildren.AddRange(parent != null ? parent.GetComponentsInChildren<Transform>(getInactive).Where(child => child.parent == parent)
                                              : Resources.FindObjectsOfTypeAll<Transform>().Where(tf => tf.hideFlags == HideFlags.None && tf.parent == null && (getInactive || tf.gameObject.activeInHierarchy)));
        return firstChildren;
    }

    public static void RemoveChildren(GameObject go, bool immediate = false, bool firstPass = true) {
        foreach (Transform t in GetFirstChildren(go.transform, true)) {
            // Bullet to delete recursively
            if (t.GetComponent<Projectile>())
                t.GetComponent<Projectile>().ctrl.Remove();
            // Sprite to delete recursively
            else if (t.GetComponent<CYFSprite>())
                if (immediate) LuaSpriteController.GetOrCreate(t.gameObject).spr.LateUpdate();
                else           LuaSpriteController.GetOrCreate(t.gameObject).Remove();
            // Text object to delete
            else if (t.GetComponentInChildren<LuaTextManager>())
                t.GetComponentInChildren<LuaTextManager>().Remove();
            // Dusting object: move it back to a valid parent
            else if (t.GetComponentInChildren<ParticleSystem>())
                while (t.parent.GetComponent<CYFSprite>() && (t.parent.GetComponent<CYFSprite>().ctrl.limbo || t.transform.parent.gameObject == go))
                    t.SetParent(t.parent.parent);
            // Normally this shouldn't happen, just a failsafe
            else
                throw new CYFException("For some reason, it seems you're trying to remove something which is neither a sprite, bullet or text object.");
        }

        if (firstPass && !immediate)
            RemoveChildren(go, false, false);
    }

    public static Dictionary<string, string> MapCorrespondanceList = new Dictionary<string, string>();

    public static void AddKeysToMapCorrespondanceList() {
        MapCorrespondanceList.Clear();
        MapCorrespondanceList.Add("test", "Snowdin - Big boy map");
        MapCorrespondanceList.Add("test2", "Hotland - Crossroads");
        // MapCorrespondanceList.Add("test3", "The Core - The test map");
        MapCorrespondanceList.Add("test4", "The Core - Bridge");
        MapCorrespondanceList.Add("test5", "Snowdin - Cooler bridge");
        MapCorrespondanceList.Add("test-1", "How did you find this one?");
        MapCorrespondanceList.Add("Void", "The final map...?");
    }

    public static void ResetOW(bool resetSave = false) {
        EventManager.instance = null;
        GameState.current = null;
        ItemBoxUI.active = false;
        GlobalControls.realName = null;
        PlayerOverworld.instance = null;
        PlayerOverworld.audioCurrTime = 0;
        PlayerOverworld.audioKept = null;
        if (resetSave)
            SaveLoad.Load();
        ShopScript.scriptName = null;
    }

    public static void ExitOverworld(bool totalUnload = true) {
        foreach (string str in NewMusicManager.audiolist.Keys)
            if ((AudioSource)NewMusicManager.audiolist[str] != null && str != "src")
                Object.Destroy(((AudioSource)NewMusicManager.audiolist[str]).gameObject);
        NewMusicManager.audiolist.Clear();
        NewMusicManager.audioname.Clear();
        Object.Destroy(GameObject.Find("Player"));
        Object.Destroy(GameObject.Find("Canvas OW"));
        Object.Destroy(GameObject.Find("Canvas Two"));
        if (GameOverBehavior.gameOverContainerOw)
            Object.Destroy(GameOverBehavior.gameOverContainerOw);
        StaticInits.InitAll("@Title");
        ResetOW(true);
        PlayerCharacter.instance.Reset();
        Inventory.inventory.Clear();
        Inventory.RemoveAddedItems();
        ScriptWrapper.instances.Clear();
        GlobalControls.isInFight = false;
        GlobalControls.isInShop = false;
        LuaScriptBinder.ClearBattleVar();
        LuaScriptBinder.Clear();
        Object.Destroy(GameObject.Find("Main Camera OW"));
    }

    public static string TimeFormatter(float time) {
        float seconds = Mathf.Floor(Mathf.Floor(time));
        float minutes = Mathf.Floor(seconds / 60);
        //float hours = Mathf.Floor(minutes / 60);
        return minutes + ":" + string.Format("{0,2:00}", seconds % 60);
    }

    public static bool IsSpecialAnnouncement(string str) {
        return str == "4eab1af3ab6a932c23b3cdb8ef618b1af9c02088";
    }

    public static bool TryCall(ScriptWrapper script, string func, DynValue param) { return TryCall(script, func, new[] { param }); }
    public static bool TryCall(ScriptWrapper script, string func, DynValue[] param = null) {
        DynValue sval = script.GetVar(func);
        script.Call(func, param);
        return (sval.Type & (DataType.Function | DataType.ClrFunction)) != 0;
    }

    public static int SelectionChoice(int items, int current, int xMov, int yMov, int rows, int columns, bool verticalRollaround = true) {
        int pageItems = rows * columns;
        int pageNumber = Mathf.CeilToInt(items / (float)pageItems);
        int currentPage = current / pageItems;
        int currentItem = current % pageItems;
        int lastPageItemNumber = Math.Mod(items - 1, pageItems) + 1;
        int xPos = currentItem % columns;
        int yPos = currentItem / columns;

        xPos += xMov;
        yPos += yMov;

        // Horizontal movement
        if (xMov != 0) {
            // Right bound
            if (xPos >= columns || (currentPage == pageNumber - 1 && xPos >= lastPageItemNumber - yPos * columns)) {
                xPos = 0;
                if (verticalRollaround)
                    currentPage++;
            }
            // Left bound
            if (xPos < 0) {
                if (currentPage == 0) xPos = Math.Mod(lastPageItemNumber - yPos * columns - 1, columns);
                else                  xPos = columns - 1;
                if (verticalRollaround)
                    currentPage--;
            }
        }

        // Vertical movement
        // Down bound
        if (yPos >= rows || (currentPage == pageNumber - 1 && yPos >= Mathf.CeilToInt((lastPageItemNumber - xPos) / (float)columns))) {
            yPos = 0;
            if (!verticalRollaround)
                currentPage++;
        }
        // Up bound
        if (yPos < 0) {
            if (currentPage == pageNumber - 1) yPos = Mathf.CeilToInt((lastPageItemNumber - xPos) / (float)columns) - 1;
            else                               yPos = rows - 1;
            if (!verticalRollaround)
                currentPage--;
        }

        // Page underflow
        while (currentPage < 0)
            currentPage += pageNumber;
        // Page overflow
        while (currentPage >= pageNumber)
            currentPage -= pageNumber;

        int result = xPos + yPos * columns + currentPage * pageItems;
        if (result >= items)
            result = items - 1;

        return result;
    }

    public static void TextObjectMoveChecker(Transform t) {
        LuaTextManager[] ltms = t.GetComponentsInChildren<LuaTextManager>();
        foreach (LuaTextManager ltm in ltms)
            if (ltm.transform != t && ltm.adjustTextDisplay)
                ltm.AlignLetters();
    }

    public static Transform GetTransform(object o) {
        LuaSpriteController sSelf = o as LuaSpriteController;
        if (sSelf != null) return sSelf.img.transform;
        LuaTextManager tSelf = o as LuaTextManager;
        if (tSelf != null) return tSelf.GetContainer().transform;
        ProjectileController pSelf = o as ProjectileController;
        if (pSelf != null) return pSelf.sprite.img.transform;
        LuaCYFObject oSelf = o as LuaCYFObject;
        if (oSelf != null) return oSelf.transform;
        Transform tsSelf = o as Transform;
        if (tsSelf != null) return tsSelf;
        return null;
    }

    public static DynValue GetObject(Transform t) {
        if (t == null)
            return DynValue.NewNil();

        GameObject go = t.gameObject;
        if (LuaSpriteController.HasSpriteController(go))
            return UserData.Create(LuaSpriteController.GetOrCreate(go));
        if (t.GetComponent<LuaProjectile>() != null)
            return UserData.Create(t.GetComponent<LuaProjectile>().ctrl);
        if (t.GetComponent<LuaTextManager>() != null)
            return UserData.Create(t.GetComponent<LuaTextManager>());
        for (int i = 0; i < t.childCount; i++) {
            Transform child = t.GetChild(i);
            if (child.GetComponent<LuaTextManager>() != null)
                return UserData.Create(child.GetComponent<LuaTextManager>());
        }
        return UserData.Create(new LuaCYFObject(t));
    }

    public static DynValue GetObjectParent(Transform t) {
        return GetObject(t.parent);
    }

    public static void SetObjectParent(object self, object p) {
        if (p == null)
            throw new CYFException("SetParent(): Can't set nil as parent.");

        LuaSpriteController sSelf = self as LuaSpriteController;
        LuaSpriteController sParent = p as LuaSpriteController;

        if (sSelf != null && sSelf.tag == "event")
            throw new CYFException("sprite.SetParent(): Cannot set the prent of an overworld event's sprite.");
        if ((sSelf != null && sSelf.tag == "letter") ^ (sParent != null && sParent.tag == "letter"))
            throw new CYFException("sprite.SetParent(): Cannot be used between letter sprites and other objects.");

        Transform t = GetTransform(p);
        if (t == null) {
            DynValue d = p as DynValue;
            throw new CYFException("SetParent(): Can't set an object of type " + d.GetType().ToString() + " as a parent!");
        }
        GetTransform(self).SetParent(GetTransform(p));
    }
}
