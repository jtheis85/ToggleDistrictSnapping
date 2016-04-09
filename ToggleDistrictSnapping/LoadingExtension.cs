using ICities;

namespace ToggleDistrictSnapping
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            ToggleSnappingDistrictTool.Deploy();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            ToggleSnappingDistrictTool.Revert();
        }
    }
}
