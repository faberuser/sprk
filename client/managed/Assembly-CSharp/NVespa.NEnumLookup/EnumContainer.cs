using System.Collections.Generic;

namespace NVespa.NEnumLookup;

public class EnumContainer : SingletonAuto<EnumContainer>
{
	public static object LockObject = new object();

	private List<IEnumLookup> _list = new List<IEnumLookup>();

	public void AddLookup(IEnumLookup lookup)
	{
		_list.Add(lookup);
	}

	public void ClearEnumLookup()
	{
		for (int i = 0; i < _list.Count; i++)
		{
			_list[i].ReleaseSelf();
		}
		_list = new List<IEnumLookup>();
	}
}
