using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thry
{
    public class Parsers
    {

        // <button> := {text:<string>,hover:<string>,action:<action>}
        public static ButtonData ParseButton(string input)
        {
            ButtonData button = new ButtonData();
            button.text = Helper.GetBetween(input, "text:", ",|}|$");
            button.hover = Helper.GetBetween(input, "hover:", ",|}|$");
            button.action = ParseDefinableAction(Helper.GetBracket(input, "action"));
            return button;
        }

        // <action> := {type:<type>,data:<string>}
        public static DefinableAction ParseDefinableAction(string input)
        {
            DefinableAction action = new DefinableAction();
            action.type = ParseDefinableActionType(Helper.GetBetween(input, "type:", ",|}|$"));
            action.data = Helper.GetBetween(input, "data:", ",|}|$");
            if (action.type == DefinableActionType.URL)
                action.data = Helper.FixUrl(action.data);
            return action;
        }

        public static DefinableActionType ParseDefinableActionType(string input)
        {
            if (input == "url")
                return DefinableActionType.URL;
            return DefinableActionType.NONE;
        }

    }
}
