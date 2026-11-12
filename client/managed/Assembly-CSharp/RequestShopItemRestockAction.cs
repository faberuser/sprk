using NShared.RequestShopList;
using UnityEngine;

public class RequestShopItemRestockAction : IAction
{
	public override void OnGUI()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("restock shop list"))
		{
			Request request = new Request
			{
				ShopIndex = 1
			};
			base.DummyClient.WebServiceRequester.Request(request, delegate(Response response)
			{
				_ = response.Result;
				_ = 1;
			});
		}
		GUILayout.EndVertical();
	}
}
