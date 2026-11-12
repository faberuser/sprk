using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NShared.NJit.NCodeMD5;

namespace NShared.NJit;

public abstract class JitDataContainer<V> : JitDataContainerAbstract, IGetAllData_for_GMS<V> where V : class
{
	private JitRef<List<V>> _baseList = new JitRef<List<V>>();

	private JitRef<V> _firstData = new JitRef<V>();

	private int _byteLength;

	public sealed override bool IsLoaded { get; protected set; }

	protected JitPacker Packer { get; private set; }

	protected JitRef<List<V>> BaseList => _baseList;

	public JitDataContainer()
	{
		Packer = new JitPacker();
		Packer.RegisterAndBind(_baseList);
		Packer.RegisterAndBind(_firstData);
	}

	public IList<V> GetAllData()
	{
		return BaseList.Eval();
	}

	public V GetFirstData()
	{
		return _firstData.Eval();
	}

	public void ForEachData(Action<V> action)
	{
		if (action == null)
		{
			return;
		}
		foreach (V allDatum in GetAllData())
		{
			action(allDatum);
		}
	}

	public sealed override void Load()
	{
		if (IsLoaded)
		{
			return;
		}
		lock (this)
		{
			LoadInternal();
			IsLoaded = true;
		}
	}

	private void LoadInternal()
	{
		if (LoadFromJit())
		{
			OnEndLoad_Final();
		}
		else if (!JitOption.LoadJitOnly)
		{
			LoadFromJson();
			if (JitOption.IsClient || ForceRebuildJit.Rebuild)
			{
				byte[] mD = JitOption.JsonLoader.GetMD5(PathDefine);
				byte[] codeHash = CodeMD5_Calculator.CalcMD5(GetType(), "Serialize");
				Packer.Serialize(PathDefine, mD, codeHash);
			}
			OnEndLoad_Final();
		}
	}

	private bool LoadFromJit()
	{
		if (ForceRebuildJit.Rebuild)
		{
			return false;
		}
		try
		{
			byte[] array = JitOption.JitLoader.Load(PathDefine);
			if (array == null)
			{
				JitOption.JitLog($"File(.jit) not exist : {PathDefine.Path}");
				return false;
			}
			_byteLength = array.Length;
			Packer.Deserialize(array);
		}
		catch (Exception ex)
		{
			JitOption.JitLogError($"Exception Occur : {PathDefine.Path}\n{ex.ToString()}");
			Packer.Reset();
			return false;
		}
		return true;
	}

	private void LoadFromJson()
	{
		object obj = JitOption.JsonLoader.Load(PathDefine);
		if (obj != null)
		{
			LoadFromJsonData(obj);
		}
		OnEndLoadFromJsonData();
		using List<V>.Enumerator enumerator = _baseList.JsonLoad_GetOrNew().GetEnumerator();
		if (enumerator.MoveNext())
		{
			V current = enumerator.Current;
			_firstData.JsonLoad_Set(current);
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void VerifyDoRegisterAndBind()
	{
		Type type = GetType();
		Type typeFromHandle = typeof(IJitContainer);
		new List<MemberInfo>();
		Type type2 = type;
		while (!(type2 == typeof(object)))
		{
			Type type3 = type2;
			type2 = type3.BaseType;
			MemberInfo[] members = type3.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (members == null)
			{
				break;
			}
			foreach (MemberInfo memberInfo in members)
			{
				Type memberValueType = UsingReflection.GetMemberValueType(memberInfo);
				if (!(memberValueType == null) && memberValueType.IsClass && typeFromHandle.IsAssignableFrom(memberValueType) && UsingReflection.GetMemberValue(memberInfo, this) is IJitContainer jitContainer && !jitContainer.IsPackerValid())
				{
					JitOption.JitLogError($"{memberInfo.Name} in {type3}: Packer.RegisterAndBind 호출이 필요");
				}
			}
		}
	}

	protected abstract void LoadFromJsonData(object jsonData);

	protected virtual void OnLoadingValue_from_JsonData(IDictionary jsonDic, V value)
	{
	}

	protected virtual void OnEndLoadFromJsonData()
	{
	}

	protected virtual void OnEndLoad_Final()
	{
	}
}
