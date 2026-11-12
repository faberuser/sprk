using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NBooster;

public class BoosterInfo
{
	public BoosterInfoType Type { get; set; }

	public EventBonusType BonusType { get; set; }

	public EventBonusType[] ExtraBonusTypes { get; set; }

	public string ItemCode { get; set; }

	public int Value { get; set; }

	public string Name { get; set; }

	public string Desc { get; set; }

	public bool IsOnetime { get; set; }

	public DateTime EndTimeUTC { get; set; }

	public int BeginHour { get; set; }

	public int BeginMinute { get; set; }

	public int EndHour { get; set; }

	public int EndMinute { get; set; }

	public ClientActivityType MoveTaskType { get; set; }

	public string[] MoveTaskValue { get; set; }

	public int MinLevel { get; set; }

	public DropBonusEvnetBuffPosition BuffPosition { get; set; }

	public BattleType[] BattleTypes { get; set; }

	public BoosterInfo()
	{
		EndTimeUTC = DateTime.UtcNow.AddYears(99);
	}

	public BoosterInfo(BoosterInfo other)
	{
		Type = other.Type;
		BonusType = other.BonusType;
		if (other.ExtraBonusTypes != null && other.ExtraBonusTypes.Length != 0)
		{
			ExtraBonusTypes = new EventBonusType[other.ExtraBonusTypes.Length];
			Array.Copy(other.ExtraBonusTypes, ExtraBonusTypes, other.ExtraBonusTypes.Length);
		}
		ItemCode = other.ItemCode;
		Value = other.Value;
		Name = other.Name;
		Desc = other.Desc;
		IsOnetime = other.IsOnetime;
		EndTimeUTC = other.EndTimeUTC;
		BeginHour = other.BeginHour;
		BeginMinute = other.BeginMinute;
		EndHour = other.EndHour;
		EndMinute = other.EndMinute;
		MoveTaskType = other.MoveTaskType;
		if (other.MoveTaskValue != null && other.MoveTaskValue.Length != 0)
		{
			MoveTaskValue = new string[other.MoveTaskValue.Length];
			Array.Copy(other.MoveTaskValue, MoveTaskValue, other.MoveTaskValue.Length);
		}
		MinLevel = other.MinLevel;
		BuffPosition = other.BuffPosition;
		if (other.BattleTypes != null && other.BattleTypes.Length != 0)
		{
			BattleTypes = new BattleType[other.BattleTypes.Length];
			Array.Copy(other.BattleTypes, BattleTypes, other.BattleTypes.Length);
		}
	}

	public virtual int GetSortScore()
	{
		return (int)((int)Type * 100 + BonusType);
	}

	public virtual bool IsEffective()
	{
		return DateTime.Compare(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime(), EndTimeUTC) < 0;
	}

	public virtual bool DisplayRemainTime()
	{
		return true;
	}

	public virtual int GetRemainSeconds()
	{
		return (int)(EndTimeUTC - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds;
	}

	public virtual void SetupBoosterTooltip(TooltipTextInfo tooltipTextInfo)
	{
		tooltipTextInfo.Name.Value = Name;
		tooltipTextInfo.Desc.Value = Desc;
		if (!DisplayRemainTime())
		{
			tooltipTextInfo.TypeText.Reset();
			return;
		}
		int remainSeconds = GetRemainSeconds();
		if (remainSeconds > 0)
		{
			tooltipTextInfo.TypeText.Assign(new BoosterTimeRemainedBuilder(remainSeconds, appendLine: false));
		}
		else
		{
			tooltipTextInfo.TypeText.Reset();
		}
	}

	public bool CheckEqualForModification(BoosterInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (Type != other.Type)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(ExtraBonusTypes) != __Extension_Method_ICollection__.COUNT(other.ExtraBonusTypes))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ExtraBonusTypes); i++)
		{
			if (ExtraBonusTypes[i] != other.ExtraBonusTypes[i])
			{
				return false;
			}
		}
		if (BonusType != other.BonusType)
		{
			return false;
		}
		if (ItemCode != other.ItemCode)
		{
			return false;
		}
		if (Value != other.Value)
		{
			return false;
		}
		if (Name != other.Name)
		{
			return false;
		}
		if (Desc != other.Desc)
		{
			return false;
		}
		if (EndTimeUTC != other.EndTimeUTC)
		{
			return false;
		}
		if (BeginHour != other.BeginHour)
		{
			return false;
		}
		if (BeginMinute != other.BeginMinute)
		{
			return false;
		}
		if (EndHour != other.EndHour)
		{
			return false;
		}
		if (EndMinute != other.EndMinute)
		{
			return false;
		}
		return true;
	}

	public virtual bool IsShowAtBattle()
	{
		return true;
	}
}
