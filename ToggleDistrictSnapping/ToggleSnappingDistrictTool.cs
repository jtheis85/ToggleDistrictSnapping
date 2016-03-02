using ColossalFramework;
using ColossalFramework.Math;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ToggleDistrictSnapping
{
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

        private static bool _deployed;

        #region Get Redirects
        // Simulation Step
        private static readonly MethodInfo fromSimulationStep = typeof(DistrictTool)
            .GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo toSimulationStep = typeof(ToggleSnappingDistrictTool)
            .GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance);
        private static RedirectCallsState _simulationStepState;

        // Apply Brush
        private static readonly MethodInfo fromApplyBrush = typeof(ToggleSnappingDistrictTool)
            .GetMethod("ApplyBrush", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo toApplyBrush = typeof(DistrictTool)
            .GetMethod("ApplyBrush", BindingFlags.NonPublic | BindingFlags.Instance);
        private static RedirectCallsState _applyBrushState;

        // Force District Alpha
        private static readonly MethodInfo fromForceDistrictAlpha = typeof(ToggleSnappingDistrictTool)
            .GetMethod("ForceDistrictAlpha", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo toForceDistrictAlpha = typeof(DistrictTool)
            .GetMethod("ForceDistrictAlpha", BindingFlags.NonPublic | BindingFlags.Instance);
        private static RedirectCallsState _forceDistrictAlphaState;

        // Set District Alpha
        private static readonly MethodInfo fromSetDistrictAlpha = typeof(ToggleSnappingDistrictTool)
            .GetMethod("SetDistrictAlpha", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo toSetDistrictAlpha = typeof(DistrictTool)
            .GetMethod("SetDistrictAlpha", BindingFlags.NonPublic | BindingFlags.Instance);
        private static RedirectCallsState _setDistrictAlphaState;

        // Normalize
        private static readonly MethodInfo fromNormalize = typeof(ToggleSnappingDistrictTool)
            .GetMethod("Normalize", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo toNormalize = typeof(DistrictTool)
            .GetMethod("Normalize", BindingFlags.NonPublic | BindingFlags.Instance);
        private static RedirectCallsState _normalizeState;

        // Check Neighbour Cells
        private static readonly MethodInfo fromCheckNeighbourCells = typeof(ToggleSnappingDistrictTool)
            .GetMethod("CheckNeighbourCells", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo toCheckNeighbourCells = typeof(DistrictTool)
            .GetMethod("CheckNeighbourCells", BindingFlags.NonPublic | BindingFlags.Instance);
        private static RedirectCallsState _checkNeighbourCellsState;

        // Get Alpha
        private static readonly MethodInfo fromGetAlpha = typeof(ToggleSnappingDistrictTool)
            .GetMethod("SetDistrictAlpha", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo toGetAlpha = typeof(DistrictTool)
            .GetMethod("SetDistrictAlpha", BindingFlags.NonPublic | BindingFlags.Instance);
        private static RedirectCallsState _getAlphaState; 
        #endregion

        public static void Deploy() {
            if (_deployed)
                return;

            #region Deploy Redirects
            _simulationStepState      = RedirectionHelper.RedirectCalls(fromSimulationStep,      toSimulationStep);
            _applyBrushState          = RedirectionHelper.RedirectCalls(fromApplyBrush,          toApplyBrush);
            _forceDistrictAlphaState  = RedirectionHelper.RedirectCalls(fromForceDistrictAlpha,  toForceDistrictAlpha);
            _setDistrictAlphaState    = RedirectionHelper.RedirectCalls(fromSetDistrictAlpha,    toSetDistrictAlpha);
            _normalizeState           = RedirectionHelper.RedirectCalls(fromNormalize,           toNormalize);
            _checkNeighbourCellsState = RedirectionHelper.RedirectCalls(fromCheckNeighbourCells, toCheckNeighbourCells);
            _getAlphaState            = RedirectionHelper.RedirectCalls(fromGetAlpha,            toGetAlpha);
            #endregion

            m_district          = typeof(DistrictTool).GetField("m_district", BindingFlags.Instance | BindingFlags.NonPublic);
            m_mousePosition     = typeof(DistrictTool).GetField("m_mousePosition", BindingFlags.Instance | BindingFlags.NonPublic);
            m_lastPaintPosition = typeof(DistrictTool).GetField("m_lastPaintPosition", BindingFlags.Instance | BindingFlags.NonPublic);
            m_mouseRay          = typeof(DistrictTool).GetField("m_mouseRay", BindingFlags.Instance | BindingFlags.NonPublic);
            m_mouseRayLength    = typeof(DistrictTool).GetField("m_mouseRayLength", BindingFlags.Instance | BindingFlags.NonPublic);
            m_painting          = typeof(DistrictTool).GetField("m_painting", BindingFlags.Instance | BindingFlags.NonPublic);
            m_erasing           = typeof(DistrictTool).GetField("m_erasing", BindingFlags.Instance | BindingFlags.NonPublic);
            m_mouseRayValid     = typeof(DistrictTool).GetField("m_mouseRayValid", BindingFlags.Instance | BindingFlags.NonPublic);
            m_errors            = typeof(DistrictTool).GetField("m_errors", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static void Revert() {
            if (!_deployed)
                return;

            #region Revert Redirects
            RedirectionHelper.RevertRedirect(fromSimulationStep,      _simulationStepState);
            RedirectionHelper.RevertRedirect(fromApplyBrush,          _applyBrushState);
            RedirectionHelper.RevertRedirect(fromForceDistrictAlpha,  _forceDistrictAlphaState);
            RedirectionHelper.RevertRedirect(fromSetDistrictAlpha,    _setDistrictAlphaState);
            RedirectionHelper.RevertRedirect(fromNormalize,           _normalizeState);
            RedirectionHelper.RevertRedirect(fromCheckNeighbourCells, _checkNeighbourCellsState);
            RedirectionHelper.RevertRedirect(fromGetAlpha,            _getAlphaState); 
            #endregion

            _deployed = false;
        }

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
                var swapToggle = (ModInfo.Options & ModOptions.DefaultToNoSnapping) != 0;
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
        private void ApplyBrush(byte district) {
            Debug.Log($"{district}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool ForceDistrictAlpha(int x, int z, byte district, int min, int max) {
            Debug.Log($"{x}-{z}-{district}-{min}-{max}");
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SetDistrictAlpha(int x, int z, byte district, int min, int max) {
            Debug.Log($"{x}-{z}-{district}-{min}-{max}");
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Normalize(ref DistrictManager.Cell cell, int ignoreIndex) {
            Debug.Log($"{cell}-{ignoreIndex}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CheckNeighbourCells(int x, int z, byte district, out int min, out int max) {
            min = 0;
            max = 0;
            Debug.Log($"{x}-{z}-{district}-{min}-{max}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int GetAlpha(ref DistrictManager.Cell cell, byte district) {
            Debug.Log($"{cell}-{district}");
            return 0;
        } 
        #endregion
    }
}
