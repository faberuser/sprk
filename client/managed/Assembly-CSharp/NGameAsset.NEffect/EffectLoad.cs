using NVespa;

namespace NGameAsset.NEffect;

public class EffectLoad : IAsyncLoad<IEffect>, IAsyncLoadBase
{
	public OpenData OpenData;

	public IEffect Value;

	public bool IsDone;

	public bool IsValid;

	public string LoadMessage = string.Empty;

	private bool _isClosed;

	public string Code { get; private set; }

	public int Layer { get; private set; }

	public ITransformData ExtraTransform { get; private set; }

	string IAsyncLoadBase.KeyDesc => Code;

	bool IAsyncLoadBase.IsDone => IsDone;

	bool IAsyncLoadBase.IsValid => IsValid;

	string IAsyncLoadBase.LoadMessage => LoadMessage;

	IEffect IAsyncLoad<IEffect>.Value => Value;

	public void Open(string code, int layer, ITransformData extraTransform = null)
	{
		_isClosed = false;
		Code = code;
		Layer = layer;
		ExtraTransform = extraTransform;
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(Code);
		if (builder == null)
		{
			Value = new EffectProxy(this);
			SetInvalid($"Not Exist Code: {code}", null);
			return;
		}
		OpenData = builder.CreateOpenData();
		if (!OpenData.ISDONE())
		{
			Value = new EffectProxy(this);
			SingletonUpdater<AsyncManager>.instance.Add(new AsyncManager.SingleEntry
			{
				AsyncLoad = OpenData,
				OnComplete = OnCompleteAsset
			});
		}
		else
		{
			OnCompleteAsset();
			if (Value == null)
			{
				Value = new EffectProxy(this);
			}
		}
	}

	public void Close(bool detachParent)
	{
		if (Value != null)
		{
			Value.Close(detachParent);
		}
		_isClosed = true;
	}

	public bool IsEmpty()
	{
		if (Value == null)
		{
			return true;
		}
		return Value.IsEmpty();
	}

	public void Release()
	{
		if (Value != null)
		{
			Value.Release();
			Value = null;
		}
		if (OpenData != null)
		{
			OpenData.Close();
			OpenData = null;
		}
	}

	private void SetInvalid(string message, IAsyncLoadBase childAsyncLoad)
	{
		IsDone = true;
		IsValid = false;
		LoadMessage = message;
		if (childAsyncLoad != null && !String_Extension_Method.IsNullOrEmpty(childAsyncLoad.LoadMessage))
		{
			LoadMessage += $"\n - {childAsyncLoad.LoadMessage}";
		}
		VespaDebug.LogError("Effect Load Fail: " + LoadMessage);
	}

	private void OnCompleteAsset()
	{
		if (_isClosed)
		{
			return;
		}
		IAsyncLoadBase invalidAsset = OpenData.GetInvalidAsset();
		if (invalidAsset != null)
		{
			SetInvalid("Invalid Asset", invalidAsset);
			return;
		}
		EffectProxy effectProxy = Value as EffectProxy;
		if (Value != null && effectProxy == null)
		{
			SetInvalid("Value is not EffectProxy", null);
			return;
		}
		EffectInstance effectInstance = new EffectInstance();
		if (!effectInstance.Open(OpenData, Layer, ExtraTransform))
		{
			effectInstance.Release();
			SetInvalid("EffectInstance Open Fail", null);
			return;
		}
		IsDone = true;
		IsValid = true;
		Value = effectInstance;
		if (effectProxy != null)
		{
			effectProxy.ApplyToReal(Value);
			Vespa_Common_Class_Extension_Method.Interface((IEffect)effectProxy).Close(detachParent: true);
			Vespa_Common_Class_Extension_Method.Interface((IEffect)effectProxy).Release();
		}
	}
}
