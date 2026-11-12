using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa;

public class EmptyBehaviour : MonoBehaviour, IInspectorGuiDrawer
{
	public List<Action> OnDestroyList = new List<Action>();

	public string Desc { get; set; }

	private void OnDestroy()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(OnDestroyList); i++)
		{
			Vespa_System_Action_Extension_Method.fire(OnDestroyList[i]);
		}
		OnDestroyList.Clear();
	}
}
