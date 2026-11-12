using NGame2.NGuild;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildSkillLevelComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickLevelUp(GuildSkillLevelData skillLevelData);

		void OnClickVote(int skillIndex);
	}

	public const string PrefabPath = "Prefab2/Component/Guild/GuildSkillLevelComponent";

	public static PrefabAuto<GuildSkillLevelComponent> PrefabAuto = new PrefabAuto<GuildSkillLevelComponent>("Prefab2/Component/Guild/GuildSkillLevelComponent");

	public UILabel Label_ActivityPoint;

	public UILabel Label_PunitivePoint;

	public UILabel Label_CenterLevel;

	public UILabel Label_VoteCount;

	public UILabel Label_SkillDesc;

	public UIButton Button_LevelUp;

	public UIButton Button_Vote;

	public UISimpleGrid SkillIconGrid;

	private GuildSkillIconComponent _skillIconComp;

	private Listener _listener;

	[HideInInspector]
	public int GuildSkillIndex;

	private GuildSkillInfo GuildSkillInfo;

	public GuildSkillData SkillData { get; private set; }

	public GuildSkillLevelData SkillLevelData { get; private set; }

	public void Open(Listener listener, GuildSkillData skillData, GuildSkillLevelData skillLevelData, GuildSkillInfo guildSkillInfo)
	{
		if (_skillIconComp != null)
		{
			FactoryBase.ReleaseComponent(_skillIconComp);
			_skillIconComp = null;
		}
		_listener = listener;
		SkillData = skillData;
		GuildSkillInfo = guildSkillInfo;
		GuildSkillIndex = SkillData.GuildSkillIndex;
		SkillLevelData = skillLevelData;
		_skillIconComp = SkillIconGrid.AddGridItem(GuildSkillIconComponent.PrefabAuto);
		_skillIconComp.Open(skillData.GuildSkillIndex, SkillLevelData.GuildSkillLevel, skillData.GuildSkillType);
		RefreshLevel(SkillLevelData);
	}

	public void RefreshComponent(GuildSkillInfo guildSkillInfo, GuildSkillLevelData skillLevelData)
	{
		GuildSkillInfo = guildSkillInfo;
		SkillLevelData = skillLevelData;
		RefreshLevel(SkillLevelData);
	}

	public void RefreshLevel(GuildSkillLevelData skillLevelData)
	{
		SkillLevelData = skillLevelData;
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null)
		{
			return;
		}
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
			UIHelper.SetLabelText(Label_ActivityPoint, data.ActivityPoint);
			UIHelper.SetLabelText(Label_PunitivePoint, data.SuppressPoint);
			UIHelper.SetLabelText(Label_CenterLevel, data.TrainingCenterLevel);
		}
		else
		{
			UIHelper.SetLabelText(Label_ActivityPoint, "-");
			UIHelper.SetLabelText(Label_PunitivePoint, "-");
			UIHelper.SetLabelText(Label_CenterLevel, "-");
		}
		if (flag)
		{
			GuildBuildingInfo guildBuildingInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.GetGuildBuildingInfo(EGuildBuildingType.TraningCenter);
			if (guildBuildingInfo != null)
			{
				int buildingLevel = guildBuildingInfo.BuildingLevel;
				bool enable = flag2 && data.ActivityPoint <= GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ActivityPoint && data.SuppressPoint <= GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.SuppressPoint && data.TrainingCenterLevel <= buildingLevel;
				UIHelper.EnableButton(Button_LevelUp, enable);
			}
		}
		if (GuildSkillInfo == null)
		{
			UIHelper.SetLabelText(Label_VoteCount, "0");
			GuildSkillInfo = new GuildSkillInfo
			{
				SkillLevel = 1,
				ReqCount = 0,
				SkillIndex = SkillData.GuildSkillIndex
			};
		}
		else
		{
			UIHelper.SetLabelText(Label_VoteCount, GuildSkillInfo.ReqCount);
		}
		int maxSkillLevel = GameRoot.Instance.GuildSkillLevelDataContainer.GetMaxSkillLevel(SkillData.GuildSkillIndex);
		byte skillLevel = GuildSkillInfo.SkillLevel;
		bool enable2 = (myMemberInfo.ReqGuildSkill == 0 || myMemberInfo.ReqGuildSkill == SkillData.GuildSkillIndex) && skillLevel < maxSkillLevel;
		UIHelper.EnableButton(Button_Vote, enable2);
	}

	public void OnClickLevelUp()
	{
		if (_listener != null)
		{
			_listener.OnClickLevelUp(SkillLevelData);
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
