using DragonEssentials.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;
using System.ComponentModel;

namespace DragonEssentials.Configuration
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Log File Access")]
        [Description("Logs to the console whenever the game opens a file")]
        [DefaultValue(false)]
        public bool FileAccessLog { get; set; } = false;

        [DisplayName("Log File Redirects")]
        [Description("Logs to the console whenever a game file is redirected to a mod file")]
        [DefaultValue(false)]
        public bool RedirectLog { get; set; } = false;

        [DisplayName("Debug Mode")]
        [Description("Logs additional information to the console that is useful for debugging.")]
        [DefaultValue(false)]
        public bool DebugEnabled { get; set; } = false;


        [DisplayName("Microsoft Store Mode")]
        [Description("Enable this if your copy of the game is from Microsoft Store (i.e. Gamepass).")]
        [DefaultValue(false)]
        public bool isGamePass { get; set; } = false;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
