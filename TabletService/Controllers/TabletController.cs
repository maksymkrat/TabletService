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

    [HttpGet]
    [Route("GetHTMLWindowView")]
    public async Task<IActionResult> GetHTMLWindowView()
    {
        try
        {
            var pathHTML = await _repository.GetPathByContentType("HTML");
        
            var strHTML = System.IO.File.ReadAllText(pathHTML);
            return Ok(strHTML);
        }
        catch (Exception e)
        {
            return BadRequest();
        }
       
    }
    
    [HttpGet]
    [Route("v.mp4")]
    public async Task<IActionResult> GetVideo()
    {
        try
        {
            var pathVideo = await _repository.GetPathByContentType("VIDEO");
            FileStream stream = System.IO.File.Open(pathVideo, FileMode.OpenOrCreate);
            return File(stream, "video/mp4");
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }
    
    [HttpGet]
    [Route("i.png")]
    public async Task<IActionResult> GetImg()
    {
        try
        {
            var pathImg = await _repository.GetPathByContentType("IMG");
            byte[] imageBytes =  System.IO.File.ReadAllBytes(pathImg);
            return File(imageBytes, "image/png", "b.png");
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }
}