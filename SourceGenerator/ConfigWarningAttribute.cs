using System;

namespace Jakojaannos.GodotSourceGenerator {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ConfigWarningAttribute : Attribute {
        public ConfigWarningAttribute() {
        }
    }
}
