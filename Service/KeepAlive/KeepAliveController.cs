using Microsoft.AspNetCore.Mvc;
using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Service.KeepAlive;
[ApiController]
[Route("Api/[controller]")]
public class KeepAliveController : ControllerBase
{
    private readonly string taskName = "KeepAliveController";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ServAliveCont;
    private CommonLib commonLib = new CommonLib();

    public class MessageModel
    {
        public string User { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    [HttpGet]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult KeepAlive()
    {
        commonLib.DisplayConsole(
            taskName,
            $"KeepAlive trigged",
            settingLevel,
            MessageLevel.Information
        );
        return Ok("Keep-alive successful");
    }

    [HttpPost]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ReceiveMessage([FromBody] MessageModel message)
    {
        commonLib.DisplayConsole(
            taskName,
            $"Received message from {message.User}: {message.Content}",
            settingLevel,
            MessageLevel.Trace
        );
        return Ok("Can use api call");
    }

    [HttpPost("Update")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Update([FromBody] MessageModel message)
    {
        commonLib.DisplayConsole(
            taskName,
            $"Received message from {message.User}: {message.Content}",
            settingLevel,
            MessageLevel.Trace
        );
        return Ok("Message received successfully");
    }

    [HttpPost("{*catchall}")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult HandleUnmatchedRoutes(string catchall)
    {
        return NotFound("The requested endpoint does not exist.");
    }
}