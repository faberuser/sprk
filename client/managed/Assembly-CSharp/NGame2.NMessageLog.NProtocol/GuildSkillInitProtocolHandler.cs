using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildSkillInitProtocolHandler : MessageProtocolHandler<GuildSkillInit>
{
	public override void Run(long senderId, GuildSkillInit protocolData)
	{
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.InitGuildSkillLevel(protocolData.ActivityPoint, protocolData.SuppressPoint);
		GlobalAutoBehaviour<GuildManager>.instance.ResetSkillLevelUpRequest();
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageUI();
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.Skill);
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildTrainningCenterUI();
	}
}
