using Microsoft.AspNetCore.Mvc;
using PRN222.Models;
using System;
using System.Linq;

namespace PRN222.Controllers
{
    public class CategoryController : Controller
    {
        private readonly Prn222Context _context;

        public CategoryController(Prn222Context context)
        {
            _context = context;
        }

        // GET: Category
        public IActionResult Index(int page = 1)
        {
            int pageSize = 10; // Số mục trên mỗi trang
            var categories = _context.Categories
                                     .OrderBy(c => c.CategoryId)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();

            int totalCategories = _context.Categories.Count();
            int totalPages = (int)Math.Ceiling((double)totalCategories / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(categories);
        }


        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            // Kiểm tra xem tên Category đã tồn tại chưa (không phân biệt chữ hoa chữ thường)
            bool isDuplicate = _context.Categories.Any(c => c.CategoryName.ToLower() == category.CategoryName.ToLower());

            if (isDuplicate)
            {
                ModelState.AddModelError("CategoryName", "Tên thể loại đã tồn tại.");
                return View(category);
            }

            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Category category)
        {
            if (id != category.CategoryId) return NotFound();

            bool isDuplicate = _context.Categories.Any(c =>
                c.CategoryName.ToLower() == category.CategoryName.ToLower() && c.CategoryId != id);

            if (isDuplicate)
            {
                ModelState.AddModelError("CategoryName", "Tên thể loại đã tồn tại.");
                return View(category);
            }

            if (ModelState.IsValid)
            {
                _context.Update(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }


        // GET: Category/Delete/5
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return Json(new { success = true });
        }


        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
