using System.Collections.Generic;
using NVespa;

namespace NGameAsset.NBattleFieldMap2;

public class Factory : SingletonAuto<Factory>
{
	private HashSet<BattleFieldMapLoad> _set = new HashSet<BattleFieldMapLoad>();

	public Factory()
	{
		SingletonUpdater<AssetUpdateManager>.CreateInstance();
	}

	public IAsyncLoad<IBattleFieldMap2> CreateAsync(BattleFieldMapStock stock)
	{
		BattleFieldMapLoad battleFieldMapLoad = new BattleFieldMapLoad();
		battleFieldMapLoad.Open(stock);
		_set.Add(battleFieldMapLoad);
		return battleFieldMapLoad;
	}

	public static void ReleaseMap(IAsyncLoad<IBattleFieldMap2> asyncLoad)
	{
		if (SingletonAuto<Factory>.isValidInstance && asyncLoad != null)
		{
			if (!(asyncLoad is BattleFieldMapLoad battleFieldMapLoad))
			{
				VespaDebug.LogError("Invalid BattleFieldMap Async: " + asyncLoad.GetType().Name);
				asyncLoad.Value.Close();
			}
			else
			{
				SingletonAuto<Factory>.instance._set.Remove(battleFieldMapLoad);
				battleFieldMapLoad.Close();
			}
		}
	}
}
