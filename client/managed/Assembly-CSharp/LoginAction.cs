using NShared.Login;
using UnityEngine;

public class LoginAction : IAction
{
	public override void OnGUI()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("login as " + base.DummyClient.LoginId))
		{
			Request request = new Request
			{
				LoginId = base.DummyClient.LoginId
			};
			base.DummyClient.WebServiceRequester.Request(request, delegate(Response response)
			{
				if (response.Result == ResultType.Success)
				{
					base.DummyClient.WebServiceRequester.SessionKey = response.UserInfo.SessionKey;
					base.DummyClient.UserInfo = response.UserInfo;
					base.DummyClient.SetAction(new ChangeAvatarHeroAction());
				}
			});
		}
		GUILayout.EndVertical();
	}
}
