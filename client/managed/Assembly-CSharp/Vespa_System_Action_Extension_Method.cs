using System;

public static class Vespa_System_Action_Extension_Method
{
	public static void fire(this Action i_action)
	{
		i_action?.Invoke();
	}

	public static void fire<TypeA>(this Action<TypeA> i_action, TypeA i_argA)
	{
		i_action?.Invoke(i_argA);
	}

	public static void fire<TypeA, TypeB>(this Action<TypeA, TypeB> i_action, TypeA i_argA, TypeB i_argB)
	{
		i_action?.Invoke(i_argA, i_argB);
	}

	public static void fire<TypeA, TypeB, TypeC>(this Action<TypeA, TypeB, TypeC> i_action, TypeA i_argA, TypeB i_argB, TypeC i_argC)
	{
		i_action?.Invoke(i_argA, i_argB, i_argC);
	}

	public static void fire<TypeA, TypeB, TypeC, TypeD>(this Action<TypeA, TypeB, TypeC, TypeD> i_action, TypeA i_argA, TypeB i_argB, TypeC i_argC, TypeD i_argD)
	{
		i_action?.Invoke(i_argA, i_argB, i_argC, i_argD);
	}

	public static TResult fire<TResult>(this Func<TResult> i_func)
	{
		if (i_func == null)
		{
			return default(TResult);
		}
		return i_func();
	}

	public static TResult fire<TypeA, TResult>(this Func<TypeA, TResult> i_func, TypeA i_argA)
	{
		if (i_func == null)
		{
			return default(TResult);
		}
		return i_func(i_argA);
	}

	public static TResult fire<TypeA, TypeB, TResult>(this Func<TypeA, TypeB, TResult> i_func, TypeA i_argA, TypeB i_argB)
	{
		if (i_func == null)
		{
			return default(TResult);
		}
		return i_func(i_argA, i_argB);
	}

	public static TResult fire<TypeA, TypeB, TypeC, TResult>(this Func<TypeA, TypeB, TypeC, TResult> i_func, TypeA i_argA, TypeB i_argB, TypeC i_argC)
	{
		if (i_func == null)
		{
			return default(TResult);
		}
		return i_func(i_argA, i_argB, i_argC);
	}
}
