using ArmyAPI.Data;
using ArmyAPI.Models;
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
        private readonly WriteLog _WriteLog; // 添加 WriteLog

        public LoginController(IConfiguration configuration, WriteLog writeLog)
        {
            _Configuration = configuration;
			_WriteLog = writeLog;
        }


        // GET: HomeController
        [HttpGet]
        public IActionResult Login()
        {
            // 實現登錄邏輯
            return Ok("Login successful!");
        }

        [HttpGet("Check")]
        public string Check(string n, string p)
        {
            string connectionString = _Configuration.GetConnectionString("DefaultConnection");

            MsSqlDataProvider.DB_Users dbUsers = new MsSqlDataProvider.DB_Users(connectionString);

            var users = dbUsers.GetAll();

            int userIndex = 0;
            foreach (var u in users!)
            {
                if (u == null) continue;

                if (u.Acc == n)
                {
                    if (u.PW == MD5EncryptionBASE64Encode(p))
                    {
                        userIndex = u.Index;
                        break;
                    }
                }
            }

            string result = "";
            if (userIndex > 0)
            {
                MsSqlDataProvider.DB_UserMenu dbUserMenu = new MsSqlDataProvider.DB_UserMenu(connectionString);
                var r = dbUserMenu.GetByUserIndex(userIndex);

                List<Menus> root = new List<Menus>();
                foreach (var item in r)
                {
                    Menus? menu = null;
                    switch (item.Level)
                    {
                        case 1:
                            menu = new Menus();
                            menu.Index = item.Index;
                            menu.Title = item.Title;
                            menu.ParentIndex = item.ParentIndex;
                            menu.CreateDatetime = item.CreateDatetime;
                            menu.C = item.C;
                            menu.U = item.U;
                            menu.D = item.D;
                            menu.R = item.R;

                            root.Add(menu);
                            break;
                        case 2:
                        case 3:
                            if (root.Count > 0)
                            {
                                foreach (var rr in root)
                                {
                                    Menus? m = rr.FindByIndex(item.ParentIndex);

                                    if (m != null)
                                    {
                                        if (m.Items == null)
                                            m.Items = new List<Menus>();

                                        menu = new Menus();
                                        menu.Index = item.Index;
                                        menu.Title = item.Title;
                                        menu.ParentIndex = item.ParentIndex;
                                        menu.CreateDatetime = item.CreateDatetime;
                                        menu.C = item.C;
                                        menu.U = item.U;
                                        menu.D = item.D;
                                        menu.R = item.R;

                                        m.Items.Add(menu);
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                }

                result = JsonConvert.SerializeObject(root);
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
