using NShared.BeginCampaign;
using UnityEngine;

public class BeginCampaignAction : IAction
{
	public override void OnGUI()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("begin campaign"))
		{
			Request request = new Request();
			request.ChapterIndex = 1;
			request.DungeonIndex = 1;
			request.HeroIndices = new int[2] { 1, 2 };
			Request request2 = request;
			base.DummyClient.WebServiceRequester.Request(request2, delegate(Response response)
			{
				if (response.Result == ResultType.Success)
				{
					base.DummyClient.SetAction(new EndCampaignAction());
				}
			});
		}
		GUILayout.EndVertical();
	}
}
