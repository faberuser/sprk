using System;
using System.Collections.Generic;
using NGame2.NUI.NManager;
using NGame2.NUI.NWorldMap;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NGuild;

public class GuildBuildingManager
{
	private Dictionary<int, Action<int>> _dic_buildingApply = new Dictionary<int, Action<int>>();

	private MyGuildInfo _guildInfo;

	public void RegistApplyAction(EGuildBuildingType buildingType, Action<int> applyAction)
	{
		_dic_buildingApply.Remove((int)buildingType);
		_dic_buildingApply.Add((int)buildingType, applyAction);
	}

	public Action<int> GetApplyAction(int buildingIndex)
	{
		_dic_buildingApply.TryGetValue(buildingIndex, out var value);
		return value;
	}

	public void SetMyGuildInfo(MyGuildInfo guildInfo)
	{
		_guildInfo = guildInfo;
	}

	public int GetGuildBuildingLevel(int index)
	{
		if (_guildInfo == null)
		{
			return 1;
		}
		return _guildInfo.GetGuildBuildingInfo(index)?.BuildingLevel ?? 1;
	}

	public bool IsReservedUpgradeBuilding(int index)
	{
		if (_guildInfo == null)
		{
			return false;
		}
		return _guildInfo.GetGuildBuildingInfo(index)?.ReservedUpdate ?? false;
	}

	public bool GetGuildBuildingReservedUpgrade(int index)
	{
		if (_guildInfo == null)
		{
			return false;
		}
		return _guildInfo.GetGuildBuildingInfo(index)?.ReservedUpdate ?? false;
	}

	public void CompletedUpgradeAnim(int buildingIndex)
	{
		if (_guildInfo != null)
		{
			GuildBuildingInfo guildBuildingInfo = _guildInfo.GetGuildBuildingInfo(buildingIndex);
			if (guildBuildingInfo != null)
			{
				guildBuildingInfo.ReservedUpdate = false;
			}
		}
	}

	public void GuildBuildingUpgrade(int buildingIndex)
	{
		if (SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (instance != null)
			{
				instance.PropManager.ChangedPropPrefabList();
			}
		}
	}
}
