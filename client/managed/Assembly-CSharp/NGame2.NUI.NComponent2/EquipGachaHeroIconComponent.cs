using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class EquipGachaHeroIconComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickHeroIcon(int heroIndex);
	}

	public const string PrefabPath = "Prefab2/Component/ShopItem/EquipGachaHeroIconComponent";

	public static PrefabAuto<EquipGachaHeroIconComponent> PrefabAuto = new PrefabAuto<EquipGachaHeroIconComponent>("Prefab2/Component/ShopItem/EquipGachaHeroIconComponent");

	public UIBundleLoader BundleTex_HeroIcon;

	public UISprite SpriteSelectImage;

	private CreatureData _creatureData;

	private IListener _listener;

	public int HeroIndex
	{
		get
		{
			if (_creatureData == null)
			{
				return 0;
			}
			return _creatureData.Index;
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_listener = null;
		_creatureData = null;
		UIHelper.SetBundleTexture(BundleTex_HeroIcon, string.Empty);
		Select(isSelect: false);
	}

	public void Open(CreatureData creature, IListener listener = null)
	{
		if (creature != null)
		{
			_creatureData = creature;
			_listener = listener;
			string heroIconPath = UIHelper.GetHeroIconPath(creature.Index, ignoreAccount: true);
			UIHelper.SetBundleTexture(BundleTex_HeroIcon, heroIconPath);
		}
	}

	public void OnClickIcon()
	{
		if (_listener != null)
		{
			_listener.OnClickHeroIcon(_creatureData.Index);
		}
	}

	public void Select(bool isSelect)
	{
		UIHelper.EnableGameObject(SpriteSelectImage, isSelect);
	}
}
