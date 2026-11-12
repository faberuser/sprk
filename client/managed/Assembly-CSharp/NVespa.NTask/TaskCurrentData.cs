namespace NVespa.NTask;

public struct TaskCurrentData
{
	public static TaskCurrentData RUNNING = new TaskCurrentData(ETaskResult.Running);

	public static TaskCurrentData SUCCESS = new TaskCurrentData(ETaskResult.Success);

	public static TaskCurrentData FAIL = new TaskCurrentData(ETaskResult.Fail);

	public static TaskCurrentData RUNNING_TOUCH_DISABLE = new TaskCurrentData(ETaskResult.Running, ETouchControl.Disable);

	public ETaskResult Result;

	public ETouchControl TouchControl;

	public TaskCurrentData(ETaskResult result)
	{
		Result = result;
		TouchControl = ETouchControl.DontCare;
	}

	public TaskCurrentData(ETaskResult result, ETouchControl disableTouch)
	{
		Result = result;
		TouchControl = disableTouch;
	}

	public bool IsRunning()
	{
		return Result == ETaskResult.Running;
	}

	public bool IsSuccess()
	{
		return Result == ETaskResult.Success;
	}

	public bool IsFail()
	{
		return Result == ETaskResult.Fail;
	}

	public override string ToString()
	{
		return $"(Result={Vespa_String_Extension_Method.GetEnumString(Result)}, Touch={Vespa_String_Extension_Method.GetEnumString(TouchControl)})";
	}
}
