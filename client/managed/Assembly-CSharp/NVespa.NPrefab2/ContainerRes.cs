using System.Text;
using UnityEngine;

namespace NVespa.NPrefab2;

public class ContainerRes : SingletonAuto<ContainerRes>
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

	public bool Contains<T>(string path)
	{
		return Contains<T>(path, isGlobal: false, EPrefabType.Ngui);
	}

	public bool Contains<T>(string path, bool isGlobal, EPrefabType prefabType)
	{
		if (String_Extension_Method.IsNullOrEmpty(path))
		{
			return false;
		}
		PrefabKey key = new PrefabKey
		{
			IsGlobal = isGlobal,
			IsPooling = true,
			PrefabType = prefabType,
			Type = typeof(T),
			Path = path
		};
		return _dic.ContainsKey(key);
	}

	public PrefabRes<T> Register<T>(T loadResource, string path, bool isGlobal, EPrefabType prefabType) where T : Component
	{
		PrefabKey key = new PrefabKey
		{
			IsGlobal = isGlobal,
			IsPooling = true,
			PrefabType = prefabType,
			Type = typeof(T),
			Path = path
		};
		if (_dic.GETVALUE(key, null) is PrefabRes<T> result)
		{
			return result;
		}
		PrefabRes<T> prefabRes = new PrefabRes<T>();
		prefabRes.Open(key, loadResource);
		_dic.Add(key, prefabRes);
		return prefabRes;
	}

	public PrefabRes<T> Load<T>(string path) where T : Object
	{
		return Load<T>(path, isGlobal: false, EPrefabType.Ngui);
	}

	public PrefabRes<T> Load<T>(string path, bool isGlobal, EPrefabType prefabType) where T : Object
	{
		if (String_Extension_Method.IsNullOrEmpty(path))
		{
			return null;
		}
		return LoadInPlaying<T>(new PrefabKey
		{
			IsGlobal = isGlobal,
			IsPooling = true,
			PrefabType = prefabType,
			Type = typeof(T),
			Path = path
		});
	}

	private PrefabRes<T> LoadInPlaying<T>(PrefabKey key) where T : Object
	{
		IPrefabBase vALUE = _dic.GETVALUE(key, null);
		if (vALUE != null)
		{
			return vALUE as PrefabRes<T>;
		}
		T val = VespaResource.Load<T>(key.Path);
		if (val == null)
		{
			return null;
		}
		PrefabRes<T> prefabRes = new PrefabRes<T>();
		prefabRes.Open(key, val);
		_dic.Add(key, prefabRes);
		return prefabRes;
	}

	public void DestroyOnSceneChanged()
	{
		for (int i = 0; i < _dic.Count; i++)
		{
			if (!_dic.GetKeyByIndex(i).IsGlobal)
			{
				_dic.GetValueByIndex(i).Close();
				_dic.RemoveByIndex(i);
				i--;
			}
		}
	}
}
