using System;
using System.Collections.Generic;
using NGameAsset.NCreature;
using UnityEngine;

namespace JsonBin;

public static class FunctorContainer
{
	public class Functor_ILoadJson<T> : IFunctorJsonRead<T>, IFunctorJsonReadBase where T : new()
	{
		T IFunctorJsonRead<T>.CreateFrom(JsonRead json)
		{
			T val = new T();
			if (val is ILoadJson loadJson)
			{
				loadJson.Load(json);
			}
			return val;
		}
	}

	public class Functor_Enum<T> : IFunctorJsonRead<T>, IFunctorJsonReadBase
	{
		T IFunctorJsonRead<T>.CreateFrom(JsonRead json)
		{
			return json.GETENUM(default(T));
		}
	}

	public class Functor_by_Func<T> : IFunctorJsonRead<T>, IFunctorJsonReadBase
	{
		private Func<JsonRead, T> _func;

		public Functor_by_Func(Func<JsonRead, T> func)
		{
			_func = func;
		}

		T IFunctorJsonRead<T>.CreateFrom(JsonRead json)
		{
			return _func(json);
		}
	}

	private static Type type_of_ILoadJson;

	private static Dictionary<Type, IFunctorJsonReadBase> _cache;

	static FunctorContainer()
	{
		type_of_ILoadJson = typeof(ILoadJson);
		_cache = new Dictionary<Type, IFunctorJsonReadBase>();
		AddCacheByFunc((JsonRead json) => json.GETBOOLEAN(iDefault: false));
		AddCacheByFunc((JsonRead json) => json.GetSByte(0));
		AddCacheByFunc((JsonRead json) => json.GetShort(0));
		AddCacheByFunc((JsonRead json) => json.GetInt(0));
		AddCacheByFunc((JsonRead json) => json.GetLong(0L));
		AddCacheByFunc((JsonRead json) => json.GetByte(0));
		AddCacheByFunc((JsonRead json) => json.GetUShort(0));
		AddCacheByFunc((JsonRead json) => json.GetUInt(0u));
		AddCacheByFunc((JsonRead json) => json.GetULong(0uL));
		AddCacheByFunc((JsonRead json) => json.GetFloat(0f));
		AddCacheByFunc((JsonRead json) => json.GETSTRING(string.Empty));
		AddCacheByFunc((JsonRead json) => json.GETVECTOR2(Vector2.zero));
		AddCacheByFunc((JsonRead json) => json.GETVECTOR3(Vector3.zero));
		AddCacheByFunc((JsonRead json) => json.GETQUATERNION(Quaternion.identity));
		AddCacheByFunc((JsonRead json) => json.GETRECT(Rect.zero));
		AddCacheByFunc((JsonRead json) => json.GETANIMATIONCURVE(null));
		AddCacheByFunc((JsonRead json) => json.GetColor(Color.white));
		AddCacheByFunc((JsonRead json) => json.GetGradient(null));
		AddCacheByFunc(AnimationKeyData.CreateFromJson);
		AddCacheByFunc(AnimationBlendDefault.BlendKey.CreateFromJson);
	}

	private static void AddCacheByFunc<T>(Func<JsonRead, T> func)
	{
		AddCache(new Functor_by_Func<T>(func));
	}

	private static void AddCache<T>(IFunctorJsonRead<T> creator)
	{
		_cache.Add(typeof(T), creator);
	}

	public static IFunctorJsonRead<T> GetUnsafe<T>()
	{
		lock (_cache)
		{
			Type typeFromHandle = typeof(T);
			if (_cache.TryGetValue(typeFromHandle, out var value) && value is IFunctorJsonRead<T> result)
			{
				return result;
			}
			return null;
		}
	}

	public static IFunctorJsonRead<T> Get<T>() where T : new()
	{
		lock (_cache)
		{
			Type typeFromHandle = typeof(T);
			if (_cache.TryGetValue(typeFromHandle, out var value) && value is IFunctorJsonRead<T> result)
			{
				return result;
			}
			IFunctorJsonRead<T> functorJsonRead = CreateFunctor<T>();
			if (functorJsonRead != null)
			{
				_cache.Add(typeFromHandle, functorJsonRead);
			}
			return functorJsonRead;
		}
	}

	private static IFunctorJsonRead<T> CreateFunctor<T>() where T : new()
	{
		Type typeFromHandle = typeof(T);
		if (type_of_ILoadJson.IsAssignableFrom(typeFromHandle))
		{
			return new Functor_ILoadJson<T>();
		}
		if (typeFromHandle.IsEnum)
		{
			return new Functor_Enum<T>();
		}
		return null;
	}
}
