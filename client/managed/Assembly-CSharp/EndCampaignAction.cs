using NShared.EndCampaign;
using UnityEngine;

public class EndCampaignAction : IAction
{
	public override void OnGUI()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("end campaign"))
		{
			Request request = new Request
			{
				ChapterIndex = 1,
				DungeonIndex = 1,
				Completed = true,
				Star = 1
			};
			base.DummyClient.WebServiceRequester.Request(request, delegate(Response response)
			{
				if (response.Result == ResultType.Success)
				{
					base.DummyClient.SetAction(new BeginCampaignAction());
				}
			});
		}
		GUILayout.EndVertical();
	}
}
