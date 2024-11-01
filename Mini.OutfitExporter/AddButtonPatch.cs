// <copyright file="AddButtonPatch.cs" company="miniduikboot">
// This file is part of Mini.OutfitExporter.
//
// Mini.OutfitExporter is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Mini.OutfitExporter is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Mini.OutfitExporter.  If not, see https://www.gnu.org/licenses/
// </copyright>

namespace Mini.OutfitExporter;

using System;
using System.Text.Json;
using AmongUs.Data;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

[HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Start))]
public static class AddButtonPatch
{
	// Justification: __instance name required by Harmony
#pragma warning disable SA1313 // ParameterNamesMustBeginWithLowerCaseLetter
	public static void Postfix(PlayerCustomizationMenu __instance)
#pragma warning restore SA1313
	{
		// The customization menu in game vs the inventory menu is named differently
		var template = GameObject.Find("PlayerCustomizationMenu(Clone)/Header/CloseButton")
			?? GameObject.Find("LobbyPlayerCustomizationMenu(Clone)/Header/CloseButton");

		// Y position depends on the scene, OnlineGame renders a few extra buttons, so move below them
		var scene = SceneManager.GetActiveScene();
		var y_position = scene.name == "OnlineGame" ? 3.8f : 3.03f;

		OutfitExporterPlugin.Logger?.LogInfo("Opening Customization menu in Scene {scene.name}");

		if (template == null)
		{
			OutfitExporterPlugin.Logger?.LogError("Could not find CloseButton in (Lobby)PlayerCustomizationMenu");
		}
		else
		{
			AddButton(template, "Copy Button", OutfitExporterPlugin.CopyButtonSprite, 1.29f, y_position, (renderer) =>
			{
				// Set the clipboard to the currently equipped outfit
				GUIUtility.systemCopyBuffer = GetSerializedOutfit();
				renderer.color = Color.green;
				__instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
				{
					if (p > 0.95)
					{
						renderer.color = Color.white;
					}
				})));
			});

			AddButton(template, "Paste Button", OutfitExporterPlugin.PasteButtonSprite, 0.43f, y_position, (renderer) =>
			{
				renderer.color = Color.yellow;
				bool success = SetSerializedOutfit(GUIUtility.systemCopyBuffer, __instance.PreviewArea);
				renderer.color = success ? Color.green : Color.red;
				__instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
				{
					if (p > 0.95)
					{
						renderer.color = Color.white;
					}
				})));
			});
		}
	}

	private static void AddButton(GameObject template, string name, Sprite? sprite, float x_position, float y_position, Action<SpriteRenderer> listener)
	{
		var button = GameObject.Instantiate(template, template.transform.parent);
		button.name = $"[{OutfitExporterPlugin.Id}] {name}";

		var btnPassive = button.GetComponent<PassiveButton>();
		var btnRenderer = button.GetComponent<SpriteRenderer>();
		btnPassive.OnClick.RemoveAllListeners();
		btnPassive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
		btnPassive.OnClick.AddListener((Action)(() => listener(btnRenderer)));

		if (sprite == null)
		{
			OutfitExporterPlugin.Logger?.LogWarning($"Couldn't load sprite for {name}, it is null");
		}
		else
		{
			btnRenderer.sprite = sprite;
		}

		var btnAspPos = button.GetComponent<AspectPosition>();
		btnAspPos.Alignment = AspectPosition.EdgeAlignments.RightTop;
		btnAspPos.DistanceFromEdge = new Vector3(x_position, y_position, -5f);
		btnAspPos.AdjustPosition();
	}

	private static string GetSerializedOutfit()
	{
		var outfit = DataManager.Player.Customization;
		var ser = new SerializedOutfit(outfit);
		return JsonSerializer.Serialize(ser, OutfitExporterPlugin.JsonSerializerOptions);
	}

	private static bool SetSerializedOutfit(string data, PoolablePlayer previewArea)
	{
		SerializedOutfit? ser = null;
		try
		{
			ser = JsonSerializer.Deserialize<SerializedOutfit>(data);
		}
		catch (JsonException e)
		{
			OutfitExporterPlugin.Logger?.LogError($"Couldn't load outfit: {e.Message}");
		}

		// Check if serialization failed
		if (ser != null)
		{
			// Set the current AU outfit
			ser.SetData(DataManager.Player.Customization);
			previewArea.UpdateFromDataManager(PlayerMaterial.MaskType.None);
		}

		return ser != null;
	}
}
