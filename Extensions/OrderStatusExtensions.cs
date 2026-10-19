using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Extensions
{
    public static class OrderStatusExtensions
    {
        public static string ToRussian(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Created => "Создан",
                OrderStatus.MaterialCutting => "Крой материала",
                OrderStatus.Skiving => "Шерфование",
                OrderStatus.Marking => "Разметка",
                OrderStatus.Doubling => "Дублирование",
                OrderStatus.Labeling => "Маркировка",
                OrderStatus.UpperAssembly => "Сборка заготовки",
                OrderStatus.HandLasting => "Ручная затяжка",
                OrderStatus.Stitching => "Прошивка",
                OrderStatus.Finishing => "Финишная отделка",
                OrderStatus.QualityControl => "Контроль качества",
                OrderStatus.Pachaging => "Упаковка",
                OrderStatus.Shipping => "Отправка",
                OrderStatus.Shipped => "Отправлен",
                OrderStatus.Cancelled => "Отменён",
                _ => status.ToString()
            };
        }
    }
}
