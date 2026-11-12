using System;
using System.Collections.Generic;

namespace NVespa.NUtil;

public class ActionExecuter
{
	private List<Action> _listAction = new List<Action>();

	public void Add(Action i_action)
	{
		if (i_action != null)
		{
			_listAction.Add(i_action);
		}
	}

	public void Execute(bool i_forward)
	{
		while (_listAction.Count > 0)
		{
			Action i_action;
			if (i_forward)
			{
				i_action = _listAction[0];
				_listAction.RemoveAt(0);
			}
			else
			{
				i_action = _listAction[_listAction.Count - 1];
				_listAction.RemoveAt(_listAction.Count - 1);
			}
			Vespa_System_Action_Extension_Method.fire(i_action);
		}
	}
}
