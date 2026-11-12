using NShared.BuyShopItem;
using UnityEngine;

public class BuyShopItemAction : IAction
{
	public override void OnGUI()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("buy shop item"))
		{
			Request request = new Request
			{
				ShopIndex = 1,
				ListNo = 1
			};
			base.DummyClient.WebServiceRequester.Request(request, delegate(Response response)
			{
				if (response.Result == ResultType.Success)
				{
					base.DummyClient.SetAction(new SellItemAction());
				}
			});
		}
		GUILayout.EndVertical();
	}
}
