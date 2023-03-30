using System.Collections.Generic;

/// <summary>
/// This class creates a text used in selection phases. It takes a string array and transforms it into a selection text.
/// Then, the text is sent to the TextMessage class.
/// </summary>
public class SelectMessage : TextMessage {
    public SelectMessage(IList<string> options, bool singleList, int columns, IList<string> colorPrefixes = null) : base("", false, true) {
        string finalMessage = "";          // String that will contain all our text when finished
        const string itemSpacing = "  ";   // String that contains the needed shift for each item
        const string rowTwoSpacing = "\t"; // String that contains a tabulation character that puts the text to the right
        string prefix = "* ";              // Prefix used for new lines

        // If there is no option, there is an error somewhere : Let's create it then by throwing an ArgumentException
        if (options.Count == 0)
            throw new CYFException("Can't create a select message for zero options.");

        // For each option...
        for (int i = 0; i < options.Count; i++) {
            string intermedPrefix = "";
            string intermedSuffix = "";
            // If the option isn't null, has an existing color and this color isn't null or empty, we'll add the color as a prefix and put a white color tag as a suffix
            if (colorPrefixes != null && i < colorPrefixes.Count && !string.IsNullOrEmpty(colorPrefixes[i])) {
                intermedPrefix = colorPrefixes[i];
                intermedSuffix = "[color:ffffff]";
            }
            int index = 0;
            string commands = "";
            bool gotIt = false;
            bool needExit = false;
            if (options[i] != null)
                if (options[i].Length > 0)
                    while (options[i][index] == '[') {
                        if (!(i == 0 && options[i].Length >= 10 + index && (options[i].Substring(index, 10) == "[starcolor" || options[i].Substring(index, 8) == "[letters"))) {
                            for (int j = index; j < options[i].Length; j++)
                                if (options[i][j] == ']') { // TODO: Somehow apply UnitaleUtil.ParseCommandInLine here maybe?
                                    commands += options[i].Substring(index, j + 1);
                                    options[i] = options[i].Substring(index + j + 1, options[i].Length - index - j - 1);
                                    gotIt = true;
                                    break;
                                }
                            if (!gotIt)
                                break;
                        } else
                            while (options[i][index] != ']') {
                                index++;
                                if (index != options[i].Length) continue;
                                needExit = true;
                                break;
                            }
                        if (needExit)
                            break;
                    }
            // If the option is null, empty or equal to "\tPAGE 1" (used for enemy pages), there will not be any prefix
            if (options[i] == null || options[i] == "" || options[i].Contains("PAGE "))
                prefix = "";
            else if (i > 0)
                if (options[i-1] == null || options[i-1] == "")
                    prefix = "* ";
            if (options[i] != null)
                options[i] = options[i].TrimStart('*', ' ');
            // If this is a single list, we don't need text on the right side of the textbox
            if (singleList)                      finalMessage += commands + itemSpacing + intermedPrefix + prefix + options[i] + intermedSuffix + "\n";
            // If the option is on the first column, it'll be at the left side of the textbox
            else if (i % columns == 0)           finalMessage += commands + itemSpacing + intermedPrefix + prefix + options[i] + intermedSuffix;
            // If the option is on the last column, add a chariot return
            else if (i % columns == columns - 1) finalMessage += commands + rowTwoSpacing + itemSpacing + intermedPrefix + prefix + options[i] + intermedSuffix + "\n";
            // Else, we'll put the textwith a tab
            else                                 finalMessage += commands + rowTwoSpacing + itemSpacing + intermedPrefix + prefix + options[i] + intermedSuffix;
        }

        // This function sends finalMessage to the real text handler function
        Setup(finalMessage, false, true);
    }
}