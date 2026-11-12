using NVespa;

namespace LitJson;

public class TupleLoadFunctor<T1, T2> : ILoadFunctor<Tuple<T1, T2>>, IFunctor
{
	private ILoadFunctor<T1> _t1Functor;

	private ILoadFunctor<T2> _t2Functor;

	public TupleLoadFunctor()
	{
		_t1Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T1>();
		_t2Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T2>();
	}

	public TupleLoadFunctor(ILoadFunctor<T1> t1Load, ILoadFunctor<T2> t2Load)
	{
		_t1Functor = t1Load;
		_t2Functor = t2Load;
	}

	Tuple<T1, T2> ILoadFunctor<Tuple<T1, T2>>.CreateFromJson(JsonData json)
	{
		T1 i_item = ((_t1Functor != null) ? _t1Functor.CreateFromJson(json["Item1"]) : default(T1));
		T2 i_item2 = ((_t2Functor != null) ? _t2Functor.CreateFromJson(json["Item2"]) : default(T2));
		return Tuple.Create(i_item, i_item2);
	}
}
public class TupleLoadFunctor<T1, T2, T3> : ILoadFunctor<Tuple<T1, T2, T3>>, IFunctor
{
	private ILoadFunctor<T1> _t1Functor;

	private ILoadFunctor<T2> _t2Functor;

	private ILoadFunctor<T3> _t3Functor;

	public TupleLoadFunctor()
	{
		_t1Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T1>();
		_t2Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T2>();
		_t3Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T3>();
	}

	public TupleLoadFunctor(ILoadFunctor<T1> t1Load, ILoadFunctor<T2> t2Load, ILoadFunctor<T3> t3Load)
	{
		_t1Functor = t1Load;
		_t2Functor = t2Load;
		_t3Functor = t3Load;
	}

	Tuple<T1, T2, T3> ILoadFunctor<Tuple<T1, T2, T3>>.CreateFromJson(JsonData json)
	{
		T1 i_item = ((_t1Functor != null) ? _t1Functor.CreateFromJson(json["Item1"]) : default(T1));
		T2 i_item2 = ((_t2Functor != null) ? _t2Functor.CreateFromJson(json["Item2"]) : default(T2));
		T3 i_item3 = ((_t3Functor != null) ? _t3Functor.CreateFromJson(json["Item3"]) : default(T3));
		return Tuple.Create(i_item, i_item2, i_item3);
	}
}
public class TupleLoadFunctor<T1, T2, T3, T4> : ILoadFunctor<Tuple<T1, T2, T3, T4>>, IFunctor
{
	private ILoadFunctor<T1> _t1Functor;

	private ILoadFunctor<T2> _t2Functor;

	private ILoadFunctor<T3> _t3Functor;

	private ILoadFunctor<T4> _t4Functor;

	public TupleLoadFunctor()
	{
		_t1Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T1>();
		_t2Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T2>();
		_t3Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T3>();
		_t4Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T4>();
	}

	public TupleLoadFunctor(ILoadFunctor<T1> t1Load, ILoadFunctor<T2> t2Load, ILoadFunctor<T3> t3Load, ILoadFunctor<T4> t4Load)
	{
		_t1Functor = t1Load;
		_t2Functor = t2Load;
		_t3Functor = t3Load;
		_t4Functor = t4Load;
	}

	Tuple<T1, T2, T3, T4> ILoadFunctor<Tuple<T1, T2, T3, T4>>.CreateFromJson(JsonData json)
	{
		T1 i_item = ((_t1Functor != null) ? _t1Functor.CreateFromJson(json["Item1"]) : default(T1));
		T2 i_item2 = ((_t2Functor != null) ? _t2Functor.CreateFromJson(json["Item2"]) : default(T2));
		T3 i_item3 = ((_t3Functor != null) ? _t3Functor.CreateFromJson(json["Item3"]) : default(T3));
		T4 i_item4 = ((_t4Functor != null) ? _t4Functor.CreateFromJson(json["Item4"]) : default(T4));
		return Tuple.Create(i_item, i_item2, i_item3, i_item4);
	}
}
public class TupleLoadFunctor<T1, T2, T3, T4, T5> : ILoadFunctor<Tuple<T1, T2, T3, T4, T5>>, IFunctor
{
	private ILoadFunctor<T1> _t1Functor;

	private ILoadFunctor<T2> _t2Functor;

	private ILoadFunctor<T3> _t3Functor;

	private ILoadFunctor<T4> _t4Functor;

	private ILoadFunctor<T5> _t5Functor;

	public TupleLoadFunctor()
	{
		_t1Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T1>();
		_t2Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T2>();
		_t3Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T3>();
		_t4Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T4>();
		_t5Functor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T5>();
	}

	public TupleLoadFunctor(ILoadFunctor<T1> t1Load, ILoadFunctor<T2> t2Load, ILoadFunctor<T3> t3Load, ILoadFunctor<T4> t4Load, ILoadFunctor<T5> t5Load)
	{
		_t1Functor = t1Load;
		_t2Functor = t2Load;
		_t3Functor = t3Load;
		_t4Functor = t4Load;
		_t5Functor = t5Load;
	}

	Tuple<T1, T2, T3, T4, T5> ILoadFunctor<Tuple<T1, T2, T3, T4, T5>>.CreateFromJson(JsonData json)
	{
		T1 i_item = ((_t1Functor != null) ? _t1Functor.CreateFromJson(json["Item1"]) : default(T1));
		T2 i_item2 = ((_t2Functor != null) ? _t2Functor.CreateFromJson(json["Item2"]) : default(T2));
		T3 i_item3 = ((_t3Functor != null) ? _t3Functor.CreateFromJson(json["Item3"]) : default(T3));
		T4 i_item4 = ((_t4Functor != null) ? _t4Functor.CreateFromJson(json["Item4"]) : default(T4));
		T5 i_item5 = ((_t5Functor != null) ? _t5Functor.CreateFromJson(json["Item5"]) : default(T5));
		return Tuple.Create(i_item, i_item2, i_item3, i_item4, i_item5);
	}
}
