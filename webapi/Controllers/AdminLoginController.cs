using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.Models;
using System.Net.Http;
using System.Security.Claims;
using webapi.JWT;
using Microsoft.AspNetCore.Authorization;

namespace QiCultureApi.Controllers
{
	/// <summary>
	/// 测试接口
	/// </summary>
	[Route("admin/Login/[action]")]
	[ApiController]
	public class AdminLoginController : ControllerBase
	{
		private readonly JwtHelper _jwtHelper;
		private readonly AppletContext _context;

		public AdminLoginController(AppletContext context, JwtHelper Helper)
		{
			_context = context;
			_jwtHelper = Helper;
		}
		/// <summary>
		/// 管理员登录
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<Admin>> PostAdminLogin(Admin Admin)
		{
			var info = await _context.Admin.SingleOrDefaultAsync(a => a.account == Admin.account && a.password == Admin.password);
			List<Claim> claims = new List<Claim>() {
					new Claim(ClaimTypes.Role,"admin"),
					new Claim("admin",info.ID.ToString()),
				   };
			string token = _jwtHelper.GenerateToken(claims);
			return Ok(new { code = 200, msg = "登录成功", data = token });

		}

		[HttpGet]
		[Authorize(Roles = "admin")]
		public string GetString()
		{
			return "ok";
		}
	}
}
