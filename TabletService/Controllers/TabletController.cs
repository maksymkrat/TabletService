using Microsoft.AspNetCore.Mvc;
using TabletService.Models;
using TabletService.Repository;

namespace TabletService.Controllers;

[ApiController]
public class TabletController : ControllerBase
{
    private readonly TabletRepository _repository;
    private readonly string AccessData;

    public TabletController(TabletRepository repository)
    {
        _repository = repository;
        AccessData = "Hilgrup1289";
    }

    [HttpPost]
    [Route("InsertOrUpdateTabletIp")]
    public async Task<IActionResult> InsertOrUpdateTabletIp([FromBody] TabletInfoModel model)
    {
        try
        {
            if (!model.AccessData.Equals(AccessData))
            {
                return BadRequest();
            }

            _repository.InsertOrUpdateTabletIp(model);
            return StatusCode(200);
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }

    [HttpPost]
    [Route("GetHTMLWindowView")]
    public async Task<IActionResult> GetHTMLWindowView([FromBody] string accessData)
    {
        if (!accessData.Equals(AccessData))
            return BadRequest();

        try
        {
            var html = await _repository.GetHTMLTemplate();
            return Ok(html);
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }

    [HttpPost]
    [Route("GetMediaFiles")]
    public async Task<IActionResult> GetMediaFiles([FromBody] string accessData)
    {
        if (!accessData.Equals(AccessData))
            return BadRequest();

        var files = new List<FileModel> { };
        try
        {
            var filePaths = await _repository.GetListHTMLAttachments();

            foreach (var path in filePaths)
            {
                var file = new FileModel();
                file.FileName = Path.GetFileNameWithoutExtension(path);
                file.FileExtension = Path.GetExtension(path);

                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        var bytes = memoryStream.ToArray();
                        file.FileBytes = bytes;
                    }
                }
                files.Add(file);
            }

            return Ok(files);
        }
        catch (Exception e)
        {
            return BadRequest();
        }
        finally
        {
           GC.Collect();
           GC.Collect(2);
        }
    }
}