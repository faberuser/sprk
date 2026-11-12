using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NPortrait;

public sealed class MonsterPortraitComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate, IMonsterPortraitProvider
{
	public new interface IListener
	{
		void OnClickBossViewInfo(IMonsterPortraitProvider iProvider);

		void OnClickChapterBook(int ChapterIndex);

		void OnClickChapterMonsterInfo(NormalMonsterArchiveData MonArchiveData);
	}

	public struct MonsterPortraitInfo
	{
		public MonsterHeroInfo MonsterHeroInfo;

		public IListener IListener;

		public string PreviewBg;

		public CreatureData CreatureData;
	}

	public const string PortraitPrefabPath = "Prefab2/Component/Portrait/MonsterPortrait";

	public static PrefabAuto<MonsterPortraitComponent> PortraitPrefabAuto = new PrefabAuto<MonsterPortraitComponent>("Prefab2/Component/Portrait/MonsterPortrait");

	public const string SmallPrefabPath = "Prefab2/Component/Portrait/MonsterPortraitSmall";

	public static PrefabAuto<MonsterPortraitComponent> SmallPrefabAuto = new PrefabAuto<MonsterPortraitComponent>("Prefab2/Component/Portrait/MonsterPortraitSmall");

	public UIWidget ThisWidget;

	public UIBundleLoader PortraitBundleTex;

	public UILabel NamePrefixLabel;

	public UILabel NameLabel;

	public UILabel DifficultyLabel;

	public int ChapterIndex;

	private IListener _listener;

	private HeroTooltipTextInfo _tooltipTextInfo;

	private bool _disableCatch;

	private ArchiveType _curArchiveType;

	private string _previewBg = string.Empty;

	public MonsterHeroInfo Info { get; private set; }

	public CreatureData TableData { get; private set; }

	public NormalMonsterArchiveData MonsterArchiveData { get; private set; }

	MonsterHeroInfo IMonsterPortraitProvider.MonsterHeroInfo => Info;

	string IMonsterPortraitProvider.PreviewBg => _previewBg;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void OpenChapter(int chapterIndex = 0, IListener listener = null, bool bDisableCatch = false)
	{
		_listener = listener;
		_disableCatch = bDisableCatch;
		_curArchiveType = ArchiveType.Normal;
		Info = null;
		ChapterIndex = chapterIndex + 1;
		UIHelper.EnableGameObject(NamePrefixLabel, enable: false);
		UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect($"COMMON.CHAPTER{ChapterIndex}"));
		UIHelper.SetLabelText(DifficultyLabel, "");
		string chapterPortraitPath = UIHelper.GetChapterPortraitPath(ChapterIndex);
		UIHelper.SetBundleTexture(PortraitBundleTex, chapterPortraitPath);
		SetupStatus();
	}

	public void OpenEtcDungeon(int chapterIndex, IListener listener = null, bool bDisableCatch = false)
	{
		_listener = listener;
		_disableCatch = bDisableCatch;
		_curArchiveType = ArchiveType.Normal;
		Info = null;
		ChapterIndex = chapterIndex + 1;
		UIHelper.EnableGameObject(NamePrefixLabel, enable: false);
		UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect("COMMON_ETC"));
		UIHelper.SetLabelText(DifficultyLabel, "");
		string assetPath = "Portrait/Dictionary/Field/ChapterEtc";
		UIHelper.SetBundleTexture(PortraitBundleTex, assetPath);
		SetupStatus();
	}

	public void Open(ArchiveType archiveType, MonsterPortraitInfo? monsterPortrait, bool bDisableCatch, int Difficulty)
	{
		if (monsterPortrait.HasValue)
		{
			_listener = monsterPortrait.Value.IListener;
			_disableCatch = bDisableCatch;
			_curArchiveType = archiveType;
			Info = monsterPortrait.Value.MonsterHeroInfo;
			ChapterIndex = 0;
			if (Info != null)
			{
				TableData = monsterPortrait.Value.CreatureData;
				_previewBg = monsterPortrait.Value.PreviewBg;
				SetupName();
				SetupPortrait();
				SetupStatus();
				SetupDifficultyLabel(Difficulty);
			}
		}
	}

	public void Open(ArchiveType archiveType, NormalMonsterArchiveData monsterArchiveData, IListener listener, bool bDisableCatch)
	{
		_listener = listener;
		_disableCatch = bDisableCatch;
		_curArchiveType = archiveType;
		MonsterArchiveData = monsterArchiveData;
		Info = monsterArchiveData.GetMainMonsterInfo();
		ChapterIndex = 0;
		if (Info != null)
		{
			TableData = GameRoot.Instance.CreatureDataContainer.GetData(MonsterArchiveData.MonsterIndex);
			SetupPortrait();
			SetupStatus();
		}
	}

	private void SetupName()
	{
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(TableData, Info.VisualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		UIHelper.SetLabelText(NameLabel, creatureName);
		UIHelper.EnableGameObject(NamePrefixLabel, !String_Extension_Method.IsNullOrEmpty(creatureNamePrefix));
		if (!String_Extension_Method.IsNullOrEmpty(creatureNamePrefix))
		{
			UIHelper.SetLabelText(NamePrefixLabel, creatureNamePrefix);
		}
		UIHelper.SetLabelText(NamePrefixLabel, creatureNamePrefix);
	}

	private void SetupPortrait(string portraitPath = "")
	{
		if (String_Extension_Method.IsNullOrEmpty(portraitPath))
		{
			if (TableData == null)
			{
				return;
			}
			string heroCode = ((!String_Extension_Method.IsNullOrEmpty(TableData.CodeName)) ? TableData.CodeName : string.Empty);
			portraitPath = UIHelper.GetBossBigPortraitPath(heroCode);
			if (_curArchiveType == ArchiveType.Normal)
			{
				portraitPath = UIHelper.GetHeroIconPath(heroCode, 0);
			}
		}
		UIHelper.SetBundleTexture(PortraitBundleTex, portraitPath);
	}

	private void SetupStatus()
	{
		if (ThisWidget != null && ThisWidget.isGrayscale != _disableCatch)
		{
			ThisWidget.isGrayscale = _disableCatch;
			UIHelper.EnableGameObject(ThisWidget, enable: false);
			UIHelper.EnableGameObject(ThisWidget, enable: true);
		}
	}

	public void SetupDifficultyLabel(int diffLocale)
	{
		string difficultyLocale = SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.GetDifficultyLocale(_curArchiveType, diffLocale);
		UIHelper.SetLabelText(DifficultyLabel, difficultyLocale);
	}

	public void ChangeMonsterInfo(MonsterHeroInfo info)
	{
		Info = info;
	}

	public void OnClickPortrait()
	{
		if (_listener != null)
		{
			if (_curArchiveType == ArchiveType.Normal)
			{
				_listener.OnClickChapterBook(ChapterIndex);
			}
			else
			{
				_listener.OnClickBossViewInfo(this);
			}
		}
	}

	public void OnClickSmallPortrait()
	{
		if (_listener != null)
		{
			_listener.OnClickChapterMonsterInfo(MonsterArchiveData);
		}
	}
}
