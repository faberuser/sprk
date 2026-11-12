using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NEntryMenu;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

internal class ValanceMainLobbyWindow : BaseCinemaWindow, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceMainLobbyWindow";

	[SerializeField]
	public GameObject[] HideGameObjs;

	public UILabel[] ButtonLabels;

	public UISimpleGrid BottomButtonGrid;

	public UISimpleGrid BuffIconGrid;

	public List<EntryMenuButtonComponent> _BottomButtons = new List<EntryMenuButtonComponent>();

	private List<GeneralIconComponent> _buffIcons = new List<GeneralIconComponent>();

	private CampaignNodeData _nodeData;

	private List<Action> _onclickActions = new List<Action>();

	public GameObject CraftNoticeGo;

	public GameObject IdentifiedNoticeGo;

	public override void OnOtherWindowOpened()
	{
		base.OnOtherWindowOpened();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		SetNotice();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateBuffIcon();
	}

	private void UpdateBuffIcon()
	{
		if (__Extension_Method_ICollection__.COUNT(_buffIcons) == 0)
		{
			return;
		}
		for (int num = _buffIcons.Count - 1; num >= 0; num--)
		{
			GeneralIconComponent comp = _buffIcons[num];
			if (!comp.IsEffective())
			{
				_buffIcons.RemoveAt(num);
				FactoryBase.SafeReleaseComponent(ref comp);
			}
		}
	}

	private void ShowWindowRoot(bool isShow)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HideGameObjs); i++)
		{
			GameObject gameObject = HideGameObjs[i];
			if (!(gameObject == null))
			{
				UIHelper.EnableGameObject(gameObject, isShow);
			}
		}
	}

	public void Open(CampaignNodeData nodeData)
	{
		Open(delegate
		{
			Init(nodeData);
			SetNotice();
		}, null);
	}

	private void SetNotice()
	{
		bool enable = ValanceHelper.IsCraftNoticeOn();
		bool enable2 = ValanceHelper.IsIdentifiedNoticeOn();
		UIHelper.EnableGameObject(CraftNoticeGo, enable);
		UIHelper.EnableGameObject(IdentifiedNoticeGo, enable2);
	}

	public void SetButtonLabel(int index, string localeKey)
	{
		if (__Extension_Method_ICollection__.COUNT(ButtonLabels) > index && !String_Extension_Method.IsNullOrEmpty(localeKey))
		{
			UILabel label = ButtonLabels[index];
			string stringDirect = LocalizationManager.GetStringDirect(localeKey);
			UIHelper.SetLabelText(label, stringDirect);
		}
	}

	private void Init(CampaignNodeData nodeData)
	{
		if (nodeData != null)
		{
			_nodeData = nodeData;
			string entryNPCCode = nodeData.EntryNPCCode;
			if (!String_Extension_Method.IsNullOrEmpty(entryNPCCode))
			{
				NPCData data = GameRoot.Instance.NPCDataContainer.GetData(entryNPCCode);
				SetupMenuButtonGroup(data);
				SetupBottomMenuButtonGroup(data);
				SetupBuffIconGroup(data);
			}
		}
	}

	protected void SetupMenuButtonGroup(NPCData npcData)
	{
		if (npcData == null)
		{
			return;
		}
		int num = 1;
		while (true)
		{
			NShared.EntryMenu entryMenuButton = npcData.GetEntryMenuButton(num);
			if (entryMenuButton != null && entryMenuButton.IsValid)
			{
				if (!entryMenuButton.IsChild && EntryMenu.CheckEntryMenuCondition(_nodeData, entryMenuButton.EntryMenuCondition))
				{
					SetupMenuButton(entryMenuButton, num);
				}
				num++;
				continue;
			}
			break;
		}
	}

	protected void SetupMenuButton(NShared.EntryMenu menu, int menuIndex)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 1);
		if (String_Extension_Method.IsNullOrEmpty(valueString))
		{
			return;
		}
		NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
		if (action != null)
		{
			string valueString2 = ArgParser.GetValueString(menu.EntryMenuButton, 0);
			SetButtonLabel(menuIndex - 1, valueString2);
			_onclickActions.Add(delegate
			{
				action.Run(_nodeData, menu);
			});
		}
	}

	protected void SetupBottomMenuButtonGroup(NPCData npcData)
	{
		_BottomButtons.ForEach(delegate(EntryMenuButtonComponent bottom)
		{
			UIHelper.EnableGameObject(bottom, enable: false);
		});
		if (npcData != null)
		{
			int num = 1;
			while (true)
			{
				NShared.EntryMenu bottomEntryMenu = npcData.GetBottomEntryMenu(num);
				if (bottomEntryMenu == null || !bottomEntryMenu.IsValid)
				{
					break;
				}
				if (EntryMenu.CheckEntryMenuCondition(_nodeData, bottomEntryMenu.EntryMenuCondition))
				{
					SetupBottomMenuButton(bottomEntryMenu, npcData);
				}
				num++;
			}
		}
		UIHelper.RepositionGrid(BottomButtonGrid);
	}

	protected void SetupBottomMenuButton(NShared.EntryMenu menu, NPCData npcData)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 1);
		if (String_Extension_Method.IsNullOrEmpty(valueString))
		{
			return;
		}
		NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
		if (action == null)
		{
			return;
		}
		string buttonName = ArgParser.GetValueString(menu.EntryMenuButton, 2);
		EntryMenuButtonComponent entryMenuButtonComponent = _BottomButtons.Find((EntryMenuButtonComponent b) => b.name == buttonName);
		if (entryMenuButtonComponent == null)
		{
			return;
		}
		entryMenuButtonComponent.gameObject.SetActive(value: true);
		if (_nodeData != null)
		{
			entryMenuButtonComponent.Open(npcData, menu, action.GetType(), delegate
			{
				action.Run(_nodeData, menu);
			});
			return;
		}
		entryMenuButtonComponent.Open(npcData, menu, action.GetType(), delegate
		{
			action.Run(menu, new List<object> { _nodeData.EntryNPCCode });
		});
	}

	protected void SetupBuffIconGroup(NPCData npcData)
	{
		ClearBuffIcon();
		if (npcData == null)
		{
			return;
		}
		if (npcData.ShowHeroBuffIcon && npcData.HeroIndex > 0)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(npcData.HeroIndex);
			if (cachedHeroInfo != null)
			{
				HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star, cachedHeroInfo.Transcended);
				if (data != null)
				{
					GeneralIconComponent generalIconComponent = BuffIconGrid.AddGridItem(GeneralIconComponent.PrefabAuto);
					generalIconComponent.Open(UIHelper.GetAttrTypeIconPath(data.Icon), data, GeneralIconComponent.DecoType.EntryMenuBuff);
					_buffIcons.Add(generalIconComponent);
				}
			}
		}
		UIHelper.CreatreNpcBuffIconComponent(npcData, _buffIcons, BuffIconGrid);
		UIHelper.RepositionGrid(BuffIconGrid);
	}

	private void ClearBuffIcon()
	{
		FactoryBase.ReleaseAndClearList(_buffIcons);
	}

	public void OnClick1()
	{
		ClickAction(0);
	}

	public void OnClick2()
	{
		ClickAction(1);
	}

	public void OnClick3()
	{
		ClickAction(2);
	}

	public void ClickAction(int i)
	{
		if (__Extension_Method_ICollection__.COUNT(_onclickActions) > i)
		{
			Vespa_System_Action_Extension_Method.fire(_onclickActions[i]);
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_onclickActions.Clear();
		_nodeData = null;
		SceneAutoBehaviour<EntryMenuManagement>.instance.CloseNodeTreeEntryMenu(null);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		SetNotice();
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (changedInfo != null && changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Add)
		{
			EquipItemInfo equipItemInfo = changedInfo.EquipItemInfo;
			if (equipItemInfo != null && equipItemInfo.IsTechnoMagic && !equipItemInfo.IsIdentified)
			{
				SetNotice();
			}
		}
	}
}
