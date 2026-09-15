using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    public class CodeViewElement : VisualElement
    {
        private class LineItem : VisualElement
        {
            private static Font TextFont
            {
                get
                {
                    if (_font == null)
                        _font = Resources.Load<Font>(MSSConstants.RESOURCES_FOLDER + "/RobotoMono-Regular");
                    return _font;
                }
            }

            private static Font _font;
            private Label _lineNumber;
            private Label _line;
            public string Text { get; }

            public LineItem() : this(0, "") {}

            public LineItem(int number, string text, int digits = 0)
            {
                Text = text;
                _lineNumber = new Label("" + number);
                _lineNumber.style.color = Color.gray;
                _lineNumber.style.width = digits == 0 ? 30 : digits * 8;
                _lineNumber.style.unityTextAlign = TextAnchor.MiddleRight;
                _lineNumber.style.unityFont = TextFont;
                _lineNumber.style.marginRight = 4;
                _lineNumber.style.marginLeft = 4;
                
                _line = new Label(text);
                _line.style.flexGrow = 1;
                _line.style.unityFont = TextFont;

                style.flexDirection = FlexDirection.Row;
                Add(_lineNumber);
                Add(_line);
            }

            public void SetText(int i, string textLine, int digits)
            {
                _lineNumber.text = "" + i;
                _lineNumber.style.width = digits == 0 ? 30 : digits * 8;
                _line.text = textLine;
                _line.MeasureTextSize(textLine, 0, MeasureMode.Exactly, 0, MeasureMode.Exactly);
            }
        }
        
        public string Text
        {
            get => string.Join("\n", _textLines);
            set
            {
                _textLines = value == null ? Array.Empty<string>() : value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                _digits = (int)Math.Floor(Math.Log10(_textLines.Length) + 1);
                _listView.itemsSource = _textLines;

                float width =((_textLines.Length == 0 ? 0 : _textLines.Max(x => x.Length)) + _digits + 1) * 10;
                _listView.style.width = width;
            }
        }
        
        public int LineCount => _textLines.Length;
        
        private Label _templateLabel;
        private string[] _textLines;
        private ListView _listView;
        private int _digits;

        public CodeViewElement()
        {
            ScrollView s = new ScrollView(ScrollViewMode.Horizontal);
            _listView = new ListView();
#if UNITY_2021_2_OR_NEWER
            _listView.fixedItemHeight = 15;
#else
            _listView.itemHeight = 15;
#endif
            _listView.AddToClassList("unity-base-text-field__input");
            _listView.AddToClassList("unity-text-field__input");
            _listView.AddToClassList("unity-base-field__input");
            _listView.style.flexGrow = 1;
            
            Func<VisualElement> makeItem = () => new LineItem();
            Action<VisualElement, int> bindItem = (e, i) => (e as LineItem).SetText(i+1, _textLines[i], _digits);
            
            _listView.makeItem = makeItem;
            _listView.bindItem = bindItem;
            _listView.selectionType = SelectionType.None;
            s.Add(_listView);
            Add(s);
            s.style.flexGrow = 1;

            style.flexGrow = 1;
        }
    }
}