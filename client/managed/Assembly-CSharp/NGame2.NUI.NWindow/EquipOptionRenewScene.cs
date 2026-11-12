using NGame2.NAccount;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipOptionRenewScene : EquipAwakenScene
{
	public interface Listener
	{
		void SelectEquipOption(EquipOptionRenewScene caller, int optionSlotIndex, int optionIndex, OptionSelectResult result);

		void AgainOptionRenew(int equipItemSlotIndex, int optionSlotIndex);
	}

	public enum OptionSelectResult
	{
		None,
		Old,
		New
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/FullScene/EquipOptionRenewScene";

	public Transform OptionSelectRoot;

	public Transform OldOptionSelectRoot;

	public StatGaugeInfo OldStat;

	public Transform NewOptionSelectRoot;

	public StatGaugeInfo NewStat;

	public UIButton ConfirmSelectButton;

	public UIWidget OldResultOptionRoot;

	public UIWidget NewResultOptionRoot;

	public Transform ChangedOptionArrowRoot;

	private Listener _listener;

	private OptionSelectResult _selectResult;

	private StatListComponent _oldOptionListComponent;

	private StatListComponent _newOptionListComponent;

	private int _oldOptionIndex;

	private int _oldOptionStep;

	private int OptionSlotIndex;

	private int _newOptionIndex;

	private int _newOptionStep;

	private GameObject _arrowFollowTarget;

	public void Open(Listener listener, int equipItemSlotIndex, int optionSlotIndex, int newOptionIndex = 0, int newOptionStep = 0)
	{
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
		if (equipItemInfo != null)
		{
			Open(equipItemInfo, null, CinemaType.Equip_OptionRenew, isSuccess: true, null);
			_listener = listener;
			OptionSlotIndex = optionSlotIndex;
			EquipItemOptionInfo optionInfo = equipItemInfo.GetOptionInfo(optionSlotIndex);
			if (optionInfo != null)
			{
				_oldOptionIndex = optionInfo.OptionIndex;
				_oldOptionStep = optionInfo.OptionStep;
			}
			_newOptionIndex = newOptionIndex;
			_newOptionStep = newOptionStep;
			UIHelper.EnableGameObject(OptionSelectRoot, enable: false);
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearOptionStat();
		_selectResult = OptionSelectResult.None;
		_listener = null;
		_oldOptionIndex = 0;
		_oldOptionStep = 0;
		_newOptionIndex = 0;
		_newOptionStep = 0;
		_arrowFollowTarget = null;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateArrowPosition();
	}

	private void ClearOptionStat()
	{
		FactoryBase.SafeReleaseComponent(ref _oldOptionListComponent);
		FactoryBase.SafeReleaseComponent(ref _newOptionListComponent);
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

	public void OnConfirmSelectOption()
	{
		if (_selectResult != OptionSelectResult.None && _listener != null)
		{
			int optionIndex = base.EquipItemInfo.GetOptionInfo(OptionSlotIndex).OptionIndex;
			int renewOptionResultIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionResultIndex;
			int optionIndex2 = ((_selectResult == OptionSelectResult.New) ? renewOptionResultIndex : optionIndex);
			_listener.SelectEquipOption(this, OptionSlotIndex, optionIndex2, _selectResult);
		}
	}

	private void UpdateSelectButtonState()
	{
		UIHelper.EnableButton(ConfirmSelectButton, _selectResult != OptionSelectResult.None);
		UIHelper.EnableGameObject(OldOptionSelectRoot, _selectResult == OptionSelectResult.Old);
		UIHelper.EnableGameObject(NewOptionSelectRoot, _selectResult == OptionSelectResult.New);
	}

	protected override void ShowAwakenResult()
	{
		UIHelper.EnableGameObject(OptionSelectRoot, enable: true);
		EquipItemOptionInfo optionInfo = base.EquipItemInfo.GetOptionInfo(OptionSlotIndex);
		int optionIndex = optionInfo.OptionIndex;
		int optionStep = optionInfo.OptionStep;
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		int current = data?.GetOptionValue(base.EquipItemInfo.Star, optionStep) ?? 0;
		int max = data?.GetOptionValueMax(base.EquipItemInfo.Star) ?? 0;
		OldStat.Open(data?.OptionType ?? OperationType.None, current, max);
		int key = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionResultIndex;
		int currentStep = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionResultStep;
		if (_newOptionIndex != 0 || _newOptionStep != 0)
		{
			key = _newOptionIndex;
			currentStep = _newOptionStep;
		}
		EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(key);
		int current2 = data2?.GetOptionValue(base.EquipItemInfo.Star, currentStep) ?? 0;
		int max2 = data2?.GetOptionValueMax(base.EquipItemInfo.Star) ?? 0;
		NewStat.Open(data2?.OptionType ?? OperationType.None, current2, max2);
		UpdateSelectButtonState();
	}

	public void ShowRenewOptionResult()
	{
		UIHelper.EnableGameObject(OptionSelectRoot, enable: false);
		base.ShowAwakenResult();
		ShowResultOption();
	}

	private void ShowResultOption()
	{
		_oldOptionListComponent = StatListComponent.Create(OldResultOptionRoot);
		_newOptionListComponent = StatListComponent.Create(NewResultOptionRoot);
		UIHelper.EnableGameObject(ChangedOptionArrowRoot, enable: false);
		foreach (EquipItemOptionInfo optionInfo in base.EquipItemInfo.GetOptionInfoList())
		{
			int optionStep = optionInfo.OptionStep;
			bool isOptionOpened = optionInfo.IsOptionOpened;
			int optionOpenLevel = base.EquipItemInfo.GetOptionOpenLevel(optionInfo.SlotIndex);
			if (optionInfo.SlotIndex == OptionSlotIndex)
			{
				StatListComponent.StatWidget<SingleStatInfo> statWidget = _oldOptionListComponent.AddOptionStat(_oldOptionIndex, base.EquipItemInfo.Star, _oldOptionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
				_newOptionListComponent.AddOptionStat(optionInfo.OptionIndex, base.EquipItemInfo.Star, optionStep, isOptionOpened, optionOpenLevel, renew: true, isFixedOption: false);
				if (statWidget != null && (bool)statWidget.GameObject)
				{
					_arrowFollowTarget = statWidget.GameObject;
					UIHelper.EnableGameObject(ChangedOptionArrowRoot, enable: true);
					UpdateArrowPosition();
				}
			}
			else
			{
				_oldOptionListComponent.AddOptionStat(optionInfo.OptionIndex, base.EquipItemInfo.Star, optionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
				_newOptionListComponent.AddOptionStat(optionInfo.OptionIndex, base.EquipItemInfo.Star, optionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
			}
		}
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

	public void OnClickRenewAgain()
	{
		if (_listener != null)
		{
			_listener.AgainOptionRenew(base.EquipItemInfo.SlotIndex, OptionSlotIndex);
		}
	}

	public void Tutorial_ShowRenewOptionResult()
	{
		UIHelper.EnableGameObject(OptionSelectRoot, enable: false);
		base.ShowAwakenResult();
		_oldOptionListComponent = StatListComponent.Create(OldResultOptionRoot);
		_newOptionListComponent = StatListComponent.Create(NewResultOptionRoot);
		UIHelper.EnableGameObject(ChangedOptionArrowRoot, enable: false);
		foreach (EquipItemOptionInfo optionInfo in base.EquipItemInfo.GetOptionInfoList())
		{
			int optionStep = optionInfo.OptionStep;
			bool isOptionOpened = optionInfo.IsOptionOpened;
			int optionOpenLevel = base.EquipItemInfo.GetOptionOpenLevel(optionInfo.SlotIndex);
			if (optionInfo.SlotIndex != OptionSlotIndex)
			{
				_oldOptionListComponent.AddOptionStat(optionInfo.OptionIndex, base.EquipItemInfo.Star, optionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
				_newOptionListComponent.AddOptionStat(optionInfo.OptionIndex, base.EquipItemInfo.Star, optionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
				continue;
			}
			StatListComponent.StatWidget<SingleStatInfo> statWidget = _oldOptionListComponent.AddOptionStat(_oldOptionIndex, base.EquipItemInfo.Star, _oldOptionStep, isOptionOpened, optionOpenLevel, renew: false, isFixedOption: false);
			_newOptionListComponent.AddOptionStat(_newOptionIndex, base.EquipItemInfo.Star, _newOptionStep, isOptionOpened, optionOpenLevel, renew: true, isFixedOption: false);
			if (statWidget != null && (bool)statWidget.GameObject)
			{
				_arrowFollowTarget = statWidget.GameObject;
				UIHelper.EnableGameObject(ChangedOptionArrowRoot, enable: true);
				UpdateArrowPosition();
			}
		}
	}
}
