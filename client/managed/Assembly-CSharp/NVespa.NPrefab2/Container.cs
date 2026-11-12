using System.Text;
using UnityEngine;

namespace NVespa.NPrefab2;

public class Container : SingletonAuto<Container>
{
	private IncreaseDic<PrefabKey, IPrefabBase> _dic = new IncreaseDic<PrefabKey, IPrefabBase>();

	public void Dump(StringBuilder builder)
	{
		builder.AppendLine($"Prefab Count={_dic.Count}");
		for (int i = 0; i < _dic.Count; i++)
		{
			_dic.GetValueByIndex(i).Dump(builder);
		}
	}

	public bool Contains<T>(PrefabKey key)
	{
		if (String_Extension_Method.IsNullOrEmpty(key.Path))
		{
			return false;
		}
		return _dic.ContainsKey(key);
	}

	public Prefab<T> Register<T>(GameObject loadResource, PrefabKey key) where T : Component
	{
		if (_dic.GETVALUE(key, null) is Prefab<T> result)
		{
			return result;
		}
		Prefab<T> prefab = new Prefab<T>();
		prefab.Open(key, loadResource);
		_dic.Add(key, prefab);
		return prefab;
	}

	public Prefab<T> Load<T>(string path, bool isGlobal = false, EPrefabType prefabType = EPrefabType.Ngui, bool isPooling = true, bool touchMaterial = false) where T : Component
	{
		if (String_Extension_Method.IsNullOrEmpty(path))
		{
			return null;
		}
		return LoadInPlaying<T>(new PrefabKey
		{
			IsGlobal = isGlobal,
			IsPooling = isPooling,
			TouchMaterial = touchMaterial,
			PrefabType = prefabType,
			Type = typeof(T),
			Path = path
		});
	}

	private Prefab<T> LoadInPlaying<T>(PrefabKey key) where T : Component
	{
		IPrefabBase vALUE = _dic.GETVALUE(key, null);
		if (vALUE != null)
		{
			return vALUE as Prefab<T>;
		}
		GameObject gameObject = VespaResource.Load<GameObject>(key.Path);
		if (gameObject == null)
		{
			return null;
		}
		Prefab<T> prefab = new Prefab<T>();
		prefab.Open(key, gameObject);
		_dic.Add(key, prefab);
		return prefab;
	}

	public void DestroyOnSceneChanged()
	{
		int num;
		for (num = 0; num < _dic.Count; num++)
		{
			_dic.GetValueByIndex(num).Close();
			_dic.RemoveByIndex(num);
			num--;
		}
	}
}
