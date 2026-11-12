using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NGame2.IAP;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NLogin.NHelper;

public class CheckInAppBilling
{
	public Tuple<EResultInit, string> TupleResultInit;

	public Tuple<EResultQueryInventory, string> TupleResultQueryInventory;

	private VespaCoroutine.RuntimeData _runtimeCo;

	public void Check()
	{
		if (_runtimeCo != null)
		{
			VespaCoroutine.Stop(_runtimeCo);
			_runtimeCo = null;
		}
		TupleResultInit = null;
		TupleResultQueryInventory = null;
		_runtimeCo = VespaCoroutine.StartRuntime(coCheck());
	}

	private IEnumerator coCheck()
	{
		if (SingletonAuto<IAPWrapper>.instance.IAPManager == null)
		{
			TupleResultInit = Tuple.Create(EResultInit.BillingNotSupported, string.Empty);
			yield break;
		}
		Task task = Task.CompletedTask;
		yield return new WaitUntil(() => task.IsCompleted);
		UnityLogQueue.Enqueue_IAP($"[{GetType()}] Initializing IAP Manager", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/Login/Helper/CheckInAppBilling.cs", 77);
		bool isComplete = false;
		SingletonAuto<IAPWrapper>.instance.IAPManager.Init(delegate(EResultInit result, string error)
		{
			isComplete = true;
			TupleResultInit = Tuple.Create(result, error);
		});
		while (!isComplete)
		{
			yield return null;
		}
		UnityLogQueue.Enqueue_IAP($"[{GetType()}] IAP Manager Initialize Complete. Result : {TupleResultInit.Item1}", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/Login/Helper/CheckInAppBilling.cs", 91);
		if (TupleResultInit.Item1 != EResultInit.Success)
		{
			yield break;
		}
		IList<string> platformProductIds = GameRoot.Instance.NewPayShopProductDataContainer.GetPlatformProductIds();
		if (__Extension_Method_ICollection__.COUNT(platformProductIds) > 0)
		{
			UnityLogQueue.Enqueue_IAP("call IAP.IAPWrapper.instance.IAPManager.QueryInventory", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/Login/Helper/CheckInAppBilling.cs", 109);
			isComplete = false;
			SingletonAuto<IAPWrapper>.instance.IAPManager.QueryInventory(EQueryInventoryType.ConfirmInAppBilling, platformProductIds, delegate(EResultQueryInventory result, string error)
			{
				isComplete = true;
				TupleResultQueryInventory = Tuple.Create(result, error);
				UnityLogQueue.Enqueue_IAP($"callback IAP.IAPWrapper.instance.IAPManager.QueryInventory: result={result}, error={error}", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/Login/Helper/CheckInAppBilling.cs", 118);
			});
			while (!isComplete)
			{
				yield return null;
			}
		}
	}
}
