namespace Thry
{
    public class ButtonData
    {
        public string text = "";
        public TextureData texture = null;
        public DefineableAction action = new DefineableAction();
        public string hover = "";
        public bool center_position = false;
        public DefineableCondition condition_show = new DefineableCondition();
    }

}