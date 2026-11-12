using System;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class EventDungeonReturnState : BaseEntryMenuReturnState
{
	protected EnterDungeonData _enterDungeon;

	protected EventDungeonGroupData _enterGroup;

	protected int[] _eventDungeonGroupIndices;

	private Action<Dictionary<SaveKey, object>> _onComplete;

	public EventDungeonReturnState()
	{
	}

	public EventDungeonReturnState(EnterDungeonData enterDungeon)
	{
		_enterDungeon = enterDungeon;
	}

	public EventDungeonReturnState(EnterDungeonData enterDungeon, EventDungeonGroupData enterGroup, int[] eventDungeonGroupIndices)
	{
		_enterDungeon = enterDungeon;
		_enterGroup = enterGroup;
		_eventDungeonGroupIndices = eventDungeonGroupIndices;
	}

	public EventDungeonReturnState(EnterDungeonData enterDungeon, EventDungeonGroupData enterGroup, int[] eventDungeonGroupIndices, List<SaveKey> saveKeys, Action<Dictionary<SaveKey, object>> onComplete)
	{
		_enterDungeon = enterDungeon;
		_enterGroup = enterGroup;
		_eventDungeonGroupIndices = eventDungeonGroupIndices;
		if (onComplete == null || __Extension_Method_ICollection__.IsNullOrEmpty(saveKeys))
		{
			return;
		}
		_saveDataDic = new Dictionary<SaveKey, object>(saveKeys.Count);
		foreach (SaveKey saveKey in saveKeys)
		{
			object value = null;
			if (_saveDataDic.TryGetValue(saveKey, out value))
			{
				VespaDebug.LogError("saveDataDic already have key - " + saveKey);
			}
			else
			{
				_saveDataDic.Add(saveKey, null);
			}
		}
		_onComplete = onComplete;
	}

	public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if (_enterGroup == null)
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonList(_enterDungeon.Chapter);
		}
		else if (_enterGroup.GroupType == EDungeonGroupType.Secret)
		{
			if (!SceneAutoBehaviour<EventDungeonManagement>.instance.IsOpened<EventDungeonSelect>())
			{
				SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonSelect(_enterDungeon.Chapter, _eventDungeonGroupIndices);
			}
		}
		else
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonList(_enterGroup, delegate
			{
				if (!SceneAutoBehaviour<EventDungeonManagement>.instance.IsOpened<EventDungeonSelect>())
				{
					SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonSelect(_enterDungeon.Chapter, _eventDungeonGroupIndices);
				}
			});
		}
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenPartySetting) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.ReturnEventDungeonAndOpenBattlePartySetting(_enterDungeon, _enterGroup);
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenInventory) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenCashShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenEventShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString());
		}
		if (_onComplete != null && !__Extension_Method_ICollection__.IsNullOrEmpty(_saveDataDic))
		{
			Vespa_System_Action_Extension_Method.fire(_onComplete, _saveDataDic);
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
