namespace Thry.ThryEditor
{
    public class ButtonData
    {
        public string text = "";
        public TextureData texture = null;
        public DefineableAction action = DefineableAction.None;
        public string hover = "";
        public bool center_position = false;
        public DefineableCondition condition_show = DefineableCondition.None;
    }

}