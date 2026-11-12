using NShared.SendClickThroughRate;
using NVespa;
using NVespa.NGlobal;
using NVespa.NWeb;

namespace NGame2.NCtr;

public class CtrManager : SingletonAuto<CtrManager>
{
	public enum ECtrMember
	{
		ComeBackJumping = 1,
		PurchaseMarketing
	}

	public void Send(ECtrMember member, EClickTag clickTag)
	{
		Send(member.ToString(), clickTag);
	}

	public void Send(string adId, EClickTag clickTag)
	{
		Request request = new Request
		{
			AdId = adId,
			ClickType = (int)clickTag
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request<Request, Response>(request, WebRequestFlag.SilenceError, delegate
		{
		});
	}
}
