using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NNguiExtension;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulWeaponReinforceWindow : CommonSoulWeaponWindow, SoulWeaponList.IListener, SoulWeaponReinforceMaterialListInfo.IListener, CountInputField.IListener, PlayAnimSoulWeaponReinforce.IListener
{
	public interface IListener
	{
		void EtherInjection(EquipItemInfo equipItemInfo, RequirePrice requirePrice, int[] itemInfos, bool excessSuccessRate);

		void ReinforceSoulWeapon(EquipItemInfo equipItemInfo, long reqPrice);

		void ResultReinforceSoulWeapon(SoulWeaponInfo soulWeaponInfo, bool isSuccess, Action onClose);

		void OpenPopupDoNotTryReinforce();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponReinforceWindow";

	private const float MaxReinforceSuccessRate = 100f;

	public CountInputField CountInputField;

	public SoulWeaponReinforceMaterialListInfo ReinforceMaterialListInfo;

	public SoulWeaponStatValueInfo SoulWeaponStatValueInfo;

	public SoulWeaponStatValueInfo ReinforceSoulWeaponStatValueInfo;

	public SoulWeaponStatValueInfo EtherInjectionSoulWeaponStatValueInfo;

	public SoulWeaponStatValueInfo EtherInjectionReinforceSoulWeaponStatValueInfo;

	public SoulWeaponStatInfo MaxReinforceSoulWeaponStatInfo;

	public SelectSoulWeaponReinforceExpInfo SoulWeaponReinforceExpInfo;

	public SelectSoulWeaponReinforceExpInfo SoulWeaponReinforceExpProgressInfo;

	public PlayAnimSoulWeaponReinforce PlayAnimSoulWeaponReinforce;

	public Transform ReinforceStatInfoTranform;

	public Transform ReinforceTransform;

	public Transform EtherInjectionTransform;

	public Transform MaxReinforceLevelTransform;

	public Transform ReinforceExpBgTransform;

	public Transform ReinforceExpDescTransform;

	public UILabel MaxReinforceLevelLabel;

	public UILabel ReinforceExpLabel;

	public UILabel MinReinforceSuccessRateLabel;

	public UILabel CurrentReinforceSuccessRateLabel;

	public UILabel ReinforceSuccessRateLabel;

	public UILabel ReinforceRequirePriceLabel;

	public UIButton EtherInjectionButton;

	public UIButton TryReinforceButton;

	public UIButton ReinforceButton;

	public UISpriteSelector TryReinforceButtonSelector;

	private ItemData _selectItemData;

	private IListener _iListener;

	private float _currentReinforceExp;

	private float _soulWeaponReinforceMaxRate;

	private int _etherAddedBonus;

	public bool IsWaitReinforceResult { get; set; }

	private int MaxEtherCountReinforceExp
	{
		get
		{
			if (ReinforceMaterialListInfo.SoulWeaponEhterData == null)
			{
				return 0;
			}
			if (base.SoulWeaponInfo == null)
			{
				return 0;
			}
			SoulWeaponReinforceData soulWeaponReinforceData = base.SoulWeaponInfo.GetSoulWeaponReinforceData();
			if (soulWeaponReinforceData == null)
			{
				return 0;
			}
			int soulWeaponEtherDataExp = SoulWeaponEtherDataExp;
			if (soulWeaponEtherDataExp == 0)
			{
				return 0;
			}
			int soulWeaponReinforceMaterialMaxCount = SoulWeaponReinforceMaterialMaxCount;
			if (soulWeaponReinforceMaterialMaxCount == 0)
			{
				return 0;
			}
			int num = Mathf.CeilToInt((float)(soulWeaponReinforceData.RequiredExp - base.SoulWeaponInfo.Exp) / (float)soulWeaponEtherDataExp);
			if (soulWeaponReinforceMaterialMaxCount >= num)
			{
				return num;
			}
			return soulWeaponReinforceMaterialMaxCount;
		}
	}

	private long MaxEtherCountReinforceSuccessRate
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				int num = 0;
				return num;
			}
			if (_selectItemData == null)
			{
				int num2 = 0;
				return num2;
			}
			int soulWeaponReinforceMaterialMaxCount = SoulWeaponReinforceMaterialMaxCount;
			long num3 = soulWeaponReinforceMaterialMaxCount;
			if (num3 == 0L)
			{
				int num4 = 0;
				return num4;
			}
			int maxEtherCountReinforceExp = MaxEtherCountReinforceExp;
			long num5 = maxEtherCountReinforceExp;
			float currentReinforceSuccessRate = GetCurrentReinforceSuccessRate(num5);
			float num6 = 100f - currentReinforceSuccessRate;
			int num7 = 0;
			long num8 = num7;
			if (num6 > 0f)
			{
				int num9 = 1;
				int num10 = Mathf.CeilToInt(num6 / GetEtherAddedBonusPercentage(num9));
				num8 = num10;
			}
			num8 = checked(num8 + num5);
			if (num3 >= num8)
			{
				return num8;
			}
			return num3;
		}
	}

	private int SoulWeaponEtherDataExp
	{
		get
		{
			if (ReinforceMaterialListInfo.SoulWeaponEhterData == null)
			{
				return 0;
			}
			return ReinforceMaterialListInfo.SoulWeaponEhterData.ExpPoints;
		}
	}

	private int SoulWeaponReinforceMaterialMaxCount
	{
		get
		{
			if (ReinforceMaterialListInfo == null)
			{
				return 0;
			}
			return ReinforceMaterialListInfo.ReinforceMaterialMaxCount;
		}
	}

	private bool IsPossibleReinforce
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return false;
			}
			if (IsFullEtherForReinforce)
			{
				return ExceedMaxSuccessRate;
			}
			return false;
		}
	}

	private bool IsFullEtherForReinforce
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return false;
			}
			return base.SoulWeaponInfo.Exp >= SoulWeaponReinforceExpInfo.RequireExp;
		}
	}

	private bool ExceedMaxSuccessRate => (float)base.SoulWeaponInfo.ReinforceRatio >= _soulWeaponReinforceMaxRate;

	private bool IsMaxReinforceLevel
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return true;
			}
			SoulWeaponInfo nextReinforceSoulWeaponInfo = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.GetNextReinforceSoulWeaponInfo(base.SoulWeaponInfo);
			if (nextReinforceSoulWeaponInfo == null)
			{
				return true;
			}
			return base.SoulWeaponInfo.ReinforceLevel == nextReinforceSoulWeaponInfo.ReinforceLevel;
		}
	}

	private long ReinforceRequirePrice
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return 0L;
			}
			SoulWeaponReinforceData soulWeaponReinforceData = base.SoulWeaponInfo.GetSoulWeaponReinforceData();
			if (soulWeaponReinforceData == null)
			{
				return 0L;
			}
			return soulWeaponReinforceData.ReqGold;
		}
	}

	private float MinReinforceSuccessRate
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return 0f;
			}
			SoulWeaponReinforceData soulWeaponReinforceData = base.SoulWeaponInfo.GetSoulWeaponReinforceData();
			if (soulWeaponReinforceData == null)
			{
				return 0f;
			}
			return (float)soulWeaponReinforceData.SuccessRateMark / 100f;
		}
	}

	private bool IsFullReinforceExp => _currentReinforceExp >= (float)SoulWeaponReinforceExpInfo.RequireExp;

	private float SoulWeaponReinforceRatio
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return 0f;
			}
			return GetPercentage(base.SoulWeaponInfo.ReinforceRatio);
		}
	}

	public void Open(IListener iListener, EquipItemInfo equipItemInfo, bool isSelectFirst)
	{
		VespaCoroutine.Start(OpenAsyc(iListener, equipItemInfo, isSelectFirst));
	}

	private IEnumerator OpenAsyc(IListener iListener, EquipItemInfo equipItemInfo, bool isSelectFirst)
	{
		Open(equipItemInfo);
		Init(iListener, isSelectFirst);
		yield return null;
	}

	private void Init(IListener iListener, bool isSelectFirst)
	{
		_iListener = iListener;
		_selectItemData = null;
		IsWaitReinforceResult = false;
		InitSoulWeaponReinforceMaxRate();
		InitSoulWeaponList(this, base.EquipItemInfo, SoulWeaponList.ListType.SoulWeapon, isSelectFirst);
		SetSelectSoulWeapon(null);
		if (PlayAnimSoulWeaponReinforce != null)
		{
			PlayAnimSoulWeaponReinforce.Open(this);
		}
		SetReinforceInfo();
		SetSoulWeaponStatInfo();
	}

	private void InitSoulWeaponReinforceMaxRate()
	{
		_soulWeaponReinforceMaxRate = GameRoot.Instance.ConstantDataContainer.GetValueInt("SoulWeaponReinforceMaxRate");
		if (_soulWeaponReinforceMaxRate <= 0f)
		{
			_soulWeaponReinforceMaxRate = 100000f;
		}
	}

	private void SetEtherAddedBonus()
	{
		if (_selectItemData == null)
		{
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(_selectItemData.Index);
		if (data != null && base.SoulWeaponInfo != null)
		{
			SoulWeaponEtherRateData data2 = GameRoot.Instance.SoulWeaponEtherRateDataContainer.GetData(new SoulWeaponEtherRateKey(base.SoulWeaponInfo.ReinforceLevel, data.Code));
			if (data2 != null)
			{
				_etherAddedBonus = data2.AddedBonusRate;
			}
		}
	}

	private void SetSoulWeaponStatInfo()
	{
		SoulWeaponStatValueInfo.Open(base.SoulWeaponInfo);
		EtherInjectionSoulWeaponStatValueInfo.Open(base.SoulWeaponInfo);
		MaxReinforceSoulWeaponStatInfo.Open(base.SoulWeaponInfo, isRenew: false, isTotal: true);
		SetReinforceLevel(MaxReinforceLevelLabel, base.SoulWeaponInfo);
		if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager != null)
		{
			SoulWeaponInfo nextReinforceSoulWeaponInfo = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.GetNextReinforceSoulWeaponInfo(base.SoulWeaponInfo);
			bool isPossibleReinforce = IsPossibleReinforce;
			ReinforceSoulWeaponStatValueInfo.Open(isPossibleReinforce ? nextReinforceSoulWeaponInfo : base.SoulWeaponInfo);
			EtherInjectionReinforceSoulWeaponStatValueInfo.Open(nextReinforceSoulWeaponInfo);
			if (IsFullEtherForReinforce)
			{
				PlaySound("UI.SoulWeapon.EtherFull");
			}
		}
	}

	private void SetReinforceLevel(UILabel levelLabel, SoulWeaponInfo soulWeaponInfo)
	{
		if (!(levelLabel == null))
		{
			if (soulWeaponInfo == null)
			{
				UIHelper.SetLabelText(levelLabel, "-");
			}
			else
			{
				UIHelper.SetLabelText(levelLabel, soulWeaponInfo.ReinforceLevel);
			}
		}
	}

	public void Refresh(EquipItemInfo equipItemInfo)
	{
		base.EquipItemInfo = equipItemInfo;
		SetReinforceInfo();
		SetSoulWeaponStatInfo();
	}

	public void PlayAnimation(SoulWeaponInfo soulWeaponInfo, bool isSuccess)
	{
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		PlayAnimSoulWeaponReinforce.StartAnim(soulWeaponInfo, isSuccess);
	}

	private void SetReinforceInfo()
	{
		_currentReinforceExp = 0f;
		SetReinforceButtonState(enable: true);
		SoulWeaponReinforceExpInfo.Open(base.SoulWeaponInfo);
		int initPrice = 0;
		int num = 0;
		SetCountInputField(num, initPrice);
		SetReinforceExpInfo(0);
		ReinforceMaterialListInfo.Open(this, (base.SoulWeaponInfo != null) ? base.SoulWeaponInfo.ReinforceLevel : 0);
		SetEhterInjectionButtonState(enable: false);
		SetTryReinforceButtonState();
		SetReinforceInfoLabel();
		SetTotalCurrentReinforceSuccessRate();
		SetResultReinforceSuccessRate();
		SetTryReinforceRequirePrice();
		SetReinforceWindowState();
		if (PlayAnimSoulWeaponReinforce != null)
		{
			PlayAnimSoulWeaponReinforce.PlayAnimEtherInjecion(IsMaxReinforceLevel ? 1f : SoulWeaponReinforceExpProgressInfo.Progress);
		}
	}

	private void SetCountInputField(long maxCount, int initPrice)
	{
		if (CountInputField != null)
		{
			CountInputField.Open(this, new CountInputField.CountInputFieldInfo
			{
				InitCount = 0,
				MaxCount = maxCount,
				InitPrice = initPrice
			});
		}
	}

	private void SetReinforceExpInfo(int addExp)
	{
		if (IsFullReinforceExp)
		{
			long currentCount = CountInputField.CurrentCount;
			int maxEtherCountReinforceExp = MaxEtherCountReinforceExp;
			if (currentCount >= maxEtherCountReinforceExp)
			{
				return;
			}
		}
		SetReinforceExpLabel();
		SetReinforceExpProgress(addExp);
	}

	private void SetReinforceExpLabel()
	{
		UIHelper.SetLabelText(ReinforceExpLabel, "-");
		UIHelper.EnableGameObject(ReinforceExpBgTransform, !IsMaxReinforceLevel);
		UIHelper.EnableGameObject(ReinforceExpDescTransform, enable: false);
		if (base.SoulWeaponInfo != null)
		{
			if (IsMaxReinforceLevel)
			{
				UIHelper.SetLabelText(ReinforceExpLabel, "");
				return;
			}
			int currentReinforceExp = GetCurrentReinforceExp(CountInputField.CurrentCount);
			string text = ((currentReinforceExp >= SoulWeaponReinforceExpInfo.RequireExp) ? GetYellowColorText(currentReinforceExp.ToString()) : GetWhiteColorText(currentReinforceExp.ToString()));
			UIHelper.SetLabelText(ReinforceExpLabel, text + " / " + SoulWeaponReinforceExpInfo.RequireExp);
			UIHelper.EnableGameObject(ReinforceExpDescTransform, enable: true);
		}
	}

	private void SetReinforceExpProgress(int addExp)
	{
		if (base.SoulWeaponInfo != null)
		{
			int exp = base.SoulWeaponInfo.Exp;
			base.SoulWeaponInfo.Exp += addExp;
			SoulWeaponReinforceExpProgressInfo.Open(base.SoulWeaponInfo);
			base.SoulWeaponInfo.Exp = exp;
		}
	}

	private void SetTotalCurrentReinforceSuccessRate()
	{
		if (base.SoulWeaponInfo != null)
		{
			float soulWeaponReinforceRatio = SoulWeaponReinforceRatio;
			float currentReinforceSuccessRate = GetCurrentReinforceSuccessRate(CountInputField.CurrentCount);
			currentReinforceSuccessRate = GetCheckMaxSuccessRate(currentReinforceSuccessRate);
			currentReinforceSuccessRate = Ceiling(currentReinforceSuccessRate, 2);
			bool flag = currentReinforceSuccessRate > soulWeaponReinforceRatio;
			string text = currentReinforceSuccessRate + "%";
			UIHelper.SetLabelText(CurrentReinforceSuccessRateLabel, flag ? GetMintColorText(text) : GetGrayColorText(text));
			CurrentReinforceSuccessRateLabel.fontSize = (flag ? 36 : 28);
		}
	}

	private void SetResultReinforceSuccessRate()
	{
		UIHelper.SetLabelText(ReinforceSuccessRateLabel, GetCheckMaxSuccessRate(SoulWeaponReinforceRatio) + "%");
	}

	public override void Close()
	{
		if (!IsWaitReinforceResult)
		{
			base.Close();
			_iListener = null;
			_selectItemData = null;
			ReinforceMaterialListInfo.Close();
			if (PlayAnimSoulWeaponReinforce != null)
			{
				PlayAnimSoulWeaponReinforce.Close();
			}
		}
	}

	private void SetEhterInjectionButtonState(bool enable)
	{
		UIHelper.EnableButton(EtherInjectionButton, !IsMaxReinforceLevel && enable);
	}

	private void SetTryReinforceButtonState()
	{
		bool flag = SoulWeaponReinforceRatio >= MinReinforceSuccessRate;
		bool flag2 = false;
		if (flag && IsFullEtherForReinforce)
		{
			long currentCount = CountInputField.CurrentCount;
			int num = 0;
			if (currentCount <= num)
			{
				flag2 = true;
			}
		}
		if (TryReinforceButtonSelector != null)
		{
			TryReinforceButtonSelector.Select(flag2.ToString());
		}
		UIHelper.EnableButton(TryReinforceButton, flag && IsFullEtherForReinforce);
	}

	public void SetReinforceButtonState(bool enable)
	{
		UIHelper.EnableButton(ReinforceButton, enable);
	}

	public void SetReinforceWindowState()
	{
		if (base.SoulWeaponInfo == null)
		{
			UIHelper.EnableGameObject(ReinforceTransform, enable: false);
			UIHelper.EnableGameObject(EtherInjectionTransform, enable: false);
			UIHelper.EnableGameObject(MaxReinforceLevelTransform, enable: false);
		}
		else
		{
			bool isPossibleReinforce = IsPossibleReinforce;
			UIHelper.EnableGameObject(ReinforceTransform, !IsMaxReinforceLevel && isPossibleReinforce);
			UIHelper.EnableGameObject(EtherInjectionTransform, !IsMaxReinforceLevel && !isPossibleReinforce);
			UIHelper.EnableGameObject(MaxReinforceLevelTransform, IsMaxReinforceLevel);
			UIHelper.EnableGameObject(ReinforceStatInfoTranform, !IsMaxReinforceLevel);
		}
	}

	public void SetReinforceInfoLabel()
	{
		UIHelper.SetLabelText(MinReinforceSuccessRateLabel, MinReinforceSuccessRate + "%");
		UIHelper.SetLabelText(ReinforceRequirePriceLabel, Vespa_Long_Extension_Method.ToCommaString(ReinforceRequirePrice));
	}

	private void SetTryReinforceRequirePrice()
	{
		if (SoulWeaponReinforceRatio >= MinReinforceSuccessRate && CountInputField.CurrentCount == 0L)
		{
			CountInputField.SetPrice(ReinforceRequirePrice);
		}
	}

	private int GetCurrentReinforceExp(long etherCount)
	{
		if (base.SoulWeaponInfo == null)
		{
			return 0;
		}
		long num = etherCount;
		int maxEtherCountReinforceExp = MaxEtherCountReinforceExp;
		if (num > maxEtherCountReinforceExp)
		{
			int maxEtherCountReinforceExp2 = MaxEtherCountReinforceExp;
			etherCount = maxEtherCountReinforceExp2;
		}
		int exp = base.SoulWeaponInfo.Exp;
		long num2 = etherCount;
		int soulWeaponEtherDataExp = SoulWeaponEtherDataExp;
		checked
		{
			long num3 = num2 * soulWeaponEtherDataExp;
			int num4 = exp;
			long num5 = num4 + num3;
			return (int)num5;
		}
	}

	private float GetCurrentReinforceSuccessRate(long etherCount)
	{
		float etherAddedBonusPercentage = GetEtherAddedBonusPercentage(etherCount);
		return SoulWeaponReinforceRatio + etherAddedBonusPercentage;
	}

	private float GetPercentage(float value)
	{
		return value / 1000f;
	}

	private float GetCheckMaxSuccessRate(float successRate)
	{
		if (successRate >= 100f && !ClientValueManager.IsShowSoulWeaponReinforceSuccessRate)
		{
			successRate = 100f;
		}
		return successRate;
	}

	private float GetEtherAddedBonusPercentage(long etherCount = 1)
	{
		int etherAddedBonus = _etherAddedBonus;
		return (float)checked(etherAddedBonus * etherCount) / _soulWeaponReinforceMaxRate * 100f;
	}

	private float Ceiling(float value, int point)
	{
		float num = value * Mathf.Pow(10f, point);
		float num2 = (float)decimal.Ceiling((decimal)num);
		num = Mathf.Pow(10f, point);
		return num2 / num;
	}

	private string GetWhiteColorText(string text)
	{
		return "[c][ffffff]" + text + "[-][/c]";
	}

	private string GetYellowColorText(string text)
	{
		return "[c][f4b826]" + text + "[-][/c]";
	}

	private string GetGrayColorText(string text)
	{
		return "[c][ffffff]" + text + "[-][/c]";
	}

	private string GetMintColorText(string text)
	{
		return "[c][8ae96d]" + text + "[-][/c]";
	}

	private void ProcessReinforce()
	{
		if (_iListener != null)
		{
			SetReinforceButtonState(enable: false);
			IsWaitReinforceResult = true;
			EnableAllSoulWeaponList(enable: false);
			_iListener.ReinforceSoulWeapon(base.EquipItemInfo, ReinforceRequirePrice);
		}
	}

	public void OnClickEhterInjection()
	{
		PlaySound("UI.SoulWeapon.EtherInsert");
		if (_selectItemData != null && _iListener != null && !(CountInputField == null))
		{
			EnableAllSoulWeaponList(enable: false);
			bool excessSuccessRate = GetCurrentReinforceSuccessRate(CountInputField.CurrentCount) > GetPercentage(_soulWeaponReinforceMaxRate);
			IListener iListener = _iListener;
			EquipItemInfo equipItemInfo = base.EquipItemInfo;
			RequirePrice requirePrice = CountInputField.RequirePrice;
			int[] obj = new int[2] { _selectItemData.Index, 0 };
			long currentCount = CountInputField.CurrentCount;
			obj[1] = checked((int)currentCount);
			iListener.EtherInjection(equipItemInfo, requirePrice, obj, excessSuccessRate);
		}
	}

	public void OnClickReinforce()
	{
		ProcessReinforce();
	}

	public void OnClickTryReinforce()
	{
		if (_iListener == null)
		{
			return;
		}
		if (SoulWeaponReinforceRatio >= MinReinforceSuccessRate)
		{
			long currentCount = CountInputField.CurrentCount;
			int num = 0;
			if (currentCount > num)
			{
				_iListener.OpenPopupDoNotTryReinforce();
				return;
			}
		}
		ProcessReinforce();
	}

	void SoulWeaponList.IListener.SelectSoulWeapon(EquipItemInfo equipItemInfo, SoulWeaponList.ListType listType)
	{
		if (equipItemInfo != null)
		{
			base.EquipItemInfo = equipItemInfo;
			SetSelectSoulWeapon(null);
			SetReinforceInfo();
			SetSoulWeaponStatInfo();
			if (listType == AllSoulWeaponList.AllListType)
			{
				SoulWeaponList.SelectSoulWeaponItemIconComponent(equipItemInfo);
				EnableAllSoulWeaponList(enable: false);
			}
		}
	}

	void SoulWeaponReinforceMaterialListInfo.IListener.SelectReinforceMaterial(ItemData selectItemData)
	{
		if (selectItemData != null && ReinforceMaterialListInfo.SoulWeaponEhterData != null && !(CountInputField == null))
		{
			_selectItemData = selectItemData;
			_currentReinforceExp = 0f;
			SetEtherAddedBonus();
			int initPrice = 0;
			int num = 0;
			SetCountInputField(num, initPrice);
			SetReinforceExpInfo(0);
			SetTotalCurrentReinforceSuccessRate();
			SetEhterInjectionButtonState(enable: false);
			SetTryReinforceButtonState();
			SetReinforceWindowState();
			SetCountInputField(MaxEtherCountReinforceSuccessRate, ReinforceMaterialListInfo.SoulWeaponEhterData.ReqGold);
			SetTryReinforceRequirePrice();
		}
	}

	void CountInputField.IListener.ChangeCount(bool isMax)
	{
		if (CountInputField == null || base.SoulWeaponInfo == null)
		{
			return;
		}
		int maxEtherCountReinforceExp = MaxEtherCountReinforceExp;
		if (isMax)
		{
			long prevCount = CountInputField.PrevCount;
			int num = maxEtherCountReinforceExp;
			if (prevCount < num)
			{
				long currentCount = CountInputField.CurrentCount;
				int num2 = maxEtherCountReinforceExp;
				if (currentCount >= num2)
				{
					CountInputField.SetStateByCurrentCount(maxEtherCountReinforceExp);
				}
			}
		}
		long currentCount2 = CountInputField.CurrentCount;
		int soulWeaponEtherDataExp = SoulWeaponEtherDataExp;
		checked
		{
			long num3 = currentCount2 * soulWeaponEtherDataExp;
			long num4 = num3;
			SetReinforceExpInfo((int)num4);
			SetTotalCurrentReinforceSuccessRate();
			long currentCount3 = CountInputField.CurrentCount;
			int num5 = 0;
			SetEhterInjectionButtonState(currentCount3 > num5);
			SetTryReinforceButtonState();
			SetReinforceWindowState();
			SetTryReinforceRequirePrice();
			int exp = base.SoulWeaponInfo.Exp;
			long num6 = num3;
			int num7 = exp;
			_currentReinforceExp = num7 + num6;
		}
	}

	void PlayAnimSoulWeaponReinforce.IListener.FinishAnimation(SoulWeaponInfo soulWeaponInfo, bool isSuccess)
	{
		if (_iListener != null)
		{
			GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
			_iListener.ResultReinforceSoulWeapon(soulWeaponInfo, isSuccess, delegate
			{
				IsWaitReinforceResult = false;
			});
		}
	}
}
