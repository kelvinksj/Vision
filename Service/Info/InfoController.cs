using System.Net;
using Microsoft.AspNetCore.Mvc;
using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Service.Info;
[Route("Api/[controller]")]
[ApiController]
public class BroadcastController : ControllerBase
{
    private readonly string taskName = "BroadcastController";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_InfoController;
    private CommonLib commonLib = new CommonLib();

    public class BroadcastModel
    {
        public string? Message { get; set; }
    }

    [HttpGet("Broadcast")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Broadcast()
    {
        IPAddress? remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
        int remotePort = HttpContext.Connection.RemotePort;
        var isHttps = HttpContext.Request.IsHttps;

        var contentType = HttpContext.Request.ContentType;
        var acceptEncoding = HttpContext.Request.Headers["Accept-Encoding"];

        commonLib?.DisplayConsole(
            taskName,
            $"HttpGet trigged",
            settingLevel,
            MessageLevel.Information
        );
        commonLib?.DisplayConsole(
            taskName,
            $"HttpGet received broadcast from IP: {remoteIpAddress}, Port: {remotePort}, Using HTTPS: {isHttps}",
            settingLevel,
            MessageLevel.Trace
        );
        commonLib?.DisplayConsole(
            taskName,
            $"Content-Type: {contentType}",
            settingLevel,
            MessageLevel.Trace
        );
        commonLib?.DisplayConsole(
            taskName,
            $"Accept-Encoding: {acceptEncoding}",
            settingLevel,
            MessageLevel.Trace
        );

        return Ok("Broadcast message received");
    }

    [HttpPost("Broadcast")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Broadcast([FromBody] BroadcastModel Message)
    {
        if (Message == null)
        {
            commonLib?.DisplayConsole(
                taskName,
                "Message is null",
                settingLevel,
                MessageLevel.Error
            );
            return BadRequest("Message is null");
        }

        IPAddress? remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
        int remotePort = HttpContext.Connection.RemotePort;
        var isHttps = HttpContext.Request.IsHttps;

        var contentType = HttpContext.Request.ContentType;
        var acceptEncoding = HttpContext.Request.Headers["Accept-Encoding"];

        commonLib?.DisplayConsole(
            taskName,
            $"HttpPost trigged",
            settingLevel,
            MessageLevel.Information
        );
        commonLib?.DisplayConsole(
            taskName,
            $"HttpPost received broadcast from IP: {remoteIpAddress}, Port: {remotePort}, Using HTTPS: {isHttps}",
            settingLevel,
            MessageLevel.Trace
        );
        commonLib?.DisplayConsole(
            taskName,
            $"HttpPost message={Message.Message}",
            settingLevel,
            MessageLevel.Trace
        );
        commonLib?.DisplayConsole(
            taskName,
            $"Content-Type: {contentType}",
            settingLevel,
            MessageLevel.Trace
        );
        commonLib?.DisplayConsole(
            taskName,
            $"Accept-Encoding: {acceptEncoding}",
            settingLevel,
            MessageLevel.Trace
        );

        return Ok("Broadcast message received");
    }

    [HttpPost("{*url}")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CatchAll()
    {
        return NotFound(new { Message = "Endpoint not found" });
    }
}