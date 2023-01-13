// <copyright file="CreateZoneBlocks.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>
// Based on code by Colossal Order, with a few lines from the Network Extensions 2 mod.

namespace ZoningAdjuster
{
    using ColossalFramework;
    using ColossalFramework.Math;
    using UnityEngine;

    /// <summary>
    ///  Hamony pre-emptive prefix for adjusting zone block creation.
    ///  Manually applied and unapplied.
    /// </summary>
    public static class CreateZoneBlocks
    {
        private const float MinHalfwidthTinyCurve = 6f;

        private static float s_setback = 0f;

        /// <summary>
        /// Gets or sets the current zoning setback distance (in metres).
        /// </summary>
        internal static float Setback { get => s_setback; set => s_setback = value; }

        /// <summary>
        /// Harmony pre-emptive Prefix for RoadAI.CreateZoneBlock to implement the mod's zone creation functionality.
        /// </summary>
        /// <param name="segment">Segment ID of this segment.</param>
        /// <param name="data">Segment data.</param>
        /// <returns>False (always).</returns>
        public static bool Prefix(ushort segment, ref NetSegment data)
        {
            // Don't do anything if zoning is disabled, or if the settings for the current segment prevent zoning.
            if (!SegmentData.Instance.NoZoning(segment))
            {
                // Local references.
                NetManager netManager = Singleton<NetManager>.instance;
                Randomizer randomizer = new Randomizer((int)segment);
                NetInfo info = data.Info;
                NetNode startNode = netManager.m_nodes.m_buffer[(int)data.m_startNode];
                NetNode endNode = netManager.m_nodes.m_buffer[(int)data.m_endNode];

                // Is this a straight or curved segment?
                Vector3 startPosition = startNode.m_position;
                Vector3 endPosition = endNode.m_position;
                Vector3 startDirection = data.m_startDirection;
                Vector3 endDirection = data.m_endDirection;
                if (NetSegment.IsStraight(startPosition, startDirection, endPosition, endDirection))
                {
                    StraightZoneBlocks(info, randomizer, segment, ref data, startNode, endNode);
                }
                else
                {
                    CurvedZoneBlocks(info, randomizer, segment, ref data);
                }
            }

            // Pre-empt original method.
            return false;
        }

        /// <summary>
        /// Create curved zone blocks.
        /// </summary>
        /// <param name="info">Network prefab.</param>
        /// <param name="randomizer">Randomizer.</param>
        /// <param name="segmentID">Segment ID.</param>
        /// <param name="segment">Network segment.</param>
        private static void CurvedZoneBlocks(NetInfo info, Randomizer randomizer, ushort segmentID, ref NetSegment segment)
        {
#pragma warning disable IDE0018 // Inline variable declaration

            // TODO: Basically game code with some NExT2 modifications and some Zoning Adjuster modifications.  Could do with a good refactor, and possible replacement with Transpiler.
            float minHalfWidth = MinHalfwidthTinyCurve;

            NetManager instance = Singleton<NetManager>.instance;
            Vector3 startPosition = instance.m_nodes.m_buffer[(int)segment.m_startNode].m_position;
            Vector3 endPosition = instance.m_nodes.m_buffer[(int)segment.m_endNode].m_position;
            Vector3 startDirection = segment.m_startDirection;
            Vector3 endDirection = segment.m_endDirection;

            float num = (startDirection.x * endDirection.x) + (startDirection.z * endDirection.z);

            // Distance of zoning block extent from edge of road.
            float halfWidth = Mathf.Max(minHalfWidth, info.m_halfWidth); // num2; minHalfWidth here.
            float blockEndDistance = 32f + s_setback; // num3;
            int distance = Mathf.RoundToInt(halfWidth);

            float num4 = VectorUtils.LengthXZ(endPosition - startPosition);
            bool flag2 = (startDirection.x * endDirection.z) - (startDirection.z * endDirection.x) > 0f;
            bool flag3 = num < -0.8f || num4 > 50f;
            if (flag2)
            {
                halfWidth = -halfWidth;
                blockEndDistance = -blockEndDistance;
            }

            Vector3 vector = startPosition - (new Vector3(startDirection.z, 0f, -startDirection.x) * halfWidth);
            Vector3 vector2 = endPosition + (new Vector3(endDirection.z, 0f, -endDirection.x) * halfWidth);
            Vector3 vector3;
            Vector3 vector4;
            NetSegment.CalculateMiddlePoints(vector, startDirection, vector2, endDirection, true, true, out vector3, out vector4);
            if (flag3)
            {
                float num5 = (num * 0.025f) + 0.04f;
                float num6 = (num * 0.025f) + 0.06f;
                if (num < -0.9f)
                {
                    num6 = num5;
                }

                Bezier3 bezier = new Bezier3(vector, vector3, vector4, vector2);
                vector = bezier.Position(num5);
                vector3 = bezier.Position(0.5f - num6);
                vector4 = bezier.Position(0.5f + num6);
                vector2 = bezier.Position(1f - num5);
            }
            else
            {
                Bezier3 bezier2 = new Bezier3(vector, vector3, vector4, vector2);
                vector3 = bezier2.Position(0.86f);
                vector = bezier2.Position(0.14f);
            }

            float num7;
            Vector3 vector5 = VectorUtils.NormalizeXZ(vector3 - vector, out num7);
            int insideBlockRows = Mathf.FloorToInt((num7 / 8f) + 0.01f);
            float num9 = (num7 * 0.5f) + ((float)(insideBlockRows - 8) * ((!flag2) ? -4f : 4f));
            if (insideBlockRows != 0)
            {
                // TODO: Limit group width here
                float angle = (!flag2) ? Mathf.Atan2(vector5.x, -vector5.z) : Mathf.Atan2(-vector5.x, vector5.z);
                Vector3 position3 = vector + new Vector3((vector5.x * num9) - (vector5.z * blockEndDistance), 0f, (vector5.z * num9) + (vector5.x * blockEndDistance));
                if (flag2)
                {
                    Singleton<ZoneManager>.instance.CreateBlock(
                        out segment.m_blockStartRight,
                        ref randomizer,
                        segmentID,
                        position3,
                        angle,
                        insideBlockRows,
                        distance,
                        segment.m_buildIndex);
                }
                else
                {
                    Singleton<ZoneManager>.instance.CreateBlock(
                        out segment.m_blockStartLeft,
                        ref randomizer,
                        segmentID,
                        position3,
                        angle,
                        insideBlockRows,
                        distance,
                        segment.m_buildIndex);
                }
            }

            if (flag3)
            {
                vector5 = VectorUtils.NormalizeXZ(vector2 - vector4, out num7);
                insideBlockRows = Mathf.FloorToInt((num7 / 8f) + 0.01f);
                num9 = (num7 * 0.5f) + ((float)(insideBlockRows - 8) * ((!flag2) ? -4f : 4f));
                if (insideBlockRows != 0)
                {
                    // TODO: Limit group width here
                    float angle2 = (!flag2) ? Mathf.Atan2(vector5.x, -vector5.z) : Mathf.Atan2(-vector5.x, vector5.z);
                    Vector3 position4 = vector4 + new Vector3((vector5.x * num9) - (vector5.z * blockEndDistance), 0f, (vector5.z * num9) + (vector5.x * blockEndDistance));
                    if (flag2)
                    {
                        Singleton<ZoneManager>.instance.CreateBlock(
                            out segment.m_blockEndRight,
                            ref randomizer,
                            segmentID,
                            position4,
                            angle2,
                            insideBlockRows,
                            distance,
                            segment.m_buildIndex + 1u);
                    }
                    else
                    {
                        Singleton<ZoneManager>.instance.CreateBlock(
                            out segment.m_blockEndLeft,
                            ref randomizer,
                            segmentID,
                            position4,
                            angle2,
                            insideBlockRows,
                            distance,
                            segment.m_buildIndex + 1u);
                    }
                }
            }

            Vector3 vector6 = startPosition + (new Vector3(startDirection.z, 0f, -startDirection.x) * halfWidth);
            Vector3 vector7 = endPosition - (new Vector3(endDirection.z, 0f, -endDirection.x) * halfWidth);
            Vector3 b;
            Vector3 c;
            NetSegment.CalculateMiddlePoints(vector6, startDirection, vector7, endDirection, true, true, out b, out c);
            Bezier3 bezier3 = new Bezier3(vector6, b, c, vector7);
            Vector3 vector8 = bezier3.Position(0.5f);
            Vector3 vector9 = bezier3.Position(0.25f);
            vector9 = Line2.Offset(VectorUtils.XZ(vector6), VectorUtils.XZ(vector8), VectorUtils.XZ(vector9));
            Vector3 vector10 = bezier3.Position(0.75f);
            vector10 = Line2.Offset(VectorUtils.XZ(vector7), VectorUtils.XZ(vector8), VectorUtils.XZ(vector10));
            Vector3 vector11 = vector6;
            Vector3 a = vector7;
            float d;
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            float num10;
            if (Line2.Intersect(VectorUtils.XZ(startPosition), VectorUtils.XZ(vector6), VectorUtils.XZ(vector11 - vector9), VectorUtils.XZ(vector8 - vector9), out d, out num10))
            {
                vector6 = startPosition + ((vector6 - startPosition) * d);
            }

            if (Line2.Intersect(VectorUtils.XZ(endPosition), VectorUtils.XZ(vector7), VectorUtils.XZ(a - vector10), VectorUtils.XZ(vector8 - vector10), out d, out num10))
            {
                vector7 = endPosition + ((vector7 - endPosition) * d);
            }

            if (Line2.Intersect(VectorUtils.XZ(vector11 - vector9), VectorUtils.XZ(vector8 - vector9), VectorUtils.XZ(a - vector10), VectorUtils.XZ(vector8 - vector10), out d, out num10))
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            {
                vector8 = vector11 - vector9 + ((vector8 - vector11) * d);
            }

            float num11;
            Vector3 vector12 = VectorUtils.NormalizeXZ(vector8 - vector6, out num11);
            int outsideBlockRows = Mathf.FloorToInt((num11 / 8f) + 0.01f);
            float num13 = (num11 * 0.5f) + ((float)(outsideBlockRows - 8) * ((!flag2) ? 4f : -4f));
            if (outsideBlockRows != 0)
            {
                // TODO: Limit group width here
                float angle3 = (!flag2) ? Mathf.Atan2(-vector12.x, vector12.z) : Mathf.Atan2(vector12.x, -vector12.z);
                Vector3 position5 = vector6 + new Vector3((vector12.x * num13) + (vector12.z * blockEndDistance), 0f, (vector12.z * num13) - (vector12.x * blockEndDistance));
                if (flag2)
                {
                    Singleton<ZoneManager>.instance.CreateBlock(
                        out segment.m_blockStartLeft,
                        ref randomizer,
                        segmentID,
                        position5,
                        angle3,
                        outsideBlockRows,
                        distance,
                        segment.m_buildIndex);
                }
                else
                {
                    Singleton<ZoneManager>.instance.CreateBlock(
                        out segment.m_blockStartRight,
                        ref randomizer,
                        segmentID,
                        position5,
                        angle3,
                        outsideBlockRows,
                        distance,
                        segment.m_buildIndex);
                }
            }

            vector12 = VectorUtils.NormalizeXZ(vector7 - vector8, out num11);
            outsideBlockRows = Mathf.FloorToInt((num11 / 8f) + 0.01f);
            num13 = (num11 * 0.5f) + ((float)(outsideBlockRows - 8) * ((!flag2) ? 4f : -4f));
            if (outsideBlockRows != 0)
            {
                // TODO: Limit group width here
                float angle4 = (!flag2) ? Mathf.Atan2(-vector12.x, vector12.z) : Mathf.Atan2(vector12.x, -vector12.z);
                Vector3 position6 = vector8 + new Vector3((vector12.x * num13) + (vector12.z * blockEndDistance), 0f, (vector12.z * num13) - (vector12.x * blockEndDistance));
                if (flag2)
                {
                    Singleton<ZoneManager>.instance.CreateBlock(
                        out segment.m_blockEndLeft,
                        ref randomizer,
                        segmentID,
                        position6,
                        angle4,
                        outsideBlockRows,
                        distance,
                        segment.m_buildIndex + 1u);
                }
                else
                {
                    Singleton<ZoneManager>.instance.CreateBlock(
                        out segment.m_blockEndRight,
                        ref randomizer,
                        segmentID,
                        position6,
                        angle4,
                        outsideBlockRows,
                        distance,
                        segment.m_buildIndex + 1u);
                }
            }
#pragma warning restore IDE0018 // Inline variable declaration
        }

        /// <summary>
        /// Zoning for straight segments.
        /// </summary>
        /// <param name="info">Network prefab.</param>
        /// <param name="randomizer">Randomizer.</param>
        /// <param name="segmentID">Segment ID.</param>
        /// <param name="segment">Segment data.</param>
        /// <param name="startNode">Segment start node.</param>
        /// <param name="endNode">Segment end node.</param>
        private static void StraightZoneBlocks(NetInfo info, Randomizer randomizer, ushort segmentID, ref NetSegment segment, NetNode startNode, NetNode endNode)
        {
            // Zoning cell size, in metres.
            const float CellSize = 8f;

            // Maximum extent of zonning from road, in metres.
            const float MaxExtent = 32f;

            // Calculated half-width to use.
            float minHalfWidth = info.m_halfWidth;

            if (minHalfWidth <= 4f)
            {
                // Minimum of 4.
                minHalfWidth = 4f;
            }
            else if (minHalfWidth < 8f)
            {
                // If half-width is between 4 and 8, push out to 8.
                minHalfWidth = 8f;
            }
            else
            {
                // Fix specifically for BadPeanut's 32m roads that have half-widths just under an even 16, but also for any others with similar issues.
                minHalfWidth = Mathf.RoundToInt(minHalfWidth);
            }

            // Local reference.
            ZoneManager zoneManager = Singleton<ZoneManager>.instance;

            // Distance of zoning block extent from centre of road.
            float maxDistance = minHalfWidth + MaxExtent + s_setback;
            int distance = Mathf.RoundToInt(maxDistance);

            // Distance from start of segment to start zoning (in metres).
            float startOffset = 0f;

            // Modify starting offset accoring to calculated half-width and settings.
            if (minHalfWidth % 8 != 0)
            {
                // Calculated halfwidth is not a multiple of eight; offset by 4m, or 0m if shiftOffset is set.
                if (!OffsetKeyThreading.ShiftOffset)
                {
                    startOffset = 4f;
                }
            }
            else
            {
                // Calculated halfwidth is a multiple of eight; default offset is zero, or 4m if shiftOffset is set.
                if (OffsetKeyThreading.ShiftOffset)
                {
                    startOffset = 4f;
                }
            }

            // Start of zoning along segment.
            Vector3 startDirection = segment.m_startDirection;
            Vector3 startPosition = startNode.m_position - (startOffset * startDirection);
            float startAngle = Mathf.Atan2(startDirection.x, -startDirection.z); // num14

            // End of zoning along segment.
            Vector3 endDirection = segment.m_endDirection;
            Vector3 endPosition = endNode.m_position - (startOffset * endDirection);
            float endAngle = Mathf.Atan2(endDirection.x, -endDirection.z); // num 16

            // Magnitude of vector from start to end position.
            float magnitude = new Vector2(endPosition.x - startPosition.x, endPosition.z - startPosition.z).magnitude;

            // Total number of rows to add.
            int rows = Mathf.FloorToInt((magnitude / CellSize) + 0.1f); // num11

            // Divide into two blocks: segment start and end, with a maximum of 8 in each (zoning manager will only do blocks of up to 8 length).
            int segmentStartRows = (rows <= 8) ? rows : ((rows + 1) >> 1); // num12
            int segmentEndRows = (rows <= 8) ? 0 : (rows >> 1); // num13

            // TODO: Limit group width here

            // Add first group of eight (if any).
            if (segmentStartRows > 0)
            {
                // Left side of road.
                Vector3 leftOffset = new Vector3((startDirection.x * MaxExtent) - (startDirection.z * maxDistance), 0f, (startDirection.z * MaxExtent) + (startDirection.x * maxDistance));
                Vector3 position = startPosition + leftOffset;
                zoneManager.CreateBlock(out segment.m_blockStartLeft, ref randomizer, segmentID, position, startAngle, segmentStartRows, distance, segment.m_buildIndex);

                // Right side of road.
                Vector3 rightOffset = new Vector3((startDirection.x * (float)(segmentStartRows - 4) * CellSize) + (startDirection.z * maxDistance), 0f, (startDirection.z * (float)(segmentStartRows - 4) * CellSize) - (startDirection.x * maxDistance));
                position = startPosition + rightOffset;
                zoneManager.CreateBlock(out segment.m_blockStartRight, ref randomizer, segmentID, position, startAngle + 3.14159274f, segmentStartRows, distance, segment.m_buildIndex);
            }

            // Add second group of eight (if any).
            if (segmentEndRows > 0)
            {
                float endOffset = magnitude - ((float)rows * CellSize);

                // Left side of road.
                Vector3 leftOffset = new Vector3((endDirection.x * (((float)(segmentEndRows - 4) * CellSize) + endOffset)) + (endDirection.z * maxDistance), 0f, (endDirection.z * (((float)(segmentEndRows - 4) * CellSize) + endOffset)) - (endDirection.x * maxDistance));
                Vector3 position = endPosition + leftOffset;
                zoneManager.CreateBlock(out segment.m_blockEndLeft, ref randomizer, segmentID, position, endAngle + 3.14159274f, segmentEndRows, distance, segment.m_buildIndex + 1u);

                // Right side of road.
                Vector3 rightOffset = new Vector3((endDirection.x * (MaxExtent + endOffset)) - (endDirection.z * maxDistance), 0f, (endDirection.z * (MaxExtent + endOffset)) + (endDirection.x * maxDistance));
                position = endPosition + rightOffset;
                zoneManager.CreateBlock(out segment.m_blockEndRight, ref randomizer, segmentID, position, endAngle, segmentEndRows, distance, segment.m_buildIndex + 1u);
            }
        }
    }
}