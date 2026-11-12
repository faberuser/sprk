using NGame2.NGuild;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class GuildSkillVoteComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickVote(int skillIndex);
	}

	public const string PrefabPath = "Prefab2/Component/Guild/GuildSkillVoteComponent";

	public static PrefabAuto<GuildSkillVoteComponent> PrefabAuto = new PrefabAuto<GuildSkillVoteComponent>("Prefab2/Component/Guild/GuildSkillVoteComponent");

	public UILabel Label_SkillDesc;

	public UILabel Label_Vote;

	public UIButton Button_Vote;

	public UISimpleGrid SkillIconGrid;

	private GuildSkillIconComponent _skillIconComp;

	private Listener _listener;

	public GuildSkillData SkillData { get; private set; }

	public void Open(Listener listener, GuildSkillData skillData)
	{
		if (_skillIconComp != null)
		{
			FactoryBase.ReleaseComponent(_skillIconComp);
			_skillIconComp = null;
		}
		_listener = listener;
		SkillData = skillData;
		_skillIconComp = SkillIconGrid.AddGridItem(GuildSkillIconComponent.PrefabAuto);
		_skillIconComp.Open(skillData.GuildSkillIndex, 0, skillData.GuildSkillType);
		Refresh();
	}

	public void Refresh()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
		{
			GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
			if (myMemberInfo != null)
			{
				int maxSkillLevel = GameRoot.Instance.GuildSkillLevelDataContainer.GetMaxSkillLevel(SkillData.GuildSkillIndex);
				byte skillLevel = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.GetSkillLevel(SkillData.GuildSkillIndex);
				bool enable = (myMemberInfo.ReqGuildSkill == 0 || myMemberInfo.ReqGuildSkill == SkillData.GuildSkillIndex) && skillLevel < maxSkillLevel;
				UIHelper.SetLabelText(Label_SkillDesc, LocalizationManager.GetStringDirect(SkillData.SkillDesc));
				UIHelper.SetLabelText(Label_Vote, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildSkillReq(SkillData.GuildSkillIndex));
				UIHelper.EnableButton(Button_Vote, enable);
			}
		}
	}

	public void OnClickVote()
	{
		if (_listener == null)
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null)
		{
			if (myMemberInfo.ReqGuildSkill == 0)
			{
				_listener.OnClickVote(SkillData.GuildSkillIndex);
			}
			else if (myMemberInfo.ReqGuildSkill == SkillData.GuildSkillIndex)
			{
				_listener.OnClickVote(0);
			}
		}
	}
}
