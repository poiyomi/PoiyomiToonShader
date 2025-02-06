namespace Thry
{
    public class PropertyOptions
    {
        public int offset = 0;
        public string tooltip = "";
        public DefineableAction altClick;
        public DefineableAction onClick;
        public DefineableCondition condition_show = new DefineableCondition();
        public string condition_showS;
        public DefineableCondition condition_enable = null;
        public DefineableCondition condition_enable_children = null;
        public PropertyValueAction[] on_value_actions;
        public string on_value;
        public DefineableAction[] actions;
        public ButtonData button_help;
        public TextureData texture;
        public string[] reference_properties;
        public string reference_property;
        public bool force_texture_options = false;
        public bool is_visible_simple = false;
        public string file_name;
        public string remote_version_url;
        public string generic_string;
        public bool never_lock;
        public float margin_top = 0;
        public string[] alts;
        public bool persistent_expand = true;
        public bool default_expand = false;
        public bool ref_float_toggles_expand = true;

        public static PropertyOptions Deserialize(string s)
        {
            if (s == null) return new PropertyOptions();
            s = s.Replace("''", "\"");
            PropertyOptions options = Parser.Deserialize<PropertyOptions>(s);
            if (options == null) return new PropertyOptions();
            // The following could be removed since the parser can now handle it. leaving it in for now /shrug
            if (options.condition_showS != null)
            {
                options.condition_show = DefineableCondition.Parse(options.condition_showS);
            }
            if (options.on_value != null)
            {
                options.on_value_actions = PropertyValueAction.ParseToArray(options.on_value);
            }
            return options;
        }
    }

}