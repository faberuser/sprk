namespace NShared.LevelUpClassBuff;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "class_buff/level_up_class_buff";

	public int TagType { get; set; }

	public int ClassBuffIndex { get; set; }

	public byte ClassBuffLevel { get; set; }
}
