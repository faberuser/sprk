using UnityEngine;

namespace NNguiExtension;

public abstract class UIBasicSelector : MonoBehaviour
{
	public abstract bool Select(string name);

	public bool SelectInt(int value)
	{
		return Select(Vespa_Int_Extension_Method.ToStringCache(value));
	}

	public bool SelectEnum<T>(T value) where T : struct
	{
		return Select(Vespa_String_Extension_Method.GetEnumString(value));
	}

	public void SelectByIndex0()
	{
		SelectByIndex(0);
	}

	public void SelectByIndex1()
	{
		SelectByIndex(1);
	}

	public void SelectByIndex2()
	{
		SelectByIndex(2);
	}

	public void SelectByIndex3()
	{
		SelectByIndex(3);
	}

	public void SelectByIndex4()
	{
		SelectByIndex(4);
	}

	public void SelectByIndex5()
	{
		SelectByIndex(5);
	}

	public abstract void SelectByIndex(int index);
}
