using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace ToggleDistrictSnapping
{
    public class ToggleSnappingDistrictTool : ToolBase
    {
        public enum Mode
        {
            [EnumPosition(0)]
            Paint,
            [EnumPosition(1)]
            Erase,
            Select,
            Specialize,
            Unspecialize
        }

        public DistrictTool.Mode m_mode;

        public DistrictPolicies.Policies m_specialization = DistrictPolicies.Policies.None;

        public float m_brushSize = 200f;

        public Texture2D m_brush;

        public CursorInfo m_paintCursor;

        public CursorInfo m_eraseCursor;

        public CursorInfo m_oilSpecializationCursor;

        public CursorInfo m_oreSpecializationCursor;

        public CursorInfo m_farmingSpecializationCursor;

        public CursorInfo m_forestSpecializationCursor;

        public CursorInfo m_genericSpecializationCursor;

        private byte m_district;

        private Vector3 m_mousePosition;

        private Vector3 m_lastPaintPosition;

        private Ray m_mouseRay;

        private float m_mouseRayLength;

        private bool m_painting;

        private bool m_erasing;

        private bool m_mouseRayValid;

        private ToolBase.ToolErrors m_errors;

        public override void SimulationStep()
        {
            ToolBase.RaycastInput input = new ToolBase.RaycastInput(this.m_mouseRay, this.m_mouseRayLength);
            input.m_netService = new ToolBase.RaycastService(ItemClass.Service.Road, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            ToolBase.RaycastOutput raycastOutput;
            if (this.m_mouseRayValid && ToolBase.RayCast(input, out raycastOutput))
            {
                if (!Input.GetKey(KeyCode.LeftAlt))
                {
                    if (raycastOutput.m_netNode != 0)
                    {
                        raycastOutput.m_hitPos = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)raycastOutput.m_netNode].m_position;
                    }
                    else if (raycastOutput.m_netSegment != 0)
                    {
                        raycastOutput.m_hitPos = Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].GetClosestPosition(raycastOutput.m_hitPos);
                    }
                }
                this.m_mousePosition = raycastOutput.m_hitPos;
                if (this.m_mode == DistrictTool.Mode.Paint || this.m_mode == DistrictTool.Mode.Erase)
                {
                    if (this.m_district != 0 && Singleton<DistrictManager>.instance.m_districts.m_buffer[(int)this.m_district].m_flags == District.Flags.None)
                    {
                        this.m_district = 0;
                    }
                    if (this.m_painting && this.m_district != 0)
                    {
                        this.ApplyBrush(this.m_district);
                        this.m_lastPaintPosition = this.m_mousePosition;
                        this.m_errors = ToolBase.ToolErrors.None;
                    }
                    else if (this.m_erasing)
                    {
                        this.ApplyBrush(0);
                        this.m_lastPaintPosition = this.m_mousePosition;
                        this.m_errors = ToolBase.ToolErrors.None;
                    }
                    else
                    {
                        this.m_district = Singleton<DistrictManager>.instance.SampleDistrict(this.m_mousePosition);
                        this.m_lastPaintPosition = new Vector3(-100000f, -100000f, -100000f);
                        if (this.m_district == 0 && !Singleton<DistrictManager>.instance.CheckLimits())
                        {
                            this.m_errors = ToolBase.ToolErrors.TooManyObjects;
                        }
                        else
                        {
                            this.m_errors = ToolBase.ToolErrors.None;
                        }
                    }
                }
                else
                {
                    this.m_district = Singleton<DistrictManager>.instance.SampleDistrict(this.m_mousePosition);
                    this.m_lastPaintPosition = new Vector3(-100000f, -100000f, -100000f);
                    this.m_errors = ToolBase.ToolErrors.None;
                }
            }
            else
            {
                this.m_lastPaintPosition = new Vector3(-100000f, -100000f, -100000f);
                this.m_errors = ToolBase.ToolErrors.RaycastFailed;
            }
        }

        private void ApplyBrush(byte district)
        {
            GameAreaManager instance = Singleton<GameAreaManager>.instance;
            DistrictManager.Cell[] districtGrid = Singleton<DistrictManager>.instance.m_districtGrid;
            float num = 19.2f;
            float num2 = this.m_brushSize * 0.35f + num;
            int num3 = 512;
            Vector3 vector = this.m_lastPaintPosition;
            Vector3 mousePosition = this.m_mousePosition;
            if (vector.x < -50000f)
            {
                vector = mousePosition;
            }
            vector.y = 0f;
            mousePosition.y = 0f;
            Vector3 vector2 = Vector3.Min(vector, mousePosition);
            Vector3 vector3 = Vector3.Max(vector, mousePosition);
            int num4 = Mathf.Max((int)((vector2.x - num2) / num + (float)num3 * 0.5f), 0);
            int num5 = Mathf.Max((int)((vector2.z - num2) / num + (float)num3 * 0.5f), 0);
            int num6 = Mathf.Min((int)((vector3.x + num2) / num + (float)num3 * 0.5f), num3 - 1);
            int num7 = Mathf.Min((int)((vector3.z + num2) / num + (float)num3 * 0.5f), num3 - 1);
            int num8 = num3;
            int num9 = -1;
            int num10 = num3;
            int num11 = -1;
            for (int i = num5; i <= num7; i++)
            {
                for (int j = num4; j <= num6; j++)
                {
                    Vector3 vector4 = new Vector3(((float)j - (float)num3 * 0.5f + 0.5f) * num, 0f, ((float)i - (float)num3 * 0.5f + 0.5f) * num);
                    Vector3 a = vector4;
                    if (instance.ClampPoint(ref a))
                    {
                        float a2 = Mathf.Sqrt(Segment3.DistanceSqr(vector, mousePosition, vector4)) - num2 + num * 2f;
                        float num12 = Vector3.Distance(a, vector4);
                        float num13 = Mathf.Max(a2, num12) - num * 2f;
                        float num14 = Mathf.Clamp01(-num13 / (num * 2f));
                        if (num14 != 0f)
                        {
                            int min = Mathf.Clamp((int)(256f * num14), 0, 255);
                            bool flag;
                            if (num12 > 1f && district != 0)
                            {
                                flag = this.ForceDistrictAlpha(j, i, district, min, 255);
                            }
                            else
                            {
                                flag = this.SetDistrictAlpha(j, i, district, min, 255);
                            }
                            if (flag)
                            {
                                num8 = Mathf.Min(num8, j);
                                num9 = Mathf.Max(num9, j);
                                num10 = Mathf.Min(num10, i);
                                num11 = Mathf.Max(num11, i);
                            }
                        }
                    }
                }
            }
            int num15 = num8;
            int num16 = num9;
            int num17 = num10;
            int num18 = num11;
            int num19 = 0;
            bool flag2;
            do
            {
                num4 = Mathf.Max(num8 - 1, 0);
                num6 = Mathf.Min(num9 + 1, num3 - 1);
                num5 = Mathf.Max(num10 - 1, 0);
                num7 = Mathf.Min(num11 + 1, num3 - 1);
                num8 = num3;
                num9 = -1;
                num10 = num3;
                num11 = -1;
                flag2 = false;
                for (int k = num5; k <= num7; k++)
                {
                    for (int l = num4; l <= num6; l++)
                    {
                        DistrictManager.Cell cell = districtGrid[k * num3 + l];
                        bool flag3 = false;
                        bool flag4 = false;
                        if (cell.m_alpha1 != 0)
                        {
                            bool flag5 = cell.m_district1 == district;
                            int num20;
                            int num21;
                            this.CheckNeighbourCells(l, k, cell.m_district1, out num20, out num21);
                            if (!flag5 && Mathf.Min((int)(cell.m_alpha1 + 120), 255) > num21)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, cell.m_district1, 0, Mathf.Max(0, num21 - 120)));
                            }
                            else if (flag5 && Mathf.Max((int)(cell.m_alpha1 - 120), 0) < num20)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, cell.m_district1, Mathf.Min(255, num20 + 120), 255));
                            }
                            if (flag5)
                            {
                                flag3 = true;
                            }
                        }
                        if (cell.m_alpha2 != 0)
                        {
                            bool flag6 = cell.m_district2 == district;
                            int num22;
                            int num23;
                            this.CheckNeighbourCells(l, k, cell.m_district2, out num22, out num23);
                            if (!flag6 && Mathf.Min((int)(cell.m_alpha2 + 120), 255) > num23)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, cell.m_district2, 0, Mathf.Max(0, num23 - 120)));
                            }
                            else if (flag6 && Mathf.Max((int)(cell.m_alpha2 - 120), 0) < num22)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, cell.m_district2, Mathf.Min(255, num22 + 120), 255));
                            }
                            if (flag6)
                            {
                                flag3 = true;
                            }
                        }
                        if (cell.m_alpha3 != 0)
                        {
                            bool flag7 = cell.m_district3 == district;
                            int num24;
                            int num25;
                            this.CheckNeighbourCells(l, k, cell.m_district3, out num24, out num25);
                            if (!flag7 && Mathf.Min((int)(cell.m_alpha3 + 120), 255) > num25)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, cell.m_district3, 0, Mathf.Max(0, num25 - 120)));
                            }
                            else if (flag7 && Mathf.Max((int)(cell.m_alpha3 - 120), 0) < num24)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, cell.m_district3, Mathf.Min(255, num24 + 120), 255));
                            }
                            if (flag7)
                            {
                                flag3 = true;
                            }
                        }
                        if (cell.m_alpha4 != 0)
                        {
                            bool flag8 = cell.m_district4 == district;
                            int num26;
                            int num27;
                            this.CheckNeighbourCells(l, k, cell.m_district4, out num26, out num27);
                            if (!flag8 && Mathf.Min((int)(cell.m_alpha4 + 120), 255) > num27)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, cell.m_district4, 0, Mathf.Max(0, num27 - 120)));
                            }
                            else if (flag8 && Mathf.Max((int)(cell.m_alpha4 - 120), 0) < num26)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, cell.m_district4, Mathf.Min(255, num26 + 120), 255));
                            }
                            if (flag8)
                            {
                                flag3 = true;
                            }
                        }
                        if (!flag3)
                        {
                            int num28;
                            int num29;
                            this.CheckNeighbourCells(l, k, district, out num28, out num29);
                            if (0 < num28)
                            {
                                flag4 = (flag4 || this.SetDistrictAlpha(l, k, district, Mathf.Min(255, num28 + 120), 255));
                            }
                        }
                        if (flag4)
                        {
                            num8 = Mathf.Min(num8, l);
                            num9 = Mathf.Max(num9, l);
                            num10 = Mathf.Min(num10, k);
                            num11 = Mathf.Max(num11, k);
                            flag2 = true;
                        }
                    }
                }
                num15 = Mathf.Min(num15, num8);
                num16 = Mathf.Max(num16, num9);
                num17 = Mathf.Min(num17, num10);
                num18 = Mathf.Max(num18, num11);
            }
            while (++num19 < 10 && flag2);
            Singleton<DistrictManager>.instance.AreaModified(num15, num17, num16, num18, true);
            Singleton<DistrictManager>.instance.m_districtsNotUsed.Disable();
        }

        private bool ForceDistrictAlpha(int x, int z, byte district, int min, int max)
        {
            int num = 512;
            DistrictManager.Cell[] districtGrid = Singleton<DistrictManager>.instance.m_districtGrid;
            DistrictManager.Cell cell = districtGrid[z * num + x];
            int num2 = Mathf.Clamp(this.GetAlpha(ref cell, district), min, max);
            DistrictManager.Cell cell2 = default(DistrictManager.Cell);
            cell2.m_district1 = district;
            cell2.m_district2 = 0;
            cell2.m_alpha1 = (byte)num2;
            cell2.m_alpha2 = (byte)(255 - num2);
            if (cell2.m_alpha1 != cell.m_alpha1 || cell2.m_alpha2 != cell.m_alpha2 || cell2.m_alpha3 != cell.m_alpha3 || cell2.m_alpha4 != cell.m_alpha4 || cell2.m_district1 != cell.m_district1 || cell2.m_district2 != cell.m_district2 || cell2.m_district3 != cell.m_district3 || cell2.m_district4 != cell.m_district4)
            {
                Singleton<DistrictManager>.instance.ModifyCell(x, z, cell2);
                return true;
            }
            return false;
        }

        private bool SetDistrictAlpha(int x, int z, byte district, int min, int max)
        {
            int num = 512;
            DistrictManager.Cell[] districtGrid = Singleton<DistrictManager>.instance.m_districtGrid;
            DistrictManager.Cell cell = districtGrid[z * num + x];
            if (cell.m_district1 == district)
            {
                int num2 = Mathf.Clamp((int)cell.m_alpha1, min, max);
                if (num2 != (int)cell.m_alpha1)
                {
                    cell.m_alpha1 = (byte)num2;
                    this.Normalize(ref cell, 1);
                    Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                    return true;
                }
            }
            else if (cell.m_district2 == district)
            {
                int num3 = Mathf.Clamp((int)cell.m_alpha2, min, max);
                if (num3 != (int)cell.m_alpha2)
                {
                    cell.m_alpha2 = (byte)num3;
                    this.Normalize(ref cell, 2);
                    Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                    return true;
                }
            }
            else if (cell.m_district3 == district)
            {
                int num4 = Mathf.Clamp((int)cell.m_alpha3, min, max);
                if (num4 != (int)cell.m_alpha3)
                {
                    cell.m_alpha3 = (byte)num4;
                    this.Normalize(ref cell, 3);
                    Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                    return true;
                }
            }
            else if (cell.m_district4 == district)
            {
                int num5 = Mathf.Clamp((int)cell.m_alpha4, min, max);
                if (num5 != (int)cell.m_alpha4)
                {
                    cell.m_alpha4 = (byte)num5;
                    this.Normalize(ref cell, 4);
                    Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                    return true;
                }
            }
            else if (min > 0)
            {
                int num6 = 256;
                int num7 = -1;
                if ((int)cell.m_alpha1 < num6)
                {
                    num6 = (int)cell.m_alpha1;
                    num7 = 1;
                }
                if ((int)cell.m_alpha2 < num6)
                {
                    num6 = (int)cell.m_alpha2;
                    num7 = 2;
                }
                if ((int)cell.m_alpha3 < num6)
                {
                    num6 = (int)cell.m_alpha3;
                    num7 = 3;
                }
                if ((int)cell.m_alpha4 < num6)
                {
                    num6 = (int)cell.m_alpha4;
                    num7 = 4;
                }
                if (num6 <= min)
                {
                    if (num7 == 1)
                    {
                        cell.m_district1 = district;
                        cell.m_alpha1 = (byte)min;
                        this.Normalize(ref cell, 1);
                        Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                        return true;
                    }
                    if (num7 == 2)
                    {
                        cell.m_district2 = district;
                        cell.m_alpha2 = (byte)min;
                        this.Normalize(ref cell, 2);
                        Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                        return true;
                    }
                    if (num7 == 3)
                    {
                        cell.m_district3 = district;
                        cell.m_alpha3 = (byte)min;
                        this.Normalize(ref cell, 3);
                        Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                        return true;
                    }
                    if (num7 == 4)
                    {
                        cell.m_district4 = district;
                        cell.m_alpha4 = (byte)min;
                        this.Normalize(ref cell, 4);
                        Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                        return true;
                    }
                }
            }
            return false;
        }

        private void Normalize(ref DistrictManager.Cell cell, int ignoreIndex)
        {
            int num = 0;
            if (ignoreIndex != 1)
            {
                num += (int)cell.m_alpha1;
            }
            if (ignoreIndex != 2)
            {
                num += (int)cell.m_alpha2;
            }
            if (ignoreIndex != 3)
            {
                num += (int)cell.m_alpha3;
            }
            if (ignoreIndex != 4)
            {
                num += (int)cell.m_alpha4;
            }
            if (num != 0)
            {
                int num2 = 255;
                if (ignoreIndex == 1)
                {
                    num2 -= (int)cell.m_alpha1;
                }
                if (ignoreIndex == 2)
                {
                    num2 -= (int)cell.m_alpha2;
                }
                if (ignoreIndex == 3)
                {
                    num2 -= (int)cell.m_alpha3;
                }
                if (ignoreIndex == 4)
                {
                    num2 -= (int)cell.m_alpha4;
                }
                if (num > num2)
                {
                    if (ignoreIndex != 1)
                    {
                        cell.m_alpha1 = (byte)((int)cell.m_alpha1 * num2 / num);
                    }
                    if (ignoreIndex != 2)
                    {
                        cell.m_alpha2 = (byte)((int)cell.m_alpha2 * num2 / num);
                    }
                    if (ignoreIndex != 3)
                    {
                        cell.m_alpha3 = (byte)((int)cell.m_alpha3 * num2 / num);
                    }
                    if (ignoreIndex != 4)
                    {
                        cell.m_alpha4 = (byte)((int)cell.m_alpha4 * num2 / num);
                    }
                }
            }
        }

        private void CheckNeighbourCells(int x, int z, byte district, out int min, out int max)
        {
            min = 255;
            max = 0;
            int num = 512;
            DistrictManager.Cell[] districtGrid = Singleton<DistrictManager>.instance.m_districtGrid;
            if (z > 0)
            {
                if (x > 0)
                {
                    DistrictManager.Cell cell = districtGrid[(z - 1) * num + x - 1];
                    int alpha = this.GetAlpha(ref cell, district);
                    min = Mathf.Min(min, alpha);
                    max = Mathf.Max(max, alpha);
                }
                DistrictManager.Cell cell2 = districtGrid[(z - 1) * num + x];
                int alpha2 = this.GetAlpha(ref cell2, district);
                min = Mathf.Min(min, alpha2);
                max = Mathf.Max(max, alpha2);
                if (x < num - 1)
                {
                    DistrictManager.Cell cell3 = districtGrid[(z - 1) * num + x + 1];
                    int alpha3 = this.GetAlpha(ref cell3, district);
                    min = Mathf.Min(min, alpha3);
                    max = Mathf.Max(max, alpha3);
                }
            }
            if (x > 0)
            {
                DistrictManager.Cell cell4 = districtGrid[z * num + x - 1];
                int alpha4 = this.GetAlpha(ref cell4, district);
                min = Mathf.Min(min, alpha4);
                max = Mathf.Max(max, alpha4);
            }
            if (x < num - 1)
            {
                DistrictManager.Cell cell5 = districtGrid[z * num + x + 1];
                int alpha5 = this.GetAlpha(ref cell5, district);
                min = Mathf.Min(min, alpha5);
                max = Mathf.Max(max, alpha5);
            }
            if (z < num - 1)
            {
                if (x > 0)
                {
                    DistrictManager.Cell cell6 = districtGrid[(z + 1) * num + x - 1];
                    int alpha6 = this.GetAlpha(ref cell6, district);
                    min = Mathf.Min(min, alpha6);
                    max = Mathf.Max(max, alpha6);
                }
                DistrictManager.Cell cell7 = districtGrid[(z + 1) * num + x];
                int alpha7 = this.GetAlpha(ref cell7, district);
                min = Mathf.Min(min, alpha7);
                max = Mathf.Max(max, alpha7);
                if (x < num - 1)
                {
                    DistrictManager.Cell cell8 = districtGrid[(z + 1) * num + x + 1];
                    int alpha8 = this.GetAlpha(ref cell8, district);
                    min = Mathf.Min(min, alpha8);
                    max = Mathf.Max(max, alpha8);
                }
            }
        }

        private int GetAlpha(ref DistrictManager.Cell cell, byte district)
        {
            if (cell.m_district1 == district)
            {
                return (int)cell.m_alpha1;
            }
            if (cell.m_district2 == district)
            {
                return (int)cell.m_alpha2;
            }
            if (cell.m_district3 == district)
            {
                return (int)cell.m_alpha3;
            }
            if (cell.m_district4 == district)
            {
                return (int)cell.m_alpha4;
            }
            return 0;
        }

        public override ToolBase.ToolErrors GetErrors()
        {
            return this.m_errors;
        }
    }
}
