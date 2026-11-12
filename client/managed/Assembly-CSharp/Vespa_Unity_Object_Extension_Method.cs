using UnityEngine;

public static class Vespa_Unity_Object_Extension_Method
{
	public static string NAME(this Object i_this)
	{
		if (i_this == null)
		{
			return "(null)";
		}
		return i_this.name;
	}
}
