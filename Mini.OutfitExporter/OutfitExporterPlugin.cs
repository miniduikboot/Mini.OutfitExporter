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

using System.Text.Json;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class OutfitExporterPlugin : BasePlugin
{
	internal static readonly JsonSerializerOptions JsonSerializerOptions = new() { IncludeFields = true };

	public Harmony Harmony { get; } = new(Id);

	internal static Sprite? CopyButtonSprite { get; private set; }

	internal static Sprite? PasteButtonSprite { get; private set; }

	internal static BepInEx.Logging.ManualLogSource? Logger { get; private set; }

	public override void Load()
	{
		Logger = this.Log;
		this.Harmony.PatchAll();

		// Load assets
		Logger.LogInfo("Loading assets");
		var bundle = AssetBundleManager.Load("default");
		if (bundle == null)
		{
			Logger.LogError($"Could not load asset bundle");
		}
		else
		{
			CopyButtonSprite = bundle.LoadAsset<Sprite>("CopyButton")?.DontUnload();
			PasteButtonSprite = bundle.LoadAsset<Sprite>("PasteButton")?.DontUnload();
			bundle.Unload(false);
			Logger.LogInfo($"Loaded {CopyButtonSprite == null} {PasteButtonSprite == null}");
		}
	}
}
