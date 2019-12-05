using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Turns any string into "Tem speak".
public static class Temmify {
    // Secret function used for "Temmifying" text in Crate Your Frisk mode
    public static string Convert(string sentence, bool random = false) {
        if (!random)
            Random.InitState(0);

        // a list of every character that can be swapped
        string swappableCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        // uncomment this line to allow numbers to be swapped:
        // swappableCharacters += "0123456789";

        // a list of characters that should be multiplied like crazy!!!!!
        string multiplyCharacters = "!?";

        string[] words = sentence.Split(new char[] {' '}, System.StringSplitOptions.RemoveEmptyEntries);

        // separate words by items in multiplyCharacters
        List<string> newWords = new List<string>();

        for (int i = 0; i < words.Length; i++) {
            bool separated = false;

            for (int j = 0; j < words[i].Length; j++) {
                if (multiplyCharacters.Contains(words[i].Substring(j, 1))) {
                    string before = words[i].Substring(0, j);
                    string after  = words[i].Substring(j);

                    if (before.Length > 0)
                        newWords.Add(before);
                    if (after.Length > 0)
                        newWords.Add(after);

                    separated = true;
                    break;
                }
            }

            if (!separated)
                newWords.Add(words[i]);
        }

        words = newWords.ToArray();

        for (int i = 0; i < words.Length; i++) {
            // capitalize every word
            words[i] = words[i].ToUpper();

            if (words[i].Length < 4 && !multiplyCharacters.Contains(words[i].Substring(0, 1)))
                continue;
            else {
                // only words with at least 5% of their letters moved will be allowed
                List<int> changesMade = new List<int>();
                bool hasAnyEditableCharacters = false;

                do {
                    for (int j = 0; j < words[i].Length; j++) {
                        // special for the first character
                        /*if (j == 0 && swappableCharacters.Contains(words[i].Substring(j, 1)) && Random.Range(1, 6) == 1
                            && !changesMade.Contains(j)) {
                            words[i] = words[i].Substring(1, 1)
                                     + words[i].Substring(0, 1)
                                     + words[i].Substring(2);
                            changesMade.Add(j + 1);
                        } else {*/

                        /*
                        DEBUGGER
                        string spaces = "";

                        for (int k = 0; k < words[i].Length; k++)
                            if (k == j)
                                spaces += "^";
                            else if (changesMade.Contains(k))
                                spaces += words[i].Substring(k, 1);
                            else
                                spaces += " ";

                        Debug.Log(words[i] + "\n" + spaces);
                        */


                        if (((j > 0 && j < words[i].Length - 2) || multiplyCharacters.Contains(words[i].Substring(j, 1))) && !changesMade.Contains(j)) {
                            // if character is swappable, see if it can be swapped
                            if (swappableCharacters.Contains(words[i].Substring(j, 1))
                             && swappableCharacters.Contains(words[i].Substring(j + 1, 1))
                             && words[i].Substring(j, 1) != words[i].Substring(j + 1, 1)
                             && !changesMade.Contains(j)) {
                                hasAnyEditableCharacters = true;
                                if (Random.Range(1, 4) == 1) {
                                    words[i] = words[i].Substring(0, j)
                                               + words[i].Substring(j + 1, 1)
                                               + words[i].Substring(j, 1)
                                               + words[i].Substring(j + 2);
                                    changesMade.Add(j + 1);
                                }
                            } else if (multiplyCharacters.Contains(words[i].Substring(j, 1)) && !changesMade.Contains(j)) {
                                hasAnyEditableCharacters = true;

                                int randomAddition = Random.Range(2, 5);

                                string toAdd = words[i].Substring(j, 1);
                                for (int k = 0; k < randomAddition; k++) {
                                    toAdd += words[i].Substring(j, 1);
                                    changesMade.Add(j + k);
                                }
                                changesMade.Add(j + randomAddition);

                                words[i] = words[i].Substring(0, j > 0 ? j : 0)
                                         + toAdd
                                         + words[i].Substring(j + 1);
                                /*
                                for (var k = 0f; k < randomAddition; k++) {
                                    words[i] = words[i].Substring(0, j > 0 ? j : 0)
                                             + words[i].Substring(j, 1)
                                             + words[i].Substring(j, 1)
                                             + words[i].Substring(j + 1);
                                    changesMade.Add(j + 1);
                                }
                                changesMade.Add(j);
                                */
                                j += randomAddition + 1;
                            }
                        }
                    }
                    // emergency: if the word is 3 characters or greater but can't be changed, just exit
                    if (!hasAnyEditableCharacters)
                        break;
                    // random: there's a 1 in 7, or 14% chance, that the loop will end and the word won't be modified further
                    if (Random.Range(1, 8) == 1)
                        break;
                } while (changesMade.Count/words[i].Length < 0.05);
                changesMade.Clear();
            }
        }

        sentence = "";

        for (int i = 0; i < words.Length; i++) {
            if (i < words.Length - 1 && multiplyCharacters.Contains(words[i + 1].Substring(0, 1)))
                sentence += words[i];
            else
                sentence += words[i] + " ";
        }
        sentence = sentence.Substring(0, sentence.Length - 1);

        return sentence;
    }
}
