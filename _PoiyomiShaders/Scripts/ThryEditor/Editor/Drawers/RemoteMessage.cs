using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Thry.GradientEditor;
using static Thry.TexturePacker;

namespace Thry
{
    public class RemoteMessageDrawer : LocalMessageDrawer
    {

        protected override void Init(string s)
        {
            if (_isInit) return;
            WebHelper.DownloadStringASync(s, (Action<string>)((string data) =>
            {
                _buttonData = Parser.Deserialize<ButtonData>(data);
            }));
            _isInit = true;
        }
    }

}