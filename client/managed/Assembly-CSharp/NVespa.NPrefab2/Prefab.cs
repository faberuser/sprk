using System.Text;
using UnityEngine;

namespace NVespa.NPrefab2;

public class Prefab<T> : IPrefabBase where T : Component
{
	private Factory<T> _factory;

	public bool IsClosed { get; private set; }

	public PrefabKey Key { get; private set; }

	public GameObject LoadGo { get; private set; }

	public Factory<T> Factory
	{
		get
		{
			InitFactory();
			return _factory;
		}
	}

	PrefabKey IPrefabBase.Key => Key;

	private void InitFactory()
	{
		if (_factory == null)
		{
			_factory = new Factory<T>(LoadGo, Key.IsGlobal, Key.IsPooling, Key.TouchMaterial);
		}
	}

	public void Open(PrefabKey key, GameObject go)
	{
		Key = key;
		LoadGo = go;
		LoadGo.SetActive(value: false);
		IsClosed = false;
	}

	void IPrefabBase.Close()
	{
		if (_factory != null)
		{
			_factory.Close();
			_factory = null;
		}
		if (LoadGo != null)
		{
			LoadGo = null;
		}
		IsClosed = true;
	}

	void IPrefabBase.Dump(StringBuilder builder)
	{
		builder.AppendLine($"{Key.Path}, IsGlobal={Key.IsGlobal}, Type={Key.Type.Name}, IsPooling={Key.IsPooling}, TouchMaterial={Key.TouchMaterial}");
		if (_factory != null)
		{
			builder.AppendLine($" - Factory: Unused={_factory.UnusedCount}, IsClose={_factory.IsClosed}");
		}
	}
}
