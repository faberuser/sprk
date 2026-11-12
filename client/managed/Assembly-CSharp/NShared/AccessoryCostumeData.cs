using System.Runtime.CompilerServices;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AccessoryCostumeData : IDataKeySelector<int>, IPayShopEventSearchInfo
{
	[CompilerGenerated]
	private int _003CReqBuyGem_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CIsOpen_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CIsBuy_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CPreviewableWhenOwned_003Ek__BackingField;

	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Code { get; set; }

	[Key(2)]
	public int[] UnableHero { get; set; }

	[Key(3)]
	public AccessoryPartType PartType { get; set; }

	[Key(4)]
	public AccessoryPartPivotType PivotType { get; set; }

	[Key(7)]
	public string NameKey { get; set; }

	[Key(8)]
	public string ProductDescKey { get; set; }

	[Key(9)]
	public string Category { get; set; }

	[Key(10)]
	public string Group { get; set; }

	[Key(11)]
	public int ReqBuyGem
	{
		[CompilerGenerated]
		get
		{
			if (!IsBuy && PreviewableWhenOwned && _003CReqBuyGem_003Ek__BackingField == 10000)
			{
				if (PartType == AccessoryPartType.Head)
				{
					return 1000;
				}
				if (PartType == AccessoryPartType.Eye)
				{
					return 500;
				}
				if (PartType == AccessoryPartType.Nose)
				{
					return 500;
				}
				if (PartType == AccessoryPartType.Body)
				{
					return 4000;
				}
			}
			return _003CReqBuyGem_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003CReqBuyGem_003Ek__BackingField = value;
		}
	}

	[Key(12)]
	public int Mileage { get; set; }

	[Key(13)]
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

	[Key(14)]
	public bool IsBuy
	{
		[CompilerGenerated]
		get
		{
			return true;
		}
		[CompilerGenerated]
		set
		{
			_003CIsBuy_003Ek__BackingField = value;
		}
	}

	[Key(15)]
	public byte New { get; set; }

	[Key(16)]
	public string ProductImagePath { get; set; }

	[Key(17)]
	public string IconPortraitPath { get; set; }

	[Key(18)]
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

	[Key(19)]
	public int OpenDate { get; set; }

	[Key(20)]
	public bool IsShow { get; set; }

	[IgnoreMember]
	public string Name => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Name", BuildLocalize_Name);

	[IgnoreMember]
	public string ProductDesc => GameRoot.LocalizeProxy.GetCache_or_Build(this, "ProductDesc", BuildLocalize_ProductDesc);

	PayShopCategory IPayShopEventSearchInfo.PayShopCategory => PayShopCategory.Accessory;

	int IPayShopEventSearchInfo.Index => Index;

	string IPayShopEventSearchInfo.Group => Group;

	private string BuildLocalize_Name()
	{
		return GameRoot.LocalizeProxy.GetString(NameKey);
	}

	private string BuildLocalize_ProductDesc()
	{
		return GameRoot.LocalizeProxy.GetString(ProductDescKey);
	}
}
