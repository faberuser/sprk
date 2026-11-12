using System.Text;
using UnityEngine;

namespace NVespa.NPrefab2;

public class PrefabGo : IPrefabBase
{
	private FactoryGo _factory;

	public int Dummy;

	public bool IsClosed { get; private set; }

	public PrefabKey Key { get; private set; }

	public GameObject LoadGo { get; private set; }

	public FactoryGo Factory
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
			_factory = new FactoryGo(LoadGo, Key.IsGlobal, Key.IsPooling, Key.TouchMaterial);
		}
	}

	public void Open(PrefabKey key, GameObject go)
	{
		Key = key;
		LoadGo = go;
		IsClosed = false;
		if (Key.TouchMaterial && go != null)
		{
			Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
			{
				Renderer renderer = componentsInChildren[i];
				Dummy = __Extension_Method_ICollection__.COUNT(renderer.sharedMaterials);
			}
		}
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
