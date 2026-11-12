using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroRunehengeIconSlotComponent : LinkFactoryComponent, InitOnCreate
{
	public static string PrefabPath = "Resources/Prefab2/Component/Rune/HeroRunehengeIconSlotComponent";

	public static PrefabAuto<HeroRunehengeIconSlotComponent> PrefabAuto = new PrefabAuto<HeroRunehengeIconSlotComponent>(PrefabPath);

	public UISpriteSelector SpriteSelecter;

	public void Refresh(RuneColorType color)
	{
		if (!(SpriteSelecter == null))
		{
			if (color == RuneColorType.None)
			{
				UIHelper.EnableGameObject(SpriteSelecter, enable: false);
				return;
			}
			UIHelper.EnableGameObject(SpriteSelecter, enable: true);
			SpriteSelecter.SelectEnum(color);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
