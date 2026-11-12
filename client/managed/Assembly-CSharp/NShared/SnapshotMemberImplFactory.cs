using System;
using System.Collections.Generic;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class SnapshotMemberImplFactory
{
	public static bool UseImpl = true;

	private static ISnapshotMemberSerializer _intImpl = new SnapshotIntSerializer();

	private static ISnapshotMemberSerializer _longImpl = new SnapshotLongSerializer();

	private static ISnapshotMemberSerializer _shortImpl = new SnapshotShortSerializer();

	private static ISnapshotMemberSerializer _stringImpl = new SnapshotStringSerializer();

	private static ISnapshotMemberSerializer _boolImpl = new SnapshotBoolSerializer();

	private static ISnapshotMemberSerializer _vector3Impl = new SnapshotVector3Serializer();

	private static ISnapshotMemberSerializer _objectImpl = new SnapshotObjectSerializer();

	private static Dictionary<Type, ISnapshotMemberSerializer> _enumImpls = new Dictionary<Type, ISnapshotMemberSerializer>();

	public static ISnapshotMemberSerializer GetImpl<T>()
	{
		return GetImpl(typeof(T));
	}

	public static ISnapshotMemberSerializer GetImpl(Type type)
	{
		if (!UseImpl)
		{
			return null;
		}
		if (type == typeof(int))
		{
			return _intImpl;
		}
		if (type == typeof(short))
		{
			return _shortImpl;
		}
		if (type == typeof(long))
		{
			return _longImpl;
		}
		if (type == typeof(string))
		{
			return _stringImpl;
		}
		if (type == typeof(bool))
		{
			return _boolImpl;
		}
		if (type == typeof(Vector3))
		{
			return _vector3Impl;
		}
		if (typeof(ISnapshotObject).IsAssignableFrom(type))
		{
			return _objectImpl;
		}
		if (type.IsEnum)
		{
			ISnapshotMemberSerializer value = null;
			_enumImpls.TryGetValue(type, out value);
			if (value != null)
			{
				return value;
			}
			value = new SnapshotEnumSerializer(type);
			_enumImpls.Add(type, value);
			return value;
		}
		VespaDebug.LogError("Undefined snapshot value type : " + type.Name);
		return null;
	}
}
