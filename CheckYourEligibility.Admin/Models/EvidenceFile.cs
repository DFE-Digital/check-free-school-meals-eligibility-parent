using System.ComponentModel.DataAnnotations.Schema;
using CheckYourEligibility.Admin.Attributes;

namespace CheckYourEligibility.Admin.Models;

public class EvidenceFile
{
    [NotMapped] public int FileIndex { get; set; }

    public string? FileName { get; set; }

    public string? FileType { get; set; }
}