using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CvManagement.Data;
using CvManagement.Models;

namespace CvManagement.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Submissions()
    {
        var submissions = await _context.CVSubmissions
            .Include(s => s.User)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
        return View(submissions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var submission = await _context.CVSubmissions.FindAsync(id);
        if (submission != null)
        {
            _context.CVSubmissions.Remove(submission);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Submissions));
    }
}