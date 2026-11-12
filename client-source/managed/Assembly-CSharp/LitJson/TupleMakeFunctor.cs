using NVespa;

namespace LitJson;

public class TupleMakeFunctor<T1, T2> : IMakeFunctor<Tuple<T1, T2>>, IFunctor
{
	private IMakeFunctor<T1> _t1Functor;

	private IMakeFunctor<T2> _t2Functor;

	public TupleMakeFunctor()
	{
		_t1Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T1>();
		_t2Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T2>();
	}

	public TupleMakeFunctor(IMakeFunctor<T1> t1Functor, IMakeFunctor<T2> t2Functor)
	{
		_t1Functor = t1Functor;
		_t2Functor = t2Functor;
	}

	JsonData IMakeFunctor<Tuple<T1, T2>>.MakeJson(Tuple<T1, T2> tuple)
	{
		JsonData jsonData = new JsonData();
		if (_t1Functor != null)
		{
			jsonData.AddField("Item1", _t1Functor.MakeJson(tuple.Item1));
		}
		if (_t2Functor != null)
		{
			jsonData.AddField("Item2", _t2Functor.MakeJson(tuple.Item2));
		}
		return jsonData;
	}
}
public class TupleMakeFunctor<T1, T2, T3> : IMakeFunctor<Tuple<T1, T2, T3>>, IFunctor
{
	private IMakeFunctor<T1> _t1Functor;

	private IMakeFunctor<T2> _t2Functor;

	private IMakeFunctor<T3> _t3Functor;

	public TupleMakeFunctor()
	{
		_t1Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T1>();
		_t2Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T2>();
		_t3Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T3>();
	}

	public TupleMakeFunctor(IMakeFunctor<T1> t1Functor, IMakeFunctor<T2> t2Functor, IMakeFunctor<T3> t3Functor)
	{
		_t1Functor = t1Functor;
		_t2Functor = t2Functor;
		_t3Functor = t3Functor;
	}

	JsonData IMakeFunctor<Tuple<T1, T2, T3>>.MakeJson(Tuple<T1, T2, T3> tuple)
	{
		JsonData jsonData = new JsonData();
		if (_t1Functor != null)
		{
			jsonData.AddField("Item1", _t1Functor.MakeJson(tuple.Item1));
		}
		if (_t2Functor != null)
		{
			jsonData.AddField("Item2", _t2Functor.MakeJson(tuple.Item2));
		}
		if (_t3Functor != null)
		{
			jsonData.AddField("Item3", _t3Functor.MakeJson(tuple.Item3));
		}
		return jsonData;
	}
}
public class TupleMakeFunctor<T1, T2, T3, T4> : IMakeFunctor<Tuple<T1, T2, T3, T4>>, IFunctor
{
	private IMakeFunctor<T1> _t1Functor;

	private IMakeFunctor<T2> _t2Functor;

	private IMakeFunctor<T3> _t3Functor;

	private IMakeFunctor<T4> _t4Functor;

	public TupleMakeFunctor()
	{
		_t1Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T1>();
		_t2Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T2>();
		_t3Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T3>();
		_t4Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T4>();
	}

	public TupleMakeFunctor(IMakeFunctor<T1> t1Functor, IMakeFunctor<T2> t2Functor, IMakeFunctor<T3> t3Functor, IMakeFunctor<T4> t4Functor)
	{
		_t1Functor = t1Functor;
		_t2Functor = t2Functor;
		_t3Functor = t3Functor;
		_t4Functor = t4Functor;
	}

	JsonData IMakeFunctor<Tuple<T1, T2, T3, T4>>.MakeJson(Tuple<T1, T2, T3, T4> tuple)
	{
		JsonData jsonData = new JsonData();
		if (_t1Functor != null)
		{
			jsonData.AddField("Item1", _t1Functor.MakeJson(tuple.Item1));
		}
		if (_t2Functor != null)
		{
			jsonData.AddField("Item2", _t2Functor.MakeJson(tuple.Item2));
		}
		if (_t3Functor != null)
		{
			jsonData.AddField("Item3", _t3Functor.MakeJson(tuple.Item3));
		}
		if (_t4Functor != null)
		{
			jsonData.AddField("Item4", _t4Functor.MakeJson(tuple.Item4));
		}
		return jsonData;
	}
}
public class TupleMakeFunctor<T1, T2, T3, T4, T5> : IMakeFunctor<Tuple<T1, T2, T3, T4, T5>>, IFunctor
{
	private IMakeFunctor<T1> _t1Functor;

	private IMakeFunctor<T2> _t2Functor;

	private IMakeFunctor<T3> _t3Functor;

	private IMakeFunctor<T4> _t4Functor;

	private IMakeFunctor<T5> _t5Functor;

	public TupleMakeFunctor()
	{
		_t1Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T1>();
		_t2Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T2>();
		_t3Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T3>();
		_t4Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T4>();
		_t5Functor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T5>();
	}

	public TupleMakeFunctor(IMakeFunctor<T1> t1Functor, IMakeFunctor<T2> t2Functor, IMakeFunctor<T3> t3Functor, IMakeFunctor<T4> t4Functor, IMakeFunctor<T5> t5Functor)
	{
		_t1Functor = t1Functor;
		_t2Functor = t2Functor;
		_t3Functor = t3Functor;
		_t4Functor = t4Functor;
		_t5Functor = t5Functor;
	}

	JsonData IMakeFunctor<Tuple<T1, T2, T3, T4, T5>>.MakeJson(Tuple<T1, T2, T3, T4, T5> tuple)
	{
		JsonData jsonData = new JsonData();
		if (_t1Functor != null)
		{
			jsonData.AddField("Item1", _t1Functor.MakeJson(tuple.Item1));
		}
		if (_t2Functor != null)
		{
			jsonData.AddField("Item2", _t2Functor.MakeJson(tuple.Item2));
		}
		if (_t3Functor != null)
		{
			jsonData.AddField("Item3", _t3Functor.MakeJson(tuple.Item3));
		}
		if (_t4Functor != null)
		{
			jsonData.AddField("Item4", _t4Functor.MakeJson(tuple.Item4));
		}
		if (_t5Functor != null)
		{
			jsonData.AddField("Item5", _t5Functor.MakeJson(tuple.Item5));
		}
		return jsonData;
	}
}
