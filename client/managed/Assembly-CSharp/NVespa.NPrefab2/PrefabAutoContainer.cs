using System.Collections.Generic;

namespace NVespa.NPrefab2;

public class PrefabAutoContainer : SingletonAuto<PrefabAutoContainer>
{
	private List<IPrefabAuto> _list = new List<IPrefabAuto>();

	public void OnSceneChanged()
	{
		lock (_list)
		{
			for (int i = 0; i < _list.Count; i++)
			{
				_list[i].OnSceneChanged();
			}
			_list.Clear();
		}
	}

	public void Add(IPrefabAuto prefabAuto)
	{
		lock (_list)
		{
			_list.Add(prefabAuto);
		}
	}
}
