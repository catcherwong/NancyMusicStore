using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NancyMusicStore.Common;
using NancyMusicStore.Models;
using System;
using System.Data;

namespace NancyMusicStore.Modules
{
    public class CheckOutModule : NancyModule
    {
        const string PromoCode = "FREE";
        public CheckOutModule() : base("/checkout")
        {
            this.RequiresAuthentication();

            Get["/addressandpayment"] = _ =>
            {
                return View["AddressAndPayment"];
            };

            Post["/addressandpayment"] = _ =>
            {
                var order = this.Bind<Order>();
                order.Username = this.Context.CurrentUser.UserName;
                order.OrderDate = DateTime.UtcNow;

                string cmd = "public.add_order";
                var res = DBHelper.ExecuteScalar(cmd, new
                {
                    odate = order.OrderDate,
                    uname = order.Username,
                    fname = order.FirstName,
                    lname = order.LastName,
                    adr = order.Address,
                    cn = order.City,
                    sn = order.State,
                    pcode = order.PostalCode,
                    cname = order.Country,
                    ph = order.Phone,
                    ea = order.Email,
                    t = order.Total
                }, null, null, CommandType.StoredProcedure);

                if (Convert.ToInt32(res) != 0)
                {
                    order.OrderId = Convert.ToInt32(res);
                    var cart = ShoppingCart.GetCart(this.Context);
                    cart.CreateOrder(order);

                    string redirectUrl = string.Format("/checkout/complete/{0}", res.ToString());
                    return Response.AsRedirect(redirectUrl);
                }
                return View["AddressAndPayment"];
            };

            Get["/complete/{id:int}"] = _ =>
            {
                int id = _.id;

                string cmd = "public.get_order_count_by_uname_and_orderid";
                var res = DBHelper.ExecuteScalar(cmd, new
                {
                    oid = id,
                    uname = this.Context.CurrentUser.UserName.ToLower()
                }, null, null, CommandType.StoredProcedure);

                if (Convert.ToInt32(res) > 0)
                {
                    return View["Complete", id];
                }
                return View["Shared/Error"];
            };
        }
    }
}