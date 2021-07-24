using System;
using System.IO;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using UnifiedUI.Helpers;


namespace ZoningAdjuster
{
	/// <summary>
	/// The zoning selection tool.
	/// </summary>
	public class ZoningTool : DefaultTool
	{
		// Previous tool state.
		private bool prevRenderZones;


		/// <summary>
		/// Instance reference.
		/// </summary>
		public static ZoningTool Instance => ToolsModifierControl.toolController?.gameObject?.GetComponent<ZoningTool>();


		// Ignore anything except segments.
		public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.All;
		public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.All;
		public override CitizenInstance.Flags GetCitizenIgnoreFlags() => CitizenInstance.Flags.All;
		public override DisasterData.Flags GetDisasterIgnoreFlags() => DisasterData.Flags.All;
		public override District.Flags GetDistrictIgnoreFlags() => District.Flags.All;
		public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.All;
		public override VehicleParked.Flags GetParkedVehicleIgnoreFlags() => VehicleParked.Flags.All;
		public override TreeInstance.Flags GetTreeIgnoreFlags() => TreeInstance.Flags.All;
		public override Vehicle.Flags GetVehicleIgnoreFlags() => Vehicle.Flags.LeftHandDrive | Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding;


		/// <summary>
		/// Called by the game.  Sets which network segments are ignored by the tool (always returns none, i.e. all segments are selectable by the tool).
		/// </summary>
		/// <param name="nameOnly">Always set to false</param>
		/// <returns>NetSegment.Flags.None</returns>
		public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
		{
			nameOnly = false;
			return NetSegment.Flags.None;
		}


		/// <summary>
		/// Initialise the tool.
		/// Called by unity when the tool is created.
		/// </summary>
		protected override void Awake()
		{
			try
			{
				base.Awake();

				// Set default cursor.
				m_cursor = TextureUtils.LoadCursor("ZAcursor.png");

				// Register UUI button.
				UIButton panelButton = UUIHelpers.RegisterToolButton(
					name: "ZoningAdjuster",
					groupName: null, // default group
					tooltip: "Zoning Adjuster tool",
					spritefile: Path.Combine(Path.Combine(ModUtils.GetAssemblyPath(), "Resources"), "uui_zoning_adjuster.png"),
					tool: this,
					activationKey: new SavedInputKey(
						ModSettings.SettingsFileName, "ZoningAdjuster",
						key: KeyCode.A, control: false, shift: false, alt: true, true)
					) as UIButton;
			}
			catch (Exception e)
			{
				Logging.LogException(e, "exception waking select tool");
			}
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
			base.OnEnable();

			Logging.Message("tool enabled");

			// Show zone grids when tool is active.
			prevRenderZones = Singleton<TerrainManager>.instance.RenderZones;
			Singleton<TerrainManager>.instance.RenderZones = true;

			// Show panel if it isn't already.
			ZoningSettingsPanel.Create();

			// Set button state to indicate tool is active.
			//ZoningAdjusterButton.ToolActive = true;
		}


		/// <summary>
		/// Called by game when tool is disabled.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();

			// Restore zone grid showing to previous state.
			Singleton<TerrainManager>.instance.RenderZones = prevRenderZones;

			// Hide panel if we're not keeping it when road tool is active.
			//if (!ModSettings.ShowPanel)
            {
				ZoningSettingsPanel.Close();
            }

			// Set panel button state to indicate tool no longer active.
			//ZoningAdjusterButton.ToolActive = false;
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
				// We have a hovered network; set the cursor to the light cursor.
				//m_cursor = lightCursor;

				// Check for mousedown events.
				if (e.type == EventType.MouseDown && e.button == 0 || e.button == 1)
				{
					// Got one; use the event.
					UIInput.MouseUsed();

					// Local references.
					NetManager netManager = Singleton<NetManager>.instance;
					NetSegment[] segmentBuffer = netManager.m_segments.m_buffer;
					NetSegment segment = segmentBuffer[segmentID];

					// Only interested in RoadAI, as that's the only one with zoning.
					if (segment.Info.GetAI() is RoadAI roadAI)
					{
						// Remove any attached zone blocks.
						RemoveZoneBlock(ref segment.m_blockStartLeft);
						RemoveZoneBlock(ref segment.m_blockStartRight);
						RemoveZoneBlock(ref segment.m_blockEndLeft);
						RemoveZoneBlock(ref segment.m_blockEndRight);

						// Replace with new zone blocks if the button click was with the primary mouse button.
						if (e.button == 0)
						{
							roadAI.CreateZoneBlocks(segmentID, ref netManager.m_segments.m_buffer[segmentID]);
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
				Logging.Message("removing zone block ", blockID.ToString());

				Singleton<ZoneManager>.instance.ReleaseBlock(blockID);
				blockID = 0;
			}
		}
	}
}
