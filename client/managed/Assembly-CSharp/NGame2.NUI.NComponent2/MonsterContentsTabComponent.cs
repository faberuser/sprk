using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class MonsterContentsTabComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnArchiveTypeClick(ArchiveType tagType);
	}

	public const string PrefabPath = "Prefab2/Component/IllustratedBook/MonsterContentsTabComponent";

	public static PrefabAuto<MonsterContentsTabComponent> PrefabAuto = new PrefabAuto<MonsterContentsTabComponent>("Prefab2/Component/IllustratedBook/MonsterContentsTabComponent");

	private Listener _Listener;

	public UIToggle TagTypeToggle;

	public UILabel TitleLabel;

	private string[] ARCHIVE_TYPE_LOCALE = new string[14]
	{
		"BROADCAST.BROADCAST_TAG_ALL", "ARCHIVE_RAID", "ARCHIVE_CHALLENGERAID", "ARCHIVE_GUILDRAID", "ARCHIVE_WORLDBOSS", "ARCHIVE_CHAPTER_BOSS", "ARCHIVE_CONQUEST", "ARCHIVE_NORMAL_MONSTER", "GUILD_PUNITIVE_EXPEDITION_TITLE", "ARCHIVE_GODKING",
		"ARCHIVE_ECLIPSE", "ARCHIVE_SHAKMEH", "ARCHIVE_TECHNOMAGIC", "PUNISHMENTRAID_TITLE"
	};

	public ArchiveType TagType { get; protected set; }

	public void Init(ArchiveType tagType, Listener listener)
	{
		TagType = tagType;
		_Listener = listener;
		string key = (Vespa_Generic_IList_Extension_Method.IsValidIndex(ARCHIVE_TYPE_LOCALE, (int)tagType) ? ARCHIVE_TYPE_LOCALE[(int)tagType] : "");
		UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect(key));
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
			_Listener.OnArchiveTypeClick(TagType);
		}
	}

	public void SelectToggle(bool On)
	{
		TagTypeToggle.Set(On);
	}
}
