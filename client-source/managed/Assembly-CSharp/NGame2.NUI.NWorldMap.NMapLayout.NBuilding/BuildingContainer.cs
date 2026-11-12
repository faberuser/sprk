using System.Collections.Generic;
using NVespa;

namespace NGame2.NUI.NWorldMap.NMapLayout.NBuilding;

public class BuildingContainer : SingletonAuto<BuildingContainer>
{
	private Dictionary<string, IBuilding> _dic = new Dictionary<string, IBuilding>();

	public BuildingContainer()
	{
		Register(new GuildBuildingOneModel());
		Register(new GuildBuildingPartialModel());
	}

	private void Register<T>(T ibuilding) where T : IBuilding, new()
	{
		string name = ibuilding.GetType().Name;
		_dic.Remove(name);
		_dic.Add(name, ibuilding);
	}

	public IBuilding GetBuilding(string key)
	{
		if (String_Extension_Method.IsNullOrEmpty(key))
		{
			return null;
		}
		_dic.TryGetValue(key, out var value);
		if (value == null)
		{
			return null;
		}
		return value;
	}
}
