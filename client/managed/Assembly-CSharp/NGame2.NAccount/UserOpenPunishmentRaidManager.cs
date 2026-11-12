using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserOpenPunishmentRaidManager : NShared.UserOpenPunishmentRaidManager
{
	private Dictionary<int, List<PunishmentRaidInfo>> _punishmentRaidInfoDic = new Dictionary<int, List<PunishmentRaidInfo>>();

	private PunishmentRaidInfo[] _infos;

	public PunishmentRaidInfo[] GetPunishmentRaidInfos()
	{
		return _infos;
	}

	public void Set(PunishmentRaidInfo[] infos)
	{
		_infos = infos;
		if (infos == null)
		{
			return;
		}
		Dictionary<int, List<PunishmentRaidInfo>> dictionary = new Dictionary<int, List<PunishmentRaidInfo>>();
		foreach (PunishmentRaidInfo punishmentRaidInfo in infos)
		{
			PunishmentRaidData data = GameRoot.Instance.PunishmentRaidDataContainer.GetData(punishmentRaidInfo.RaidIndex, punishmentRaidInfo.RaidLevel);
			if (data != null)
			{
				int groupIndex = data.GroupIndex;
				if (!dictionary.ContainsKey(groupIndex))
				{
					dictionary.Add(groupIndex, new List<PunishmentRaidInfo>());
				}
				dictionary[groupIndex].Add(punishmentRaidInfo);
			}
		}
		_punishmentRaidInfoDic.Clear();
		foreach (KeyValuePair<int, List<PunishmentRaidInfo>> item in dictionary)
		{
			_punishmentRaidInfoDic[item.Key] = item.Value;
		}
	}

	public PunishmentRaidInfo GetPunishmentRaidInfo(int groupIndex, int index, int level)
	{
		if (!_punishmentRaidInfoDic.ContainsKey(groupIndex))
		{
			return null;
		}
		foreach (PunishmentRaidInfo item in _punishmentRaidInfoDic[groupIndex])
		{
			if (item.RaidIndex == index && item.RaidLevel == level)
			{
				return item;
			}
		}
		return null;
	}

	public List<PunishmentRaidInfo> GetPunishmentRaidInfoList(int groupIndex)
	{
		if (!_punishmentRaidInfoDic.ContainsKey(groupIndex))
		{
			return null;
		}
		return _punishmentRaidInfoDic[groupIndex];
	}

	public void Apply(OpenPunishmentRaidInfo openPunishmentRaidInfo)
	{
		if (openPunishmentRaidInfo != null)
		{
			Add(openPunishmentRaidInfo);
		}
	}

	public void Apply(IList<OpenPunishmentRaidInfo> info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
		{
			Apply(info[i]);
		}
	}

	public OpenPunishmentRaidInfo Get(int groupIndex, EPunishmentDungeonType punishmentDungeonType)
	{
		return Get(new OpenPunishmentRaidKey(groupIndex, (int)punishmentDungeonType));
	}

	public bool IsOpenPunishmentRaid(int groupIndex, EPunishmentDungeonType punishmentDungeonType)
	{
		OpenPunishmentRaidKey key = new OpenPunishmentRaidKey(groupIndex, (int)punishmentDungeonType);
		OpenPunishmentRaidInfo openPunishmentRaidInfo = Get(key);
		if (openPunishmentRaidInfo == null)
		{
			return false;
		}
		return openPunishmentRaidInfo.IsOpen == 1;
	}
}
