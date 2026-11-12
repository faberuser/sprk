using NGame2.NMessage;
using NGameAsset;
using NVespa;

namespace NGame2.NUI.NRender3D.NCinema;

public class BgmHandler : BaseHandler
{
	public override string Command => "Common/PlayBgm";

	public override void Action(Cinema owner, CommandMessage message)
	{
		string arg = message.GetArg<string>(0);
		if (String_Extension_Method.IsNullOrEmpty(arg))
		{
			SingletonAuto<BgmManager2>.instance.StopEvent(owner.BgmCode);
			owner.BgmCode = string.Empty;
		}
		else
		{
			owner.BgmCode = arg;
			SingletonAuto<BgmManager2>.instance.Request(owner.BgmCode, isEvent: true);
		}
	}
}
