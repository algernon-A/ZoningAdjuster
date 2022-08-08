namespace ZoningAdjuster
{
	using AlgernonCommons;
	using AlgernonCommons.Translation;
	using AlgernonCommons.UI;
	using ColossalFramework;
	using ColossalFramework.UI;
	using UnifiedUI.Helpers;
	using UnityEngine;

	/// <summary>
	/// The zoning selection tool.
	/// </summary>
	public class ZoningTool : DefaultTool
	{
		// Previous tool state.
		private static ToolBase s_previousTool;
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
		/// Sets which network segments are ignored by the tool (always returns none, i.e. all are selectable by the tool).
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
		/// Toggles the current tool to/from the zoning tool.
		/// </summary>
		internal static void ToggleTool()
		{
			// Activate zoning tool if it isn't already; if already active, deactivate it by selecting the previously active tool instead.
			if (!IsActiveTool)
			{
				// Record previous tool.
				s_previousTool = ToolsModifierControl.toolController.CurrentTool;
				ToolsModifierControl.toolController.CurrentTool = Instance;
			}
			else
			{
				// Revert to previously selected tool.
				ToolsModifierControl.toolController.CurrentTool = s_previousTool;
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
			ToolBase.ForceInfoMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.None);
		}

		/// <summary>
		/// Called by game when tool is enabled.
		/// </summary>
		protected override void OnEnable()
		{
			// Don't do anything if game isn't loaded.
			if (!Loading.IsLoaded)
			{
				return;
			}

			Logging.Message("tool enabled");
			base.OnEnable();

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
		/// <param name="e">Event</param>
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
				if (e.type == EventType.MouseDown && e.button == 0 || e.button == 1)
				{
					// Got one; use the event.
					UIInput.MouseUsed();

					// Store current 'disable zoning' state and temporarily disable.
					bool zoningDisabled = ModSettings.disableZoning;
					ModSettings.disableZoning = false;

					// Local references.
					NetManager netManager = Singleton<NetManager>.instance;
					NetSegment[] segmentBuffer = netManager.m_segments.m_buffer;
					NetSegment segment = segmentBuffer[segmentID];
					NetAI segmentAI = segment.Info.m_netAI;

					// Check for supported network types.
					RoadAI roadAI = segmentAI as RoadAI;
					if (roadAI != null || segmentAI is PedestrianPathAI || segmentAI is PedestrianWayAI)
					{
						// Determine existing blocks.
						bool hasLeft = segment.m_blockStartLeft != 0 || segment.m_blockEndLeft != 0;
						bool hasRight = segment.m_blockStartRight != 0 || segment.m_blockEndRight != 0;

						// Remove any attached zone blocks.
						RemoveZoneBlock(ref segment.m_blockStartLeft);
						RemoveZoneBlock(ref segment.m_blockStartRight);
						RemoveZoneBlock(ref segment.m_blockEndLeft);
						RemoveZoneBlock(ref segment.m_blockEndRight);

						// Replace with new zone blocks if the button click was with the primary mouse button.
						if (e.button == 0)
						{
							// If shift key is held down, toggle offset.
							if ((e.modifiers & EventModifiers.Shift) != EventModifiers.None)
                            {
								OffsetKeyThreading.shiftOffset = true;
                            }

							// Create zone block via road AI, if this is a road AI - allow for any other mods with patches attached.
							if (roadAI != null)
							{
								roadAI.CreateZoneBlocks(segmentID, ref netManager.m_segments.m_buffer[segmentID]);
							}
							else
                            {
								// Non-road AI; call CreateZoneBlocks prefix directory.
								ZoneBlockPatch.Prefix(segmentID, ref netManager.m_segments.m_buffer[segmentID]);
                            }

							// If alt key is held down, toggle left/right/both.
							if (((e.modifiers & EventModifiers.Alt) != EventModifiers.None) && hasLeft)
							{
								if (hasRight)
								{
									// Both->left.
									RemoveZoneBlock(ref netManager.m_segments.m_buffer[segmentID].m_blockStartRight);
									RemoveZoneBlock(ref netManager.m_segments.m_buffer[segmentID].m_blockEndRight);
								}
								else
								{
									// Left->right.
									RemoveZoneBlock(ref netManager.m_segments.m_buffer[segmentID].m_blockStartLeft);
									RemoveZoneBlock(ref netManager.m_segments.m_buffer[segmentID].m_blockEndLeft);
								}
							}

							// Reset shift offset.
							OffsetKeyThreading.shiftOffset = false;

							// Restore 'disable zoning' state.
							ModSettings.disableZoning = zoningDisabled;
						}
					}
				}
			}
		}

		/// <summary>
		/// Removes a zoning block.
		/// </summary>
		/// <param name="blockID">Zoning block reference to remove (will be set to zero)</param>
		private void RemoveZoneBlock(ref ushort blockID)
		{
			if (blockID != 0)
			{
				Logging.Message("removing zone block ", blockID);

				Singleton<ZoneManager>.instance.ReleaseBlock(blockID);
				blockID = 0;
			}
		}
	}
}
