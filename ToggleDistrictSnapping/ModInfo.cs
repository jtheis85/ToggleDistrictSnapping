using ICities;

namespace ToggleDistrictSnapping
{
    public class ModInfo : IUserMod
    {
        public string Name        { get { return "Toggle District Snapping"; } }
        public string Description { get { return "Allows toggling of snapping to roads on and off while painting districts."; } }
    }
}
