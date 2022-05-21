using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace webapi.JWT
{
    public class JwtHelper
    {
        private readonly IConfiguration Configuration;

        public JwtHelper(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        public string GenerateToken(List<Claim> claims)
        {
            //秘钥，就是标头，这里用Hmacsha256算法，需要256bit的密钥
            var securityKey = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JWT:Secret"])), SecurityAlgorithms.HmacSha256);
            //Claim，JwtRegisteredClaimNames中预定义了好多种默认的参数名，也可以像下面的Guid一样自己定义键名.
            //ClaimTypes也预定义了好多类型如role、email、name。Role用于赋予权限，不同的角色可以访问不同的接口
            //相当于有效载荷
            List<Claim> baseClaims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.Iss,Configuration["JWT:Issuer"]),
                new Claim(JwtRegisteredClaimNames.Aud,Configuration["JWT:Audience"]),
                new Claim("Guid",Guid.NewGuid().ToString("D")),
             };
            claims = claims.Union<Claim>(baseClaims).ToList<Claim>();//合并Claim，删除重复项目

            SecurityToken securityToken = new JwtSecurityToken(
                signingCredentials: securityKey,
                expires: DateTime.Now.AddDays(1),//过期时间
                claims: claims
            );
            //生成jwt令牌
            return "Bearer "+ new JwtSecurityTokenHandler().WriteToken(securityToken);
        }
    }
}