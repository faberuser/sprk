using System;
using System.Collections;
using System.Collections.Generic;

namespace NShared.NJit;

public abstract class JitDataContainerListOnly<V> : JitDataContainer<V> where V : class
{
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
