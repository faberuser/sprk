using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NGuild;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildSuppressDungeonList : BaseWindow, GuildSuppressDungeonComponent.Listener
{
	public interface Listener
	{
		void OnClickEnter(int raidIndex, int level);

		void OnClickInfo(GuildSuppressDungeonData guildSuppressDungeonData, int level);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressDungeonList";

	private Listener _listener;

	public UIBaseGrid DungeonListGrid;

	private List<GuildSuppressDungeonComponent> _componentList = new List<GuildSuppressDungeonComponent>();

	private float _lastRefreshTime;

	private int _raidIndex;

	private GuildSuppressChapterData _guildSuppressChapterData;

	private GuildSuppressSessionInfo _guildSuppressSessionInfo;

	public void Open(Listener listener, int raidIndex, bool dungeonCleared, int clearedSuppressDungeonIndex, int clearedLevel, Action onOpen = null)
	{
		_listener = listener;
		_raidIndex = raidIndex;
		_guildSuppressChapterData = GameRoot.Instance.GuildSuppressChapterDataContainer.GetData(_raidIndex);
		_lastRefreshTime = 0f;
		if (_guildSuppressChapterData != null)
		{
			base.Open(delegate
			{
				Init(dungeonCleared, clearedSuppressDungeonIndex, clearedLevel);
				Vespa_System_Action_Extension_Method.fire(onOpen);
			});
		}
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

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _lastRefreshTime > 10f)
		{
			_lastRefreshTime = Time.time;
			Refresh();
		}
	}

	public void Init(bool dungeonCleared, int clearedSuppressDungeonIndex, int clearedLevel)
	{
		SetupDungeonComponentList(dungeonCleared, clearedSuppressDungeonIndex);
		Action postAction = delegate
		{
			_componentList.ForEach(delegate(GuildSuppressDungeonComponent e)
			{
				e.StartOpenedEffect();
				e.StartFadeEffect();
			});
		};
		if (dungeonCleared)
		{
			OpenDungeonClearedPopup(clearedSuppressDungeonIndex, clearedLevel, postAction);
		}
	}

	public void Refresh()
	{
		foreach (GuildSuppressDungeonComponent component in _componentList)
		{
			component.Refresh();
		}
		_lastRefreshTime = Time.time;
	}

	private void SetupDungeonComponentList(bool dungeonCleared, int clearedDungeonIndex)
	{
		ClearGuildRaidDungeonList();
		GuildSuppressHelper.CreateDungeonList(delegate(int raidIndex, int level, int suppressDungeonIndex)
		{
			GuildSuppressDungeonData data = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(suppressDungeonIndex);
			if (data != null)
			{
				GuildSuppressPlayInfo guildSuppressPlayInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressPlayInfo(raidIndex, level);
				if (guildSuppressPlayInfo != null)
				{
					bool clearEffect = false;
					bool flag = false;
					if (dungeonCleared && suppressDungeonIndex == clearedDungeonIndex)
					{
						clearEffect = true;
					}
					if (guildSuppressPlayInfo.MonsterHp0 < 0)
					{
						flag = true;
					}
					GuildSuppressDungeonComponent guildSuppressDungeonComponent = DungeonListGrid.AddGridItem(GuildSuppressDungeonComponent.PrefabAuto);
					guildSuppressDungeonComponent.Open(this, level, data, guildSuppressPlayInfo, clearEffect);
					if (flag)
					{
						guildSuppressDungeonComponent.SetComplete();
					}
					_componentList.Add(guildSuppressDungeonComponent);
				}
			}
		});
		DungeonListGrid.RepositionGrid();
	}

	private void OpenDungeonClearedPopup(int clearedSuppressDungeonIndex, int clearedLevel, Action postAction)
	{
		GuildRaidDungeonClearedPopup.OpenGuildSuppressClearedPopup(_guildSuppressChapterData.RaidIndex, clearedSuppressDungeonIndex, clearedLevel, postAction);
	}

	void GuildSuppressDungeonComponent.Listener.OnClickEnter(GuildSuppressDungeonComponent caller)
	{
		GuildSuppressPlayInfo guildSuppressPlayInfo = caller.GuildSuppressPlayInfo;
		if (guildSuppressPlayInfo != null && guildSuppressPlayInfo.SessionIndex != 0 && caller.GuildSuppressDungeonData != null && _listener != null)
		{
			_listener.OnClickEnter(guildSuppressPlayInfo.RaidIndex, guildSuppressPlayInfo.Level);
		}
	}

	void GuildSuppressDungeonComponent.Listener.OnClickViewInfo(GuildSuppressDungeonComponent caller)
	{
		if (_listener != null)
		{
			_listener.OnClickInfo(caller.GuildSuppressDungeonData, caller.Level);
		}
	}

	public void Tutorial_InitUi(int raidIndex, int raidLevel, int[] BanHeroIndex)
	{
		ClearGuildRaidDungeonList();
		foreach (int raidLevel2 in GameRoot.Instance.RaidDataContainer.GetRaidLevels(raidIndex))
		{
			GuildSuppressDungeonData data = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(raidLevel2);
			if (data != null)
			{
				bool clearEffect = false;
				GuildSuppressDungeonComponent guildSuppressDungeonComponent = DungeonListGrid.AddGridItem(GuildSuppressDungeonComponent.PrefabAuto);
				GuildSuppressPlayInfo guildSuppressPlayInfo = new GuildSuppressPlayInfo(raidIndex, raidLevel2);
				guildSuppressDungeonComponent.Open(this, raidLevel2, data, guildSuppressPlayInfo, clearEffect);
				guildSuppressDungeonComponent.Tutorial_BanInfo(BanHeroIndex);
				_componentList.Add(guildSuppressDungeonComponent);
			}
		}
		DungeonListGrid.RepositionGrid();
	}
}
