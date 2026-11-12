using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.ClassBuff;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.ClassBuff;

public class ClassBuffLevelUpPopup : BaseWindow
{
	public interface IListener
	{
		void LevelUp(ClassBuffData data);
	}

	public const string PrefabPath = "Prefab2/Window/ClassBuff/ClassBuffLevelUpPopup";

	private const int DefaultCount = 1;

	public UILabel PopupTitleLabel;

	public UILabel StatLevelUpLabel;

	public UILabel NeedPointLabel;

	public UIInput CountInput;

	public UILabel BeforeLevelLabel;

	public UILabel AfterLevelLabel;

	public UILabel StatLabel;

	public UILabel BeforeStatValueLabel;

	public UILabel AfterStatValueLabel;

	public Transform IconComponentTransform;

	public UIWidget IconParentWidget;

	public UIButton IncreaseButton;

	public UIButton DecreaseButton;

	private IListener _iListener;

	private ClassBuffWrapper _wrapper;

	private ClassBuffData _classBuffData;

	private ClassBuffIconComponent _classBuffIconComponent;

	private IconProxy _iconProxy;

	private int _currentCount;

	public void Open(IListener iListener, ClassBuffData data)
	{
		Open();
		Init(iListener, data);
	}

	private void Init(IListener iListener, ClassBuffData data)
	{
		_iListener = iListener;
		_classBuffData = data;
		_wrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		InitCount();
		SetClassBuffInfo();
		SetCurrentClassBuffInfo();
		SetButtonStatus();
	}

	private void InitCount()
	{
		_currentCount = 0;
		IncreaseCount();
	}

	protected override void OnDestroy()
	{
		ReleaseComponent();
		base.OnDestroy();
	}

	private void ReleaseComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _classBuffIconComponent);
	}

	private void SetClassBuffInfo()
	{
		SetPopupTitle();
		SetStatLevelUp();
		SetIconComponent();
		SetClassBuffIcon();
	}

	private bool CheckSetLabel()
	{
		if (_wrapper == null)
		{
			return false;
		}
		if (_classBuffData == null)
		{
			return false;
		}
		if (_classBuffData.SkillIndex <= 0)
		{
			return false;
		}
		return true;
	}

	private void SetPopupTitle()
	{
		if (!(PopupTitleLabel == null))
		{
			UIHelper.SetLabelText(PopupTitleLabel, "");
			if (_wrapper != null && CheckSetLabel() && _wrapper.GetSkillData(_classBuffData.SkillIndex) != null)
			{
				string stringDirect = LocalizationManager.GetStringDirect(_classBuffData.BuffOptionTitleName);
				string stringDirect2 = LocalizationManager.GetStringDirect(_wrapper.LocalizeClassBuffLevelUpPopupTitle, stringDirect);
				UIHelper.SetLabelText(PopupTitleLabel, stringDirect2);
			}
		}
	}

	private void SetIconComponent()
	{
		if (!(IconComponentTransform == null))
		{
			UIHelper.EnableGameObject(IconComponentTransform, enable: false);
			if (_classBuffIconComponent == null)
			{
				CreateIconComponent();
			}
			_classBuffIconComponent.SetInfo(_classBuffData);
			UIHelper.EnableGameObject(IconComponentTransform, enable: true);
		}
	}

	private void SetClassBuffIcon()
	{
		if (_classBuffData != null && _wrapper != null)
		{
			_wrapper.SetClassBuffIcon(ref IconParentWidget, ref _iconProxy, _classBuffData.TagType);
		}
	}

	private void CreateIconComponent()
	{
		if (!(IconComponentTransform == null))
		{
			ReleaseComponent();
			_classBuffIconComponent = ClassBuffIconComponent.PrefabAuto.Factory.Create(IconComponentTransform);
		}
	}

	private void SetStatLevelUp()
	{
		if (!(StatLevelUpLabel == null))
		{
			UIHelper.SetLabelText(StatLevelUpLabel, "");
			if (_wrapper != null && CheckSetLabel() && _wrapper.GetSkillData(_classBuffData.SkillIndex) != null)
			{
				string stringDirect = LocalizationManager.GetStringDirect(_classBuffData.BuffOptionTitleName);
				string stringDirect2 = LocalizationManager.GetStringDirect(_wrapper.LocalizeClassBuffLevelUpDesc, stringDirect);
				UIHelper.SetLabelText(StatLevelUpLabel, stringDirect2);
			}
		}
	}

	private void SetCurrentClassBuffInfo()
	{
		if (BeforeLevelLabel != null)
		{
			UIHelper.SetLabelText(BeforeLevelLabel, (_classBuffData.ClassBuffLevel < 1) ? "-" : ("Lv. " + _classBuffData.ClassBuffLevel));
		}
		if (BeforeStatValueLabel != null)
		{
			UIHelper.SetLabelText(BeforeStatValueLabel, "-");
			if (_classBuffData != null)
			{
				UIHelper.SetLabelText(BeforeStatValueLabel, GetStatValue(_classBuffData));
			}
		}
	}

	private void SetLevelUpClassBuffInfo()
	{
		if (StatLabel != null)
		{
			string text = "";
			if (_wrapper != null && _wrapper.GetSkillData(_classBuffData.SkillIndex) != null)
			{
				text = LocalizationManager.GetStringDirect(_classBuffData.BuffOptionTitleName);
			}
			UIHelper.SetLabelText(StatLabel, text);
		}
		ClassBuffData classBuffData = GetClassBuffData(_classBuffData.ClassBuffLevel + _currentCount);
		if (classBuffData != null)
		{
			if (AfterLevelLabel != null)
			{
				UIHelper.SetLabelText(AfterLevelLabel, "Lv. " + classBuffData.ClassBuffLevel);
			}
			if (AfterStatValueLabel != null)
			{
				UIHelper.SetLabelText(AfterStatValueLabel, GetStatValue(classBuffData));
			}
		}
	}

	private void SetLevelUpInfo(int count)
	{
		SetCount(count);
		SetNeedPoint();
		SetLevelUpClassBuffInfo();
		SetButtonStatus();
	}

	private void SetCount(int count)
	{
		_currentCount += count;
		int currentCount = _currentCount;
		CountInput.value = currentCount.ToString();
	}

	private void SetNeedPoint()
	{
		if (!(NeedPointLabel == null))
		{
			ClassBuffData levelUpClassBuffData = GetLevelUpClassBuffData();
			if (levelUpClassBuffData != null && _wrapper != null)
			{
				long totalNeedPoint = _wrapper.GetTotalNeedPoint(levelUpClassBuffData);
				long classBuffPoint = GetClassBuffPoint();
				UIHelper.SetLabelText(NeedPointLabel, Vespa_Long_Extension_Method.ToCommaString(totalNeedPoint) + " / " + Vespa_Long_Extension_Method.ToCommaString(classBuffPoint));
			}
		}
	}

	private int GetMaxClassBuffLevel()
	{
		return GameRoot.Instance.ClassBuffDataContainer.GeMaxtClassBuffLevel(_classBuffData.TagType, _classBuffData.ClassBuffIndex);
	}

	private ClassBuffData GetClassBuffData(int level)
	{
		return GameRoot.Instance.ClassBuffDataContainer.GetClassBuffData(_classBuffData.TagType, _classBuffData.ClassBuffIndex, level);
	}

	private ClassBuffData GetLevelUpClassBuffData()
	{
		return GetClassBuffData(_classBuffData.ClassBuffLevel + _currentCount);
	}

	private long GetClassBuffPoint()
	{
		if (_classBuffData == null)
		{
			return 0L;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager?.GetClassBuffPointByCreatureType(_classBuffData.TagType) ?? 0;
	}

	private string GetStatValue(ClassBuffData data)
	{
		if (data == null)
		{
			return "-";
		}
		string statValue = ClassBuffDataContainer.GetStatValue(data.SkillIndex, data.StatUsePercentage);
		if (!(statValue == ""))
		{
			return statValue + ((data.StatUsePercentage == 1) ? "%" : "");
		}
		return "-";
	}

	private bool CheckIncreaseCount(int count)
	{
		if (_wrapper == null)
		{
			return false;
		}
		ClassBuffData classBuffData = GetClassBuffData(_classBuffData.ClassBuffLevel + _currentCount + count);
		return _wrapper.CheckPossibleLevelUp(classBuffData);
	}

	private bool CheckDecreaseCount()
	{
		if (_currentCount <= 1)
		{
			return false;
		}
		return true;
	}

	private int GetMaxCount()
	{
		int currentCount = _currentCount;
		_currentCount = 0;
		int maxClassBuffLevel = GetMaxClassBuffLevel();
		int result = 0;
		for (int num = maxClassBuffLevel; num >= 0; num--)
		{
			if (CheckIncreaseCount(num))
			{
				result = num;
				break;
			}
		}
		_currentCount = currentCount;
		return result;
	}

	private void IncreaseCount()
	{
		if (CheckIncreaseCount(1))
		{
			SetLevelUpInfo(1);
		}
	}

	private void SetButtonStatus()
	{
		int result = 0;
		if (int.TryParse(CountInput.value, out result))
		{
			if (IncreaseButton != null)
			{
				bool enable = result < GetMaxCount();
				UIHelper.EnableButton(IncreaseButton, enable);
			}
			if (DecreaseButton != null)
			{
				bool enable2 = result > 1;
				UIHelper.EnableButton(DecreaseButton, enable2);
			}
		}
	}

	public void OnClickIncrease()
	{
		IncreaseCount();
	}

	public void OnClickDecrease()
	{
		if (CheckDecreaseCount())
		{
			SetLevelUpInfo(-1);
		}
	}

	public void OnClickMax()
	{
		if (_wrapper != null)
		{
			int maxCount = GetMaxCount();
			_currentCount = 0;
			SetLevelUpInfo(maxCount);
		}
	}

	public void OnSubmitCount()
	{
		if (!(CountInput == null))
		{
			int result = 0;
			int maxCount = GetMaxCount();
			if (!int.TryParse(CountInput.value, out result))
			{
				result = ((!(CountInput.value == "")) ? maxCount : _currentCount);
			}
			else if (result >= maxCount)
			{
				result = maxCount;
			}
			else if (result < 1)
			{
				result = _currentCount;
			}
			_currentCount = 0;
			SetLevelUpInfo(result);
		}
	}

	public void OnClickLevelUp()
	{
		if (_iListener != null)
		{
			ClassBuffData levelUpClassBuffData = GetLevelUpClassBuffData();
			if (levelUpClassBuffData != null)
			{
				_iListener.LevelUp(levelUpClassBuffData);
				Close();
			}
		}
	}
}
