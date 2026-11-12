using System;
using System.Collections.Generic;
using System.Linq;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponStatInfo
{
	public class StatInfo
	{
		public long BaseStat { get; set; }

		public long BonusStat { get; set; }
	}

	public List<UILabel> StatNameLabelList = new List<UILabel>();

	public List<UILabel> StatLabelList = new List<UILabel>();

	public List<UILabel> BonusStatLabelList = new List<UILabel>();

	public List<UISprite> RatioSpriteList = new List<UISprite>();

	public List<UISpriteSelector> StatTypeSmallIconSpriteSelectorList = new List<UISpriteSelector>();

	public List<UISpriteSelector> StatTypeIconSpriteSelectorList = new List<UISpriteSelector>();

	private List<StatType> _statTypeList = new List<StatType>();

	private Dictionary<ESoulWeaponStatType, StatInfo> _statInfoDic = new Dictionary<ESoulWeaponStatType, StatInfo>();

	public SoulWeaponInfo SoulWeaponInfo { get; private set; }

	public void Open(SoulWeaponInfo soulWeaponInfo, bool isRenew = false, bool isTotal = false)
	{
		Init(soulWeaponInfo, isRenew, isTotal);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo, bool isRenew, bool isTotal)
	{
		SetStatInfo(ref soulWeaponInfo, isRenew);
		SetStatTypeList(ref soulWeaponInfo, ref _statTypeList);
		SetEmptyLabel(ref StatNameLabelList, ref StatLabelList, ref BonusStatLabelList);
		SetStatName(ref _statTypeList, ref StatNameLabelList);
		SetStat(isTotal, isRenew);
		SetStatRatio(isRenew);
		SetStatTypeSmallIcon();
		SetStatTypeIcon();
	}

	public void SetStatInfo(ref SoulWeaponInfo soulWeaponInfo, bool isRenew)
	{
		SoulWeaponInfo = soulWeaponInfo;
		_statInfoDic.Clear();
		StatInfo statInfo = null;
		long num = 0L;
		for (ESoulWeaponStatType eSoulWeaponStatType = ESoulWeaponStatType.Attack; eSoulWeaponStatType < ESoulWeaponStatType.End; eSoulWeaponStatType++)
		{
			if (soulWeaponInfo != null)
			{
				num = soulWeaponInfo.GetBaseStat(eSoulWeaponStatType, isRenew);
				statInfo = new StatInfo
				{
					BaseStat = soulWeaponInfo.GetBaseStat(eSoulWeaponStatType, isRenew),
					BonusStat = soulWeaponInfo.GetBonusStat(num, eSoulWeaponStatType, isRenew)
				};
				_statInfoDic.Add(eSoulWeaponStatType, statInfo);
			}
		}
	}

	public void SetStatTypeList(ref SoulWeaponInfo soulWeaponInfo, ref List<StatType> list)
	{
		list.Clear();
		if (soulWeaponInfo == null)
		{
			return;
		}
		SoulWeaponOptionData soulWeaponOptionData = soulWeaponInfo.GetSoulWeaponOptionData();
		if (soulWeaponOptionData == null)
		{
			return;
		}
		StatType[] array = null;
		for (int i = 0; i < soulWeaponInfo.GetStatCount(); i++)
		{
			array = soulWeaponOptionData.GetOptionStatTypes(i);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(array) && array.Length != 0)
			{
				list.Add(array[0]);
			}
		}
	}

	public void SetStatName(ref List<StatType> list, ref List<UILabel> labelList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list) || __Extension_Method_ICollection__.IsNullOrEmpty(labelList))
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (labelList.Count > i)
			{
				UIHelper.SetLabelText(labelList[i], LocalizationManager.GetString(list[i]));
			}
		}
	}

	private void SetStat(bool isTotal, bool isRenew)
	{
		if (isTotal)
		{
			SetStat(ref StatLabelList);
		}
		else
		{
			SetStat(ref StatLabelList, ref BonusStatLabelList, isRenew);
		}
	}

	public void SetStat(ref List<UILabel> statLabelList, ref List<UILabel> bonusStatLabelList, bool isTotal, bool isRenew)
	{
		if (isTotal)
		{
			SetStat(ref statLabelList);
		}
		else
		{
			SetStat(ref statLabelList, ref bonusStatLabelList, isRenew);
		}
	}

	private void SetStat(ref List<UILabel> statLabelList, ref List<UILabel> bonusStatLabelList, bool isRenew)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_statInfoDic))
		{
			return;
		}
		long num = 0L;
		long num2 = 0L;
		StatInfo statInfo = null;
		for (int i = 0; i < _statInfoDic.Count; i++)
		{
			statInfo = _statInfoDic.Values.ToList()[i];
			num = 0L;
			num2 = 0L;
			if (statInfo != null)
			{
				num = statInfo.BaseStat;
				num2 = statInfo.BonusStat;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(statLabelList) && statLabelList.Count > i)
			{
				UIHelper.SetLabelText(statLabelList[i], num);
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(bonusStatLabelList) && bonusStatLabelList.Count > i)
			{
				UIHelper.SetLabelText(bonusStatLabelList[i], "+" + num2);
			}
		}
	}

	private void SetStat(ref List<UILabel> statLabelList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_statInfoDic))
		{
			return;
		}
		long num = 0L;
		long num2 = 0L;
		StatInfo statInfo = null;
		for (int i = 0; i < _statInfoDic.Count; i++)
		{
			statInfo = _statInfoDic.Values.ToList()[i];
			num = 0L;
			num2 = 0L;
			if (statInfo != null)
			{
				num = statInfo.BaseStat;
				num2 = statInfo.BonusStat;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(statLabelList) && statLabelList.Count > i)
			{
				UIHelper.SetLabelText(statLabelList[i], num + num2);
			}
		}
	}

	public void SetEmptyLabel(ref List<UILabel> statNameLabelList, ref List<UILabel> statLabelList, ref List<UILabel> bonusStatLabelList)
	{
		SetEmptyStatNameLabel(ref statNameLabelList);
		SetEmptyStatLabel(ref statLabelList, ref bonusStatLabelList);
	}

	private void SetEmptyStatNameLabel(ref List<UILabel> list)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		UILabel uILabel = null;
		for (int i = 0; i < list.Count; i++)
		{
			uILabel = list[i];
			if (uILabel != null)
			{
				UIHelper.SetLabelText(uILabel, "-");
			}
		}
	}

	private void SetEmptyStatLabel(ref List<UILabel> statLabelList, ref List<UILabel> bonusStatLabelList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(statLabelList))
		{
			return;
		}
		UILabel uILabel = null;
		for (int i = 0; i < statLabelList.Count; i++)
		{
			uILabel = statLabelList[i];
			if (uILabel != null)
			{
				UIHelper.SetLabelText(uILabel, "-");
			}
		}
	}

	private void SetStatRatio(bool isRenew)
	{
		if (SoulWeaponInfo == null || __Extension_Method_ICollection__.IsNullOrEmpty(RatioSpriteList))
		{
			return;
		}
		UISprite uISprite = null;
		long num = 0L;
		ESoulWeaponStatType eSoulWeaponStatType = ESoulWeaponStatType.End;
		int num2 = RatioSpriteList.Count - 1;
		while (0 <= num2)
		{
			uISprite = RatioSpriteList[num2];
			if (!(uISprite == null))
			{
				eSoulWeaponStatType = (ESoulWeaponStatType)num2;
				num = (isRenew ? SoulWeaponInfo.GetRenewOptionRatio(eSoulWeaponStatType) : SoulWeaponInfo.GetOptionRatio(eSoulWeaponStatType));
				uISprite.fillAmount = (float)num / 1000f;
			}
			num2--;
		}
	}

	private void SetStatTypeSmallIcon()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_statTypeList) || __Extension_Method_ICollection__.IsNullOrEmpty(StatTypeSmallIconSpriteSelectorList))
		{
			return;
		}
		UISpriteSelector uISpriteSelector = null;
		for (int i = 0; i < _statTypeList.Count; i++)
		{
			if (StatTypeSmallIconSpriteSelectorList.Count > i)
			{
				uISpriteSelector = StatTypeSmallIconSpriteSelectorList[i];
				if (!(uISpriteSelector == null))
				{
					uISpriteSelector.SelectEnum(_statTypeList[i]);
				}
			}
		}
	}

	private void SetStatTypeIcon()
	{
		if (_statTypeList == null || __Extension_Method_ICollection__.IsNullOrEmpty(StatTypeIconSpriteSelectorList))
		{
			return;
		}
		UISpriteSelector uISpriteSelector = null;
		for (int i = 0; i < _statTypeList.Count; i++)
		{
			if (StatTypeIconSpriteSelectorList.Count > i)
			{
				uISpriteSelector = StatTypeIconSpriteSelectorList[i];
				if (!(uISpriteSelector == null))
				{
					uISpriteSelector.SelectEnum(_statTypeList[i]);
				}
			}
		}
	}

	public void Close()
	{
		SoulWeaponInfo = null;
	}
}
