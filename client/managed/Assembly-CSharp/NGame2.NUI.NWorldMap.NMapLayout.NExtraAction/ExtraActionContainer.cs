using System.Collections.Generic;
using NVespa;

namespace NGame2.NUI.NWorldMap.NMapLayout.NExtraAction;

public class ExtraActionContainer : SingletonAuto<ExtraActionContainer>
{
	private Dictionary<string, IAction> _dic = new Dictionary<string, IAction>();

	public ExtraActionContainer()
	{
		Register(new RequestOpenGuildBuilding());
	}

	private void Register<T>(T ibuilding) where T : IAction, new()
	{
		string name = ibuilding.GetType().Name;
		_dic.Remove(name);
		_dic.Add(name, ibuilding);
	}

	public IAction GetExtraAction(string key)
	{
		_dic.TryGetValue(key, out var value);
		if (value == null)
		{
			return null;
		}
		return value;
	}
}
