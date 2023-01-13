// <copyright file="ZoningTool.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.Math;
    using ColossalFramework.UI;
    using UnifiedUI.Helpers;
    using UnityEngine;

    /// <summary>
    /// The zoning selection tool.
    /// </summary>
    public class ZoningTool : DefaultTool
    {
        private static ZoningTool s_instance;

        // UI thread to simulation thread communication.
        private readonly object _simulationLock = new object();
        private ushort _segmentID = 0;
        private bool _buttonZero = false;
        private bool _buttonOne = false;
        private bool _shiftOffset = false;
        private bool _altPressed = false;

        // Previous tool state.
        private bool _prevRenderZones;

        /// <summary>
        /// Gets the active tool instance.
        /// </summary>
        public static ZoningTool Instance => ToolsModifierControl.toolController?.gameObject?.GetComponent<ZoningTool>();

        /// <summary>
        /// Gets a value indicating whether the tool is currently active (true) or inactive (false).
        /// </summary>
        public static bool IsActiveTool => Instance != null && ToolsModifierControl.toolController.CurrentTool == Instance;

        /// <summary>
        /// Gets the currently selected segment, if any.
        /// </summary>
        internal static ushort CurrentSegment
        {
            get
            {
                if (s_instance != null)
                {
                    return s_instance._segmentID;
                }

                // If we got here, the tool isn't ready; return 0.
                return 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether terrain is ignored by the tool (always returns true, i.e. terrain is ignored by the tool).
        /// </summary>
        /// <returns>True.</returns>
        public override bool GetTerrainIgnore() => true;

        /// <summary>
        /// Gets which net nodes are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>NetNode.Flags.All.</returns>
        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.All;

        /// <summary>
        /// Gets which buildings are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>NetSegment.Flags.All.</returns>
        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.All;

        /// <summary>
        /// Gets which trees are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>TreeInstance.Flags.All.</returns>
        public override global::TreeInstance.Flags GetTreeIgnoreFlags() => global::TreeInstance.Flags.All;

        /// <summary>
        /// Gets which props are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>PropInstance.Flags.All.</returns>
        public override PropInstance.Flags GetPropIgnoreFlags() => PropInstance.Flags.All;

        /// <summary>
        /// Gets which parked vehicles are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>VehicleParked.Flags.All.</returns>
        public override VehicleParked.Flags GetParkedVehicleIgnoreFlags() => VehicleParked.Flags.All;

        /// <summary>
        /// Gets which citizens are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>CitizenInstance.Flags.All.</returns>
        public override CitizenInstance.Flags GetCitizenIgnoreFlags() => CitizenInstance.Flags.All;

        /// <summary>
        /// Gets which transport lines are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>TransportLine.Flags.All.</returns>
        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.All;

        /// <summary>
        /// Gets a value indicating which transport types are supported by the tool (always returns zero, i.e. no transport type is supported by the tool).
        /// </summary>
        /// <returns>Zero.</returns>
        public override int GetTransportTypes() => 0;

        /// <summary>
        /// Gets which districts are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>District.Flags.All.</returns>
        public override District.Flags GetDistrictIgnoreFlags() => District.Flags.All;

        /// <summary>
        /// Gets which parks are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>DistrictPark.Flags.All.</returns>
        public override DistrictPark.Flags GetParkIgnoreFlags() => DistrictPark.Flags.All;

        /// <summary>
        /// Gets which disasters are ignored by the tool (always returns all, i.e. none are selectable by the tool).
        /// </summary>
        /// <returns>DisasterData.Flags.All.</returns>
        public override DisasterData.Flags GetDisasterIgnoreFlags() => DisasterData.Flags.All;

        /// <summary>
        /// Gets which network segments are ignored by the tool (always returns none, i.e. all are selectable by the tool).
        /// </summary>
        /// <param name="nameOnly">Always set to false.</param>
        /// <returns>NetSegment.Flags.None.</returns>
        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.None;
        }

        /// <summary>
        /// Sets vehicle ingore flags to ignore all vehicles.
        /// </summary>
        /// <returns>Vehicle flags ignoring all vehicles.</returns>
        public override Vehicle.Flags GetVehicleIgnoreFlags() =>
            Vehicle.Flags.LeftHandDrive
            | Vehicle.Flags.Created
            | Vehicle.Flags.Deleted
            | Vehicle.Flags.Spawned
            | Vehicle.Flags.Inverted
            | Vehicle.Flags.TransferToTarget
            | Vehicle.Flags.TransferToSource
            | Vehicle.Flags.Emergency1
            | Vehicle.Flags.Emergency2
            | Vehicle.Flags.WaitingPath
            | Vehicle.Flags.Stopped
            | Vehicle.Flags.Leaving
            | Vehicle.Flags.Arriving
            | Vehicle.Flags.Reversed
            | Vehicle.Flags.TakingOff
            | Vehicle.Flags.Flying
            | Vehicle.Flags.Landing
            | Vehicle.Flags.WaitingSpace
            | Vehicle.Flags.WaitingCargo
            | Vehicle.Flags.GoingBack
            | Vehicle.Flags.WaitingTarget
            | Vehicle.Flags.Importing
            | Vehicle.Flags.Exporting
            | Vehicle.Flags.Parking
            | Vehicle.Flags.CustomName
            | Vehicle.Flags.OnGravel
            | Vehicle.Flags.WaitingLoading
            | Vehicle.Flags.Congestion
            | Vehicle.Flags.DummyTraffic
            | Vehicle.Flags.Underground
            | Vehicle.Flags.Transition
            | Vehicle.Flags.InsideBuilding;

        /// <summary>
        /// Called by the game every simulation step.
        /// Used to perform any zone manipulations from the simulation thread.
        /// </summary>
        public override void SimulationStep()
        {
            base.SimulationStep();

            // Thread locking.
            lock (_simulationLock)
            {
                // Check to see if there's any valid segment.
                if (_segmentID != 0)
                {
                    // Store current 'disable zoning' state and temporarily disable.
                    bool zoningDisabled = ModSettings.DisableZoning;
                    ModSettings.DisableZoning = false;

                    // Local references.
                    NetManager netManager = Singleton<NetManager>.instance;
                    NetSegment[] segmentBuffer = netManager.m_segments.m_buffer;
                    ref NetSegment segment = ref segmentBuffer[_segmentID];
                    NetAI segmentAI = segment.Info.m_netAI;

                    // Check for supported network types.
                    RoadAI roadAI = segmentAI as RoadAI;
                    if (roadAI != null || segmentAI is PedestrianPathAI || segmentAI is PedestrianWayAI)
                    {
                        // Determine existing blocks.
                        bool hasLeft = segment.m_blockStartLeft != 0 || segment.m_blockEndLeft != 0;
                        bool hasRight = segment.m_blockStartRight != 0 || segment.m_blockEndRight != 0;

                        // Remove any attached zone blocks.
                        RemoveLeftZoneBlocks(ref segment);
                        RemoveRightZoneBlocks(ref segment);

                        // Replace with new zone blocks if the button click was with the primary mouse button.
                        if (_buttonZero)
                        {
                            // Record current settings on selected segment, overwriting any existing settings.
                            SegmentData.Instance.SetCurrentMode(_segmentID);

                            // Create zone block via road AI, if this is a road AI - allow for any other mods with patches attached.
                            if (roadAI != null)
                            {
                                roadAI.CreateZoneBlocks(_segmentID, ref segment);
                            }
                            else
                            {
                                // Non-road AI; call CreateZoneBlocks prefix directory.
                                CreateZoneBlocks.Prefix(_segmentID, ref segment);
                            }

                            // If alt key is held down, toggle left/right/both.
                            if (_altPressed)
                            {
                                // Default right->both is already done at this stage; to do both->left and left->right we need to remove a side.
                                if (hasLeft)
                                {
                                    if (hasRight)
                                    {
                                        // Both->left.
                                        RemoveRightZoneBlocks(ref segment);
                                    }
                                    else
                                    {
                                        // Left->right.
                                        RemoveLeftZoneBlocks(ref segment);
                                    }
                                }
                            }

                            // Restore 'disable zoning' state.
                            ModSettings.DisableZoning = zoningDisabled;
                        }

                        // Update renderer - expand bounds outward to catch rounding errors.
                        Vector3 min = segment.m_bounds.min - Vector3.one;
                        Vector3 max = segment.m_bounds.max + Vector3.one;
                        Quad2 quad = new Quad2(new Vector2(min.x, min.z), new Vector2(min.x, max.z), new Vector2(max.x, max.z), new Vector2(max.x, min.z));
                        Singleton<ZoneManager>.instance.UpdateBlocks(quad);
                    }

                    // Clear segment reference to indicate that work is donw.
                    _segmentID = 0;
                }
            }
        }

        /// <summary>
        /// Toggles the current tool to/from the zoning tool.
        /// </summary>
        internal static void ToggleTool()
        {
            // Activate zoning tool if it isn't already; if already active, deactivate it by selecting the previously active tool instead.
            if (!IsActiveTool)
            {
                // Activate tool.
                ToolsModifierControl.toolController.CurrentTool = Instance;
            }
            else
            {
                // Activate default tool.
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }

        /// <summary>
        /// Initialise the tool.
        /// Called by unity when the tool is created.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Set default cursor.
            m_cursor = UITextures.LoadCursor("ZoningAdjusterCursor.png");

            // Create new UUI button.
            UIComponent uuiButton = UUIHelpers.RegisterToolButton(
                name: nameof(Mod),
                groupName: null, // default group
                tooltip: Translations.Translate("ZMD_NAME"),
                tool: this,
                icon: UUIHelpers.LoadTexture(UUIHelpers.GetFullPath<Mod>("Resources", "ZoningAdjusterUUI.png")),
                hotkeys: new UUIHotKeys { ActivationKey = ModSettings.ToolKey });
        }

        /// <summary>
        /// Unity late update handling.
        /// Called by game every late update.
        /// </summary>
        protected override void OnToolLateUpdate()
        {
            base.OnToolLateUpdate();

            // Force the info mode to none.
            ForceInfoMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.None);
        }

        /// <summary>
        /// Called by game when tool is enabled.
        /// </summary>
        protected override void OnEnable()
        {
            // Call base even before loaded checks to properly initialize tool.
            base.OnEnable();

            // Don't do anything if game isn't loaded.
            if (!Loading.IsLoaded)
            {
                return;
            }

            Logging.Message("tool enabled");

            // Set instance.
            s_instance = this;

            // Create zoning settings panel if it isn't already created, and in any case make sure it's visible.
            ZoningSettingsPanel.Create();

            // Show zone grids when tool is active.
            _prevRenderZones = Singleton<TerrainManager>.instance.RenderZones;
            Singleton<TerrainManager>.instance.RenderZones = true;

            // Set button state to indicate tool is active.
            ZoningAdjusterButton.ToolActive = true;
        }

        /// <summary>
        /// Called by game when tool is disabled.
        /// </summary>
        protected override void OnDisable()
        {
            Logging.Message("tool disabled");

            base.OnDisable();

            // Restore zone grid showing to previous state.
            Singleton<TerrainManager>.instance.RenderZones = _prevRenderZones;

            // Set panel button state to indicate tool no longer active.
            ZoningAdjusterButton.ToolActive = false;

            // Close panel.
            ZoningSettingsPanel.Close();
        }

        /// <summary>
        /// Tool GUI event processing.
        /// Called by game every GUI update.
        /// </summary>
        /// <param name="e">Event.</param>
        protected override void OnToolGUI(Event e)
        {
            // Check for escape key.
            if (e.type == EventType.keyDown && e.keyCode == KeyCode.Escape)
            {
                // Escape key pressed - revert to default tool.
                e.Use();
                ToolsModifierControl.SetTool<DefaultTool>();
            }

            // Don't do anything if mouse is inside UI or if there are any errors other than failed raycast.
            if (m_toolController.IsInsideUI || (m_selectErrors != ToolErrors.None && m_selectErrors != ToolErrors.RaycastFailed))
            {
                return;
            }

            // Try to get a hovered network instance.
            ushort segmentID = m_hoverInstance.NetSegment;
            if (segmentID != 0)
            {
                // Check for mousedown events.
                if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 1))
                {
                    // Got one; use the event.
                    UIInput.MouseUsed();

                    // Need to update zoning via simulation thread - set the fields for SimulationStep to pick up.
                    lock (_simulationLock)
                    {
                        _segmentID = segmentID;
                        _buttonZero = e.button == 0;
                        _buttonOne = e.button == 1;
                        _shiftOffset = OffsetKeyThreading.ShiftOffset;
                        _altPressed = (e.modifiers & EventModifiers.Alt) != EventModifiers.None;
                    }
                }
            }
        }

        /// <summary>
        /// Removes the left-side zoning blocks from a segment.
        /// </summary>
        /// <param name="segment">Segment data reference.</param>
        private void RemoveLeftZoneBlocks(ref NetSegment segment)
        {
            // Release blocks.
            ZoneManager zoneManager = Singleton<ZoneManager>.instance;
            if (segment.m_blockStartLeft != 0)
            {
                zoneManager.ReleaseBlock(segment.m_blockStartLeft);
                segment.m_blockStartLeft = 0;
            }

            if (segment.m_blockEndLeft != 0)
            {
                zoneManager.ReleaseBlock(segment.m_blockEndLeft);
                segment.m_blockEndLeft = 0;
            }
        }

        /// <summary>
        /// Removes the right-side zoning blocks from a segment.
        /// </summary>
        /// <param name="segment">Segment data reference.</param>
        private void RemoveRightZoneBlocks(ref NetSegment segment)
        {
            // Release blocks.
            ZoneManager zoneManager = Singleton<ZoneManager>.instance;
            if (segment.m_blockStartRight != 0)
            {
                zoneManager.ReleaseBlock(segment.m_blockStartRight);
                segment.m_blockStartRight = 0;
            }

            if (segment.m_blockEndRight != 0)
            {
                zoneManager.ReleaseBlock(segment.m_blockEndRight);
                segment.m_blockEndRight = 0;
            }
        }
    }
}
