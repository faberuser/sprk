using NShared.GetShopList;
using UnityEngine;

public class GetShopItemListAction : IAction
{
	public override void OnGUI()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("get shop list"))
		{
			Request request = new Request
			{
				ShopIndex = 1
			};
			base.DummyClient.WebServiceRequester.Request(request, delegate(Response response)
			{
				if (response.Result == ResultType.Success)
				{
					base.DummyClient.SetAction(new BuyShopItemAction());
				}
			});
		}
		GUILayout.EndVertical();
	}
}
