using System.Collections.Generic;
using NGame2.N3D;
using NShared;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public class GuildPcAvatarFactory : IPcAvatarFactory
{
	private IUserInfoGetter _InfoGetter;

	private List<CachedUserInfo> _createdUserList = new List<CachedUserInfo>();

	public GuildPcAvatarFactory()
	{
		_InfoGetter = new GuildMemberUserInfoGetter(_createdUserList);
	}

	void IPcAvatarFactory.ClearCache()
	{
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_createdUserList);
	}

	PcAvatarStock IPcAvatarFactory.CreateAvatar(string[] values)
	{
		CachedUserInfo userInfo = _InfoGetter.GetUserInfo();
		if (userInfo == null)
		{
			return PcAvatarStock.EmptyStock;
		}
		if (!ArgParser.GetValueBool(values, 0))
		{
			_createdUserList.Add(userInfo);
		}
		return new PcAvatarStock
		{
			RenderFacadeBulider = RenderFacadeBuilder.CreateRenderFacadeBuliderFrom(userInfo),
			Uid = userInfo.AccountId,
			Name = userInfo.Nick,
			Mediator = new GuildMediator(),
			Avatar = new GuildPcAvatar()
		};
	}
}
