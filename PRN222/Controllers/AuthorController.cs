using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;

public class AuthorController : Controller
{
    private readonly Prn222Context _prn222Context;

    public AuthorController(Prn222Context prn222Context)
    {
        _prn222Context = prn222Context;
    }

    public async Task<IActionResult> AuthorDetail(int id)
    {
        var authors = await _prn222Context.Authors.ToListAsync();
        var author = await _prn222Context.Authors.Include(a => a.Books)
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
        var authors = await _prn222Context.Authors.ToListAsync(); // Lấy danh sách tất cả các tác giả
        return View("~/Views/Admin/ManageAuthors.cshtml", authors); // Trả về view quản lý tác giả
    }
}
