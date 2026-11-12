using NVespa;

namespace NGameAsset.NBattleFieldMap2;

public class BattleFieldMapLoad : IAsyncLoad<IBattleFieldMap2>, IAsyncLoadBase
{
	public OpenData OpenData;

	public IBattleFieldMap2 Value;

	public bool IsDone;

	public bool IsValid;

	public string LoadMessage = string.Empty;

	private bool _isClosed;

	public BattleFieldMapStock Stock { get; private set; }

	string IAsyncLoadBase.KeyDesc => Stock.Code;

	bool IAsyncLoadBase.IsDone => IsDone;

	bool IAsyncLoadBase.IsValid => IsValid;

	string IAsyncLoadBase.LoadMessage => LoadMessage;

	IBattleFieldMap2 IAsyncLoad<IBattleFieldMap2>.Value => Value;

	public void Open(BattleFieldMapStock stock)
	{
		_isClosed = false;
		Stock = stock;
		Value = new BattleFieldMapProxy();
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(Stock.Code);
		if (builder == null)
		{
			SetInvalid($"Not Exist Code: {Stock.Code}", null);
			return;
		}
		OpenData = builder.CreateOpenData(Stock);
		if (OpenData.ISDONE())
		{
			OnCompleteAsset();
			return;
		}
		SingletonUpdater<AsyncManager>.instance.Add(new AsyncManager.SingleEntry
		{
			AsyncLoad = OpenData,
			OnComplete = OnCompleteAsset
		});
	}

	public void Close()
	{
		if (Value != null)
		{
			Value.Close();
			Value = null;
		}
		if (OpenData != null)
		{
			OpenData.Close();
			OpenData = null;
		}
		_isClosed = true;
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
		if (!(Value is BattleFieldMapProxy battleFieldMapProxy))
		{
			SetInvalid("Value is not BattleFieldMapProxy", null);
			return;
		}
		BattleFieldMapInstance battleFieldMapInstance = new BattleFieldMapInstance();
		if (!battleFieldMapInstance.Open(OpenData))
		{
			Vespa_Common_Class_Extension_Method.Interface((IBattleFieldMap2)battleFieldMapInstance).Close();
			SetInvalid("BattleFieldMapInstance Open Fail", null);
			return;
		}
		IsDone = true;
		IsValid = true;
		Value = battleFieldMapInstance;
		battleFieldMapProxy.ApplyToReal(Value);
		Vespa_Common_Class_Extension_Method.Interface((IBattleFieldMap2)battleFieldMapProxy).Close();
	}
}
