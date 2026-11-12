using System.Runtime.CompilerServices;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PortalRenewalData : IDataKeySelector<PortalRenewalDataKey>
{
	[CompilerGenerated]
	private PortalRenewalCompType _003CSubComponentShape_003Ek__BackingField;

	PortalRenewalDataKey IDataKeySelector<PortalRenewalDataKey>.Key => new PortalRenewalDataKey(Category, SubCategory);

	[Key(0)]
	public PortalRenewalCategory Category { get; set; }

	[Key(1)]
	public PortalRenewalSubCategory SubCategory { get; set; }

	[Key(2)]
	public int MenuShowIndex { get; set; }

	[Key(3)]
	public int ContentsShowIndex { get; set; }

	[Key(4)]
	public bool Pined { get; set; }

	[Key(5)]
	public string MenuButtonLocalkey { get; set; }

	[Key(6)]
	public string TitleLocalKey { get; set; }

	[Key(7)]
	public PortalRenewalCompType SubComponentShape
	{
		[CompilerGenerated]
		get
		{
			return SprkPetPresentation.Shape(_003CSubComponentShape_003Ek__BackingField, this);
		}
		[CompilerGenerated]
		set
		{
			_003CSubComponentShape_003Ek__BackingField = value;
		}
	}

	[Key(8)]
	public string MainImagePath { get; set; }

	[Key(9)]
	public CurrencyShowType CurrencyShowType { get; set; }

	[Key(10)]
	public CurrencyType CurrencyType { get; set; }

	[Key(11)]
	public string[] CurrencyParam { get; set; }

	[Key(12)]
	public int ShopIndex { get; set; }

	[Key(13)]
	public string[] ShopParam { get; set; }

	[Key(14)]
	public string DescLocalKey { get; set; }

	[Key(15)]
	public string[] OpenCondition { get; set; }

	[Key(16)]
	public string[] ShowCondition { get; set; }

	[Key(17)]
	public int TutorialIndex { get; set; }

	[Key(18)]
	public string OpenConditionLocalKey { get; set; }

	[Key(19)]
	public string[] EventBuffCondition { get; set; }

	[Key(20)]
	public ClientActivityType MoveTaskType { get; set; }

	[Key(21)]
	public string[] MoveTaskValue { get; set; }

	[Key(22)]
	public ContentsStateIcon[] ContentsStateIcon { get; set; }

	[Key(23)]
	public int[] AdventureNoticeMove { get; set; }

	[Key(24)]
	public string VisualChapterIndex { get; set; }
}
