using Microsoft.AspNetCore.Mvc;
using PRN222.Models;
using Microsoft.EntityFrameworkCore;

namespace PRN222.Controllers
{
    public class ProfileController : Controller
    {
        private readonly Prn222Context _prn222Context;

        public ProfileController(Prn222Context prn222Context)
        {
            _prn222Context = prn222Context;
        }

        // Display the user's current profile information
        public async Task<IActionResult> Index(int id)
        {
            // Get user by id
            var user = await _prn222Context.Users
                                            .FirstOrDefaultAsync(u => u.PersonId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Return the view with the user data
            return View("~/Views/User/Profile.cshtml", user);
        }


        
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, string gender, DateOnly dateOfBirth, string address, string email, string phone)
        {
            var user = await _prn222Context.Users
                                       .FirstOrDefaultAsync(u => u.PersonId == id);

            if (user == null)
            {
                return NotFound();  // If user not found
            }

            // Update user info
            user.Name = name;
            user.Gender = gender;
            user.DateOfBirth = dateOfBirth;
            user.Address = address;
            user.Email = email;
            user.Phone = phone;

            // Save changes
            _prn222Context.Update(user);
            await _prn222Context.SaveChangesAsync();

            // Add a success message to TempData
            TempData["msg"] = "Thông tin đã được cập nhật!";
            return RedirectToAction("Index", new { id = user.PersonId });
        }
    }
}
