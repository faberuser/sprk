using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NPrefab2.NEvent;

public class EventContainer : MonoBehaviour, IReleaseEvent
{
	private List<IReleaseEvent> _releaseEvent = new List<IReleaseEvent>();

	void IReleaseEvent.OnRelease()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_releaseEvent); i++)
		{
			_releaseEvent[i].OnRelease();
		}
		_releaseEvent.Clear();
	}
}
