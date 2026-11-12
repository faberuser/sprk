using NGame2.NUI.NDescription;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ArchiveTranscendDescComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Prefab2/Component/IllustratedBook/ArchiveTranscendDescComponent";

	public static PrefabAuto<ArchiveTranscendDescComponent> PrefabAuto = new PrefabAuto<ArchiveTranscendDescComponent>("Prefab2/Component/IllustratedBook/ArchiveTranscendDescComponent");

	public UILabel LabelName;

	public UILabel LabelDesc;

	public UISimpleGrid IconGrid;

	private HeroInfo _heroInfo;

	private HeroTranscendSkillIconComponent iconComponent;

	private Listener _Listener;

	public void Open(Listener listener, HeroInfo heroInfo, int transcended, int skillIndex, CreatureData.TranscendSkill skill)
	{
		_Listener = listener;
		_heroInfo = heroInfo;
		Setup(transcended, skillIndex, skill);
	}

	public void Change(int transcended, int skillIndex, CreatureData.TranscendSkill skill)
	{
		Setup(transcended, skillIndex, skill);
	}

	private void Setup(int transcended, int skillIndex, CreatureData.TranscendSkill skill)
	{
		Reset();
		SetupIcon(transcended, skillIndex);
		SetupName(skill);
		SetupDesc(skill);
	}

	private void SetupIcon(int transcended, int skillIndex)
	{
		iconComponent = UIHelper.AddGridItem(IconGrid, HeroTranscendSkillIconComponent.PrefabAuto);
		CreatureData.TranscendSkill transcendSkillData = _heroInfo.CreatureData.GetTranscendSkillData(transcended, skillIndex);
		if (transcendSkillData != null)
		{
			iconComponent.Open(_heroInfo, 0, transcendSkillData, null, readOnly: true);
		}
	}

	private void SetupName(CreatureData.TranscendSkill skill)
	{
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skill.TranscendSkillIndex);
		string skillName = SkillDescContainer.GetSkillName(_heroInfo, data);
		UIHelper.SetLabelText(LabelName, skillName);
	}

	private void SetupDesc(CreatureData.TranscendSkill skill)
	{
		SkillDescBuilder skillDescBuilder = new SkillDescBuilder(skill.TranscendSkillIndex, _heroInfo, appendLine: false);
		UIHelper.SetLabelText(LabelDesc, skillDescBuilder.Build());
	}

	public void Reset()
	{
		FactoryBase.SafeReleaseComponent(ref iconComponent);
	}

	void InitOnCreate.OnCreate()
	{
		Reset();
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}
}
