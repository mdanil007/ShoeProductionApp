using ShoeProductionApp.Entities;

namespace ShoeProductionApp.ViewModels;

public class ReferenceIndexViewModel
{
    public string Tab { get; set; } = "models";

    public string? SearchModels { get; set; }
    public string? SearchArticles { get; set; }
    public string? SearchColors { get; set; }
    public string? SearchLeathers { get; set; }
    public string? SearchLinings { get; set; }

    public List<Model> Models { get; set; } = [];
    public List<Product> Products { get; set; } = [];
    public List<ArticleListItemViewModel> Articles { get; set; } = [];
    public List<Color> Colors { get; set; } = [];
    public List<Leather> Leathers { get; set; } = [];
    public List<Lining> Linings { get; set; } = [];

    public int? EditModelId { get; set; }
    public int? EditArticleId { get; set; }
    public int? EditColorId { get; set; }
    public int? EditLeatherId { get; set; }
    public int? EditLiningId { get; set; }
}
