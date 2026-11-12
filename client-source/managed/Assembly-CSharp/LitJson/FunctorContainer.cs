using System;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace LitJson;

public class FunctorContainer : SingletonAuto<FunctorContainer>
{
	private Dictionary<Type, IFunctor> _loadDic = new Dictionary<Type, IFunctor>();

	private Dictionary<Type, IFunctor> _makeDic = new Dictionary<Type, IFunctor>();

	protected override bool ThreadSafe => true;

	public ILoadFunctor<T> GetLoadFunctorIfExist<T>()
	{
		lock (_loadDic)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_loadDic, typeof(T), null) as ILoadFunctor<T>;
		}
	}

	public ILoadFunctor<T> GetLoadFunctor<T>() where T : new()
	{
		lock (_loadDic)
		{
			Type typeFromHandle = typeof(T);
			if (Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_loadDic, typeFromHandle, null) is ILoadFunctor<T> result)
			{
				return result;
			}
			if (typeFromHandle.IsEnum)
			{
				ILoadFunctor<T> loadFunctor = new EnumLoadFunctor<T>();
				_loadDic.Add(typeFromHandle, loadFunctor);
				return loadFunctor;
			}
			if (typeFromHandle.IsClass && typeof(ILoadJson).IsAssignableFrom(typeFromHandle))
			{
				ILoadFunctor<T> loadFunctor = new JsonLoadFunctor<T>();
				_loadDic.Add(typeFromHandle, loadFunctor);
				return loadFunctor;
			}
			return null;
		}
	}

	public IMakeFunctor<T> GetMakeFunctor<T>()
	{
		lock (_makeDic)
		{
			Type typeFromHandle = typeof(T);
			if (Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_makeDic, typeFromHandle, null) is IMakeFunctor<T> result)
			{
				return result;
			}
			if (typeFromHandle.IsEnum)
			{
				IMakeFunctor<T> makeFunctor = new EnumMakeFunctor<T>();
				_makeDic.Add(typeFromHandle, makeFunctor);
				return makeFunctor;
			}
			if (typeFromHandle.IsClass && typeof(IMakeJson).IsAssignableFrom(typeFromHandle))
			{
				IMakeFunctor<T> makeFunctor = new JsonMakeFunctor<T>();
				_makeDic.Add(typeFromHandle, makeFunctor);
				return makeFunctor;
			}
			return null;
		}
	}

	public void AddLoadFunctor<T>(Func<JsonData, T> func)
	{
		lock (_loadDic)
		{
			_loadDic.Add(typeof(T), new GenericLoadFunctor<T>
			{
				Func = func
			});
		}
	}

	public void AddMakeFunctor<T>(Func<T, JsonData> func)
	{
		lock (_makeDic)
		{
			_makeDic.Add(typeof(T), new GenericMakeFunctor<T>
			{
				Func = func
			});
		}
	}

	public FunctorContainer()
	{
		SingletonAuto<InitialConstructor>.CreateInstance();
		AddLoadFunctor((JsonData json) => json.GETBOOLEAN(i_default: false));
		AddMakeFunctor((bool value) => new JsonData(value));
		AddLoadFunctor((JsonData json) => json.GETINT(0));
		AddMakeFunctor((int value) => new JsonData(value));
		AddLoadFunctor((JsonData json) => json.GETLONG(0L));
		AddMakeFunctor((long value) => new JsonData(value));
		AddLoadFunctor((JsonData json) => json.GETFLOAT(0f));
		AddMakeFunctor((float value) => new JsonData(value));
		AddLoadFunctor((JsonData json) => json.GETDOUBLE(0.0));
		AddMakeFunctor((double value) => new JsonData(value));
		AddLoadFunctor((JsonData json) => json.GETSTRING(string.Empty));
		AddMakeFunctor((string value) => new JsonData(value));
		AddLoadFunctor((JsonData json) => json.GETCOLOR(Color.white));
		AddMakeFunctor((Color value) => JsonTools.FromColor(value));
		AddLoadFunctor((JsonData json) => json.GETVECTOR2(Vector2.zero));
		AddMakeFunctor((Vector2 value) => JsonTools.FromVector2(value));
		AddLoadFunctor((JsonData json) => JsonTools.ToVector3(json, Vector3.zero));
		AddMakeFunctor((Vector3 value) => JsonTools.FromVector3(value));
		AddLoadFunctor((JsonData json) => JsonTools.ToVector4(json, Vector4.zero));
		AddMakeFunctor((Vector4 value) => JsonTools.FromVector4(value));
		AddLoadFunctor((JsonData json) => JsonTools.ToVectorXZ(json, VectorXZ.zero));
		AddMakeFunctor((VectorXZ value) => JsonTools.FromVectorXZ(value));
		AddLoadFunctor((JsonData json) => JsonTools.ToQuaternion(json, Quaternion.identity));
		AddMakeFunctor((Quaternion value) => JsonTools.FromQuaternion(value));
		AddLoadFunctor((JsonData json) => JsonTools.ToCurve(json));
		AddMakeFunctor((AnimationCurve value) => JsonTools.FromCurve(value));
		AddLoadFunctor((JsonData json) => JsonTools.ToGradient(json));
		AddMakeFunctor((Gradient value) => JsonTools.FromGradient(value));
	}
}
