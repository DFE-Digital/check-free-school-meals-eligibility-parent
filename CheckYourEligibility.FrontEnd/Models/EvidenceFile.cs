using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility.FrontEnd.Models;

public class EvidenceFile
{
    [NotMapped] public int FileIndex { get; set; }

    public string? FileName { get; set; }

    public string? FileType { get; set; }
    public string StorageAccountReference { get; set; }
}