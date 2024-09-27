// <copyright file="OutfitExporterPlugin.cs" company="miniduikboot">
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

using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Text.Json;
using AmongUs.Data;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using Reactor;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class OutfitExporterPlugin : BasePlugin
{
	private static Sprite copyButtonSprite;
	private static Sprite pasteButtonSprite;

	private static JsonSerializerOptions jsonSerializerOptions = new () { IncludeFields = true };

	public static BepInEx.Logging.ManualLogSource? Logger { get; private set; }

	public Harmony Harmony { get; } = new (Id);

	public override void Load()
	{
		Logger = this.Log;
		this.Harmony.PatchAll();

		// Load assets
		var bundle = AssetBundleManager.Load("default");
		copyButtonSprite = bundle.LoadAsset<Sprite>("CopyButton")?.DontUnload();
		pasteButtonSprite = bundle.LoadAsset<Sprite>("PasteButton")?.DontUnload();
		bundle.Unload(false);
	}

	public static string GetSerializedOutfit()
	{
		var outfit = DataManager.Player.Customization;
		var ser = new SerializedOutfit(outfit);
		return JsonSerializer.Serialize(ser, jsonSerializerOptions);
	}

	public static bool SetSerializedOutfit(string data, PoolablePlayer previewArea)
	{
		SerializedOutfit? ser = null;
		try
		{
			ser = JsonSerializer.Deserialize<SerializedOutfit>(data);
		}
		catch (JsonException e)
		{
			Logger.LogError($"Couldn't load outfit: {e.Message}");
		}

		// Check if serialization failed
		var ret = ser != null;
		if (ret)
		{
			// Set the current AU outfit
			ser.SetData(DataManager.Player.Customization);
			previewArea.UpdateFromDataManager(PlayerMaterial.MaskType.None);
		}

		return ret;
	}

	[HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Start))]
	public static class AddButtonPatch
	{
		public static void Postfix(PlayerCustomizationMenu __instance)
		{
			var template = GameObject.Find("CloseButton");
			Logger?.LogInfo($"Got {template} | {template == null}");
			var copyButton = GameObject.Instantiate(template, template.transform.parent);
			copyButton.name = $"[{Id}] Copy Button";
			copyButton.transform.localPosition += Vector3.right * 9.2f;
			var copyButtonPassive = copyButton.GetComponent<PassiveButton>();
			var copyButtonRenderer = copyButton.GetComponent<SpriteRenderer>();
			copyButtonRenderer.sprite = copyButtonSprite;
			copyButtonPassive.OnClick.RemoveAllListeners();
			copyButtonPassive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
			copyButtonPassive.OnClick.AddListener((Action)(() =>
			{
				// Set the clipboard to the currently equipped outfit
				GUIUtility.systemCopyBuffer = GetSerializedOutfit();
				copyButtonRenderer.color = Color.green;
				__instance.StartCoroutine(Effects.Lerp(1f, new System.Action<float>((p) =>
				{
					if (p > 0.95)
					{
						copyButtonRenderer.color = Color.white;
					}
				})));
			}));

			var pasteButton = GameObject.Instantiate(template, template.transform.parent);
			pasteButton.name = $"[{Id}] Paste Button";
			pasteButton.transform.localPosition += Vector3.right * 10.0f;
			var pasteButtonPassive = pasteButton.GetComponent<PassiveButton>();
			var pasteButtonRenderer = pasteButton.GetComponent<SpriteRenderer>();
			pasteButtonRenderer.sprite = pasteButtonSprite;
			pasteButtonPassive.OnClick.RemoveAllListeners();
			pasteButtonPassive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
			pasteButtonPassive.OnClick.AddListener((Action)(() =>
			{
				pasteButtonRenderer.color = Color.yellow;
				bool success = SetSerializedOutfit(GUIUtility.systemCopyBuffer, __instance.PreviewArea);
				pasteButtonRenderer.color = success ? Color.green : Color.red;
				__instance.StartCoroutine(Effects.Lerp(1f, new System.Action<float>((p) =>
				{
					if (p > 0.95)
					{
						pasteButtonRenderer.color = Color.white;
					}
				})));
			}));
		}
	}
}
