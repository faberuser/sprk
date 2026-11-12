using NGame2.NUI.NHelper.NPayShop;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopCostumeHeroComponent : PayShopBaseComponent<CreatureData>
{
	public enum FlagType
	{
		New,
		Sale
	}

	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopCostumeHeroComponent";

	public static PrefabAuto<PayShopCostumeHeroComponent> PrefabAuto = new PrefabAuto<PayShopCostumeHeroComponent>("Resources/Prefab2/Component/PayShop2/PayShopCostumeHeroComponent");

	public UIBundleLoader PortraitBundleTex;

	public UILabel Label_HeroName;

	public UISpriteSelector SpriteSelectorTag;

	public UISpriteSelector Sprite_Flag;

	private EHeroCostumeType _heroCostumeType;

	public override void Open(CreatureData creatureData, IListener listener)
	{
		base.Open(creatureData, listener);
	}

	public void InitHeroCostumeType(EHeroCostumeType heroCostumeType)
	{
		_heroCostumeType = heroCostumeType;
		Init(_data);
	}

	protected override void Init(CreatureData creatureData)
	{
		UIHelper.SetLabelText(Label_HeroName, creatureData.Name);
		CostumeData[] buyableCostumeByHero = PayShopHelper.GetBuyableCostumeByHero(creatureData.Index, _heroCostumeType);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(buyableCostumeByHero))
		{
			return;
		}
		CostumeData costumeData = null;
		CostumeGroupData costumeGroupData = null;
		int num = 0;
		CostumeData[] array = buyableCostumeByHero;
		foreach (CostumeData costumeData2 in array)
		{
			CostumeGroupData dataByGroupName = GameRoot.Instance.CostumeGroupDataContainer.GetDataByGroupName(costumeData2.Group);
			if (dataByGroupName != null && num < dataByGroupName.CostumeGroupIndex)
			{
				num = dataByGroupName.CostumeGroupIndex;
				costumeData = costumeData2;
				costumeGroupData = dataByGroupName;
			}
		}
		string portraitPath = costumeData.PortraitPath;
		PortraitBundleTex.SetAssetPath(portraitPath);
		if (SpriteSelectorTag != null)
		{
			SpriteSelectorTag.Select(creatureData.TagType.ToString());
		}
		bool flag = costumeData.New > 0;
		CostumeGroupData newestData = GameRoot.Instance.CostumeGroupDataContainer.GetNewestData(costumeData.CostumeType);
		if (newestData != null && costumeGroupData != null && newestData.CostumeGroupIndex == costumeGroupData.CostumeGroupIndex && flag)
		{
			UIHelper.EnableGameObject(Sprite_Flag, enable: true);
			Sprite_Flag.Select(FlagType.New.ToString());
		}
		else
		{
			UIHelper.EnableGameObject(Sprite_Flag, enable: false);
		}
	}

	public override void ReleaseComponent()
	{
		_heroCostumeType = EHeroCostumeType.None;
	}
}
