using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NDispatchBattle;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NEntryMenu;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ForgeShortCutWindow : BaseWindow, SubEntryMenuButtonComponent.Listener
{
	public enum ShortCutType
	{
		Forge,
		Valance
	}

	public class SubEntryUi
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/ForgeShortCutWindow";

	public ShortCutType shortCutType;

	[Header("SubEntry")]
	public UILabel Label_Title;

	public UITable Table_SubMenu_Root;

	public UIWidget Group_Widget_Frame;

	[Header("Data")]
	private NPCData _npcData;

	protected EquipItemInfo _targetInfo;

	private int _subMenuIndex;

	private List<object> _extraValues;

	private Dictionary<string, SubEntryMenuButtonComponent> _componentDic = new Dictionary<string, SubEntryMenuButtonComponent>();

	private static Dictionary<string, Action<SubEntryMenuButtonComponent, EquipItemInfo>> _actionsForMenuButton;

	static ForgeShortCutWindow()
	{
		_actionsForMenuButton = new Dictionary<string, Action<SubEntryMenuButtonComponent, EquipItemInfo>>();
		_actionsForMenuButton.Add("OpenEquipAwaken", SetEnableAwaken);
		_actionsForMenuButton.Add("OpenEquipLimitBreak", SetEnableLimitBreak);
		_actionsForMenuButton.Add("OpenEquipOptionUpgrade", delegate(SubEntryMenuButtonComponent component, EquipItemInfo targetInfo)
		{
			UIHelper.EnableButton(component.GetComponent<UIButton>(), EquipOptionUpgrade.CanUpgrade(targetInfo), grayscaleIfDisable: true);
		});
		_actionsForMenuButton.Add("OpenEquipOptionRenew", delegate(SubEntryMenuButtonComponent component, EquipItemInfo targetInfo)
		{
			UIHelper.EnableButton(component.GetComponent<UIButton>(), EquipOptionRenew.CanRenew(targetInfo), grayscaleIfDisable: true);
		});
		_actionsForMenuButton.Add("OpenEquipEnchantOption", delegate(SubEntryMenuButtonComponent component, EquipItemInfo targetInfo)
		{
			UIHelper.EnableButton(component.GetComponent<UIButton>(), EquipEnchantOption.CanEnchant(targetInfo), grayscaleIfDisable: true);
		});
		_actionsForMenuButton.Add("OpenValanceIdentifiedInven", delegate(SubEntryMenuButtonComponent component, EquipItemInfo targetInfo)
		{
			bool enable = !targetInfo.IsIdentified;
			UIHelper.EnableButton(component.GetComponent<UIButton>(), enable, grayscaleIfDisable: true);
		});
		_actionsForMenuButton.Add("OpenValanceAwakenInven", delegate(SubEntryMenuButtonComponent component, EquipItemInfo targetInfo)
		{
			bool isIdentified = targetInfo.IsIdentified;
			isIdentified &= targetInfo.CLI_CanAwaken();
			UIHelper.EnableButton(component.GetComponent<UIButton>(), isIdentified, grayscaleIfDisable: true);
		});
		_actionsForMenuButton.Add("OpenValanceRenewInven", delegate(SubEntryMenuButtonComponent component, EquipItemInfo targetInfo)
		{
			bool isIdentified = targetInfo.IsIdentified;
			isIdentified &= EquipOptionRenew.CanRenew(targetInfo);
			UIHelper.EnableButton(component.GetComponent<UIButton>(), isIdentified, grayscaleIfDisable: true);
		});
		_actionsForMenuButton.Add("OpenValanceTierUpInven", delegate(SubEntryMenuButtonComponent component, EquipItemInfo targetInfo)
		{
			bool enable = targetInfo.CLI_CanTechnoTierUpgrade();
			UIHelper.EnableButton(component.GetComponent<UIButton>(), enable, grayscaleIfDisable: true);
		});
		_actionsForMenuButton.Add("OpenValanceEnchantInven", delegate(SubEntryMenuButtonComponent component, EquipItemInfo targetInfo)
		{
			bool isIdentified = targetInfo.IsIdentified;
			UIHelper.EnableButton(component.GetComponent<UIButton>(), isIdentified, grayscaleIfDisable: true);
		});
	}

	public void Open(NPCData npcData, EquipItemInfo itemInfo, int subMenuIndex, List<object> extraValues, ShortCutType shortCutType)
	{
		base.Open();
		this.shortCutType = shortCutType;
		_npcData = npcData;
		_subMenuIndex = subMenuIndex;
		_targetInfo = itemInfo;
		_extraValues = extraValues;
		InitSubEntryMenuButtons();
		if (IsDispatched(_targetInfo))
		{
			SetButtonEnableAll(isEnable: false);
		}
	}

	public override void Reset()
	{
		base.Reset();
		foreach (KeyValuePair<string, SubEntryMenuButtonComponent> item in _componentDic)
		{
			FactoryBase.ReleaseComponent(item.Value);
		}
		_componentDic.Clear();
	}

	public void OnClickExitButton()
	{
		Close();
	}

	private void InitSubEntryMenuButtons()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_npcData.MenuTitleKey);
		UIHelper.SetLabelText(Label_Title, stringDirect);
		NShared.EntryMenu parentEntryMenu = _npcData.GetEntryMenuButton(_subMenuIndex);
		string valueString = ArgParser.GetValueString(parentEntryMenu.EntryMenuButton, 1);
		NGame2.NUI.NWindow.NEntryMenu.IAction parentAction = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
		foreach (NShared.EntryMenu menu in _npcData.GetSubEntryMenu(_subMenuIndex))
		{
			if (Table_SubMenu_Root == null)
			{
				break;
			}
			string valueString2 = ArgParser.GetValueString(menu.EntryMenuButton, 1);
			if (String_Extension_Method.IsNullOrEmpty(valueString2))
			{
				continue;
			}
			NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString2);
			CampaignNodeData _nodeData = GameRoot.Instance.CampaignNodeDataContainer.GetEntryMenuNode(_npcData.Code);
			if (_nodeData != null)
			{
				Action openEntryMenu = delegate
				{
					SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenu(_nodeData.ChapterIndex, _nodeData.DungeonIndex, skipAni: false, playNPCSound: true, isIgnoreTrigger: false, null);
				};
				Action onClickDepth = delegate
				{
					parentAction.Run(_nodeData, parentEntryMenu);
				};
				Action onClickDepth2 = delegate
				{
					action.Run(_nodeData, menu, _extraValues);
				};
				SubEntryMenuButtonComponent subEntryMenuButtonComponent = SubEntryMenuButtonComponent.PrefabAuto.Factory.Create(Table_SubMenu_Root.transform);
				subEntryMenuButtonComponent.Open(this, menu, action.GetType(), openEntryMenu, onClickDepth, onClickDepth2);
				Vespa_System_Action_Extension_Method.fire(Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_actionsForMenuButton, valueString2, null), subEntryMenuButtonComponent, _targetInfo);
				_componentDic.Add(valueString2, subEntryMenuButtonComponent);
			}
		}
		if (Table_SubMenu_Root != null)
		{
			UITable table_SubMenu_Root = Table_SubMenu_Root;
			table_SubMenu_Root.onReposition = (UITable.OnReposition)Delegate.Combine(table_SubMenu_Root.onReposition, new UITable.OnReposition(OnReposition_Table_SubMenu_Root));
			Table_SubMenu_Root.Reposition();
			UITable table_SubMenu_Root2 = Table_SubMenu_Root;
			table_SubMenu_Root2.onReposition = (UITable.OnReposition)Delegate.Remove(table_SubMenu_Root2.onReposition, new UITable.OnReposition(OnReposition_Table_SubMenu_Root));
		}
	}

	private void OnReposition_Table_SubMenu_Root()
	{
		if (!(Table_SubMenu_Root == null) && !(Group_Widget_Frame == null))
		{
			float num = UIHelper.CalculateTableHeight(Table_SubMenu_Root);
			Vector3 localPosition = Table_SubMenu_Root.transform.localPosition;
			localPosition.y = num / 2f;
			Table_SubMenu_Root.transform.localPosition = localPosition;
			Group_Widget_Frame.height = (int)num;
			Group_Widget_Frame.ForceUpdateAnchors(includeChildren: true);
		}
	}

	void SubEntryMenuButtonComponent.Listener.OnClick(SubEntryMenuButtonComponent component)
	{
		base.Close();
		Close();
	}

	private static void SetEnableAwaken(SubEntryMenuButtonComponent component, EquipItemInfo itemInfo)
	{
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.GetCanUseAwakeEquipItemList().ContainsKey(itemInfo.ItemIndex);
		if (itemInfo.IsUniqueWeaponWithoutClassWeapon)
		{
			bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CheckHavePublicUniqueEquipAwakenItem();
			flag = flag || flag2;
		}
		else if (itemInfo.IsUniqueTreasure)
		{
			bool flag3 = GlobalAutoBehaviour<AccountManager>.instance.CheckHavePublicTreasureAwakenItem();
			flag = flag || flag3;
		}
		flag &= itemInfo.CLI_CanAwaken();
		UIHelper.EnableButton(component.GetComponent<UIButton>(), flag, grayscaleIfDisable: true);
	}

	private static void SetEnableLimitBreak(SubEntryMenuButtonComponent component, EquipItemInfo itemInfo)
	{
		int num = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(itemInfo)?.WeaponUniqueCount ?? 0;
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		foreach (EquipItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager)
		{
			if (SingletonAuto<SoulWeaponLimitBreak>.instance.CanUse_as_Material(item, itemInfo))
			{
				list.Add(item);
				if (__Extension_Method_ICollection__.COUNT(list) >= num)
				{
					break;
				}
			}
		}
		bool enable = SingletonAuto<SoulWeaponLimitBreak>.instance.CanRequest_LimitBreak(itemInfo, list);
		UIHelper.EnableButton(component.GetComponent<UIButton>(), enable, grayscaleIfDisable: true);
	}

	private void SetButtonEnableAll(bool isEnable)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_componentDic))
		{
			return;
		}
		foreach (KeyValuePair<string, SubEntryMenuButtonComponent> item in _componentDic)
		{
			SubEntryMenuButtonComponent value = item.Value;
			if (!(value == null))
			{
				UIHelper.EnableButton(value.GetComponent<UIButton>(), isEnable, grayscaleIfDisable: true);
			}
		}
	}

	private bool IsDispatched(EquipItemInfo equipInfo)
	{
		if (equipInfo == null)
		{
			return false;
		}
		return DispatchHelper.IsDispatchedHero(equipInfo.EquippedHeroInfo);
	}
}
