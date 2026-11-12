using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroClassComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnCreatureTagTypeClick(CreatureTagType tagType);
	}

	public const string PrefabPath = "Prefab2/Component/IllustratedBook/HeroClassComponent";

	public static PrefabAuto<HeroClassComponent> PrefabAuto = new PrefabAuto<HeroClassComponent>("Prefab2/Component/IllustratedBook/HeroClassComponent");

	private Listener _Listener;

	public UIToggle TagTypeToggle;

	public UISpriteSelector IconSpriteSelector;

	public UILabel TitleLabel;

	public UISpriteSelector EventTypeSpriteSelector;

	public CreatureTagType TagType { get; protected set; }

	public void Init(CreatureTagType tagType, Listener listener)
	{
		TagType = tagType;
		_Listener = listener;
		if (TagType == CreatureTagType.None)
		{
			string text = LocalizationManager.GetString(Hero.HERO_DECK_TOTAL);
			UIHelper.SetLabelText(TitleLabel, text);
		}
		else
		{
			UIHelper.SetLabelText(TitleLabel, TagType);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public virtual void OnClickToggle()
	{
		if (!(TagTypeToggle == null) && TagTypeToggle.value && _Listener != null)
		{
			_Listener.OnCreatureTagTypeClick(TagType);
		}
	}

	public void SelectToggle()
	{
		TagTypeToggle.value = true;
	}
}
