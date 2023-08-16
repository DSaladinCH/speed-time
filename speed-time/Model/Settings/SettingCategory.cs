using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model.Settings
{
    internal enum SettingCategory
    {
        [Description("settings.ui.title")]
        UI,
        [Description("settings.behavior.title")]
        Behavior,
        [Description("settings.integrations.title")]
        Integrations
    }
}
