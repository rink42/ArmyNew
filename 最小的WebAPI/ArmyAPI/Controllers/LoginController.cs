using ArmyAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ArmyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _Configuration;

        public LoginController(IConfiguration configuration)
        {
            _Configuration = configuration;
        }


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
            //string accPWs_JSON = "[{\"A\":\"Army\", \"P\":\"kU/9esipmrbMKMhJKgJQmg==\", \"M\":\"ArmyAdmin\"}," +
            //                        "{\"A\":\"Acc1\", \"P\":\"ci2n2FuVXP2qorjHFYPj8g==\", \"M\":\"Acc1\"}]";

            //var accPWs = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(accPWs_JSON);
            string connectionString = _Configuration.GetConnectionString("DefaultConnection");

            MsSqlDataProvider.DB_Users dbUsers = new MsSqlDataProvider.DB_Users(connectionString);

            var users = dbUsers.GetAll();

            bool result = false;
            foreach (var u in users!)
            {
                //if (ap.TryGetValue("A", out string? accountName) && accountName == n)
                //{
                //    string password = ap.TryGetValue("P", out string? p1) ? p1 : "";
                //    if (p1 == MD5EncryptionBASE64Encode(p))
                //        result = true;
                //    // 在这里执行与匹配的操作
                //}
                if (u == null) continue;

                if (u.Name == n)
                {
                    if (u.PW == MD5EncryptionBASE64Encode(p))
                    {
                        result = true;
                        break;
                    }
                }
            }

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
