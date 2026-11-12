using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildSkillLevelUpProtocolHandler : MessageProtocolHandler<GuildSkillLevelUp>
{
	public override void Run(long senderId, GuildSkillLevelUp protocolData)
	{
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.SetSkillLevel(protocolData.GuildSkillIndex, protocolData.Level);
		GuildSkillLevelData data = GameRoot.Instance.GuildSkillLevelDataContainer.GetData(protocolData.GuildSkillIndex, protocolData.Level - 1);
		if (data != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.IncActivityPoint(-data.ActivityPoint);
		}
		GlobalAutoBehaviour<GuildManager>.instance.ResetSkillLevelUpRequest();
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageUI();
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.Skill);
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildTrainningCenterUI();
	}
}
