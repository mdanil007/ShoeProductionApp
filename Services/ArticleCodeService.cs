using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Services;

public class ArticleCodeService
{
    public string BuildArticleCode(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (!Enum.IsDefined(product.Gender))
            throw new ArgumentException("Необходимо выбрать допустимый тип обуви.", nameof(product));

        if (product.Model is null || product.Color is null || product.Leather is null ||
            product.Lining is null || product.Sole is null)
            throw new ArgumentException("Для формирования артикула необходимо выбрать все составляющие.", nameof(product));

        var code = string.Concat(
            ((int)product.Gender).ToString(),
            product.Leather.Code,
            product.Lining.Code,
            product.Model.Code,
            product.Color.Code,
            product.Sole.Code);

        if (code.Length != 11 || code.Any(c => c is < '0' or > '9'))
            throw new InvalidOperationException("Не удалось сформировать корректный одиннадцатизначный артикул.");

        return code;
    }
}
