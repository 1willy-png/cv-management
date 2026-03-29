using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CvManagement.Data;
using CvManagement.Models;
using CvManagement.Services;

namespace CvManagement.Controllers;

[Authorize] // Only authenticated users can access
public class SubmissionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SupabaseStorageService _storageService;

    public SubmissionsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SupabaseStorageService storageService)
    {
        _context = context;
        _userManager = userManager;
        _storageService = storageService;
    }

    // GET: Submissions
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var submissions = await _context.CVSubmissions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
        return View(submissions);
    }

    // GET: Submissions/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Submissions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("FirstName,LastName,Email,Position,AdditionalInfo")] CVSubmission submission,
    IFormFile cvFile)
    {
        Console.WriteLine("=== Create POST started ===");
        Console.WriteLine($"Email: {submission.Email}");
        Console.WriteLine($"File present: {cvFile != null}, size: {cvFile?.Length}");

        // Set UserId
        var userId = _userManager.GetUserId(User);
        submission.UserId = userId;
        Console.WriteLine($"UserId: {userId}");

        if (cvFile != null && cvFile.Length > 0)
        {
            Console.WriteLine("Attempting to upload file...");
            try
            {
                var fileUrl = await _storageService.UploadFileAsync(cvFile);
                Console.WriteLine($"Upload successful. URL: {fileUrl}");
                submission.CVFileUrl = fileUrl;
                submission.CVFileName = cvFile.FileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload FAILED: {ex.Message}");
                ModelState.AddModelError("cvFile", "Error uploading file: " + ex.Message);
                return View(submission);
            }
        }
        else
        {
            Console.WriteLine("No file provided.");
            ModelState.AddModelError("cvFile", "CV file is required.");
            return View(submission);
        }

        Console.WriteLine("ModelState valid? " + ModelState.IsValid);
        if (ModelState.IsValid)
        {
            Console.WriteLine("Saving to database...");
            _context.Add(submission);
            await _context.SaveChangesAsync();
            Console.WriteLine("Saved successfully. ID: " + submission.Id);
            TempData["SuccessMessage"] = "Your CV has been submitted successfully!";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            Console.WriteLine("ModelState invalid. Errors:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }

        return View(submission);
    }
    // GET: Submissions/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var submission = await _context.CVSubmissions
            .FirstOrDefaultAsync(m => m.Id == id);
        if (submission == null) return NotFound();

        // Ensure the user can only see their own submission
        if (submission.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            return Forbid();

        return View(submission);
    }

    // GET: Submissions/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var submission = await _context.CVSubmissions
            .FirstOrDefaultAsync(m => m.Id == id);
        if (submission == null) return NotFound();

        if (submission.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            return Forbid();

        return View(submission);
    }

    // POST: Submissions/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var submission = await _context.CVSubmissions.FindAsync(id);
        if (submission != null)
        {
            // Optional: delete the file from Supabase (requires the file path)
            // await _storageService.DeleteFileAsync(submission.CVFileUrl); // if you implement delete
            _context.CVSubmissions.Remove(submission);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}