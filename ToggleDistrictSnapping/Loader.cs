using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ICities;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ToggleDistrictSnapping
{
    public class Loader : LoadingExtensionBase
    {
        static bool hooked = false;
        static private Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();

        public override void OnCreated(ILoading loading)
        {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "[ToggleDistrictSnapping] Loader - OnCreated");
            base.OnCreated(loading);

            if (hooked)
            {
                foreach (var kvp in redirects)
                {
                    RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
                }
                redirects.Clear();
            }

            var allFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            MethodInfo method = typeof(DistrictTool).GetMethod("SimulationStep", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(ToggleSnappingDistrictTool).GetMethod("SimulationStep", allFlags)));

            hooked = true;
        }
    }
}
