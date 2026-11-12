using System.Collections.Generic;
using NVespa;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public class AvatarConstructorFactoryContainer : SingletonAuto<AvatarConstructorFactoryContainer>
{
	private Dictionary<string, IPcAvatarFactory> dic = new Dictionary<string, IPcAvatarFactory>();

	public AvatarConstructorFactoryContainer()
	{
		Register(new GuildPcAvatarFactory());
		Register(new CreatureAvatarFactory());
	}

	public void Register<T>(T iConstructor) where T : IPcAvatarFactory
	{
		string name = iConstructor.GetType().Name;
		dic.Remove(name);
		dic.Add(name, iConstructor);
	}

	public IPcAvatarFactory GetConstructor(string key)
	{
		if (String_Extension_Method.IsNullOrEmpty(key))
		{
			return null;
		}
		dic.TryGetValue(key, out var value);
		return value;
	}

	public void ClearCache()
	{
		Dictionary<string, IPcAvatarFactory>.Enumerator enumerator = dic.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.ClearCache();
		}
	}
}
