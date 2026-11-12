using System;
using System.Collections.Generic;
using System.IO;
using NVespa;

namespace NShared;

public static class SnapshotFactory
{
	private class CreatorInfo
	{
		public Type Type;

		public Func<ISnapshotObject> Creator;
	}

	private static Dictionary<string, CreatorInfo> _creatorDictionary;

	static SnapshotFactory()
	{
		_creatorDictionary = new Dictionary<string, CreatorInfo>();
		using (new SnapshotWritableBlock())
		{
			new BattleInstance();
			new BattleCreature();
		}
	}

	public static ISnapshotObject Create(string typeName)
	{
		using (new SnapshotWritableBlock())
		{
			_creatorDictionary.TryGetValue(typeName, out var value);
			if (value == null)
			{
				return null;
			}
			ISnapshotObject snapshotObject = value.Creator();
			CheckCreatedType(value, snapshotObject);
			return snapshotObject;
		}
	}

	public static bool HasCreator(string typeName)
	{
		return _creatorDictionary.ContainsKey(typeName);
	}

	public static void Register<T>(string typeName = null) where T : class, ISnapshotObject, new()
	{
		using (new SnapshotWritableBlock())
		{
			Type typeFromHandle = typeof(T);
			typeName = typeName ?? typeFromHandle.Name;
			if (!_creatorDictionary.ContainsKey(typeName))
			{
				SnapshotObjectPool<T>.InitInstance(() => new T());
				CreatorInfo creatorInfo = new CreatorInfo();
				creatorInfo.Type = typeFromHandle;
				creatorInfo.Creator = () => new T();
				_creatorDictionary.Add(typeName, creatorInfo);
				T created = new T();
				CheckCreatedType(creatorInfo, created);
			}
		}
	}

	private static bool CheckCreatedType(CreatorInfo info, object created)
	{
		if (info == null)
		{
			return false;
		}
		if (created == null)
		{
			VespaDebug.LogError("Snapshot creator returns null. type= " + info.Type.Name);
			return false;
		}
		Type type = info.Type;
		Type type2 = created.GetType();
		if (type2 != type)
		{
			VespaDebug.LogError("Snapshot type mismatch. staticType= " + type.Name + " dynamicType=" + type2.Name);
			return false;
		}
		return true;
	}

	public static void UnitTest()
	{
		string path = "D:\\work\\r1\\Program\\GameClient\\unity_project\\Snapshot.txt";
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		if (array != null && array.Length != 0)
		{
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				_ = array2[i];
			}
		}
	}
}
