using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Services
{
    public class ArticleCodeServices
    {
        public string BuildArticleCode(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (product.Model is null)
                throw new ArgumentException(
                    "Для формирования артикула необходимо выбрать модель",
                    nameof(product));

            if (product.Model.Color is null)
                throw new ArgumentException(
                    "Для формирования артикула необходимо выбрать цвет модели",
                    nameof(product));

            if (product.Leather is null)
                throw new ArgumentException(
                    "Для формирования артикула необходимо выбрать кожу",
                    nameof(product));

            if (product.Lining is null)
                throw new ArgumentException(
                    "Для формирования артикула необходимо выбрать подкладку",
                    nameof(product));

            var genderCode = ((int)product.Gender).ToString();

            var articleCode = string.Concat(
                genderCode,
                product.Leather,
                product.Lining,
                product.Model,
                product.Model.Color
                );

            if (articleCode.Length != 6 || articleCode.Any(c => c is < '0' or '9'))
                throw new InvalidOperationException(
                    "Не удалось сформировать корректный шестизначный артикул");

            return articleCode;
        }
    }
}
