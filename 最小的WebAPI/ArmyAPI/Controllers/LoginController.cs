using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArmyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        // GET: HomeController
        [HttpGet]
        public IActionResult Login()
        {
            // 實現登錄邏輯
            return Ok("Login successful!");
        }

        [HttpGet("Check")]
        public bool Check(string n, string p)
        {
            string acc = "Army";
            string pw = "kU/9esipmrbMKMhJKgJQmg=="; //ArmyAdmin

            bool result = false;

            if (n == acc && MD5EncryptionBASE64Encode(p) == pw)
                result = true;

            return result;
        }

        private string MD5EncryptionBASE64Encode(string s)
        {
            System.Security.Cryptography.MD5 hs = System.Security.Cryptography.MD5.Create();
            byte[] db = hs.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));
            return Convert.ToBase64String(db);

        }
    }
}
