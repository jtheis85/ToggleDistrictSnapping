using ICities;
using System;

namespace ToggleDistrictSnapping
{
    [Flags]
    public enum ModOptions : long
    {
        None                = 0,
        DefaultToNoSnapping = 1
    }

    public class ModInfo : LoadingExtensionBase, IUserMod
    {
        private const string modName = "Toggle District Snapping";
        private const string modDescription = "Allows toggling of snapping to roads on and off while painting districts.";

        public static ModOptions Options = ModOptions.None;

        public string Name {
            get {
                OptionsLoader.LoadOptions();
                return modName;
            }
        }
        public string Description { get { return modDescription; } }

        public override void OnLevelLoaded(LoadMode mode) {
            base.OnLevelLoaded(mode);

            if(mode != LoadMode.NewGame && mode != LoadMode.LoadGame) {
                return;
            }

            ToggleSnappingDistrictTool.Deploy();
        }

        public override void OnLevelUnloading() {
            base.OnLevelUnloading();

            ToggleSnappingDistrictTool.Revert();
        }


        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Toggle District Snapping");
            group.AddCheckbox("Default to no snapping (hold hotkey to enable snapping)", (Options & ModOptions.DefaultToNoSnapping) != 0, 
                (isChecked) => {
                    if (isChecked)
                        Options |= ModOptions.DefaultToNoSnapping;
                    else
                        Options &= ~ModOptions.DefaultToNoSnapping;

                    OptionsLoader.SaveOptions();
                }
            );
        }
    }    
}
