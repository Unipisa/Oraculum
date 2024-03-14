using Microsoft.AspNetCore.Mvc;

/*
    This class is the result of a refactoring to FrontOfficeController.
    Please, refer to the original file for the original code history.
*/
public class PushStreamResult : IActionResult
{
    private readonly Func<Stream, Action<Exception>, CancellationToken, Task> _onStreamAvailable;
    private readonly string _contentType;

    public PushStreamResult(Func<Stream, Action<Exception>, CancellationToken, Task> onStreamAvailable, string contentType = null)
    {
        _onStreamAvailable = onStreamAvailable ?? throw new ArgumentNullException(nameof(onStreamAvailable));
        _contentType = contentType ?? "text/event-stream";
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var response = context.HttpContext.Response;
        response.ContentType = _contentType;
        response.Headers["Cache-Control"] = "no-store, no-cache";
        response.Headers["Connection"] = "keep-alive";

        await _onStreamAvailable(response.Body, ex =>
        {
            context.HttpContext.Abort();
        }, context.HttpContext.RequestAborted);
    }
}