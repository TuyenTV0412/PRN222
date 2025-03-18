﻿using Microsoft.AspNetCore.Mvc;
using PRN222.Models;
using Microsoft.EntityFrameworkCore;

namespace PRN222.Controllers
{
    public class BookDetailController : Controller
    {
        private readonly Prn222Context _prn222Context;

        public BookDetailController(Prn222Context prn222Context)
        {
            _prn222Context = prn222Context;
        }

        // Hiển thị chi tiết sách
        public async Task<IActionResult> Index(int id)
        {
            // Lấy thông tin sách từ database theo BookID
            var book = await _prn222Context.Books
                                         .Include(b => b.Author)      // Bao gồm thông tin tác giả
                                         .Include(b => b.Category)    // Bao gồm thông tin danh mục
                                         .Include(b => b.Publisher)   // Bao gồm thông tin nhà xuất bản
                                         .FirstOrDefaultAsync(b => b.BookId == id);  // Lấy sách theo BookID

            if (book == null)
            {
                return NotFound();  // Nếu sách không tìm thấy, trả về lỗi 404
            }

            // Lấy CategoryId từ sách
            var categoryId = book.CategoryId;

            // Lấy 4 sách ngẫu nhiên cùng category
            var randomBooks = await _prn222Context.Books
                                                   .Where(b => b.CategoryId == categoryId && b.BookId != id)  // Lọc sách theo category và loại bỏ sách hiện tại
                                                   .Include(b => b.Author)
                                                   .Include(b => b.Category)
                                                   .Include(b => b.Publisher)
                                                   .OrderByDescending(b => b.BookId)
                                                   .Take(4)
                                                   .ToListAsync();

            var categories = await _prn222Context.Categories.ToListAsync(); // Lấy danh sách category
            ViewBag.Categories = categories;
            // Truyền book và danh sách sách ngẫu nhiên sang View
            ViewBag.RandomBooks = randomBooks;

            return View("~/Views/Home/BookDetail.cshtml", book);  // Đảm bảo đường dẫn chính xác
        }


        [HttpGet]
        public async Task<IActionResult> UpdateBookInfo(int bookID)
        {
            var book = await _prn222Context.Books
                                           .Include(b => b.Author)
                                           .Include(b => b.Category)
                                           .Include(b => b.Publisher)
                                           .FirstOrDefaultAsync(b => b.BookId == bookID);

            if (book == null)
            {
                return NotFound();
            }

            var categories = await _prn222Context.Categories.ToListAsync();
            var authors = await _prn222Context.Authors.ToListAsync();
            var publishers = await _prn222Context.Publishers.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Author = authors;
            ViewBag.Publisher = publishers;

            // Chỉ định đường dẫn cụ thể đến view
            return View("~/Views/Book/UpdateBookInfo.cshtml", book);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookInfo(Book updatedBook)
        {
            var book = await _prn222Context.Books.FirstOrDefaultAsync(b => b.BookId == updatedBook.BookId);
            if (book == null)
            {
                return NotFound(); // Nếu không tìm thấy sách, trả về lỗi 404
            }

            // Cập nhật các thông tin khác của sách
            book.BookName = updatedBook.BookName;
            book.AuthorId = updatedBook.AuthorId;
            book.CategoryId = updatedBook.CategoryId;
            book.PublisherId = updatedBook.PublisherId;
            book.Description = updatedBook.Description;
            book.PublishingYear = updatedBook.PublishingYear;
            book.Quantity = updatedBook.Quantity;
            if(updatedBook.Images != null)
            {
                book.Images = updatedBook.Images;
            }
            else
            {
                book.Images = book.Images;
            }
           
            try
            {
                await _prn222Context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin sách thành công!";
                return RedirectToAction("ManageBooks", "Admin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể cập nhật sách. Lỗi: {ex.Message}";
                return RedirectToAction("ManageBooks", "Admin");
            }
        }








    }
}
