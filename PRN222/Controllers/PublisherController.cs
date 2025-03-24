using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;
using System.Linq;

public class PublisherController : Controller
{
    private readonly Prn222Context _context; // Thay MyDbContext bằng DbContext của bạn

    public PublisherController(Prn222Context context)
    {
        _context = context;
    }

    // Hiển thị danh sách nhà xuất bản
    public IActionResult Index()
    {
        var publishers = _context.Publishers.ToList();
        return View(publishers);
    }

    // Hiển thị form tạo mới nhà xuất bản
    public IActionResult Create()
    {
        return View();
    }

    // Xử lý thêm nhà xuất bản
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Publisher publisher)
    {
        bool isDuplicate = _context.Publishers.Any(p => p.PublisherName.ToLower() == publisher.PublisherName.ToLower());
        if (isDuplicate)
        {
            ModelState.AddModelError("PublisherName", "Publisher name already exists.");
            return View(publisher);
        }

        if (ModelState.IsValid)
        {
            _context.Publishers.Add(publisher);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(publisher);
    }

    // Hiển thị form chỉnh sửa
    public IActionResult Edit(int id)
    {
        var publisher = _context.Publishers.Find(id);
        if (publisher == null) return NotFound();
        return View(publisher);
    }

    // Xử lý cập nhật nhà xuất bản
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Publisher publisher)
    {
        if (id != publisher.PublisherId) return NotFound();

        bool isDuplicate = _context.Publishers.Any(p =>
            p.PublisherName.ToLower() == publisher.PublisherName.ToLower() && p.PublisherId != id);

        if (isDuplicate)
        {
            ModelState.AddModelError("PublisherName", "Publisher name already exists.");
            return View(publisher);
        }

        if (ModelState.IsValid)
        {
            _context.Update(publisher);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(publisher);
    }

    // Xác nhận xóa nhà xuất bản
    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var publisher = _context.Publishers.Find(id);
        if (publisher == null)
        {
            return NotFound();
        }

        _context.Publishers.Remove(publisher);
        _context.SaveChanges();
        return Ok();
    }


    // Xử lý xóa nhà xuất bản
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var publisher = _context.Publishers.Find(id);
        if (publisher == null) return NotFound();

        _context.Publishers.Remove(publisher);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}
