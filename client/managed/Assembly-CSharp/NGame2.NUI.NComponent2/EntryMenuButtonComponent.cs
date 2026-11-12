using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EntryMenuButtonComponent : LinkFactoryComponent
{
	public static string PrefabPath = "Prefab2/Component/EntryMenu/EntryMenuButtonComponent";

	public static PrefabAuto<EntryMenuButtonComponent> PrefabAuto = new PrefabAuto<EntryMenuButtonComponent>(PrefabPath);

	public UILabel Label_ButtonText;

	public Transform[] EventRoots;

	public UILabel RemainTimeLabel;

	public Transform EffectRoot;

	protected Action _onClick;

	protected float _lastUpdateEventTime;

	protected NPCData NPCData;

	protected NShared.EntryMenu Menu;

	private const string _entryMenuKey = "EntryMenuResetDay_";

	private BattleType _showRemainTimeBattleType;

	private int? _resetDay;

	public Type ActionType { get; private set; }

	public NShared.EntryMenu GetEntryMenuData => Menu;

	public void Open(NPCData npcData, NShared.EntryMenu menu, Type actionType, Action onClickButton)
	{
		if (menu != null && onClickButton != null && npcData != null)
		{
			Menu = menu;
			NPCData = npcData;
			ActionType = actionType;
			_onClick = onClickButton;
			_lastUpdateEventTime = Time.time;
			_showRemainTimeBattleType = BattleType.None;
			_resetDay = null;
			Init();
			SetupEventInfo();
			CheckResetEffect();
		}
	}

	protected virtual void Init()
	{
		string valueString = ArgParser.GetValueString(Menu.EntryMenuButton, 0);
		if (!String_Extension_Method.IsNullOrEmpty(valueString))
		{
			string stringDirect = LocalizationManager.GetStringDirect(valueString);
			UIHelper.SetLabelText(Label_ButtonText, LocalizationManager.GetStringDirect(stringDirect));
		}
	}

	private void CheckResetEffect()
	{
		UIHelper.EnableGameObject(EffectRoot, enable: false);
		if (_showRemainTimeBattleType == BattleType.None || !_resetDay.HasValue)
		{
			return;
		}
		string i_key = "EntryMenuResetDay_" + _showRemainTimeBattleType;
		LocalAccountInt localAccountInt = new LocalAccountInt(0, i_key);
		if (localAccountInt.Value > 0)
		{
			DateTime dateTime = new DateTime(1970, 1, 1).AddSeconds(localAccountInt.Value);
			DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
			if (dateTime.Year <= currentServerLocalTime.Year && dateTime.Month < currentServerLocalTime.Month && _resetDay <= currentServerLocalTime.Day)
			{
				UIHelper.EnableGameObject(EffectRoot, enable: true);
			}
		}
		else
		{
			UIHelper.EnableGameObject(EffectRoot, enable: true);
		}
	}

	private void SetupEventInfo()
	{
		if (Menu == null)
		{
			return;
		}
		List<string[]> list = new List<string[]>();
		if (__Extension_Method_ICollection__.COUNT(Menu.ChildIndices) > 0)
		{
			int[] childIndices = Menu.ChildIndices;
			foreach (int menuIndex in childIndices)
			{
				NShared.EntryMenu entryMenuButton = NPCData.GetEntryMenuButton(menuIndex);
				if (entryMenuButton != null)
				{
					list.Add(entryMenuButton.EntryMenuEvent);
				}
			}
		}
		else
		{
			list.Add(Menu.EntryMenuEvent);
		}
		List<string> list2 = new List<string>();
		foreach (string[] item2 in list)
		{
			int num = 0;
			while (true)
			{
				int num2 = num * 2;
				NpcMenuEventReqType valueEnum = ArgParser.GetValueEnum(item2, num2, NpcMenuEventReqType.None);
				if (valueEnum == NpcMenuEventReqType.None)
				{
					break;
				}
				string valueString = ArgParser.GetValueString(item2, num2 + 1);
				string text = CheckEvent(valueEnum, valueString);
				if (!String_Extension_Method.IsNullOrEmpty(text))
				{
					list2.Add(text);
				}
				num++;
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(EventRoots); j++)
		{
			Transform transform = EventRoots[j];
			string item = ((transform != null) ? transform.name : string.Empty);
			bool flag = list2.Contains(item);
			if (flag != UIHelper.IsEnabledGameObjet(transform))
			{
				UIHelper.EnableGameObject(transform, flag);
				NGame2.NUI.NWindow.EntryMenu instance = SceneAutoBehaviour<EntryMenuManagement>.instance.GetInstance<NGame2.NUI.NWindow.EntryMenu>(openedOnly: true);
				if (instance != null)
				{
					instance.RepositionTableGrid();
				}
			}
		}
	}

	private string CheckEvent(NpcMenuEventReqType reqType, string reqValue)
	{
		string[] values = reqValue?.Split(new char[1] { ';' });
		switch (reqType)
		{
		case NpcMenuEventReqType.Always:
			return ArgParser.GetValueString(values, 0);
		case NpcMenuEventReqType.ShowRemainTimeByMenuCondition:
		{
			EventBonusType valueEnum2 = ArgParser.GetValueEnum(values, 0, EventBonusType.None);
			if (valueEnum2 == EventBonusType.None)
			{
				break;
			}
			string valueString4 = ArgParser.GetValueString(values, 1);
			string valueString5 = ArgParser.GetValueString(values, 2);
			DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum2, valueString4);
			if (effectiveEvent == null)
			{
				break;
			}
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(effectiveEvent.EndTime, out var o_dateTime))
			{
				int num = (int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds;
				if (num <= 0)
				{
					break;
				}
				string empty = string.Empty;
				empty = ((num >= 60) ? UIHelper.GetRemainTimeText(num) : LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_MIN, 0));
				string stringDirect = LocalizationManager.GetStringDirect("IMPROVE_RESETREMAININGDATE", empty);
				UIHelper.SetLabelText(RemainTimeLabel, stringDirect);
			}
			return valueString5;
		}
		case NpcMenuEventReqType.DropBonusEvent:
		{
			EventBonusType valueEnum3 = ArgParser.GetValueEnum(values, 0, EventBonusType.None);
			if (valueEnum3 != EventBonusType.None)
			{
				string valueString6 = ArgParser.GetValueString(values, 1);
				string valueString7 = ArgParser.GetValueString(values, 2);
				if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum3, valueString6) == null)
				{
					return null;
				}
				return valueString7;
			}
			break;
		}
		case NpcMenuEventReqType.ItemCountByCode:
		{
			string valueString2 = ArgParser.GetValueString(values, 0);
			if (!String_Extension_Method.IsNullOrEmpty(valueString2))
			{
				int valueInt3 = ArgParser.GetValueInt(values, 1);
				string valueString3 = ArgParser.GetValueString(values, 2);
				if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(valueString2) < valueInt3)
				{
					return null;
				}
				return valueString3;
			}
			break;
		}
		case NpcMenuEventReqType.ShowRemainTime:
		{
			BattleType valueEnum = ArgParser.GetValueEnum(values, 0, BattleType.None);
			if (valueEnum == BattleType.None)
			{
				break;
			}
			_ = string.Empty;
			int valueInt = ArgParser.GetValueInt(values, 1);
			int valueInt2 = ArgParser.GetValueInt(values, 2);
			string valueString = ArgParser.GetValueString(values, 3);
			if (valueEnum != BattleType.ChallengeTower && valueEnum != BattleType.Maze)
			{
				break;
			}
			TowerData data = GameRoot.Instance.TowerDataContainer.GetData(valueInt);
			if (data != null)
			{
				_showRemainTimeBattleType = valueEnum;
				_resetDay = data.MonthlyResetDate[0];
				if (!TowerHelper.SetupRemainResetTimeLabel(RemainTimeLabel, valueInt, valueInt2))
				{
					return null;
				}
				return valueString;
			}
			break;
		}
		}
		return null;
	}

	private void Update()
	{
		UpdateEventInfo();
	}

	private void UpdateEventInfo()
	{
		if (!(Time.time - _lastUpdateEventTime < 1f))
		{
			_lastUpdateEventTime = Time.time;
			SetupEventInfo();
		}
	}

	public void OnClickButton()
	{
		Vespa_System_Action_Extension_Method.fire(_onClick);
		if (_showRemainTimeBattleType == BattleType.None)
		{
			return;
		}
		string i_key = "EntryMenuResetDay_" + _showRemainTimeBattleType;
		LocalAccountInt localAccountInt = new LocalAccountInt(0, i_key);
		if (localAccountInt.Value > 0)
		{
			DateTime dateTime = new DateTime(1970, 1, 1).AddSeconds(localAccountInt.Value);
			DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
			if (dateTime.Year <= currentServerLocalTime.Year && dateTime.Month < currentServerLocalTime.Month && _resetDay <= currentServerLocalTime.Day)
			{
				localAccountInt.Value = checked((int)(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime() - new DateTime(1970, 1, 1)).TotalSeconds);
			}
		}
		else
		{
			localAccountInt.Value = checked((int)(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime() - new DateTime(1970, 1, 1)).TotalSeconds);
		}
	}

	public void Close()
	{
		_onClick = null;
	}
}
