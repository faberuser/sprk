using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using NGame2.NLogin.NState;
using NGameAsset;
using NShared.NContentsDefine;
using UnityEngine;

namespace NShared;

public static class ContentsDefine
{
	public interface IDefine
	{
	}

	private static Dictionary<Type, object> _dic = new Dictionary<Type, object>();

	public static Attendance Attendance => GetDefine<Attendance>();

	public static CurrencyMaxCap CurrencyMaxCap => GetDefine<CurrencyMaxCap>();

	public static MatchArena MatchArena => GetDefine<MatchArena>();

	public static OfficialChannel OfficialChannel => GetDefine<OfficialChannel>();

	public static ReturnUserJumping ReturnUserJumping => GetDefine<ReturnUserJumping>();

	public static NShared.NContentsDefine.SoulWeaponLimitBreak SoulWeaponLimitBreak => GetDefine<NShared.NContentsDefine.SoulWeaponLimitBreak>();

	public static T GetDefine<T>() where T : class, IDefine, new()
	{
		lock (_dic)
		{
			if (!CanLoadDefine())
			{
				return null;
			}
			if (_dic.TryGetValue(typeof(T), out var value))
			{
				return value as T;
			}
			T val = CreateFromFile<T>();
			if (val == null)
			{
				val = new T();
			}
			_dic.Add(typeof(T), val);
			return val;
		}
	}

	private static bool CanLoadDefine()
	{
		if (!Application.isPlaying)
		{
			return true;
		}
		if (Define.LocalAssetLoading.Value)
		{
			return true;
		}
		if (!StateBase_PatchDownload.IsReady)
		{
			return false;
		}
		return true;
	}

	public static void ClearAll()
	{
		_dic.Clear();
	}

	private static T CreateFromFile<T>() where T : class, IDefine, new()
	{
		string loadPath = GetLoadPath<T>();
		if (!File.Exists(loadPath))
		{
			return null;
		}
		object obj = Json.Deserialize(File.ReadAllText(loadPath));
		if (obj is List<object> value)
		{
			obj = new Dictionary<string, object> { { "Datas", value } };
		}
		if (obj is IDictionary d)
		{
			return CustomJsonMarshaler.CustomParse<T>(d);
		}
		return null;
	}

	private static string GetLoadPath<T>()
	{
		string name = typeof(T).Name;
		return Path.GetFullPath($"{GlobalPathDefine.TableData_ContentsDefine}/{name}.json");
	}
}
