// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thry
{
    public abstract class ModuleSettings
    {
        public const string MODULES_CONFIG = "Thry/modules_config";

        public abstract void Draw();
    }
}