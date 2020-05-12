using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    [Authorize(Roles = "Dev")]
    [Authorize(Policy = "Dev")]
    public class DevController : Controller
    {
        [Authorize(Roles = "Dev")]
        [Authorize(Policy = "Dev")]
        public IActionResult Decode()
        {
            return View();
        }
    }
}
