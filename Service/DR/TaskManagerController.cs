using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Http;

namespace TaskManagerWeb.Components.Service.DR;
[ApiController]
[Route("Api/[controller]")]
public class TaskManagerController : ControllerBase
{
    private string taskName = "TaskManagerController";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_TaskManagerController;
    private CommonLib? commonLib;
    private TaskManagerService? taskManagerService;

    public TaskManagerController(
        CommonLib CommonLib,
        TaskManagerService TaskManagerService
    )
    {
        CommonLib.DisplayConsole(
            taskName,
            $"TaskManagerController triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib = CommonLib;
            taskManagerService = TaskManagerService;
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    [HttpPost]
    [Route("")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult HandleRootUrl()
    {
        return NotFound("No api at requested endpoint");
    }

    [HttpPost("Messages")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Messages([FromBody] dynamic message)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"Messages triggered",
            settingLevel,
            MessageLevel.Information
        );

        HttpClass.GenericReply reply = new()
        {
            Code = 1000,
            ResponseId = string.Empty,
            Desc = "succeed"
        };

        try
        {
            commonLib?.DisplayConsole(
                taskName,
                $"Messages: message={message}",
                settingLevel,
                MessageLevel.Trace
            );

            HttpClass.HttpUISubscriptionUpdate data
                = JsonSerializer.Deserialize<HttpClass.HttpUISubscriptionUpdate>(message);
            reply.ResponseId = data.CallID;

            commonLib?.DisplayConsole(
                taskName,
                $"Messages: data={commonLib?.JsonSerialize(data)}",
                settingLevel,
                MessageLevel.Disable
            );

            if (data.IsLast)
            {
                taskManagerService?.AddUISubscriberQueue();
            }

            taskManagerService?.ClientUpdateMessage(data.Messages);
            taskManagerService?.ClientUpdateTag(data.Messages);
            commonLib?.DisplayConsole(
                taskName,
                $"Messages: data ok",
                settingLevel,
                MessageLevel.Information
            );
        }
        catch (Exception ex)
        {
            if (ex.HResult == -2146233088)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"Messages: data structure wrong",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            else
            {
                commonLib?.DisplayConsole(
                    taskName,
                    ex,
                    settingLevel,
                    MessageLevel.Error
                );
            }
        }

        return Ok(reply);
    }

    [HttpPost("AMR")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AMR([FromBody] dynamic message)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AMR triggered",
            settingLevel,
            MessageLevel.Information
        );

        HttpClass.GenericReply reply = new()
        {
            Code = 1000,
            ResponseId = string.Empty,
            Desc = "succeed"
        };

        try
        {
            commonLib?.DisplayConsole(
                taskName,
                $"AMR: message={message}",
                settingLevel,
                MessageLevel.Trace
            );

            HttpClass.HttpAmrSubscriptionUpdate data
                = JsonSerializer.Deserialize<HttpClass.HttpAmrSubscriptionUpdate>(message);
            reply.ResponseId = data.CallID;

            commonLib?.DisplayConsole(
                taskName,
                $"AMR: data={commonLib?.JsonSerialize(data)}",
                settingLevel,
                MessageLevel.Disable
            );

            if (data.IsLast)
            {
                taskManagerService?.AddAMRSubscriberQueue();
            }

            taskManagerService?.AMRClientUpdate(data.AMR);
            commonLib?.DisplayConsole(
                taskName,
                $"AMR: data ok",
                settingLevel,
                MessageLevel.Information
            );
        }
        catch (Exception ex)
        {
            if (ex.HResult == -2146233088)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"AMR: data structure wrong",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            else
            {
                commonLib?.DisplayConsole(
                    taskName,
                    ex,
                    settingLevel,
                    MessageLevel.Error
                );
            }
        }

        return Ok(reply);
    }

    [HttpPost("{*catchall}")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult HandleUnmatchedRoutes(string catchall)
    {
        return NotFound($"The requested api ({catchall}) does not exist.");
    }
}