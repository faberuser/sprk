using System;
using System.Collections.Generic;
using NVespa;

namespace NGame2.IAP;

public abstract class StateBase
{
	public abstract EStatus Status { get; }

	public virtual void Begin()
	{
		UnityLogQueue.Enqueue_IAP($"[{GetType()}] Begin", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/IAP/StateBase.cs", 13);
	}

	public virtual void ChainUpdate()
	{
	}

	public virtual void End()
	{
		UnityLogQueue.Enqueue_IAP($"[{GetType()}] End", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/IAP/StateBase.cs", 23);
	}

	public virtual void QueryInventory(EQueryInventoryType queryType, IList<string> productIdList, Action<EResultQueryInventory, string> callback)
	{
		UnityLogQueue.Enqueue_IAP($"Invalide State in QueryInventory: [{GetType()}]", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/IAP/StateBase.cs", 29);
		Vespa_System_Action_Extension_Method.fire(callback, EResultQueryInventory.InvalidStatus, $"Invalid State: {Vespa_String_Extension_Method.GetEnumString(Status)}");
	}

	public virtual void RequestPurchase(bool silent, string productId, int productIndex, Action<EResultPurchase, PurchaseApplyData, string> callback)
	{
		UnityLogQueue.Enqueue_IAP($"Invalide State in RequestPurchase: [{GetType()}]", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/IAP/StateBase.cs", 35);
		Vespa_System_Action_Extension_Method.fire(callback, EResultPurchase.InvalidStatus, null, $"Invalid State: {Vespa_String_Extension_Method.GetEnumString(Status)}");
	}
}
