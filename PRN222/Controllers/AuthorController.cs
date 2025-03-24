using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;

public class AuthorController : Controller
{
    private readonly Prn222Context _context;

    public AuthorController(Prn222Context context)
    {
        _context = context;
    }
    // Hiển thị danh sách tác giả
    public IActionResult Index()
    {
        var authors = _context.Authors.ToList();
        return View(authors);
    }
    public async Task<IActionResult> AuthorDetail(int id)
    {
        var authors = await _context.Authors.ToListAsync();
        var author = await _context.Authors.Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.AuthorId == id);

        if (author == null)
        {
            return NotFound(); // Nếu không tìm thấy tác giả, trả về lỗi 404
        }
        ViewBag.Authors = authors; // Để hiển thị danh sách tác giả trong sidebar

        return View("~/Views/Home/AuthorDetail.cshtml", author);
    }

    public async Task<IActionResult> GetAllAuthor()
    {
        var authors = await _context.Authors.ToListAsync(); // Lấy danh sách tất cả các tác giả
        return View("~/Views/Admin/ManageAuthors.cshtml", authors); // Trả về view quản lý tác giả
    }




    // Hiển thị form thêm tác giả
    public IActionResult Create()
    {
        return View();
    }

    // Xử lý thêm tác giả
    [HttpPost]
    public IActionResult Create(Author author)
    {
        if (ModelState.IsValid)
        {
            _context.Authors.Add(author);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(author);
    }

    // Hiển thị chi tiết tác giả
    public IActionResult Detail(int id)  // Đảm bảo action có tên "Detail"
    {
        var author = _context.Authors.Find(id);
        if (author == null) return NotFound();
        return View(author);
    }


    // Hiển thị form cập nhật tác giả
    public IActionResult Edit(int id)
    {
        var author = _context.Authors.Find(id);
        if (author == null) return NotFound();
        return View(author);
    }

    // Xử lý cập nhật tác giả
    [HttpPost]
    public IActionResult Edit(int id, Author author)
    {
        if (id != author.AuthorId) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Authors.Update(author);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(author);
    }

    // Xác nhận xóa tác giả
    public IActionResult Delete(int id)
    {
        var author = _context.Authors.Find(id);
        if (author == null) return NotFound();
        return View(author);
    }

    // Xử lý xóa tác giả
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var author = _context.Authors.Find(id);
        if (author == null) return NotFound();

        _context.Authors.Remove(author);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}
