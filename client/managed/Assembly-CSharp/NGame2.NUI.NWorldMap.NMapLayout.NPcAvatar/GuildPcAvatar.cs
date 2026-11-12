using NGame2.NAccount;
using NGame2.NGuild;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class GuildPcAvatar : InteractionPcAvatar, IUserObserver<GuildMemberInfo>, IUserObserver
{
	protected override void AddObserver()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.ObserverManager.AddObserver(this);
		}
	}

	protected override void RemoveObserver()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.ObserverManager.RemoveObserver(this);
		}
	}

	void IUserObserver<GuildMemberInfo>.OnChanged(GuildMemberInfo changedInfo)
	{
		if (changedInfo != null && Uid == changedInfo.AccountId && Name != changedInfo.Nick)
		{
			base.AiInteractionComponent.SetName(changedInfo.Nick);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
