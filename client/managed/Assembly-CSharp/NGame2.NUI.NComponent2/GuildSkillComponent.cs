using NGame2.NGuild;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class GuildSkillComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickLevelUp(GuildSkillLevelData skillLevelData);
	}

	public const string PrefabPath = "Prefab2/Component/Guild/GuildSkillComponent";

	public static PrefabAuto<GuildSkillComponent> PrefabAuto = new PrefabAuto<GuildSkillComponent>("Prefab2/Component/Guild/GuildSkillComponent");

	public UILabel Label_SkillDesc;

	public UILabel Label_ScoreLevelUp;

	public UIButton Button_LevelUp;

	public UISimpleGrid SkillIconGrid;

	private GuildSkillIconComponent _skillIconComp;

	private Listener _listener;

	public GuildSkillData SkillData { get; private set; }

	public GuildSkillLevelData SkillLevelData { get; private set; }

	public void Open(Listener listener, GuildSkillData skillData, GuildSkillLevelData skillLevelData)
	{
		if (_skillIconComp != null)
		{
			FactoryBase.ReleaseComponent(_skillIconComp);
			_skillIconComp = null;
		}
		_listener = listener;
		SkillData = skillData;
		SkillLevelData = skillLevelData;
		_skillIconComp = SkillIconGrid.AddGridItem(GuildSkillIconComponent.PrefabAuto);
		_skillIconComp.Open(skillData.GuildSkillIndex, skillLevelData.GuildSkillLevel, skillData.GuildSkillType);
		RefreshLevel(skillLevelData);
		UIHelper.EnableGameObject(Button_LevelUp, enable: false);
		UIHelper.SetLabelText(Label_ScoreLevelUp, string.Empty);
	}

	public void RefreshLevel(GuildSkillLevelData skillLevelData)
	{
		SkillLevelData = skillLevelData;
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(skillLevelData.Values))
			{
				UIHelper.SetLabelText(Label_SkillDesc, LocalizationManager.GetStringDirect(SkillData.SkillDesc));
			}
			else
			{
				UIHelper.SetLabelText(Label_SkillDesc, LocalizationManager.GetStringDirect(SkillData.SkillDescParam, skillLevelData.Values[0]));
			}
			_skillIconComp.RefreshLevel(skillLevelData.GuildSkillLevel);
			bool flag = myMemberInfo.Rank == 1;
			UIHelper.EnableGameObject(Button_LevelUp, flag);
			GuildSkillLevelData data = GameRoot.Instance.GuildSkillLevelDataContainer.GetData(skillLevelData.GuildSkillIndex, skillLevelData.GuildSkillLevel);
			GuildSkillLevelData data2 = GameRoot.Instance.GuildSkillLevelDataContainer.GetData(skillLevelData.GuildSkillIndex, skillLevelData.GuildSkillLevel + 1);
			bool flag2 = data != null && data2 != null;
			if (flag2)
			{
				UIHelper.SetLabelText(Label_ScoreLevelUp, data.ActivityPoint);
			}
			else
			{
				UIHelper.SetLabelText(Label_ScoreLevelUp, "-");
			}
			if (flag)
			{
				bool enable = flag2 && data.ActivityPoint <= GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ActivityPoint;
				UIHelper.EnableButton(Button_LevelUp, enable);
			}
		}
	}

	public void OnClickLevelUp()
	{
		if (_listener != null)
		{
			_listener.OnClickLevelUp(SkillLevelData);
		}
	}
}
