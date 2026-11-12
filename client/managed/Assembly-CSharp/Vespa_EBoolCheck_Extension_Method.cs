using NVespa;

public static class Vespa_EBoolCheck_Extension_Method
{
	public static bool Check(this EBoolCheck i_this, bool i_boolean)
	{
		switch (i_this)
		{
		case EBoolCheck.DontCare:
			return true;
		case EBoolCheck.True:
			if (i_boolean)
			{
				return true;
			}
			break;
		}
		if (i_this == EBoolCheck.False && !i_boolean)
		{
			return true;
		}
		return false;
	}
}
