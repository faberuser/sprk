using System.Collections.Generic;
using NVespa;

namespace NShared;

public class OperationModifierContainer
{
	private Dictionary<int, IOperationModifier> _modifierDictionary = new Dictionary<int, IOperationModifier>();

	private void Init()
	{
		Register<OperationModifier_840>(840);
		Register<OperationModifier_700310>(700310);
		Register<OperationModifier_710310>(710310);
		Register<OperationModifier_1630>(1630);
		Register<OperationModifier_2630>(2630);
		Register<OperationModifier_3430>(3430);
	}

	public OperationModifierContainer()
	{
		Init();
	}

	protected void Register<T>(int skillIndex) where T : IOperationModifier, new()
	{
		if (_modifierDictionary.Remove(skillIndex))
		{
			VespaDebug.LogError("Duplicated operation-modifier : skillIndex=" + skillIndex);
		}
		_modifierDictionary.Add(skillIndex, new T());
	}

	public IOperationModifier GetModifier(int skillIndex)
	{
		IOperationModifier value = null;
		_modifierDictionary.TryGetValue(skillIndex, out value);
		return value;
	}
}
