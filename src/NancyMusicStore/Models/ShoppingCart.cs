using Nancy;
using NancyMusicStore.Common;
using NancyMusicStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NancyMusicStore.Models
{
    public partial class ShoppingCart
    {
        string ShoppingCartId { get; set; }

        public const string CartSessionKey = "CartId";

        public static ShoppingCart GetCart(NancyContext context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(context);
            return cart;
        }

        public void AddToCart(Album album)
        {
            string getItemCmd = "public.get_cart_item_by_cartid_and_albumid";
            var cartItem = DBHelper.QueryFirstOrDefault<Cart>(getItemCmd, new
            {
                cid = ShoppingCartId,
                aid = album.AlbumId
            }, null, null, CommandType.StoredProcedure);
            string addToCartCmd = string.Empty;

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                AddCartItem(cartItem, album.AlbumId);
            }
            else
            {
                UpdateCartItem(cartItem);
            }
        }

        public int RemoveFromCart(int id)
        {
            string getItemCmd = "public.get_cart_item_by_cartid_and_recordid";
            var cartItem = DBHelper.QueryFirstOrDefault<Cart>(getItemCmd, new
            {
                cid = ShoppingCartId,
                rid = id
            }, null, null, CommandType.StoredProcedure);

            int itemCount = 0;
            if (cartItem != null)
            {                
                if (cartItem.Count > 1)
                {
                    UpdateCartItemCount(cartItem, itemCount);                   
                }
                else
                {
                    RemoveCartItem(cartItem.RecordId);
                }
            }
            return itemCount;
        }

        public void EmptyCart()
        {
            string cmd = "public.delete_cart_item_by_cid";
            DBHelper.Execute(cmd, new
            {
                cid = ShoppingCartId
            }, null, null, CommandType.StoredProcedure);
        }

        public List<CartViewModel> GetCartItems()
        {
            string cmd = "public.get_cart_item_by_cid";
            return DBHelper.Query<CartViewModel>(cmd, new
            {
                cid = ShoppingCartId
            }, null, true, null, CommandType.StoredProcedure).ToList();
        }

        public int GetCount()
        {
            string cmd = "public.get_total_count_by_cartid";
            var res = DBHelper.ExecuteScalar(cmd, new
            {
                cid = ShoppingCartId
            }, null, null, CommandType.StoredProcedure);

            return Convert.ToInt32(res);
        }

        public decimal GetTotal()
        {
            string cmd = "public.get_total_order_by_cartid";
            var res = DBHelper.ExecuteScalar(cmd, new
            {
                cid = ShoppingCartId
            }, null, null, CommandType.StoredProcedure);

            return res == null ? decimal.Zero : decimal.Parse(res.ToString());
        }

        public int CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();                        
            foreach (var item in cartItems)
            {                
                AddOrderDetails(new OrderDetail
                {
                    AlbumId = item.AlbumId,
                    OrderId = order.OrderId,
                    UnitPrice = item.Price,
                    Quantity = item.Count
                });
                // Set the order total of the shopping cart
                orderTotal += (item.Count * item.Price);
            }

            UpdateOrderTotal(order.OrderId, orderTotal);         

            // Empty the shopping cart
            EmptyCart();

            // Return the OrderId as the confirmation number
            return order.OrderId;
        }

        public string GetCartId(NancyContext context)
        {
            if (context.Request.Session[CartSessionKey] == null)
            {
                if (context.CurrentUser != null)
                {
                    context.Request.Session[CartSessionKey] = context.CurrentUser.UserName;
                }
                else
                {
                    Guid tempCartId = Guid.NewGuid();
                    context.Request.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return context.Request.Session[CartSessionKey].ToString();
        }

        public void MigrateCart(string userName)
        {
            string cmd = "public.update_cartid_by_recordids";
            DBHelper.ExecuteScalar(cmd, new
            {
                ncid = userName,
                ocid = ShoppingCartId
            }, null, null, CommandType.StoredProcedure);
        }

        #region private method
        private void AddCartItem(Cart cartItem, int albumid)
        {
            cartItem = new Cart
            {
                AlbumId = albumid,//album.AlbumId,
                CartId = ShoppingCartId,
                Count = 1,
                DateCreated = DateTime.Now
            };
            string addToCartCmd = "public.add_cart_item";
            DBHelper.Execute(addToCartCmd, new
            {
                cid = cartItem.CartId,
                aid = cartItem.AlbumId,
                num = cartItem.Count,
                cdate = cartItem.DateCreated
            }, null, null, CommandType.StoredProcedure);

        }

        private void UpdateCartItem(Cart cartItem)
        {
            cartItem.Count++;
            string addToCartCmd = "public.update_cart_item";
            DBHelper.Execute(addToCartCmd, new
            {
                cid = cartItem.CartId,
                aid = cartItem.AlbumId,
                num = cartItem.Count
            }, null, null, CommandType.StoredProcedure);
        }

        private void UpdateCartItemCount(Cart cartItem, int itemCount)
        {
            cartItem.Count--;
            itemCount = cartItem.Count;

            string cmd = "public.update_cart_count_by_recordid";
            DBHelper.Execute(cmd, new
            {
                rid = cartItem.RecordId,
                num = cartItem.Count
            }, null, null, CommandType.StoredProcedure);
        }

        private void RemoveCartItem(int recordId)
        {
            string cmd = "public.delete_cart_item_by_recordid";
            DBHelper.Execute(cmd, new
            {
                rid = recordId
            }, null, null, CommandType.StoredProcedure);
        }

        private void AddOrderDetails(OrderDetail orderDetail)
        {
            string createCmd = "public.add_order_details";
            DBHelper.ExecuteScalar(createCmd, new
            {
                oid = orderDetail.OrderId,
                aid = orderDetail.AlbumId,
                qty = orderDetail.Quantity,
                uprice = orderDetail.UnitPrice
            }, null, null, CommandType.StoredProcedure);
        }

        private void UpdateOrderTotal(int orderId, decimal orderTotal)
        {
            string updateCmd = "public.update_order_total_by_orderid";

            var res = DBHelper.ExecuteScalar(updateCmd, new
            {
                t = orderTotal,
                oid = orderId
            }, null, null, CommandType.StoredProcedure);
        } 
        #endregion
    }
}