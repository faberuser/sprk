using System.Collections.Generic;

namespace NVespa;

public class LinkedTransformContainer : SingletonAuto<LinkedTransformContainer>
{
	private List<ILinkedTransform> _list = new List<ILinkedTransform>();

	public void Add(ILinkedTransform linkedTransform)
	{
		lock (_list)
		{
			_list.Add(linkedTransform);
		}
	}

	public void OnSceneChanged()
	{
		lock (_list)
		{
			for (int i = 0; i < _list.Count; i++)
			{
				_list[i].OnSceneChanged();
			}
		}
	}
}
