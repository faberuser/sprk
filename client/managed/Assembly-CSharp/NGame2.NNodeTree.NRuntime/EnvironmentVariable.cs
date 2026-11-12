using System.Collections.Generic;
using LitJson;
using NGame2.NBattleField;
using NGame2.NNodeTree.NRuntime.NVariable;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NRuntime;

public class EnvironmentVariable : SingletonAuto<EnvironmentVariable>
{
	public static readonly StringKey Env_GroundMin = StringKey.Get("Env_GroundMin");

	public static readonly StringKey Env_GroundMax = StringKey.Get("Env_GroundMax");

	private StringKeyDictionary<IVariableValueBase> _valueDic = new StringKeyDictionary<IVariableValueBase>();

	private int _frameCount;

	private VectorXZ _groundMin;

	private VectorXZ _groundMax;

	public EnvironmentVariable()
	{
		_valueDic.Add(Env_GroundMin, new ValueGenericFunc<Vector3>(GetGroundMin));
		_valueDic.Add(Env_GroundMax, new ValueGenericFunc<Vector3>(GetGroundMax));
	}

	private Vector3 GetGroundMin()
	{
		UpdateVariable();
		return _groundMin.ToVec3();
	}

	private Vector3 GetGroundMax()
	{
		UpdateVariable();
		return _groundMax.ToVec3();
	}

	private void UpdateVariable()
	{
		if (_frameCount != Time.frameCount)
		{
			_frameCount = Time.frameCount;
			if (GetGroundFromBattleInstance(out _groundMin, out _groundMax))
			{
				_groundMin -= VectorXZ.one;
				_groundMax += VectorXZ.one;
			}
			else
			{
				_groundMin = VectorXZ.zero;
				_groundMax = VectorXZ.zero;
			}
		}
	}

	private bool GetGroundFromBattleInstance(out VectorXZ o_min, out VectorXZ o_max)
	{
		o_min = VectorXZ.zero;
		o_max = VectorXZ.zero;
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return false;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		o_min.x = (float)battleCache.BattleArea.MinMx * 0.001f;
		o_min.z = (float)battleCache.BattleArea.MinMz * 0.001f;
		o_max.x = (float)battleCache.BattleArea.MaxMx * 0.001f;
		o_max.z = (float)battleCache.BattleArea.MaxMz * 0.001f;
		return true;
	}

	public IVariableValueBase GetValueBase(StringKey key)
	{
		if (key.IsNullOrEmpty())
		{
			return null;
		}
		if (_valueDic.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public JsonData DumpJson()
	{
		Dictionary<StringKey, IVariableValueBase> dictionary = _valueDic.CreateDictionary();
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Variable Type", "LocalVariable");
		foreach (KeyValuePair<StringKey, IVariableValueBase> item in dictionary)
		{
			JsonData value = item.Value.DumpJson();
			jsonData.AddField(item.Key.Value, value);
		}
		return jsonData;
	}
}
