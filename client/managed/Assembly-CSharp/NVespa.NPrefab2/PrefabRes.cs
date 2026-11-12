using System.Text;
using UnityEngine;

namespace NVespa.NPrefab2;

public class PrefabRes<T> : IPrefabBase where T : Object
{
	public bool IsClosed { get; private set; }

	public PrefabKey Key { get; private set; }

	public T Res { get; private set; }

	PrefabKey IPrefabBase.Key => Key;

	public void Open(PrefabKey key, T res)
	{
		Key = key;
		Res = res;
		IsClosed = false;
	}

	void IPrefabBase.Close()
	{
		if (Res != null)
		{
			Res = null;
		}
		IsClosed = true;
	}

	void IPrefabBase.Dump(StringBuilder builder)
	{
		builder.AppendLine(string.Format("{0}, IsGlobal={1}, Type={2}, IsPooling={3}, TouchMaterial={3}", Key.Path, Key.IsGlobal, Key.Type.Name, Key.IsPooling, Key.TouchMaterial));
	}
}
