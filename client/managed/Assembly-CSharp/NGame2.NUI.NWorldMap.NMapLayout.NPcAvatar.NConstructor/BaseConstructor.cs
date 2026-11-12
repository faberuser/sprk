using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public class BaseConstructor : IConstructor
{
	public virtual CachedUserInfo GetPcAvatarUserInfo(List<CachedUserInfo> exceptUserPools)
	{
		return null;
	}

	public virtual List<CachedUserInfo> AllPcUserInfos()
	{
		return null;
	}

	public virtual BasePcAvatar GetAvatarCreater()
	{
		return new BasePcAvatar();
	}

	public virtual BasePcMediator GetMediator()
	{
		return new BasePcMediator();
	}
}
