using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class EventDungeonList : BaseWindow, LobbyTopmostMenu.ISubStaminaObserver, LobbyTopmostMenu.ICustomStaminaObserver, EventDungeonComponent.Listener, WorldMapManagement.IAvatarHider
{
	public interface Listener
	{
		void OnClickEnterEventDungeon(EventDungeonList caller, EventDungeonData dungeonData);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventDungeon/EventDungeonList";

	public UIBaseGrid DungeonListGrid;

	private Listener _listener;

	private List<EventDungeonComponent> _componentList = new List<EventDungeonComponent>();

	private Action _closedCallback;

	private float _lastRefreshTime;

	private int _difficulty;

	private int lastChapter;

	private int lastDungeon;

	private StaminaType _mainStaminaType;

	private StaminaType _subStaminaType;

	private int _maxEnterCount;

	private int _completedCount;

	public UISpriteSelector[] DecorationSpriteSelectors;

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

	public void Open(Listener listener, int chapterIndex, int[] dungeonIndices = null)
	{
		_listener = listener;
		SceneAutoBehaviour<WorldMapManagement>.instance.AddAvatarHider(this);
		SceneAutoBehaviour<EntryMenuManagement>.instance.SetSpeech(string.Empty);
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null)
		{
			_mainStaminaType = data.StaminaType;
			_subStaminaType = data.SubStaminaType;
			_maxEnterCount = data.DailyEnterMaxCount;
			UserManager.UserStatType o_value = UserManager.UserStatType.None;
			UserManager.UserStatType o_value2 = UserManager.UserStatType.None;
			string spriteName = string.Empty;
			string spriteNameSub = string.Empty;
			if (data.UserStatType != null)
			{
				if (EnumTools.TryParse<UserManager.UserStatType>(data.UserStatType[0].ToString(), out o_value))
				{
					UserStat = o_value;
				}
				if (data.UserStatType.Length > 1)
				{
					EnumTools.TryParse<UserManager.UserStatType>(data.UserStatType[1].ToString(), out o_value2);
				}
			}
			if (data.UserStatTypeIconSprite != null)
			{
				if (data.UserStatTypeIconSprite.Length != 0)
				{
					spriteName = data.UserStatTypeIconSprite[0];
				}
				if (data.UserStatTypeIconSprite.Length > 1)
				{
					spriteNameSub = data.UserStatTypeIconSprite[1];
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
			title.Key = data.Name;
		}
		Open(delegate
		{
			Init(chapterIndex, dungeonIndices);
		});
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_componentList != null)
		{
			_componentList.ForEach(delegate(EventDungeonComponent e)
			{
				e.ChainUpdate();
			});
		}
	}

	public override void Reset()
	{
		base.Reset();
		_closedCallback = null;
		ClearDungeonList();
	}

	public override void Close()
	{
		UserStat = UserManager.UserStatType.None;
		Vespa_System_Action_Extension_Method.fire(_closedCallback);
		base.Close();
	}

	public void SetClosedCallback(Action closedCallback)
	{
		_closedCallback = closedCallback;
	}

	private void ClearDungeonList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void Init(int chapterIndex, int[] dungeonIndices)
	{
		ClearDungeonList();
		EventDungeonComponent eventDungeonComponent = null;
		int num = 0;
		List<EventDungeonData> list = null;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(dungeonIndices))
		{
			list = GameRoot.Instance.EventDungeonDataContainer.GetDatasByChapterIndex(chapterIndex).ToList();
		}
		else
		{
			list = new List<EventDungeonData>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(dungeonIndices); i++)
			{
				EventDungeonData data = GameRoot.Instance.EventDungeonDataContainer.GetData(chapterIndex, dungeonIndices[i]);
				if (data != null)
				{
					list.Add(data);
				}
			}
		}
		_completedCount = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetDailyCompletedCountByChapter(chapterIndex, dungeonIndices);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list); j++)
		{
			EventDungeonData eventDungeonData = list[j];
			if (eventDungeonData == null)
			{
				continue;
			}
			bool flag = CheckedOpenCondtion(eventDungeonData);
			if (flag || eventDungeonData.CloseType != EEventDungeonCloseType.Hide)
			{
				EventDungeonComponent eventDungeonComponent2 = DungeonListGrid.AddGridItem(EventDungeonComponent.PrefabAuto);
				eventDungeonComponent2.Open(this, eventDungeonData);
				_componentList.Add(eventDungeonComponent2);
				if (flag && num < eventDungeonData.DungeonIndex)
				{
					num = eventDungeonData.DungeonIndex;
					eventDungeonComponent = eventDungeonComponent2;
				}
			}
		}
		DungeonListGrid.RepositionGrid();
		if ((bool)eventDungeonComponent && num != 1)
		{
			DungeonListGrid.ScrollTo(eventDungeonComponent, 0f, 0f);
		}
		SetDecorationSpriteSelector(list);
	}

	private void SetDecorationSpriteSelector(List<EventDungeonData> eventDungeonList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(eventDungeonList))
		{
			return;
		}
		EventDungeonData eventDungeonData = eventDungeonList.Find((EventDungeonData data) => !String_Extension_Method.IsNullOrEmpty(data.DecorationSelectorName));
		if (eventDungeonData != null)
		{
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(DecorationSpriteSelectors); num++)
			{
				DecorationSpriteSelectors[num].Select(eventDungeonData.DecorationSelectorName);
			}
		}
	}

	void EventDungeonComponent.Listener.OnClickEnter(EventDungeonComponent caller)
	{
		if (_listener != null)
		{
			EventDungeonData eventDungeonData = caller.EventDungeonData;
			if (eventDungeonData != null)
			{
				_listener.OnClickEnterEventDungeon(this, eventDungeonData);
			}
		}
	}

	private bool CheckedOpenCondtion(EventDungeonData eventDungeonData)
	{
		string notOpenText = "";
		return UIHelper.CheckEventDungeonOpenCondition(eventDungeonData, out notOpenText);
	}

	private bool CheckPossibleEnter()
	{
		return _maxEnterCount - _completedCount > 0;
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
