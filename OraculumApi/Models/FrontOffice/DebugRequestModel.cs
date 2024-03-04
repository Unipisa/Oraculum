public class DebugRequestModel
{
    public required string Query { get; set; }
    public bool OneShot { get; set; } = false;
}
