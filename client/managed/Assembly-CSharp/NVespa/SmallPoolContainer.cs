using System.Collections.Generic;

namespace NVespa;

public class SmallPoolContainer : SingletonAuto<SmallPoolContainer>
{
	private class PoolInfo
	{
		public ISmallPool Pool;

		public int BackupCreateNew;

		public int BackupCreate;

		public int BackupRelease;

		public int BackupUnused;
	}

	private List<PoolInfo> _poolList = new List<PoolInfo>();

	public void AddPool(ISmallPool pool)
	{
		_poolList.Add(new PoolInfo
		{
			Pool = pool
		});
	}

	public void OnSceneChanged()
	{
		for (int i = 0; i < _poolList.Count; i++)
		{
			_poolList[i].Pool.OnSceneChanged();
		}
		_poolList.Clear();
	}
}
