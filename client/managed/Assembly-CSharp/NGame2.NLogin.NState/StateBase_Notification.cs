using System.Collections;
using NGame2.NNotification;
using NShared.InitDeviceId;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NLogin.NState;

public class StateBase_Notification : StateBase
{
	public override void OnBegin()
	{
		base.OnBegin();
		StartCoroutine(coEntry());
	}

	public override void OnEnd()
	{
		base.OnEnd();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		StateOwner.SetNextState(new StateBase_ConfirmAgreement());
	}

	public override IEnumerator coRun()
	{
		_ = SingletonAuto<NotificationManager>.instance;
		if (Helper.IsNewDevice() && !String_Extension_Method.IsNullOrEmpty(SingletonUpdater<LoginManager>.instance.CountryCode))
		{
			StateBase.SetBgMessage("Send Guid To GameServer", animate: true);
			string deviceUid = Helper.GetDeviceUid();
			Request request = new Request
			{
				DeviceId = deviceUid,
				CountryCode = SingletonUpdater<LoginManager>.instance.CountryCode
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request<Request, Response>(request);
		}
		yield break;
	}
}
