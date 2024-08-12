using AllUp.Models;
using AllUp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace AllUp.Areas.Manage.Controllers;

[Area("Manage")]
public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        IEnumerable<AppUser> users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> ChangeStatus(string id)
    {
        AppUser? user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return BadRequest();
        user.IsBlocked = !user.IsBlocked;
        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Create(NewUserVM newUser)
    {
        ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        if (!ModelState.IsValid) return View(newUser);

        bool roleExist = await _roleManager.RoleExistsAsync(newUser.RoleName);

        if (!roleExist)
        {
            ModelState.AddModelError("RoleName", "Invalid role.");
            return View(newUser);
        }
        AppUser user = new() { Fullname = newUser.Fullname, Email = newUser.Email, UserName = newUser.Username };
        IdentityResult result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(newUser);
        }
        await _userManager.AddToRoleAsync(user, newUser.RoleName);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(string? id)
    {
        if (id is null) return BadRequest();
        AppUser? user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        NewUserVM userVM = new() { Email = user.Email, Fullname = user.Fullname, RoleName = (await _userManager.GetRolesAsync(user))[0], Username = user.UserName };
        return View(userVM);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Update(string? id, NewUserVM userVM)
    {
        if (!ModelState.IsValid) return View(userVM);
        if (id == null) return BadRequest();
        AppUser? user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        if (userVM.Password != userVM.RePassword)
        {
            ModelState.AddModelError("RePassword", "Passwords do not match");
            return View(userVM);
        }
        if (!await _roleManager.RoleExistsAsync(userVM.RoleName))
        {
            ModelState.AddModelError("RoleName", "Invalid role");
            return View(userVM);
        }
        if (userVM.Password is not null)
        {

            if (!await _userManager.CheckPasswordAsync(user, userVM.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Wrong Password");
                return View(userVM);
            }
            await _userManager.ChangePasswordAsync(user, userVM.CurrentPassword, userVM.Password);
        }
        user.UserName = userVM.Username;
        user.Email = userVM.Email;
        user.Fullname = userVM.Fullname;
        await _userManager.UpdateAsync(user);
        await _userManager.UpdateAsync(user);
        await _userManager.RemoveFromRoleAsync(user, (await _userManager.GetRolesAsync(user))[0]);
        await _userManager.AddToRoleAsync(user, userVM.RoleName);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string? id)
    {
        if (id is null) return BadRequest();
        AppUser? user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        await _userManager.DeleteAsync(user);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(string? id)
    {
        if (id is null) return BadRequest();
        AppUser? user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }
}
