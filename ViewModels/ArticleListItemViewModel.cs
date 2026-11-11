using ShoeProductionApp.Entities;

namespace ShoeProductionApp.ViewModels;

public class ArticleListItemViewModel
{
    public int Id { get; set; }
    public string Article { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int ModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ModelCode { get; set; } = string.Empty;
    public int ColorId { get; set; }
    public string ColorName { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
    public int LeatherId { get; set; }
    public string LeatherName { get; set; } = string.Empty;
    public string LeatherCode { get; set; } = string.Empty;
    public int LiningId { get; set; }
    public string LiningName { get; set; } = string.Empty;
    public string LiningCode { get; set; } = string.Empty;
    public int SoleId { get; set; }
    public string SoleName { get; set; } = string.Empty;
    public string SoleCode { get; set; } = string.Empty;
    public string? ImageFileName { get; set; }
}
