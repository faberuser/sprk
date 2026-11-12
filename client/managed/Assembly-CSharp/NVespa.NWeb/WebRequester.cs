using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NVespa.NWeb;

public class WebRequester<TReq, TRes> : CustomYieldInstruction where TReq : Request, IServiceRequest where TRes : Response, new()
{
	private TRes _response;

	private bool _completed;

	public override bool keepWaiting => !_completed;

	public TRes Response => _response;

	public WebRequester(TReq request)
	{
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(TRes r)
		{
			_response = r;
			_completed = true;
		});
	}
}
