using System.Xml.Serialization;

namespace ToggleDistrictSnapping.OptionsFramework
{
    public interface IModOptions
    {
        [XmlIgnore]
        string FileName
        {
            get;
        }
    }
}