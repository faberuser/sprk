using NShared;

public static class ITriggerValueGetter_ExtensionMethod
{
	public static bool GetTriggerValues(this ITriggerValueGetter self, BattleCreature firedCreature, int valueIndex, out int[] values, char splitter = ';')
	{
		values = null;
		string triggerValue = self.GetTriggerValue(firedCreature, valueIndex);
		if (string.IsNullOrEmpty(triggerValue))
		{
			return false;
		}
		string[] array = triggerValue.Split(new char[1] { splitter });
		if (array == null || array.Length == 0)
		{
			return false;
		}
		values = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			int result = 0;
			int.TryParse(array[i], out result);
			values[i] = result;
		}
		return true;
	}

	public static bool GetTriggerValue(this ITriggerValueGetter self, BattleCreature firedCreature, int valueIndex, out int value)
	{
		string triggerValue = self.GetTriggerValue(firedCreature, valueIndex);
		if (string.IsNullOrEmpty(triggerValue))
		{
			value = 0;
			return false;
		}
		int.TryParse(triggerValue, out value);
		return true;
	}

	public static bool GetTriggerValue(this ITriggerValueGetter self, BattleCreature firedCreature, int valueIndex, out string value)
	{
		string triggerValue = self.GetTriggerValue(firedCreature, valueIndex);
		if (string.IsNullOrEmpty(triggerValue))
		{
			value = null;
			return false;
		}
		value = triggerValue;
		return true;
	}

	public static bool GetTriggerValueT<T>(this ITriggerValueGetter self, BattleCreature firedCreature, int valueIndex, out T value) where T : struct
	{
		string triggerValue = self.GetTriggerValue(firedCreature, valueIndex);
		if (string.IsNullOrEmpty(triggerValue))
		{
			value = default(T);
			return false;
		}
		value = BaseJsonMarshaler.To<T>(triggerValue);
		return true;
	}
}
