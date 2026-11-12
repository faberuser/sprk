using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroPortraitTextureName : UIScrollItemBehaviour
{
	public static string PrefabPath = "Prefab2/Component/Portrait/HeroPortraitTextureName";

	public static PrefabAuto<HeroPortraitTextureName> PrefabAuto = new PrefabAuto<HeroPortraitTextureName>(PrefabPath);

	public UITexture HeroImage;

	public UIBundleLoader BundleTexHero;

	public UILabel LabelHeroName;

	public UILabel LabelHeroLevel;

	public int HeroIndex { get; private set; }

	public int HeroStar { get; private set; }

	public int HeroLevel { get; private set; }

	public bool PrintName { get; private set; }

	public int CoustumeIndex { get; private set; }

	public bool Open(int heroIndex)
	{
		HeroIndex = heroIndex;
		HeroStar = 3;
		HeroLevel = 0;
		PrintName = true;
		SetupInfo();
		return true;
	}

	public void Open(int heroIndex, int heroStar, int heroLevel, bool printName = true, int costumeIndex = 0)
	{
		HeroIndex = heroIndex;
		HeroStar = heroStar;
		HeroLevel = heroLevel;
		PrintName = printName;
		CoustumeIndex = costumeIndex;
		SetupInfo();
	}

	private void SetupInfo()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
		if (data != null)
		{
			string heroIconPath = UIHelper.GetHeroIconPath(data.Index, HeroStar, "", CoustumeIndex);
			UIHelper.SetBundleTexture(BundleTexHero, heroIconPath);
			UIHelper.EnableGameObject(LabelHeroLevel, HeroLevel != 0);
			UIHelper.EnableGameObject(LabelHeroName, PrintName && HeroLevel == 0);
			UIHelper.SetLabelText(LabelHeroLevel, "Lv. " + HeroLevel);
			UIHelper.SetLabelText(LabelHeroName, data.Name);
		}
	}
}
