using System.Text;
using UnityEngine;

namespace NVespa.NPrefab2;

public class ContainerGo : SingletonAuto<ContainerGo>
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

	public bool Contains(string path)
	{
		return Contains(path, isGlobal: false, EPrefabType.Ngui, isPooling: true, touchMaterial: false);
	}

	public bool Contains(string path, bool isGlobal, EPrefabType prefabType, bool isPooling, bool touchMaterial)
	{
		if (String_Extension_Method.IsNullOrEmpty(path))
		{
			return false;
		}
		PrefabKey key = new PrefabKey
		{
			IsGlobal = isGlobal,
			IsPooling = isPooling,
			TouchMaterial = touchMaterial,
			PrefabType = prefabType,
			Type = typeof(GameObject),
			Path = path
		};
		return _dic.ContainsKey(key);
	}

	public PrefabGo Register(GameObject loadResource, string path, bool isGlobal, EPrefabType prefabType, bool isPooling, bool touchMaterial)
	{
		PrefabKey key = new PrefabKey
		{
			IsGlobal = isGlobal,
			IsPooling = isPooling,
			TouchMaterial = touchMaterial,
			PrefabType = prefabType,
			Type = typeof(GameObject),
			Path = path
		};
		if (_dic.GETVALUE(key, null) is PrefabGo result)
		{
			return result;
		}
		PrefabGo prefabGo = new PrefabGo();
		prefabGo.Open(key, loadResource);
		_dic.Add(key, prefabGo);
		return prefabGo;
	}

	public PrefabGo Load(string path, bool isGlobal = false, EPrefabType prefabType = EPrefabType.Ngui, bool isPooling = true, bool touchMaterial = false)
	{
		if (String_Extension_Method.IsNullOrEmpty(path))
		{
			return null;
		}
		return LoadInPlaying(new PrefabKey
		{
			IsGlobal = isGlobal,
			IsPooling = isPooling,
			TouchMaterial = touchMaterial,
			PrefabType = prefabType,
			Type = typeof(GameObject),
			Path = path
		});
	}

	private PrefabGo LoadInPlaying(PrefabKey key)
	{
		IPrefabBase vALUE = _dic.GETVALUE(key, null);
		if (vALUE != null)
		{
			return vALUE as PrefabGo;
		}
		GameObject gameObject = VespaResource.Load<GameObject>(key.Path);
		if (gameObject == null)
		{
			return null;
		}
		PrefabGo prefabGo = new PrefabGo();
		prefabGo.Open(key, gameObject);
		_dic.Add(key, prefabGo);
		return prefabGo;
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
