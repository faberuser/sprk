using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceOptionRenewResultWindow : ValanceEnhanceResultBase
{
	public interface Listener
	{
		void SelectEquipOption(int equipItemSlotIndex, int optionSlotIndex, int optionIndex, OptionSelectResult result);

		void AgainOptionRenew(int equipItemSlotIndex, int optionSlotIndex);

		void SelectEquipSkill(int equipItemSlotIndex, int skillSlotIndex, int optionIndex, OptionSelectResult result);

		void AgainSkillRenew(int equipItemSlotIndex, int skillSlotIndex);
	}

	public enum OptionSelectResult
	{
		None,
		Old,
		New
	}

	public enum ERenewType
	{
		Option,
		Skill
	}

	[Serializable]
	public class OptionSelectWindow
	{
		public Transform SelectRoot;

		public Transform OldOptionRoot;

		public StatGaugeInfo OldOption;

		public Transform NewOptionRoot;

		public StatGaugeInfo NewOption;

		public UIButton ConfirmButton;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceOptionRenewResultWindow";

	public OptionSelectWindow StatSelectWindow;

	public OptionSelectWindow SkillSelectWindow;

	[Header("Result")]
	public Transform OptionReslutRoot;

	public UIWidget OldResultOptionRoot;

	public UIWidget NewResultOptionRoot;

	public Transform ChangedOptionArrowRoot;

	public UIButton Button_Againe;

	private Listener _listener;

	private OptionSelectResult _selectResult;

	private ERenewType _renewType;

	private int _renewSlotIndex;

	private EquipItemOptionInfo _oldInfo;

	private ValanceRenewOptionInfo _renewOptionInfo;

	private ValanceRenewSkillInfo _renewSkillInfo;

	private StatListComponent _oldOptionListComponent;

	private StatListComponent _newOptionListComponent;

	private GameObject _arrowFollowTarget;

	private OptionSelectWindow _curSelectWindow;

	private EquipItemOptionInfo GetOptionInfo(ERenewType renewType, int renewSlotIndex)
	{
		if (renewType != ERenewType.Option)
		{
			return base.EquipItemInfo.GetExtraOptionInfo(renewSlotIndex);
		}
		return base.EquipItemInfo.GetOptionInfo(renewSlotIndex);
	}

	private bool isEnoughMaterial()
	{
		EquipItemDetailData data = GameRoot.Instance.EquipItemDetailDataContainer.GetData(base.EquipItemData.DetailIndex);
		if (data == null)
		{
			return false;
		}
		bool flag = false;
		if (_renewType == ERenewType.Option)
		{
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.RenewItemIndex1);
			bool num = data.RenewItemCount1 <= itemCount;
			int itemCount2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.RenewItemIndex2);
			bool flag2 = data.RenewItemCount2 <= itemCount2;
			return num && flag2;
		}
		int itemCount3 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.ExtraSkillRenewItemIndex);
		return data.ExtraSkillRenewItemCount <= itemCount3;
	}

	public void Open(Listener listener, int equipItemSlotIndex, int renewSlotIndex, ERenewType renewType = ERenewType.Option)
	{
		UIHelper.EnableGameObject(HideRoot, enable: false);
		base.Open(delegate
		{
			_listener = listener;
			_renewSlotIndex = renewSlotIndex;
			_renewType = renewType;
			base.EquipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
			if (base.EquipItemInfo != null)
			{
				_oldInfo = GetOptionInfo(renewType, renewSlotIndex);
				if (_oldInfo != null)
				{
					_renewOptionInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetValanceOptionRenew();
					_renewSkillInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetValanceSkillRenew();
					base.EquipItemData = base.EquipItemInfo.EquipItemData;
					_curSelectWindow = ((_renewType == ERenewType.Option) ? StatSelectWindow : SkillSelectWindow);
					Init();
				}
			}
		}, delegate
		{
			UIHelper.EnableGameObject(HideRoot, enable: true);
		});
	}

	public override void Init()
	{
		base.Init();
		UIHelper.EnableGameObject(_curSelectWindow.SelectRoot, enable: true);
		UIHelper.EnableGameObject(OptionReslutRoot, enable: false);
		SetSelectInfo();
		UpdateSelectButtonState();
	}

	private void SetSelectInfo()
	{
		if (_renewType == ERenewType.Option)
		{
			SetOldOptionInfo(_oldInfo);
			SetRenewOptionInfo(_renewOptionInfo);
		}
		else
		{
			SetOldSkillInfo(_oldInfo);
			SetRenewSkillInfo(_renewSkillInfo);
		}
	}

	private void UpdateSelectButtonState()
	{
		UIHelper.EnableButton(_curSelectWindow.ConfirmButton, _selectResult != OptionSelectResult.None);
		UIHelper.EnableGameObject(_curSelectWindow.OldOptionRoot, _selectResult == OptionSelectResult.Old);
		UIHelper.EnableGameObject(_curSelectWindow.NewOptionRoot, _selectResult == OptionSelectResult.New);
	}

	private void SetOldOptionInfo(EquipItemOptionInfo oldInfo)
	{
		int optionIndex = oldInfo.OptionIndex;
		int optionStep = oldInfo.OptionStep;
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		int current = data?.GetOptionValue(base.EquipItemInfo.Star, optionStep) ?? 0;
		int max = data?.GetOptionValueMax(base.EquipItemInfo.Star) ?? 0;
		_curSelectWindow.OldOption.Open(data?.OptionType ?? OperationType.None, current, max);
	}

	private void SetRenewOptionInfo(ValanceRenewOptionInfo renewInfo)
	{
		int renewOptionResultIndex = renewInfo.RenewOptionResultIndex;
		int renewOptionResultStep = renewInfo.RenewOptionResultStep;
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(renewOptionResultIndex);
		int current = data?.GetOptionValue(base.EquipItemInfo.Star, renewOptionResultStep) ?? 0;
		int max = data?.GetOptionValueMax(base.EquipItemInfo.Star) ?? 0;
		_curSelectWindow.NewOption.Open(data?.OptionType ?? OperationType.None, current, max);
	}

	private void SetOldSkillInfo(EquipItemOptionInfo oldInfo)
	{
		int optionIndex = oldInfo.OptionIndex;
		int optionStep = oldInfo.OptionStep;
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		int optionValue = data?.GetOptionValue(base.EquipItemInfo.Star, optionStep) ?? 0;
		string optionDisplay = StatData.GetOptionDisplay(data.OptionType, optionValue, opened: true, 0, appendColor: false);
		_curSelectWindow.OldOption.OpenExtraSkill(optionDisplay);
	}

	private void SetRenewSkillInfo(ValanceRenewSkillInfo renewInfo)
	{
		int renewSkillResultIndex = renewInfo.RenewSkillResultIndex;
		int renewSkillResultStep = renewInfo.RenewSkillResultStep;
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(renewSkillResultIndex);
		int optionValue = data?.GetOptionValue(base.EquipItemInfo.Star, renewSkillResultStep) ?? 0;
		string optionDisplay = StatData.GetOptionDisplay(data.OptionType, optionValue, opened: true, 0, appendColor: false);
		_curSelectWindow.NewOption.OpenExtraSkill(optionDisplay);
	}

	public void Refresh()
	{
		base.EquipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(base.EquipItemInfo.SlotIndex);
		UIHelper.EnableGameObject(_curSelectWindow.SelectRoot, enable: false);
		UIHelper.EnableGameObject(OptionReslutRoot, enable: true);
		ShowResultOption();
	}

	private void ShowResultOption()
	{
		if (_oldOptionListComponent != null || _newOptionListComponent != null)
		{
			ClearOptionStat();
		}
		_oldOptionListComponent = StatListComponent.Create(OldResultOptionRoot, 0, findParentPanel: true, isValance: true);
		_newOptionListComponent = StatListComponent.Create(NewResultOptionRoot, 0, findParentPanel: true, isValance: true);
		UIHelper.EnableGameObject(ChangedOptionArrowRoot, enable: false);
		foreach (EquipItemOptionInfo item in (_renewType == ERenewType.Option) ? base.EquipItemInfo.GetOptionInfoList() : base.EquipItemInfo.GetExtraOptionInfoList())
		{
			int optionStep = item.OptionStep;
			bool isOptionOpened = item.IsOptionOpened;
			int optionOpenLevel = base.EquipItemInfo.GetOptionOpenLevel(item.SlotIndex);
			if (item.SlotIndex == _renewSlotIndex)
			{
				StatListComponent.StatWidget<SingleStatInfo> statWidget = _oldOptionListComponent.AddOptionStat(_oldInfo.OptionIndex, base.EquipItemInfo.Star, _oldInfo.OptionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
				_newOptionListComponent.AddOptionStat(item.OptionIndex, base.EquipItemInfo.Star, optionStep, isOptionOpened, optionOpenLevel, renew: true, isFixedOption: false);
				if (statWidget != null && (bool)statWidget.GameObject)
				{
					_arrowFollowTarget = statWidget.GameObject;
					UIHelper.EnableGameObject(ChangedOptionArrowRoot, enable: true);
					UpdateArrowPosition();
				}
			}
			else
			{
				_oldOptionListComponent.AddOptionStat(item.OptionIndex, base.EquipItemInfo.Star, optionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
				_newOptionListComponent.AddOptionStat(item.OptionIndex, base.EquipItemInfo.Star, optionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
			}
		}
		UIHelper.EnableButton(Button_Againe, isEnoughMaterial(), grayscaleIfDisable: true);
	}

	private void UpdateArrowPosition()
	{
		if ((bool)ChangedOptionArrowRoot && (bool)_arrowFollowTarget)
		{
			Vector3 position = ChangedOptionArrowRoot.transform.position;
			float y = _arrowFollowTarget.transform.position.y;
			if (!Mathf.Approximately(position.y, y))
			{
				Vector3 position2 = new Vector3(position.x, y, position.z);
				ChangedOptionArrowRoot.transform.position = position2;
			}
		}
	}

	protected void Release()
	{
		_selectResult = OptionSelectResult.None;
		_listener = null;
		_arrowFollowTarget = null;
		_renewType = ERenewType.Option;
		_oldInfo = null;
		_renewOptionInfo = null;
		_renewSkillInfo = null;
		ClearOptionStat();
	}

	private void ClearOptionStat()
	{
		FactoryBase.SafeReleaseComponent(ref _oldOptionListComponent);
		FactoryBase.SafeReleaseComponent(ref _newOptionListComponent);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Release();
	}

	public void OnConfirmSelectOption()
	{
		if (_selectResult != OptionSelectResult.None && _listener != null)
		{
			bool num = _renewType == ERenewType.Option;
			int optionIndex = _oldInfo.OptionIndex;
			int num2 = (num ? _renewOptionInfo.RenewOptionResultIndex : _renewSkillInfo.RenewSkillResultIndex);
			int optionIndex2 = ((_selectResult == OptionSelectResult.New) ? num2 : optionIndex);
			if (num)
			{
				_listener.SelectEquipOption(base.EquipItemInfo.SlotIndex, _renewSlotIndex, optionIndex2, _selectResult);
			}
			else
			{
				_listener.SelectEquipSkill(base.EquipItemInfo.SlotIndex, _renewSlotIndex, optionIndex2, _selectResult);
			}
		}
	}

	public void OnSelectOldOption()
	{
		_selectResult = OptionSelectResult.Old;
		UpdateSelectButtonState();
	}

	public void OnSelectNewOption()
	{
		_selectResult = OptionSelectResult.New;
		UpdateSelectButtonState();
	}

	public void OnClickRenewAgain()
	{
		string stringDirect = LocalizationManager.GetStringDirect("EQUIP_OPTION_RENEW.ASK_RENEW_OPTION_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("VALANCE_RENEW_CONFIRM_POPUP");
		MessagePopup.Open(stringDirect, stringDirect2, delegate
		{
			if (_listener != null)
			{
				bool num = _renewType == ERenewType.Option;
				_selectResult = OptionSelectResult.None;
				if (num)
				{
					_listener.AgainOptionRenew(base.EquipItemInfo.SlotIndex, _renewSlotIndex);
				}
				else
				{
					_listener.AgainSkillRenew(base.EquipItemInfo.SlotIndex, _renewSlotIndex);
				}
			}
		}, null);
	}
}
