using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thry
{
    public abstract class ModuleSettings
    {
        public const string MODULES_CONFIG = ".thry_modules_config";

        public abstract void Draw();
    }
}