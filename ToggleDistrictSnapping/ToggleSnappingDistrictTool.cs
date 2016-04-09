using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Math;
using System.Reflection;
using System.Runtime.CompilerServices;
using ToggleDistrictSnapping.OptionsFramework;
using ToggleDistrictSnapping.Redirection;
using UnityEngine;

namespace ToggleDistrictSnapping
{
    [TargetType(typeof(DistrictTool))]
    public class ToggleSnappingDistrictTool : DistrictTool
    {
        private static FieldInfo m_district;
        private static FieldInfo m_mousePosition;
        private static FieldInfo m_lastPaintPosition;
        private static FieldInfo m_mouseRay;
        private static FieldInfo m_mouseRayLength;
        private static FieldInfo m_painting;
        private static FieldInfo m_erasing;
        private static FieldInfo m_mouseRayValid;
        private static FieldInfo m_errors;

        private void setDistrict(byte value) {
            m_district.SetValue(this, value);
        }
        private void setMousePosition(Vector3 value) {
            m_mousePosition.SetValue(this, value);
        }
        private void setlastPaintPosition(Vector3 value) {
            m_lastPaintPosition.SetValue(this, value);
        }
        private void setErrors(ToolBase.ToolErrors value) {
            m_errors.SetValue(this, value);
        }

        private static Dictionary<MethodInfo, RedirectCallsState> _redirects;

        public static void Deploy()
        {
            if (_redirects != null)
            {
                return;
            }
            _redirects = RedirectionUtil.RedirectType(typeof(ToggleSnappingDistrictTool));
            m_district = typeof(DistrictTool).GetField("m_district", BindingFlags.Instance | BindingFlags.NonPublic);
            m_mousePosition = typeof(DistrictTool).GetField("m_mousePosition", BindingFlags.Instance | BindingFlags.NonPublic);
            m_lastPaintPosition = typeof(DistrictTool).GetField("m_lastPaintPosition", BindingFlags.Instance | BindingFlags.NonPublic);
            m_mouseRay = typeof(DistrictTool).GetField("m_mouseRay", BindingFlags.Instance | BindingFlags.NonPublic);
            m_mouseRayLength = typeof(DistrictTool).GetField("m_mouseRayLength", BindingFlags.Instance | BindingFlags.NonPublic);
            m_painting = typeof(DistrictTool).GetField("m_painting", BindingFlags.Instance | BindingFlags.NonPublic);
            m_erasing = typeof(DistrictTool).GetField("m_erasing", BindingFlags.Instance | BindingFlags.NonPublic);
            m_mouseRayValid = typeof(DistrictTool).GetField("m_mouseRayValid", BindingFlags.Instance | BindingFlags.NonPublic);
            m_errors = typeof(DistrictTool).GetField("m_errors", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static void Revert()
        {
            if (_redirects == null)
            {
                return;
            }
            foreach (var redirect in _redirects)
            {
                RedirectionHelper.RevertRedirect(redirect.Key, redirect.Value);
            }
            _redirects = null;
        }

        [RedirectMethod]
        public override void SimulationStep() {
            var district       = (byte)   m_district      .GetValue(this);
            var painting       = (bool)   m_painting      .GetValue(this);
            var erasing        = (bool)   m_erasing       .GetValue(this);
            var mouseRayValid  = (bool)   m_mouseRayValid .GetValue(this);
            var mouseRayLength = (float)  m_mouseRayLength.GetValue(this);
            var mouseRay       = (Ray)    m_mouseRay      .GetValue(this);
            var mousePosition  = (Vector3)m_mousePosition .GetValue(this);

            ToolBase.RaycastInput input = new ToolBase.RaycastInput(mouseRay, mouseRayLength);
            input.m_netService = new ToolBase.RaycastService(ItemClass.Service.Road, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            ToolBase.RaycastOutput raycastOutput;
            if (mouseRayValid && ToolBase.RayCast(input, out raycastOutput)) {
                //begin mod
                var swapToggle = OptionsWrapper<Options>.Options.defaultToNoSnapping;
                if ((swapToggle && Input.GetKey(KeyCode.LeftAlt)) ||
                     (!swapToggle && !Input.GetKey(KeyCode.LeftAlt))) {
                    if (raycastOutput.m_netNode != 0) {
                        raycastOutput.m_hitPos = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)raycastOutput.m_netNode].m_position;
                    }
                    else if (raycastOutput.m_netSegment != 0) {
                        raycastOutput.m_hitPos = Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].GetClosestPosition(raycastOutput.m_hitPos);
                    }
                }
                //end mod

                setMousePosition(raycastOutput.m_hitPos);
                if (this.m_mode == DistrictTool.Mode.Paint || this.m_mode == DistrictTool.Mode.Erase) {
                    if (district != 0 && Singleton<DistrictManager>.instance.m_districts.m_buffer[(int)district].m_flags == District.Flags.None) {
                        setDistrict(0);
                    }
                    if (painting && district != 0) {
                        this.ApplyBrush(district);
                        setlastPaintPosition(mousePosition);
                        setErrors(ToolBase.ToolErrors.None);
                    }
                    else if (erasing) {
                        this.ApplyBrush(0);
                        setlastPaintPosition(mousePosition);
                        setErrors(ToolBase.ToolErrors.None);
                    }
                    else {
                        var value = Singleton<DistrictManager>.instance.SampleDistrict(mousePosition);
                        setDistrict(value);

                        setlastPaintPosition(new Vector3(-100000f, -100000f, -100000f));
                        if (district == 0 && !Singleton<DistrictManager>.instance.CheckLimits()) {
                            setErrors(ToolBase.ToolErrors.TooManyObjects);
                        }
                        else {
                            setErrors(ToolBase.ToolErrors.None);
                        }
                    }
                }
                else {
                    var value = Singleton<DistrictManager>.instance.SampleDistrict(mousePosition);
                    setDistrict(value);

                    setlastPaintPosition(new Vector3(-100000f, -100000f, -100000f));
                    setErrors(ToolBase.ToolErrors.None);
                }
            }
            else {
                setlastPaintPosition(new Vector3(-100000f, -100000f, -100000f));
                setErrors(ToolBase.ToolErrors.RaycastFailed);
            }
        }

        #region Dummy Methods
        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectReverse]
        private void ApplyBrush(byte district) {
            Debug.Log($"{district}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectReverse]
        private bool ForceDistrictAlpha(int x, int z, byte district, int min, int max) {
            Debug.Log($"{x}-{z}-{district}-{min}-{max}");
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectReverse]
        private bool SetDistrictAlpha(int x, int z, byte district, int min, int max) {
            Debug.Log($"{x}-{z}-{district}-{min}-{max}");
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectReverse]
        private void Normalize(ref DistrictManager.Cell cell, int ignoreIndex) {
            Debug.Log($"{cell}-{ignoreIndex}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectReverse]
        private void CheckNeighbourCells(int x, int z, byte district, out int min, out int max) {
            min = 0;
            max = 0;
            Debug.Log($"{x}-{z}-{district}-{min}-{max}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectReverse]
        private int GetAlpha(ref DistrictManager.Cell cell, byte district) {
            Debug.Log($"{cell}-{district}");
            return 0;
        } 
        #endregion
    }
}
