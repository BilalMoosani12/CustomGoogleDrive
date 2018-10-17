using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomGoogleDrive.Data;
using CustomGoogleDrive.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomGoogleDrive.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleDriveService _googleDriveService;
        private readonly UserManager<IdentityUser<int>> _userManager;
        private Task<IdentityUser<int>> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public IndexModel(ApplicationDbContext context,
            UserManager<IdentityUser<int>> userManager,
            GoogleDriveService googleDriveService
            )
        {
            _userManager = userManager;
            _context = context;
            _googleDriveService = googleDriveService;
        }

        public async Task OnGetAsync()
        {
            var users = await _userManager.GetUserAsync(HttpContext.User);
            var ProviderKey = _context.AspNetUserLogins
                .Where(i => i.UserId == users.Id)
                .Select(i => i.ProviderKey).Single();

            //var userToken = _context.AspNetUserTokens
            //    .Where(i => i.UserId == users.Id)
            //    .Select(i => new
            //    {
            //        i.Value,
            //        i.Name
            //    }).ToDictionary(i => new { i.Name, i.Value });

            //var accessToken = userToken.TryGetValue( "a", out var accessType);

            var userToken = _context.AspNetUserTokens
               .Where(i => i.UserId == users.Id)
               .Select(i => new
               {
                   i.Value,
                   i.Name
               }).ToList();

            var accessToken = userToken
                .Where(i => i.Name == "access_token")
                .Select(i => i.Value).Single();
            var expires_at = userToken
               .Where(i => i.Name == "expires_at")
               .Select(i => i.Value).Single();
            var token_type = userToken
               .Where(i => i.Name == "token_type")
               .Select(i => i.Value).Single();
            var TicketCreated = userToken
              .Where(i => i.Name == "TicketCreated")
              .Select(i => i.Value).Single();

            googleDriveService.

        }
    }
}
