using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroViewAnimationComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void PlayAnimation(HeroViewAnimationComponent component);

		void PlayCinema(HeroViewAnimationComponent component);
	}

	private enum EMode
	{
		Animation,
		Cinema
	}

	public const string PrefabPath = "Prefab2/Component/Hero/HeroViewAnimationComponent";

	public static PrefabAuto<HeroViewAnimationComponent> PrefabAuto = new PrefabAuto<HeroViewAnimationComponent>("Prefab2/Component/Hero/HeroViewAnimationComponent");

	public const string PetPrefabPath = "Prefab2/Component/Pet/PetViewAnimationComponent";

	public static PrefabAuto<HeroViewAnimationComponent> PetPrefabAuto = new PrefabAuto<HeroViewAnimationComponent>("Prefab2/Component/Pet/PetViewAnimationComponent");

	public UILabel LabelName;

	public UISprite SpriteSelected;

	public UISprite SpriteIcon;

	private HeroAnimationViewData _heroAnimationViewData;

	private int _sequenceIndex;

	private Listener _Listener;

	private EMode _mode;

	private GetNewHero2.EOpenMode _openCinemaMode;

	public HeroAnimationViewData HeroAnimationViewData => _heroAnimationViewData;

	public int SequenceIndex => _sequenceIndex;

	public GetNewHero2.EOpenMode OpenCinemaMode => _openCinemaMode;

	public GetNewHeroSceneBundleData GetNewHeroSceneBundleData { get; private set; }

	public void Open(Listener listener, int sequenceIndex, HeroAnimationViewData heroAnimationViewData, CachedHeroInfo heroInfo, bool selected = false)
	{
		_mode = EMode.Animation;
		_Listener = listener;
		_sequenceIndex = sequenceIndex;
		_heroAnimationViewData = heroAnimationViewData;
		if (SpriteIcon != null)
		{
			SpriteIcon.spriteName = _heroAnimationViewData.IconType;
		}
		string text = string.Empty;
		string text2 = "SKILL_NAME_";
		string animationText = _heroAnimationViewData.AnimationText;
		if (animationText.Contains(text2))
		{
			int key = Vespa_String_Extension_Method.TOINT(animationText.Substring(text2.Length), 0);
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(key);
			if (data != null)
			{
				text = ((heroInfo == null) ? LocalizationManager.GetStringDirect(animationText) : SkillDescContainer.GetSkillName(heroInfo, data));
			}
		}
		else
		{
			text = LocalizationManager.GetStringDirect(animationText);
		}
		UIHelper.SetLabelText(LabelName, text);
		SetSelected(selected);
	}

	public void Open(Listener listener, GetNewHero2.EOpenMode openMode, GetNewHeroSceneBundleData getNewHeroBundleData)
	{
		_mode = EMode.Cinema;
		_Listener = listener;
		_openCinemaMode = openMode;
		GetNewHeroSceneBundleData = getNewHeroBundleData;
		string key = string.Empty;
		switch (openMode)
		{
		case GetNewHero2.EOpenMode.Awaken:
			key = "HERO_GET_MOVIE";
			break;
		case GetNewHero2.EOpenMode.Transcend:
			key = "HERO_TRANSCEND_MOVIE";
			break;
		case GetNewHero2.EOpenMode.LimitBreak:
			key = "HERO_LIMIT_BREAK_MOVIE";
			break;
		case GetNewHero2.EOpenMode.NewHero:
			key = ((GetNewHeroSceneBundleData != null) ? GetNewHeroSceneBundleData.Name : string.Empty);
			break;
		}
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.SetLabelText(LabelName, stringDirect);
		if (SpriteIcon != null)
		{
			SpriteIcon.spriteName = "Icon_Cutscene";
		}
	}

	private void Reset()
	{
	}

	void InitOnCreate.OnCreate()
	{
		Reset();
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	public void OnClick()
	{
		if (_Listener != null)
		{
			if (_mode == EMode.Animation)
			{
				_Listener.PlayAnimation(this);
			}
			else if (_mode == EMode.Cinema)
			{
				_Listener.PlayCinema(this);
			}
		}
	}

	public void SetSelected(bool selected)
	{
		UIHelper.EnableGameObject(SpriteSelected, selected);
	}
}
