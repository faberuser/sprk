using NGameAsset;
using UnityEngine;

namespace NVespa;

public class GameObjectCache
{
	private GameObject _go;

	public GameObject Go
	{
		get
		{
			return _go;
		}
		set
		{
			if (!(_go == value))
			{
				_go = value;
				if (_go == null)
				{
					Tm = null;
				}
				else
				{
					Tm = _go.transform;
				}
			}
		}
	}

	public Transform Tm { get; private set; }

	public int Layer
	{
		get
		{
			if (!(_go == null))
			{
				return _go.layer;
			}
			return 0;
		}
	}

	public void DestroyGameObject()
	{
		if (!(_go == null))
		{
			VespaObject.DestroyGo(_go);
			Tm = null;
		}
	}

	public void DestroyInGameAsset()
	{
		if (!(_go == null))
		{
			AssetUpdateManager.PendingDestroyGo(_go);
			Tm = null;
		}
	}
}
