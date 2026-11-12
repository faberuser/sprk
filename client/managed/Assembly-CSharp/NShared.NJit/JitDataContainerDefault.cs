using System;
using System.Collections;
using System.Collections.Generic;

namespace NShared.NJit;

public abstract class JitDataContainerDefault<K, V> : JitDataContainer<V>, IGetData_for_GMS<K, V>, IGetAllData_for_GMS<V> where V : class, IDataKeySelector<K>
{
	protected JitDictionaryWithKey<K, V> BaseDic { get; private set; }

	public JitDataContainerDefault()
	{
		BaseDic = new JitDictionaryWithKey<K, V>();
		base.Packer.RegisterAndBind(BaseDic);
	}

	public V GetData(K key)
	{
		if (BaseDic.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	protected sealed override void LoadFromJsonData(object jsonData)
	{
		if (jsonData == null)
		{
			JitOption.JitLogError("Failed to load table : " + PathDefine.ToString());
			return;
		}
		if (!(jsonData is List<object> list))
		{
			JitOption.JitLogError("Parsing table error : " + PathDefine.ToString());
			return;
		}
		foreach (IDictionary item in list)
		{
			try
			{
				if (JsonMarshalerUsingReflection.Parse(typeof(V), item) is V val)
				{
					OnLoadingValue_from_JsonData(item, val);
					K key = val.Key;
					if (BaseDic.JsonLoad_Get().ContainsKey(key))
					{
						JitOption.JitLogWarning($"[Jit Table] Duplicate Key!!!! ({val.GetType()}, {key})");
						continue;
					}
					BaseDic.JsonLoad_Get().Add(val.Key, val);
					base.BaseList.JsonLoad_GetOrNew().Add(val);
				}
			}
			catch (Exception ex)
			{
				JitOption.JitLogError("Error while add data : " + ex);
			}
		}
	}
}
