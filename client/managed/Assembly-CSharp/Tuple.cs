using System;
using NShared;
using UnityEngine;

[Serializable]
public static class Tuple
{
	private static class GenericHelper<T>
	{
		public static Func<T, int> CalcHashCode;

		public static Func<T, T, bool> EqualsT;

		static GenericHelper()
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsClass || typeFromHandle.IsInterface)
			{
				CalcHashCode = CalcClassHashCode;
				EqualsT = EqualsClass;
			}
			else
			{
				CalcHashCode = CalcStructHashCode;
				EqualsT = EqualsStruct;
			}
		}

		private static bool IsUnityNull(object value)
		{
			if (value == null)
			{
				return true;
			}
			if (value is UnityEngine.Object)
			{
				return value as UnityEngine.Object == null;
			}
			return false;
		}

		private static int CalcClassHashCode(T value)
		{
			if (IsUnityNull(value))
			{
				return 0;
			}
			return value.GetHashCode();
		}

		private static bool EqualsClass(T lhs, T rhs)
		{
			object value = lhs;
			object value2 = rhs;
			bool flag = IsUnityNull(value);
			bool flag2 = IsUnityNull(value2);
			if (flag != flag2)
			{
				return false;
			}
			if (flag)
			{
				return true;
			}
			return object.Equals(lhs, rhs);
		}

		private static int CalcStructHashCode(T value)
		{
			return value.GetHashCode();
		}

		private static bool EqualsStruct(T lhs, T rhs)
		{
			return lhs.Equals(rhs);
		}
	}

	private const string _null = "null";

	private const char _sep = '_';

	private static char[] _seps = new char[1] { '_' };

	public static bool EqualsT<T>(T lhs, T rhs)
	{
		return GenericHelper<T>.EqualsT(lhs, rhs);
	}

	public static int CalcHashCode<T>(T value)
	{
		return GenericHelper<T>.CalcHashCode(value);
	}

	public static Tuple<T1> Create<T1>(T1 i_item1)
	{
		return new Tuple<T1>(i_item1);
	}

	public static Tuple<T1, T2> Create<T1, T2>(T1 i_item1, T2 i_item2)
	{
		return new Tuple<T1, T2>(i_item1, i_item2);
	}

	public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 i_item1, T2 i_item2, T3 i_item3)
	{
		return new Tuple<T1, T2, T3>(i_item1, i_item2, i_item3);
	}

	public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 i_item1, T2 i_item2, T3 i_item3, T4 i_item4)
	{
		return new Tuple<T1, T2, T3, T4>(i_item1, i_item2, i_item3, i_item4);
	}

	public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 i_item1, T2 i_item2, T3 i_item3, T4 i_item4, T5 i_item5)
	{
		return new Tuple<T1, T2, T3, T4, T5>(i_item1, i_item2, i_item3, i_item4, i_item5);
	}

	public static string Serialize<T1>(T1 t1)
	{
		if (t1 == null)
		{
			return "null";
		}
		return t1.ToString();
	}

	public static string Serialize<T1, T2>(T1 t1, T2 t2)
	{
		return Serialize(t1) + "_" + Serialize(t2);
	}

	public static string Serialize<T1, T2, T3>(T1 t1, T2 t2, T3 t3)
	{
		return Serialize(t1, t2) + "_" + Serialize(t3);
	}

	public static string Serialize<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return Serialize(t1, t2, t3) + "_" + Serialize(t4);
	}

	public static string Serialize<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Serialize(t1, t2, t3, t4) + "_" + Serialize(t5);
	}

	private static void Deserialize<T>(string[] tokens, int index, ref T value)
	{
		value = ((tokens.Length > index) ? BaseJsonMarshaler.To<T>(tokens[index]) : default(T));
	}

	public static void Deserialize<T1>(string input, ref T1 t1)
	{
		Deserialize(input.Split(_seps), 0, ref t1);
	}

	public static void Deserialize<T1, T2>(string input, ref T1 t1, ref T2 t2)
	{
		string[] tokens = input.Split(_seps);
		Deserialize(tokens, 0, ref t1);
		Deserialize(tokens, 1, ref t2);
	}

	public static void Deserialize<T1, T2, T3>(string input, ref T1 t1, ref T2 t2, ref T3 t3)
	{
		string[] tokens = input.Split(_seps);
		Deserialize(tokens, 0, ref t1);
		Deserialize(tokens, 1, ref t2);
		Deserialize(tokens, 2, ref t3);
	}

	public static void Deserialize<T1, T2, T3, T4>(string input, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4)
	{
		string[] tokens = input.Split(_seps);
		Deserialize(tokens, 0, ref t1);
		Deserialize(tokens, 1, ref t2);
		Deserialize(tokens, 2, ref t3);
		Deserialize(tokens, 3, ref t4);
	}

	public static void Deserialize<T1, T2, T3, T4, T5>(string input, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5)
	{
		string[] tokens = input.Split(_seps);
		Deserialize(tokens, 0, ref t1);
		Deserialize(tokens, 1, ref t2);
		Deserialize(tokens, 2, ref t3);
		Deserialize(tokens, 3, ref t4);
		Deserialize(tokens, 4, ref t5);
	}
}
[Serializable]
public class Tuple<T1> : ITuple
{
	public T1 Item1;

	public Tuple(T1 t1)
	{
		Item1 = t1;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Tuple<T1> tuple))
		{
			return false;
		}
		return Tuple.EqualsT(Item1, tuple.Item1);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	void ITuple.Deserialize(string input)
	{
		Tuple.Deserialize(input, ref Item1);
	}

	string ITuple.Serialize()
	{
		return Tuple.Serialize(Item1);
	}
}
[Serializable]
public class Tuple<T1, T2> : ITuple
{
	public T1 Item1;

	public T2 Item2;

	public Tuple(T1 i_item1, T2 i_item2)
	{
		Item1 = i_item1;
		Item2 = i_item2;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Tuple<T1, T2> tuple))
		{
			return false;
		}
		if (Tuple.EqualsT(Item1, tuple.Item1))
		{
			return Tuple.EqualsT(Item2, tuple.Item2);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Tuple.CalcHashCode(Item1) ^ Tuple.CalcHashCode(Item2);
	}

	void ITuple.Deserialize(string input)
	{
		Tuple.Deserialize(input, ref Item1, ref Item2);
	}

	string ITuple.Serialize()
	{
		return Tuple.Serialize(Item1, Item2);
	}
}
[Serializable]
public class Tuple<T1, T2, T3> : ITuple
{
	public T1 Item1;

	public T2 Item2;

	public T3 Item3;

	public Tuple(T1 i_item1, T2 i_item2, T3 i_item3)
	{
		Item1 = i_item1;
		Item2 = i_item2;
		Item3 = i_item3;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Tuple<T1, T2, T3> tuple))
		{
			return false;
		}
		if (Tuple.EqualsT(Item1, tuple.Item1) && Tuple.EqualsT(Item2, tuple.Item2))
		{
			return Tuple.EqualsT(Item3, tuple.Item3);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Tuple.CalcHashCode(Item1) ^ Tuple.CalcHashCode(Item2) ^ Tuple.CalcHashCode(Item3);
	}

	void ITuple.Deserialize(string input)
	{
		Tuple.Deserialize(input, ref Item1, ref Item2, ref Item3);
	}

	string ITuple.Serialize()
	{
		return Tuple.Serialize(Item1, Item2, Item3);
	}
}
[Serializable]
public class Tuple<T1, T2, T3, T4> : ITuple
{
	public T1 Item1;

	public T2 Item2;

	public T3 Item3;

	public T4 Item4;

	public Tuple(T1 i_item1, T2 i_item2, T3 i_item3, T4 i_item4)
	{
		Item1 = i_item1;
		Item2 = i_item2;
		Item3 = i_item3;
		Item4 = i_item4;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Tuple<T1, T2, T3, T4> tuple))
		{
			return false;
		}
		if (Tuple.EqualsT(Item1, tuple.Item1) && Tuple.EqualsT(Item2, tuple.Item2) && Tuple.EqualsT(Item3, tuple.Item3))
		{
			return Tuple.EqualsT(Item4, tuple.Item4);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Tuple.CalcHashCode(Item1) ^ Tuple.CalcHashCode(Item2) ^ Tuple.CalcHashCode(Item3) ^ Tuple.CalcHashCode(Item4);
	}

	void ITuple.Deserialize(string input)
	{
		Tuple.Deserialize(input, ref Item1, ref Item2, ref Item3, ref Item4);
	}

	string ITuple.Serialize()
	{
		return Tuple.Serialize(Item1, Item2, Item3, Item4);
	}
}
[Serializable]
public class Tuple<T1, T2, T3, T4, T5> : ITuple
{
	public T1 Item1;

	public T2 Item2;

	public T3 Item3;

	public T4 Item4;

	public T5 Item5;

	public Tuple(T1 i_item1, T2 i_item2, T3 i_item3, T4 i_item4, T5 i_item5)
	{
		Item1 = i_item1;
		Item2 = i_item2;
		Item3 = i_item3;
		Item4 = i_item4;
		Item5 = i_item5;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Tuple<T1, T2, T3, T4, T5> tuple))
		{
			return false;
		}
		if (Tuple.EqualsT(Item1, tuple.Item1) && Tuple.EqualsT(Item2, tuple.Item2) && Tuple.EqualsT(Item3, tuple.Item3) && Tuple.EqualsT(Item4, tuple.Item4))
		{
			return Tuple.EqualsT(Item5, tuple.Item5);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Tuple.CalcHashCode(Item1) ^ Tuple.CalcHashCode(Item2) ^ Tuple.CalcHashCode(Item3) ^ Tuple.CalcHashCode(Item4) ^ Tuple.CalcHashCode(Item5);
	}

	void ITuple.Deserialize(string input)
	{
		Tuple.Deserialize(input, ref Item1, ref Item2, ref Item3, ref Item4, ref Item5);
	}

	string ITuple.Serialize()
	{
		return Tuple.Serialize(Item1, Item2, Item3, Item4, Item5);
	}
}
