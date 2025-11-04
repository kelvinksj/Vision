using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Service.DR;
[ApiController]
[Route("Api/[controller]")]
public class ScanController : ControllerBase
{
    private string taskName = "ScanController";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ScanController;
    private CommonLib? commonLib;
    private ScanService? scanService;

    public ScanController(
        CommonLib CommonLib,
        ScanService ScanService
    )
    {
        CommonLib.DisplayConsole(
            taskName,
            $"ScanController triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib = CommonLib;
            scanService = ScanService;
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

    [HttpPost("ScanUpdate")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ScanUpdate([FromBody] dynamic message)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ScanUpdate triggered",
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
                $"ScanUpdate: message={message}",
                settingLevel,
                MessageLevel.Trace
            );

            HttpClass.ScanData data
                = JsonSerializer.Deserialize<HttpClass.ScanData>(message);
            reply.ResponseId = data.CallID;

            commonLib?.DisplayConsole(
                taskName,
                $"ScanUpdate: data={commonLib?.JsonSerialize(data)}",
                settingLevel,
                MessageLevel.Disable
            );

            List<MattelClass.Sheet> scanList = new();

            var KitCount = 1;
            foreach (var item in data.KitData)
            {
                scanList.Add(
                    new()
                    {
                        ID = KitCount,
                        KitNumber = item.Kit,
                        Description = string.Empty,
                        SheetData = []
                    }
                );

                var itemCount = 1;
                foreach (var part in item.PartNoData)
                {
                    scanList[KitCount - 1].SheetData.Add(
                        new()
                        {
                            ID = itemCount++,
                            PartNumber = part.PartNo,
                            Description = part.Desc
                        }
                    );
                }

                KitCount++;
            }

            scanService?.ScanClientUpdate(scanList, data.Guid);
            commonLib?.DisplayConsole(
                taskName,
                $"ScanUpdate: scanList={commonLib?.JsonSerialize(scanList)}",
                settingLevel,
                MessageLevel.Disable
            );
            commonLib?.DisplayConsole(
                taskName,
                $"ScanUpdate: data ok",
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
                    $"ScanUpdate: data structure wrong",
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
                MessageLevel.Disable
            );

            HttpClass.ScanUISubscriptionUpdate data
                = JsonSerializer.Deserialize<HttpClass.ScanUISubscriptionUpdate>(message);
            reply.ResponseId = data.CallID;

            commonLib?.DisplayConsole(
                taskName,
                $"Messages: data={commonLib?.JsonSerialize(data)}",
                settingLevel,
                MessageLevel.Disable
            );

            if (data.IsLast)
            {
                scanService?.AddSubscriberMessage();
            }

            scanService?.MessageUpdate(data.Messages);
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

    [HttpPost("{*catchall}")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult HandleUnmatchedRoutes(string catchall)
    {
        return NotFound($"The requested api ({catchall}) does not exist.");
    }
}