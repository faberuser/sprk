using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildRaidDungeonList : BaseWindow, GuildRaidDungeonComponent.Listener
{
	public interface Listener
	{
		void OnClickGuildRaidDungeonInfo(GuildRaidDungeonList caller, GuildRaidDungeonData dungeonData, CreatureData bossCreatureData);

		void OnClickEnterGuildRaidDungeon(GuildRaidDungeonList caller, NShared.GuildRaidDungeonInfo guildRaidDungeonInfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildRaid/GuildRaidDungeonList";

	public UIBaseGrid DungeonListGrid;

	private Listener _listener;

	private List<GuildRaidDungeonComponent> _componentList = new List<GuildRaidDungeonComponent>();

	public void Open(Listener listener, Action onOpen = null)
	{
		_listener = listener;
		Open(delegate
		{
			Init();
		}, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onOpen);
		});
	}

	public override void Reset()
	{
		base.Reset();
		ClearGuildRaidDungeonList();
	}

	private void ClearGuildRaidDungeonList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public void ForceChangeAllClearMode()
	{
		foreach (GuildRaidDungeonComponent component in _componentList)
		{
			component.SetComplete();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
	}

	private bool Init()
	{
		SetGuildRaidDungeonList();
		return true;
	}

	private void SetGuildRaidDungeonList()
	{
		ClearGuildRaidDungeonList();
		Dictionary<int, NShared.GuildRaidDungeonInfo> guildRaidDungeonInfoDic = GlobalAutoBehaviour<GuildManager>.instance.GuildRaidDungeonInfoDic;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(guildRaidDungeonInfoDic))
		{
			return;
		}
		NShared.GuildRaidDungeonInfo guildRaidDungeonInfo = null;
		GuildRaidDungeonComponent guildRaidDungeonComponent = null;
		List<NShared.GuildRaidDungeonInfo> list = guildRaidDungeonInfoDic.Values.ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		list.Sort(SortCompare);
		for (int i = 0; i < list.Count(); i++)
		{
			guildRaidDungeonInfo = list[i];
			if (guildRaidDungeonInfo != null && (guildRaidDungeonInfo.IsOngoing == 1 || ((guildRaidDungeonInfo.IsBonusDungeon != 1 || !guildRaidDungeonInfo.IsDead()) && guildRaidDungeonInfo.IsDead())))
			{
				guildRaidDungeonComponent = DungeonListGrid.AddGridItem(GuildRaidDungeonComponent.PrefabAuto);
				if (!(guildRaidDungeonComponent == null))
				{
					guildRaidDungeonComponent.Open(new GuildRaidDungeonComponent.GuildRaidDungeonCompInfo
					{
						Listener = this,
						GuildRaidDungeonInfo = guildRaidDungeonInfo
					}, openingEffect: true, clearEffect: true);
					_componentList.Add(guildRaidDungeonComponent);
				}
			}
		}
		UIHelper.RepositionGrid(DungeonListGrid);
	}

	private int SortCompare(NShared.GuildRaidDungeonInfo info, NShared.GuildRaidDungeonInfo compareInfo)
	{
		if (info == null)
		{
			return 0;
		}
		if (compareInfo == null)
		{
			return 0;
		}
		int num = compareInfo.IsBonusDungeon.CompareTo(info.IsBonusDungeon);
		if (num != 0)
		{
			return num;
		}
		num = compareInfo.IsDummyDungeon.CompareTo(info.IsDummyDungeon);
		if (num != 0)
		{
			return num;
		}
		return info.ChapterIndex.CompareTo(compareInfo.ChapterIndex);
	}

	void GuildRaidDungeonComponent.Listener.OnClickEnter(NShared.GuildRaidDungeonInfo info)
	{
		if (_listener != null && info != null)
		{
			_listener.OnClickEnterGuildRaidDungeon(this, info);
		}
	}

	void GuildRaidDungeonComponent.Listener.OnClickViewInfo(GuildRaidDungeonComponent caller)
	{
		if (_listener != null)
		{
			GuildRaidDungeonData guildRaidDungeonData = caller.GuildRaidDungeonData;
			if (guildRaidDungeonData != null)
			{
				_listener.OnClickGuildRaidDungeonInfo(this, guildRaidDungeonData, caller.BossCreatureData);
			}
		}
	}
}
