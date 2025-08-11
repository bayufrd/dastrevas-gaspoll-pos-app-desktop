using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KASIR.Database.ModalDatabase;
using Microsoft.EntityFrameworkCore;

namespace KASIR.Database
{
    public class CartDetailsRepository
    {
        private readonly AppDbContext _context;

        public CartDetailsRepository()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated();
        }

        // CREATE Cart Modal
        public void AddCartModal(dbCartModal cart)
        {
            _context.Carts.Add(cart);
            _context.SaveChanges();
        }

        // CREATE Cart Item
        public void AddCartItem(dbCartDetails item)
        {
            _context.CartItems.Add(item);
            _context.SaveChanges();
        }

        // READ Cart Modal
        public List<dbCartModal> GetAllCarts()
        {
            return _context.Carts.Include(c => c.CartDetails).ToList();
        }

        // READ Cart Items
        public List<dbCartDetails> GetAllCartItems()
        {
            return _context.CartItems.ToList();
        }

        // UPDATE Cart Item
        public void UpdateCartItem(dbCartDetails item)
        {
            var existingItem = _context.CartItems.Find(item.Id);
            if (existingItem != null)
            {
                existingItem.Qty = item.Qty;
                existingItem.Price = item.Price;
                existingItem.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        // DELETE Cart Item
        public void DeleteCartItem(int id)
        {
            var item = _context.CartItems.Find(id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }
        }

        // DELETE Cart Modal and all associated items
        public void DeleteCartModal(int id)
        {
            var cart = _context.Carts.Include(c => c.CartDetails).FirstOrDefault(c => c.Id == id);
            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartDetails); // Remove all associated items
                _context.Carts.Remove(cart);
                _context.SaveChanges();
            }
        }
    }
}
