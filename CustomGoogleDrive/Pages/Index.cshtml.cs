using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CustomGoogleDrive.Data;
using CustomGoogleDrive.Extensions;
using CustomGoogleDrive.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CustomGoogleDrive.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleDriveService _googleDriveService;

        public IndexModel(ApplicationDbContext context,
            GoogleDriveService googleDriveService
            )
        {
            _context = context;
            _googleDriveService = googleDriveService;
        }

        public async Task OnGetAsync()
        {
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            var userToken = await _context.AspNetUserTokens
                .Where(i => i.UserId == userId)
                .Select(i => new
                {
                    i.Value,
                    i.Name
                }).ToDictionaryAsync(i => i.Name, i => i.Value);

            var accessToken = userToken["access_token"];
            var expiresAt = DateTime.Parse(userToken["expires_at"]);
            //var tokenType = userToken["token_type"];

            var googleDrive =
                _googleDriveService.GetDriveService(
                    accessToken,
                    "",
                    expiresAt,
                    (long)expiresAt.ConvertToUnixTimestamp(),
                    userId.ToString());


        }
    }
}
