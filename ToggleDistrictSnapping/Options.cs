using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ToggleDistrictSnapping.OptionsFramework;

namespace ToggleDistrictSnapping
{
    public class Options : IModOptions
    {
        [Checkbox("Default to no snapping (hold hotkey to enable snapping)")]
        public bool defaultToNoSnapping { get; set; }

        [XmlIgnore]
        public string FileName => "ToggleDistrictSnapping-Options.xml";
    }
}
