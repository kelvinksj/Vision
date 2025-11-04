using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Http;

namespace TaskManagerWeb.Components.Service.DR;
[ApiController]
[Route("Api/[controller]")]
public class ContainerController : ControllerBase
{
    private string taskName = "ContainerController";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ContainerController;
    private CommonLib? commonLib;
    private ContainerService? containerService;

    public ContainerController(
        CommonLib CommonLib,
        ContainerService ContainerService
    )
    {
        CommonLib.DisplayConsole(
            taskName,
            $"ContainerController triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib = CommonLib;
            containerService = ContainerService;
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

    [HttpPost("LocationUpdate")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult LocationUpdate([FromBody] dynamic message)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"LocationUpdate triggered",
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
                $"LocationUpdate: message={message}",
                settingLevel,
                MessageLevel.Trace
            );

            HttpClass.SubscriptionUpdate data
                = JsonSerializer.Deserialize<HttpClass.SubscriptionUpdate>(message);
            reply.ResponseId = data.CallID;

            commonLib?.DisplayConsole(
                taskName,
                $"LocationUpdate: data={commonLib?.JsonSerialize(data)}",
                settingLevel,
                MessageLevel.Disable
            );

            if (data.IsLast)
            {
                containerService?.TriggerLocationSubscriberQueue();
            }

            containerService?.ContainerClientUpdate(data.Data);
            commonLib?.DisplayConsole(
                taskName,
                $"LocationUpdate: data ok",
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
                    $"LocationUpdate: data structure wrong",
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