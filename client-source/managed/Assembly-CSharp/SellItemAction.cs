using NShared.SellItem;
using UnityEngine;

public class SellItemAction : IAction
{
	public override void OnGUI()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("sell item"))
		{
			int[] array = new int[1];
			int[] array2 = new int[1];
			array[0] = 1001;
			array2[0] = 1;
			Request request = new Request
			{
				ItemIndices = array,
				ItemCount = array2
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
