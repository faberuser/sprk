using System.Collections;
using System.Linq;
using UnityEngine;

namespace NGame2.NLobby.NActivity;

public class OpenExternalURL : IActivity
{
	private string _url;

	public static IActivity Create(string[] values)
	{
		return new OpenExternalURL(values);
	}

	public OpenExternalURL(string[] url)
	{
		_url = url.FirstOrDefault();
	}

	IEnumerator IActivity.coStart()
	{
		Application.OpenURL(_url);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
