using System.Collections.Generic;
using UnityEngine;

namespace NGame2.NUI.NAtlasAssignComponent;

public class AtlasAssignerController : MonoBehaviour
{
	public List<AtlasAssignerBase> Container;

	private void Awake()
	{
		Assign();
	}

	public void Assign()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Container); i++)
		{
			Container[i].Assign();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(Container);
	}
}
