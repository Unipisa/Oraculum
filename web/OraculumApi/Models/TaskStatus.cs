public class TaskStatus {

    public TaskState? State { get; set;}
    public float? Progress { get; set;}
    public Guid? ResultId { get; set;}

}

public class TaskStatus<T> : TaskStatus
{
    public T? Metadata { get; set;}
}