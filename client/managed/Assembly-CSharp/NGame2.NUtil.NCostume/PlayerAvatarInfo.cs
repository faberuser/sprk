using NGame2.NAccount;
using NGame2.NUI;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NCostume;

public class PlayerAvatarInfo
{
	public static bool IsEqualCurrentPlayerAvatarHero(UIAvatarHelper.AvatarInfo changeAvatarInfo)
	{
		if (changeAvatarInfo.AvatarIndex != GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex)
		{
			return false;
		}
		PlayerAvatarHeroInfo playerAvatarHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.PlayerAvatarHeroInfo;
		if (playerAvatarHeroInfo == null)
		{
			return false;
		}
		return SprkLobbyAvatar.IsEqualCostumeAndRender(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(changeAvatarInfo.HeroIndex), playerAvatarHeroInfo);
	}
}
