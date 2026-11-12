using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public interface IConstructor
{
	BasePcAvatar GetAvatarCreater();

	BasePcMediator GetMediator();

	CachedUserInfo GetPcAvatarUserInfo(List<CachedUserInfo> exceptUserPools);

	List<CachedUserInfo> AllPcUserInfos();
}
