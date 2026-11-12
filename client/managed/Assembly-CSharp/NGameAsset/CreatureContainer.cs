using System.Collections.Generic;
using NGameAsset.NCreature;
using NVespa;

namespace NGameAsset;

public class CreatureContainer : SingletonAuto<CreatureContainer>
{
	private HashSet<CreatureLoad> _set = new HashSet<CreatureLoad>();

	private List<CreatureLoad> _removeList = new List<CreatureLoad>();

	public ICreatureLoad CreateAsync(CreatureStock stock)
	{
		CreatureLoad creatureLoad = new CreatureLoad();
		creatureLoad.Open(stock);
		_set.Add(creatureLoad);
		return creatureLoad;
	}

	public static void ReleaseCreature(ICreatureLoad creatureAsync)
	{
		if (SingletonAuto<CreatureContainer>.isValidInstance)
		{
			SingletonAuto<CreatureContainer>.instance.ReleaseCreatureInternal(creatureAsync);
		}
	}

	private void ReleaseCreatureInternal(ICreatureLoad creatureAsync)
	{
		if (creatureAsync != null)
		{
			if (!(creatureAsync is CreatureLoad creatureLoad))
			{
				creatureAsync.Value.Close();
				return;
			}
			SingletonAuto<CreatureContainer>.instance._set.Remove(creatureLoad);
			creatureLoad.Close();
		}
	}

	public void OnSceneLoading()
	{
		foreach (CreatureLoad item in _set)
		{
			if (!item.Stock.DontDestroyOnLoad)
			{
				_removeList.Add(item);
			}
		}
		foreach (CreatureLoad remove in _removeList)
		{
			remove.Close();
			_set.Remove(remove);
		}
	}
}
