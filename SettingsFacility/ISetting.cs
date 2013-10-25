using System;

namespace SettingsFacility {
    public interface ISetting  {
        string Name { get; }
        string XmlName { get; }
        string Description { get; }
        string QualifiedName { get; }
        bool Group { get; }

        event Action<ISetting> Changed;
    }
}