using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenderGuesser : EditorWindow
{
    /*
    [MenuItem("Thry/GenderGuesser")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        GenderGuesser window = (GenderGuesser)EditorWindow.GetWindow(typeof(GenderGuesser));
        window.Show();
    }

    private string guessname = "";

    private void OnGUI()
    {
        guessname = EditorGUI.TextField(EditorGUILayout.GetControlRect(), guessname);
        GUILayout.Label("is male: "+(guessGender(name).ToString()));
    }*/

    public enum Gender { Male, Female, UNDEFINED};

    private static string[] FEMALE_NAME_PARTS = new string[] { "chan", "miss" ,"girl", "woman" , "princess" , "miku" , "kanna" , "succubus", "mistress" , "madam" , "queen"};
    private static string[] FEMALE_NAME_ENDINGS = new string[] { "a" , "i" , "e"};

    private static string[] MALE_NAME_PARTS = new string[] { "man", "guy", "boy" , "boi", "dr", "mr" ,"king"};
    private static string[] MALE_NAME_ENDINGS = new string[] { "n" ,"x" , "p" , "s" , "m" , "d" ,"g" ,"t" , "c" , "r"};

    private static string[] UNCERTAIN_NAME_ENDINGS = new string[] { "fox" };

    public static Gender guessGender(string name)
    {
        name = name.ToLower();

        bool ignoreEnding = false;
        foreach (string p in UNCERTAIN_NAME_ENDINGS) if (name.EndsWith(p)) ignoreEnding = true;

        foreach (string p in FEMALE_NAME_PARTS) if (name.Contains(p)) return Gender.Female;
        foreach(string p in MALE_NAME_PARTS) if (name.Contains(p)) return Gender.Male;

        if (!ignoreEnding)
        {
            if (name.EndsWith("h")) name = name.Substring(0, name.Length - 1);
            foreach (string p in FEMALE_NAME_ENDINGS) if (name.EndsWith(p)) return Gender.Female;
            foreach (string p in MALE_NAME_ENDINGS) if (name.EndsWith(p)) return Gender.Male;
        }
        return Gender.UNDEFINED;
    }
}
