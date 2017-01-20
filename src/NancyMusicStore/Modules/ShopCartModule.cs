using Nancy;
using Nancy.ModelBinding;
using NancyMusicStore.Common;
using NancyMusicStore.Models;
using NancyMusicStore.ViewModels;
using System.Data;

namespace NancyMusicStore.Modules
{
    public class ShopCartModule : NancyModule
    {
        public ShopCartModule() : base("/shoppingcart")
        {
            Get["/cartsummary"] = _ =>
            {
                var cart = ShoppingCart.GetCart(this.Context);
                return Response.AsJson(cart.GetCount());
            };

            Get["/addtocart/{id:int}"] = _ =>
            {
                int id = 0;
                if (int.TryParse(_.id, out id))
                {
                    string cmd = "public.get_album_by_aid";
                    var addedAlbum = DBHelper.QueryFirstOrDefault<Album>(cmd, new
                    {
                        aid = id
                    }, null, null, CommandType.StoredProcedure);

                    var cart = ShoppingCart.GetCart(this.Context);
                    cart.AddToCart(addedAlbum);
                }
                return Response.AsRedirect("~/");
            };

            Get["/index"] = _ =>
            {
                var cart = ShoppingCart.GetCart(this.Context);

                // Set up our ViewModel
                var viewModel = new ShoppingCartViewModel
                {
                    CartItems = cart.GetCartItems(),
                    CartTotal = cart.GetTotal()
                };

                // Return the view
                return View["Index", viewModel];
            };

            Post["/removefromcart"] = _ =>
            {
                var vm = this.Bind<ShoppingCartRemoveRequestViewModel>();
                string albumName = string.Empty;
                return Response.AsJson(GetRemoveResult(vm.Id, albumName));
            };
        }

        private ShoppingCartRemoveViewModel GetRemoveResult(int rid, string albumName)
        {
            int itemCount = 0;

            // Remove the item from the cart
            var cart = ShoppingCart.GetCart(this.Context);

            string cmd = "public.get_album_title_by_recordid";
            var res = DBHelper.ExecuteScalar(cmd, new
            {
                rid = rid
            }, null, null, CommandType.StoredProcedure);

            if (res != null)
            {
                albumName = res.ToString();
                itemCount = cart.RemoveFromCart(rid);
            }

            var results = new ShoppingCartRemoveViewModel
            {
                Message = albumName + " has been removed from your shopping cart.",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = rid
            };
            return results;
        }
    }
}