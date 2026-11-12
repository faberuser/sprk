using NShared.ChangeAvatarHero;
using UnityEngine;

public class ChangeAvatarHeroAction : IAction
{
	public override void OnGUI()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("change avatar hero index"))
		{
			int testAvatarHeroIndex = 1000;
			Request request = new Request
			{
				AvatarHeroIndex = testAvatarHeroIndex
			};
			base.DummyClient.WebServiceRequester.Request(request, delegate(Response response)
			{
				if (response.Result == ResultType.Success)
				{
					base.DummyClient.UserInfo.AvatarHeroIndex = testAvatarHeroIndex;
				}
			});
		}
		GUILayout.EndVertical();
	}
}
