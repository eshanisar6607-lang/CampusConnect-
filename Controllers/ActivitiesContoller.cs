
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusKonnect.Data;
using CampusKonnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusKonnect.Controllers
{
    [Authorize]
    public class ActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivitiesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Campus Activities Dashboard
        public async Task<IActionResult> Index(
            string filter = "all",
            string search = "")
        {
            var userId = _userManager.GetUserId(User);

            IQueryable<Activity> activitiesQuery = _context.Activities
                .Include(a => a.CreatedByUser)
                .Include(a => a.Participants);

        // Filter Activities
            if (filter == "created")
            {
                activitiesQuery = activitiesQuery.Where(a =>
                    a.CreatedByUserId == userId);
            }
            else if (filter == "joined")
            {
                activitiesQuery = activitiesQuery.Where(a =>
                    a.Participants.Any(p =>
                        p.StudentId == userId &&
                        p.ParticipationStatus == "Active"));
            }
            else
            {
                filter = "all";
            }

         // Load Activities into List
            List<Activity> activityList = await activitiesQuery
                .OrderBy(a => a.ScheduledAt)
                .ToListAsync();

        // Linear Search using List
            if (!string.IsNullOrWhiteSpace(search))
            {
                List<Activity> searchResults = new List<Activity>();

                foreach (Activity activity in activityList)
                {
                    if (activity.Title.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        searchResults.Add(activity);
                    }
                }

                activityList = searchResults;
            }

            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentFilter = filter;
            ViewBag.Search = search;

            return View(activityList);
        }

        // Create Activity Page
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Save New Activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateActivityViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.ScheduledAt <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "ScheduledAt",
                    "Activity date and time must be in the future.");

                return View(model);
            }

            if (model.ActivityType != "Online" &&
                model.ActivityType != "OnCampus")
            {
                ModelState.AddModelError(
                    "ActivityType",
                    "Please select a valid activity type.");

                return View(model);
            }

            if (model.ActivityType == "OnCampus" &&
                string.IsNullOrWhiteSpace(model.Location))
            {
                ModelState.AddModelError(
                    "Location",
                    "Location is required for On-Campus activities.");

                return View(model);
            }

            if (model.ActivityType == "Online" &&
                !string.IsNullOrWhiteSpace(model.MeetingLink))
            {
                bool validLink = Uri.TryCreate(
                    model.MeetingLink,
                    UriKind.Absolute,
                    out Uri? meetingUri) &&
                    (meetingUri.Scheme == Uri.UriSchemeHttp ||
                     meetingUri.Scheme == Uri.UriSchemeHttps);

                if (!validLink)
                {
                    ModelState.AddModelError(
                        "MeetingLink",
                        "Please enter a valid meeting link starting with https:// or http://.");

                    return View(model);
                }
            }

            var activity = new Activity
            {
                Title = model.Title,
                Description = model.Description,
                ScheduledAt = model.ScheduledAt,
                ActivityType = model.ActivityType,

                Location = model.ActivityType == "OnCampus"
                    ? model.Location
                    : null,

                Platform = model.ActivityType == "Online"
                    ? model.Platform
                    : null,

                MeetingLink = model.ActivityType == "Online"
                    ? model.MeetingLink
                    : null,

                MaximumParticipants = model.MaximumParticipants,
                CreatedByUserId = _userManager.GetUserId(User)!,
                Status = "Available",
                CreatedAt = DateTime.Now
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Edit Activity Page
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (activity.CreatedByUserId != userId)
                return Forbid();

            var model = new CreateActivityViewModel
            {
                Title = activity.Title,
                Description = activity.Description,
                ScheduledAt = activity.ScheduledAt,
                ActivityType = activity.ActivityType,
                Location = activity.Location,
                Platform = activity.Platform,
                MeetingLink = activity.MeetingLink,
                MaximumParticipants = activity.MaximumParticipants
            };

            ViewBag.ActivityId = activity.ActivityId;

            return View(model);
        }

        // Update Activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CreateActivityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActivityId = id;
                return View(model);
            }

            var activity = await _context.Activities
                .Include(a => a.Participants)
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (activity.CreatedByUserId != userId)
                return Forbid();

            if (model.ScheduledAt <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "ScheduledAt",
                    "Activity date and time must be in the future.");

                ViewBag.ActivityId = id;
                return View(model);
            }

            if (model.ActivityType != "Online" &&
                model.ActivityType != "OnCampus")
            {
                ModelState.AddModelError(
                    "ActivityType",
                    "Please select a valid activity type.");

                ViewBag.ActivityId = id;
                return View(model);
            }

            if (model.ActivityType == "OnCampus" &&
                string.IsNullOrWhiteSpace(model.Location))
            {
                ModelState.AddModelError(
                    "Location",
                    "Location is required for On-Campus activities.");

                ViewBag.ActivityId = id;
                return View(model);
            }

            if (model.ActivityType == "Online" &&
                !string.IsNullOrWhiteSpace(model.MeetingLink))
            {
                bool validLink = Uri.TryCreate(
                    model.MeetingLink,
                    UriKind.Absolute,
                    out Uri? meetingUri) &&
                    (meetingUri.Scheme == Uri.UriSchemeHttp ||
                     meetingUri.Scheme == Uri.UriSchemeHttps);

                if (!validLink)
                {
                    ModelState.AddModelError(
                        "MeetingLink",
                        "Please enter a valid meeting link.");

                    ViewBag.ActivityId = id;
                    return View(model);
                }
            }

            int activeParticipants = activity.Participants.Count(
                p => p.ParticipationStatus == "Active");

            if (model.MaximumParticipants < activeParticipants)
            {
                ModelState.AddModelError(
                    "MaximumParticipants",
                    "Maximum participants cannot be less than current active participants.");

                ViewBag.ActivityId = id;
                return View(model);
            }

            activity.Title = model.Title;
            activity.Description = model.Description;
            activity.ScheduledAt = model.ScheduledAt;
            activity.ActivityType = model.ActivityType;

            activity.Location = model.ActivityType == "OnCampus"
                ? model.Location
                : null;

            activity.Platform = model.ActivityType == "Online"
                ? model.Platform
                : null;

            activity.MeetingLink = model.ActivityType == "Online"
                ? model.MeetingLink
                : null;

            activity.MaximumParticipants = model.MaximumParticipants;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Join Activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);

            var activity = await _context.Activities
                .Include(a => a.Participants)
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound();

            if (activity.CreatedByUserId == userId)
            {
                TempData["Message"] = "You cannot join your own activity.";
                return RedirectToAction("Index");
            }

            if (activity.Status != "Available" ||
                activity.ScheduledAt <= DateTime.Now)
            {
                TempData["Message"] = "This activity is no longer available.";
                return RedirectToAction("Index");
            }

            var alreadyJoined = activity.Participants.Any(p =>
                p.StudentId == userId &&
                p.ParticipationStatus == "Active");

            if (alreadyJoined)
            {
                TempData["Message"] = "You have already joined this activity.";
                return RedirectToAction("Index");
            }

            int activeParticipants = activity.Participants.Count(p =>
                p.ParticipationStatus == "Active");

            if (activeParticipants >= activity.MaximumParticipants)
            {
                activity.Status = "Full";
                await _context.SaveChangesAsync();

                TempData["Message"] = "Sorry, this activity is full.";
                return RedirectToAction("Index");
            }

            var participant = new ActivityParticipant
            {
                ActivityId = activity.ActivityId,
                StudentId = userId!,
                JoinedAt = DateTime.Now,
                ParticipationStatus = "Active"
            };

            _context.ActivityParticipants.Add(participant);

            if (activeParticipants + 1 >= activity.MaximumParticipants)
            {
                activity.Status = "Full";
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "You have successfully joined the activity!";

            return RedirectToAction("Index");
        }

        // Leave Activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);

            var participant = await _context.ActivityParticipants
                .Include(p => p.Activity)
                .FirstOrDefaultAsync(p =>
                    p.ActivityId == id &&
                    p.StudentId == userId &&
                    p.ParticipationStatus == "Active");

            if (participant == null)
            {
                TempData["Message"] = "You have not joined this activity.";
                return RedirectToAction("Index");
            }

            var activity = participant.Activity;

            if (activity == null)
                return NotFound();

            if (activity.ScheduledAt <= DateTime.Now ||
                activity.Status == "Cancelled" ||
                activity.Status == "Completed")
            {
                TempData["Message"] = "You cannot leave this activity now.";
                return RedirectToAction("Index");
            }

            participant.ParticipationStatus = "Left";

            int activeParticipants = await _context.ActivityParticipants
                .CountAsync(p =>
                    p.ActivityId == id &&
                    p.ParticipationStatus == "Active");

            if (activeParticipants - 1 < activity.MaximumParticipants)
            {
                activity.Status = "Available";
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "You have successfully left the activity!";

            return RedirectToAction("Index", new { filter = "joined" });
        }

        // DSA Hashset to store unique participant IDs

        // Activity Details
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var activity = await _context.Activities
                .Include(a => a.CreatedByUser)
                .Include(a => a.Participants)
                    .ThenInclude(p => p.Student)
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound();

            bool isCreator = activity.CreatedByUserId == userId;

            bool isJoined = activity.Participants.Any(p =>
                p.StudentId == userId &&
                p.ParticipationStatus == "Active");

            // HashSet to store unique participant IDs
            HashSet<string> uniqueStudentIds = new HashSet<string>();

            foreach (var participant in activity.Participants)
            {
                if (participant.ParticipationStatus == "Active")
                {
                    uniqueStudentIds.Add(participant.StudentId);
                }
            }

            // Get unique active participants
            var uniqueParticipants = activity.Participants
                .Where(p =>
                    p.ParticipationStatus == "Active" &&
                    uniqueStudentIds.Contains(p.StudentId))
                .ToList();

            ViewBag.UniqueParticipants = uniqueParticipants;
            ViewBag.IsCreator = isCreator;
            ViewBag.IsJoined = isJoined;

            ViewBag.CurrentParticipants = uniqueStudentIds.Count;

            ViewBag.CanSeeMeetingLink = isCreator || isJoined;

            return View(activity);
        }


        // Cancel Activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);

            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound();

            if (activity.CreatedByUserId != userId)
                return Forbid();

            if (activity.Status == "Cancelled" ||
                activity.Status == "Completed")
            {
                TempData["Message"] = "This activity cannot be cancelled.";
                return RedirectToAction("Index");
            }

            activity.Status = "Cancelled";

            await _context.SaveChangesAsync();

            TempData["Message"] = "Activity cancelled successfully!";

            return RedirectToAction("Index", new { filter = "created" });
        }
    }
}
