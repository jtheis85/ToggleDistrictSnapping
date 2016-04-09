using ICities;
using ToggleDistrictSnapping.OptionsFramework;

namespace ToggleDistrictSnapping
{
    public class Mod : IUserMod
    {
        public string Name => "Toggle District Snapping";
        public string Description => "Allows toggling of snapping to roads on and off while painting districts.";

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddOptionsGroup<Options>();
        }
    }
}