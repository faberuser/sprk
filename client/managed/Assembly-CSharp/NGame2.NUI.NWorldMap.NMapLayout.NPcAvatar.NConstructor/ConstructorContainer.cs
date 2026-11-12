using System.Collections.Generic;
using NVespa;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public class ConstructorContainer : SingletonAuto<ConstructorContainer>
{
	private Dictionary<string, BaseConstructor> dic = new Dictionary<string, BaseConstructor>();

	public ConstructorContainer()
	{
		Register(new GuildPcAvatarConstructor());
	}

	public void Register<T>(T iConstructor) where T : BaseConstructor
	{
		string name = iConstructor.GetType().Name;
		dic.Remove(name);
		dic.Add(name, iConstructor);
	}

	public BaseConstructor GetConstructor(string key)
	{
		if (String_Extension_Method.IsNullOrEmpty(key))
		{
			return null;
		}
		dic.TryGetValue(key, out var value);
		return value;
	}
}
