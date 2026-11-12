using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NWindow.NEntryMenu;
using NShared;
using NVespa;

namespace NGame2.NLobby.NActivity;

public class UsingActionContainer : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new UsingActionContainer(values);
	}

	public UsingActionContainer(string[] values)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(values))
		{
			return;
		}
		string valueString = ArgParser.GetValueString(values, 0);
		NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
		if (action != null)
		{
			List<object> list = new List<object>();
			foreach (string item in values)
			{
				list.Add(item);
			}
			action.Run(null, null, list);
		}
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
