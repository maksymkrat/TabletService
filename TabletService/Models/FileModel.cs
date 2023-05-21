using Microsoft.AspNetCore.Mvc;

namespace TabletService.Models;

public class FileModel
{
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    
}