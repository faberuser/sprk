using NGame2.N3D;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public class CreatureAvatarFactory : IPcAvatarFactory
{
	void IPcAvatarFactory.ClearCache()
	{
	}

	PcAvatarStock IPcAvatarFactory.CreateAvatar(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			VespaDebug.LogError("[CreatureAvatarFactory] values Count = " + __Extension_Method_ICollection__.COUNT(values));
			return PcAvatarStock.EmptyStock;
		}
		string code = values[0];
		int valueInt = ArgParser.GetValueInt(values, 1);
		string stringDirect = LocalizationManager.GetStringDirect(values[2]);
		return new PcAvatarStock
		{
			RenderFacadeBulider = new RenderFacadeBuilder(null, TeamType.Left, code, valueInt),
			Uid = 0L,
			Name = stringDirect,
			Mediator = new BasePcMediator(),
			Avatar = new InteractionPcAvatar()
		};
	}
}
