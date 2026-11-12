using System.Collections;
using NVespa;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_QueryHost : StateBase
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
		StateOwner.SetNextState(new StateBase_QueryMarketInfo());
	}

	public override IEnumerator coRun()
	{
		StateBase.SetBgMessage("Find Server", animate: true);
		string queryHost = StateBase.ClientConfig.QueryHost;
		if (!String_Extension_Method.IsNullOrEmpty(SingletonUpdater<LoginManager>.instance.PatchQueryHost.Value))
		{
			queryHost = SingletonUpdater<LoginManager>.instance.PatchQueryHost.Value;
		}
		if (String_Extension_Method.IsNullOrEmpty(queryHost))
		{
			yield return new WaitQuitError("Query host is not assigned");
		}
		int retryCount = 3;
		int timeoutTime = 10;
		string resultText = string.Empty;
		for (int tryNo = 0; tryNo < retryCount; tryNo++)
		{
			string original = queryHost;
			_ = tryNo;
			WwwText wwwText = new WwwText(SprkAccounts.ResolveHostUrl(original));
			float expireAt = Time.time + (float)timeoutTime;
			while (!wwwText.isDone && Time.time < expireAt)
			{
				yield return null;
			}
			if (wwwText.isDone && String_Extension_Method.IsNullOrEmpty(wwwText.error))
			{
				resultText = wwwText.text;
				break;
			}
			if (tryNo != retryCount - 1)
			{
				float waitExpireAt = Time.time + 5f;
				while (Time.time < waitExpireAt)
				{
					yield return null;
				}
			}
		}
		if (String_Extension_Method.IsNullOrEmpty(resultText))
		{
			yield return new WaitQuitError("Cannot read Host files due to bad internet connection.");
		}
		if (!Helper.FindHostInfo(resultText, StateBase.ClientConfig, out var hostData, out var versionInfo))
		{
			yield return new WaitQuitError("Failed to find my host");
		}
		if (__Extension_Method_ICollection__.COUNT(hostData.Server) == 0)
		{
			yield return new WaitQuitError("No available server");
		}
		SingletonUpdater<LoginManager>.instance.SetHostAndVersion(hostData, versionInfo);
	}
}
