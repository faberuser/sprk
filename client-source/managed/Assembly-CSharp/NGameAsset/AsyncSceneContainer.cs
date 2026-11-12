using System.Collections.Generic;
using NVespa;

namespace NGameAsset;

public class AsyncSceneContainer : SingletonAuto<AsyncSceneContainer>
{
	private List<IAsyncLoadBase> _asyncLoadList = new List<IAsyncLoadBase>();

	public void Add(IAsyncLoadBase async)
	{
		_asyncLoadList.Add(async);
	}

	public void AddList<T>(IList<T> list) where T : IAsyncLoadBase
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			_asyncLoadList.Add(list[i]);
		}
	}

	public void OnSceneChanged()
	{
		for (int i = 0; i < _asyncLoadList.Count; i++)
		{
			Tools.ReleaseAsyncLoad(_asyncLoadList[i]);
		}
		_asyncLoadList.Clear();
	}
}
