﻿using ArmyAPI.Data;
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
        public bool Check(string n, string p)
        {
            string connectionString = _Configuration.GetConnectionString("DefaultConnection");
            _WriteLog.LogMsg(connectionString);
            _WriteLog.Flush();
            MsSqlDataProvider.DB_Users dbUsers = new MsSqlDataProvider.DB_Users(connectionString);

            var users = dbUsers.GetAll();

            bool result = false;
            foreach (var u in users!)
            {
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
