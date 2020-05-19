using CleanShop.Core.Dtos;
using CleanShop.Core.Extensions;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Vendr.Core;
using Vendr.Core.Exceptions;
using Vendr.Core.Services;
using Vendr.Core.Session;

namespace CleanShop.Core.Controllers.Surface
{
    public class CartSurfaceController : SurfaceController
    {
        private readonly ISessionManager _sessionManager;
        private readonly IOrderService _orderService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public CartSurfaceController(ISessionManager sessionManager, 
            IOrderService orderService,
            IUnitOfWorkProvider unitOfWorkProvider)
        {
            _sessionManager = sessionManager;
            _orderService = orderService;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        [ChildActionOnly]
        public ActionResult CartCount()
        {
            var store = CurrentPage.GetStore();
            var order = _sessionManager.GetCurrentOrder(store.Id);

            return PartialView("~/Views/Partials/cartCount.cshtml", order?.TotalQuantity ?? 0);
        }

        [HttpPost]
        public ActionResult AddToCart(AddToCartDto postModel)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Create())
                {
                    var store = CurrentPage.GetStore();
                    var order = _sessionManager.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .AddProduct(postModel.ProductReference, postModel.Quantity);

                    _orderService.SaveOrder(order);

                    uow.Complete();
                }
            }
            catch(ValidationException ex)
            {
                ModelState.AddModelError("productReference", "Failed to add product to cart");
                return CurrentUmbracoPage();
            }

            TempData["addedProductReference"] = postModel.ProductReference;

            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        public ActionResult UpdateCart(UpdateCartDto postModel)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Create())
                {
                    var store = CurrentPage.GetStore();
                    var order = _sessionManager.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow);

                    foreach (var orderLine in postModel.OrderLines)
                    {
                        order.WithOrderLine(orderLine.Id)
                            .SetQuantity(orderLine.Quantity);
                    }

                    _orderService.SaveOrder(order);

                    uow.Complete();
                }
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("productReference", "Failed to update cart");
                return CurrentUmbracoPage();
            }

            TempData["cartUpdated"] = "true";
            return RedirectToCurrentUmbracoPage();
        }

        public ActionResult RemoveFromCart(RemoveFromCartDto postModel)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Create())
                {
                    var store = CurrentPage.GetStore();
                    var order = _sessionManager.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .RemoveOrderLine(postModel.OrderLineId);

                    _orderService.SaveOrder(order);

                    uow.Complete();
                }
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("productReference", "Failed to remove item from cart");
                return CurrentUmbracoPage();
            }

            TempData["itemRemovedFromCart"] = "true";

            return RedirectToCurrentUmbracoPage();
        }
    }
}
