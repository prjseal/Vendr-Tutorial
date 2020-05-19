using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Vendr.Core;
using Vendr.Core.Api;
using Vendr.Core.Models;

namespace CleanShop.Core.Extensions
{
    public static class PublishedContentExtensions
    {
        public static string GetProductReference(this IPublishedContent content)
        {
            return content.Key.ToString();
        }

        public static IProductSnapshot AsVendrProduct(this IPublishedContent content)
        {
            return VendrApi.Instance.GetProduct(content.GetProductReference(), null);
        }

        public static Price CalculatePrice(this IPublishedContent content)
        {
            return content.AsVendrProduct()?.CalculatePrice();
        }

        public static StoreReadOnly GetStore(this IPublishedContent content)
        {
            return content.Root().Value<StoreReadOnly>("store");
        }

        public static OrderReadOnly GetCurrentOrder(this IPublishedContent content)
        {
            return VendrApi.Instance.GetCurrentOrder(content.GetStore().Id);
        }
    }
}
