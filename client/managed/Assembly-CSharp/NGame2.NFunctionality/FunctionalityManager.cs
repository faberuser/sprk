using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NFunctionality;

public class FunctionalityManager : GlobalAutoBehaviour<FunctionalityManager>, IInspectorGuiDrawer
{
	private readonly Dictionary<FunctionalityType, IFunctionalityValue> _valueDictionary = new Dictionary<FunctionalityType, IFunctionalityValue>();

	private readonly Dictionary<FunctionalityType, string> _defaultDictionary = new Dictionary<FunctionalityType, string>();

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void Init()
	{
		EnumAttributeContainer<FunctionalityType, FunctionalityAttribute>.ForEach(delegate(FunctionalityType type, FunctionalityAttribute attribute)
		{
			Func<IFunctionalityValue> valueCreator = attribute.ValueCreator;
			if (valueCreator != null)
			{
				IFunctionalityValue functionalityValue = Vespa_System_Action_Extension_Method.fire(valueCreator);
				if (functionalityValue != null)
				{
					functionalityValue.SetValue(attribute.DefaultValue);
					_valueDictionary.Add(type, functionalityValue);
					_defaultDictionary.Add(type, attribute.DefaultValue);
				}
			}
		});
	}

	private IFunctionalityValue GetFunction(FunctionalityType type)
	{
		IFunctionalityValue value = null;
		_valueDictionary.TryGetValue(type, out value);
		return value;
	}

	public bool Check<T>(FunctionalityType type, T value) where T : struct
	{
		if (!(GetFunction(type) is GenericValue<T> genericValue))
		{
			return false;
		}
		return genericValue.Check(value);
	}

	public void SetValue(FunctionalityType type, string value)
	{
		GetFunction(type)?.SetValue(value);
	}

	public void SetDefaultValue(FunctionalityType type)
	{
		_defaultDictionary.TryGetValue(type, out var value);
		SetValue(type, value);
	}
}
