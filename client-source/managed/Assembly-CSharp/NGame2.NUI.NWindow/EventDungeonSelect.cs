using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class EventDungeonSelect : BaseWindow, LobbyTopmostMenu.ISubStaminaObserver, LobbyTopmostMenu.ICustomStaminaObserver, EventDungeonGroupComponent.Listener, WorldMapManagement.IAvatarHider
{
	public interface Listener
	{
		void OnClickEnterEventDungeonGroup(EventDungeonSelect caller, EventDungeonGroupData dungeonGroupData);

		void OnClickEnterBattleDungeonInfo(EventDungeonSelect caller, EventDungeonGroupData dungeonGroupData);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventDungeon/EventDungeonSelect";

	public UIBaseGrid DungeonListGrid;

	private Listener _listener;

	private int _chapterIndex;

	private List<EventDungeonGroupData> _eventDungeonGroupList;

	private List<EventDungeonGroupComponent> _componentList = new List<EventDungeonGroupComponent>();

	private StaminaType _mainStaminaType;

	private StaminaType _subStaminaType;

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType => _mainStaminaType;

	StaminaType LobbyTopmostMenu.ISubStaminaObserver.SubStaminaType
	{
		get
		{
			if (_subStaminaType == StaminaType.None)
			{
				return StaminaType.Empty;
			}
			return _subStaminaType;
		}
	}

	public void Open(Listener listener, int chapterIndex, params int[] dungeonGroupIndices)
	{
		base.Open();
		SceneAutoBehaviour<WorldMapManagement>.instance.AddAvatarHider(this);
		SceneAutoBehaviour<EntryMenuManagement>.instance.SetSpeech(string.Empty);
		_listener = listener;
		_chapterIndex = chapterIndex;
		_eventDungeonGroupList = GetEventDungeonGroupDatas(dungeonGroupIndices);
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		SetEventAsset(data);
		Init();
	}

	private void SetEventAsset(CampaignChapterData chapterData)
	{
		if (chapterData == null)
		{
			return;
		}
		_mainStaminaType = chapterData.StaminaType;
		_subStaminaType = chapterData.SubStaminaType;
		UserManager.UserStatType o_value = UserManager.UserStatType.None;
		UserManager.UserStatType o_value2 = UserManager.UserStatType.None;
		string spriteName = string.Empty;
		string spriteNameSub = string.Empty;
		if (chapterData.UserStatType != null)
		{
			if (EnumTools.TryParse<UserManager.UserStatType>(chapterData.UserStatType[0].ToString(), out o_value))
			{
				UserStat = o_value;
			}
			if (chapterData.UserStatType.Length > 1)
			{
				EnumTools.TryParse<UserManager.UserStatType>(chapterData.UserStatType[1].ToString(), out o_value2);
			}
		}
		if (chapterData.UserStatTypeIconSprite != null)
		{
			if (chapterData.UserStatTypeIconSprite.Length != 0)
			{
				spriteName = chapterData.UserStatTypeIconSprite[0];
			}
			if (chapterData.UserStatTypeIconSprite.Length > 1)
			{
				spriteNameSub = chapterData.UserStatTypeIconSprite[1];
			}
		}
		LobbyTopmostMenu.EventDungeonAssetInfo eventDungeonAssetInfo = new LobbyTopmostMenu.EventDungeonAssetInfo
		{
			StatType = o_value,
			StatTypeSub = o_value2,
			SpriteName = spriteName,
			SpriteNameSub = spriteNameSub
		};
		SetEventDungeonAssetInfo(eventDungeonAssetInfo);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_componentList != null)
		{
			_componentList.ForEach(delegate(EventDungeonGroupComponent e)
			{
				e.ChainUpdate();
			});
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearDungeonList();
		ClearDungeonGroupList();
	}

	public override void Close()
	{
		UserStat = UserManager.UserStatType.None;
		base.Close();
	}

	private void ClearDungeonList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void ClearDungeonGroupList()
	{
		if (_eventDungeonGroupList != null)
		{
			_eventDungeonGroupList.Clear();
		}
		_eventDungeonGroupList = null;
	}

	private void Init()
	{
		CreateDungeonGroup();
	}

	private void CreateDungeonGroup()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_eventDungeonGroupList); i++)
		{
			EventDungeonGroupData eventDungeonGroupData = _eventDungeonGroupList[i];
			if (eventDungeonGroupData != null)
			{
				EventDungeonGroupComponent eventDungeonGroupComponent = DungeonListGrid.AddGridItem(EventDungeonGroupComponent.PrefabAuto);
				eventDungeonGroupComponent.Open(this, eventDungeonGroupData);
				_componentList.Add(eventDungeonGroupComponent);
			}
		}
		DungeonListGrid.RepositionGrid();
	}

	private List<EventDungeonGroupData> GetEventDungeonGroupDatas(params int[] dungeonGroupIndices)
	{
		List<EventDungeonGroupData> list = new List<EventDungeonGroupData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(dungeonGroupIndices) && dungeonGroupIndices[i] != 0; i++)
		{
			EventDungeonGroupData data = GameRoot.Instance.EventDungeonGroupDataContainer.GetData(_chapterIndex, dungeonGroupIndices[i]);
			if (data != null)
			{
				list.Add(data);
			}
		}
		return list;
	}

	void EventDungeonGroupComponent.Listener.OnClickEnter(EventDungeonGroupComponent caller)
	{
		if (_listener == null)
		{
			return;
		}
		EventDungeonGroupData eventDungeonGroupData = caller.EventDungeonGroupData;
		if (eventDungeonGroupData != null)
		{
			if (eventDungeonGroupData.GroupType == EDungeonGroupType.Secret)
			{
				_listener.OnClickEnterBattleDungeonInfo(this, eventDungeonGroupData);
			}
			else
			{
				_listener.OnClickEnterEventDungeonGroup(this, eventDungeonGroupData);
			}
		}
	}

	bool WorldMapManagement.IAvatarHider.IsOpen()
	{
		return base.IsOpen();
	}

	WorldMapManagement.AvatarHideFlag WorldMapManagement.IAvatarHider.GetAvatarHideFlag()
	{
		return WorldMapManagement.AvatarHideFlag.HideAll;
	}
}
