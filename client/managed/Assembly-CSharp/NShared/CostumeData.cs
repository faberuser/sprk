using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CostumeData : IDataKeySelector<int>, IPayShopEventSearchInfo
{
	[CompilerGenerated]
	private bool _003CIsOpen_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CPreviewableWhenOwned_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CIsShow_003Ek__BackingField;

	[Key(0)]
	public int CostumeIndex { get; set; }

	[Key(1)]
	public string CostumeCode { get; set; }

	[Key(2)]
	public string Group { get; set; }

	[Key(3)]
	public EEquipCondition EquipCondition { get; set; }

	[Key(4)]
	public int HeroIndex { get; set; }

	[Key(5)]
	public int HeroStar { get; set; }

	[Key(6)]
	public int HeroTranscended { get; set; }

	[Key(8)]
	public string ReplaceNameKey { get; set; }

	[Key(9)]
	public string[] NameParam { get; set; }

	[Key(11)]
	public string ReplaceObtainKey { get; set; }

	[Key(12)]
	public string[] ObtainParam { get; set; }

	[Key(13)]
	public int ReqBuyGem { get; set; }

	[Key(14)]
	public int ReqBuyGold { get; set; }

	[Key(15)]
	public int ReqBuyMileage { get; set; }

	[Key(16)]
	public int Mileage { get; set; }

	[Key(17)]
	public bool IsOpen
	{
		[CompilerGenerated]
		get
		{
			return true;
		}
		[CompilerGenerated]
		set
		{
			_003CIsOpen_003Ek__BackingField = value;
		}
	}

	[Key(18)]
	public bool IsBuy { get; set; }

	[Key(19)]
	public bool IsDefault { get; set; }

	[Key(20)]
	public bool PreviewableWhenOwned
	{
		[CompilerGenerated]
		get
		{
			return false;
		}
		[CompilerGenerated]
		set
		{
			_003CPreviewableWhenOwned_003Ek__BackingField = value;
		}
	}

	[Key(21)]
	public string BodyPrefix { get; set; }

	[Key(22)]
	public string WeaponPrefix { get; set; }

	[Key(23)]
	public string PortraitPath { get; set; }

	[Key(24)]
	public string IconPortraitPath { get; set; }

	[Key(25)]
	public string BattlePortraitPath { get; set; }

	[Key(26)]
	public string VoicePostfix { get; set; }

	[Key(27)]
	public byte New { get; set; }

	[Key(28)]
	public byte Hit { get; set; }

	[Key(29)]
	public CostumeAbilityType AbilityType1 { get; set; }

	[Key(30)]
	public string[] AbilityValue1 { get; set; }

	[Key(31)]
	public CostumeAbilityType AbilityType2 { get; set; }

	[Key(32)]
	public string[] AbilityValue2 { get; set; }

	[Key(33)]
	public CostumeAbilityType AbilityType3 { get; set; }

	[Key(34)]
	public string[] AbilityValue3 { get; set; }

	[Key(35)]
	public string IllustPath { get; set; }

	[Key(36)]
	public int MainIndex { get; set; }

	[Key(37)]
	public int[] BonusCostumeIndices { get; set; }

	[Key(38)]
	public int OpenDate { get; set; }

	[Key(39)]
	public EHeroCostumeType CostumeType { get; set; }

	[Key(40)]
	public int ProductIndex { get; set; }

	[Key(41)]
	public string BattlePortraitEffectPath { get; set; }

	[Key(42)]
	public bool IsShow
	{
		[CompilerGenerated]
		get
		{
			return true;
		}
		[CompilerGenerated]
		set
		{
			_003CIsShow_003Ek__BackingField = value;
		}
	}

	[IgnoreMember]
	public string CostumeName => GameRoot.LocalizeProxy.GetCache_or_Build(this, "CostumeName", BuildLocalize_CostumeName);

	[IgnoreMember]
	public string ObtainDescription => GameRoot.LocalizeProxy.GetCache_or_Build(this, "ObtainDescription", BuildLocalize_ObtainDescription);

	int IDataKeySelector<int>.Key => CostumeIndex;

	PayShopCategory IPayShopEventSearchInfo.PayShopCategory => PayShopCategory.Costume;

	int IPayShopEventSearchInfo.Index => CostumeIndex;

	string IPayShopEventSearchInfo.Group => Group;

	private string NAME_KEY => "COSTUME_NAME_" + CostumeIndex;

	private string OBTAIN_KEY => "COSTUME_OBTAIN_DESC_" + CostumeIndex;

	private string BuildLocalize_CostumeName()
	{
		return GameRoot.LocalizeProxy.GetStringWithReplaceKey(NAME_KEY, ReplaceNameKey, GetCreatureName());
	}

	private string BuildLocalize_ObtainDescription()
	{
		return GameRoot.LocalizeProxy.GetStringWithReplaceKey(OBTAIN_KEY, ReplaceObtainKey, GetCreatureName());
	}

	private string GetCreatureName()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
		if (data == null)
		{
			return string.Empty;
		}
		return data.Name;
	}

	public bool IsFreeCostume()
	{
		if (ReqBuyGem == 0 && ReqBuyGold == 0)
		{
			return ReqBuyMileage == 0;
		}
		return false;
	}

	public bool IsCoupleCostume()
	{
		if (BonusCostumeIndices != null)
		{
			return BonusCostumeIndices.Length != 0;
		}
		return false;
	}

	public bool IsBonusCostume()
	{
		if (IsCoupleCostume())
		{
			return MainIndex != CostumeIndex;
		}
		return false;
	}

	public bool IsLegendCostume()
	{
		return CostumeType == EHeroCostumeType.Legend;
	}

	public List<CostumeAbilityInfo> GetCostumeAbilities()
	{
		List<CostumeAbilityInfo> list = new List<CostumeAbilityInfo>();
		if (AbilityType1 != CostumeAbilityType.None)
		{
			list.Add(new CostumeAbilityInfo
			{
				Type = AbilityType1,
				Value = AbilityValue1
			});
		}
		if (AbilityType2 != CostumeAbilityType.None)
		{
			list.Add(new CostumeAbilityInfo
			{
				Type = AbilityType2,
				Value = AbilityValue2
			});
		}
		if (AbilityType3 != CostumeAbilityType.None)
		{
			list.Add(new CostumeAbilityInfo
			{
				Type = AbilityType3,
				Value = AbilityValue3
			});
		}
		return list;
	}
}
