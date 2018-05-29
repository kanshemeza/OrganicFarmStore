using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganicFarmStore.Models;

namespace OrganicFarmStore.Controllers
{
    public class CartController : Controller
    {
        private OrganicStoreDbContext _context;

        public CartController(OrganicStoreDbContext context)
        {
            _context = context;
        }

    public async Task<IActionResult> Index()
        {
            Guid cartId;
            Cart cart = null;
            if (Request.Cookies.ContainsKey("cartId"))
            {
                if (Guid.TryParse(Request.Cookies["cartId"], out cartId))
                {
                    cart = await _context.Carts
                        .Include(carts => carts.CartItems)
                        .ThenInclude(cartitems => cartitems.Product)
                        .FirstOrDefaultAsync(x => x.CookieIdentifier == cartId);
                }
            }
            if (cart == null)
            {
                cart = new Cart();
            }
            return View(cart);
        }

        public async Task<IActionResult> Remove(int id)
        {
            Guid cartId;
            Cart cart = null;
            if (Request.Cookies.ContainsKey("cartId"))
            {
                if (Guid.TryParse(Request.Cookies["cartId"], out cartId))
                {
                    cart = await _context.Carts
                        .Include(carts => carts.CartItems)
                        .ThenInclude(cartitems => cartitems.Product)
                        .FirstOrDefaultAsync(x => x.CookieIdentifier == cartId);
                }
            }
            CartItem item = cart.CartItems.FirstOrDefault(x => x.ID == id);
           
            cart.LastModified = DateTime.Now;
            _context.CartItems.Remove(item);

           await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Edit(int id, int quantity)
        {
            Guid cartId;
            Cart cart = null;
            if (Request.Cookies.ContainsKey("cartId"))
            {
                if (Guid.TryParse(Request.Cookies["cartId"], out cartId))
                {
                    cart = await _context.Carts
                        .Include(carts => carts.CartItems)
                        .ThenInclude(cartitems => cartitems.Product)
                        .FirstOrDefaultAsync(x => x.CookieIdentifier == cartId);
                }
            }
            
               CartItem item = cart.CartItems.FirstOrDefault(x => x.ID == id);
            item.Quantity =quantity;
            cart.LastModified = DateTime.Now;

            _context.CartItems.Update(item);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }
    }
}