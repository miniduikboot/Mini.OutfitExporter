// <copyright file="SerializedOutfit.cs" company="miniduikboot">
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

using System.Text.Json.Serialization;
using AmongUs.Data.Player;

public class SerializedOutfit
{
	public SerializedOutfit(PlayerCustomizationData data)
	{
		this.Color = data.Color;
		this.Hat = data.Hat;
		this.Pet = data.Pet;
		this.Skin = data.Skin;
		this.Visor = data.Visor;
	}

	public SerializedOutfit()
	{
		// Empty constructor for JSON deserialization
	}

	public byte Color { get; set; } = 0;

	public string Hat { get; set; } = string.Empty;

	public string Pet { get; set; } = string.Empty;

	public string Skin { get; set; } = string.Empty;

	public string Visor { get; set; } = string.Empty;

	public void SetData(PlayerCustomizationData data)
	{
		data.Color = this.Color;
		data.Hat = this.Hat;
		data.Pet = this.Pet;
		data.Skin = this.Skin;
		data.Visor = this.Visor;
	}
}
