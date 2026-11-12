using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HairCostumeData : IDataKeySelector<int>, IPayShopEventSearchInfo
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int HeroIndex { get; set; }

	[Key(2)]
	public int Star { get; set; }

	[Key(5)]
	public string Group { get; set; }

	[Key(6)]
	public string NameKey { get; set; }

	[Key(7)]
	public string DescKey { get; set; }

	[Key(8)]
	public string HairPrefix { get; set; }

	[Key(9)]
	public int[] NeedCostume { get; set; }

	[Key(10)]
	public int ReqBuyGem { get; set; }

	[Key(11)]
	public int Mileage { get; set; }

	[Key(12)]
	public bool IsDefault { get; set; }

	[Key(13)]
	public bool IsOpen { get; set; }

	[Key(14)]
	public bool PreviewableWhenNeedCostumeOwned { get; set; }

	[Key(15)]
	public bool PreviewableWhenNeedHairOwned { get; set; }

	[Key(16)]
	public string PortraitPath { get; set; }

	[Key(17)]
	public bool IsNew { get; set; }

	[Key(18)]
	public bool IsShow { get; set; }

	[IgnoreMember]
	public string Name => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Name", BuildLocalize_Name);

	[IgnoreMember]
	public string Desc => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Desc", BuildLocalize_Desc);

	PayShopCategory IPayShopEventSearchInfo.PayShopCategory => PayShopCategory.Accessory;

	int IPayShopEventSearchInfo.Index => Index;

	string IPayShopEventSearchInfo.Group => Group;

	private string BuildLocalize_Name()
	{
		return GetCheckLocalizeHeroName(NameKey);
	}

	private string BuildLocalize_Desc()
	{
		return GetCheckLocalizeHeroName(DescKey);
	}

	private string GetCheckLocalizeHeroName(string localKey)
	{
		string text = GameRoot.LocalizeProxy.GetString(localKey, null);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
		if (data == null)
		{
			return text;
		}
		string name = data.Name;
		CostumeData costumeData = ((NeedCostume == null) ? null : GameRoot.Instance.CostumeDataContainer.GetData(NeedCostume[0]));
		if (costumeData == null)
		{
			return string.Format(text, "", name, "");
		}
		string costumeName = costumeData.CostumeName;
		return string.Format(text, "", name, costumeName);
	}
}
