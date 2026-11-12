namespace NShared.CheckNewMail;

[Response]
public class Response : GenericResponse<ResultType>
{
	public bool NewMailArrived { get; set; }

	public int TotalMailCount { get; set; }

	public long LastCheckedMailIndex { get; set; }

	public int[] OpenedMailIndices { get; set; }
}
