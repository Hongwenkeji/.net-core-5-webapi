# .net-core-5-webapi
<!-- 1:创建项目 -->
dotnet new webapi -o 项目名称 --no-https
<!-- 2:链接数据库 -->
appsettings.json配置
"ConnectionStrings": {
    "Sqlconn": "server=IP;Database=数据库名称;uid=sa;pwd=密码;"
},
<!--3:添加nuGet包 -->
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet tool install -g dotnet-aspnet-codegenerator
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
<!-- 4:注册数据库上下文 -->
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
services.AddDbContext<AppletContext>(option => option.UseSqlServer(
                Configuration.GetConnectionString("Sqlconn")
            ));
<!-- 5:生成控制器 -->
dotnet aspnet-codegenerator controller -name 控制器名称 -async -api -m 实体类 -dc 数据库上下文 -outDir Controllers
<!-- 6:跨域设置 -->
配置ConfigureServices
//跨域请求配置
services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                        builder =>
                        {
                            builder.WithOrigins("允许跨域地址")
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials();
                        });
            });
<!-- 7:Token配置 -->
<!-- 7-(1)引入nuGet -->
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
<!-- 7-(2)创建JWT -->
appsettings.json配置
"JWT": {
        <!-- 密钥 -->
        "Secret": "~!@#$%^&*()_+qwertyuiopasldkh[o51485421ajshk^%*)kasd",
        <!-- 发行方 -->
        "Issuer": "kfjdhf",
        <!-- 订阅方 -->
        "Audience": "kfjdhf"
    }
<!-- 7-(3)配置Startup.cs -->
<!-- 开启swagger验证 -->
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
{

    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    BearerFormat = "JWT",
    Scheme = "Bearer"
});

c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] { }
    }
});
<!-- 认证参数 -->
//生成密钥
var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JWT:Secret"]));
services.AddAuthentication("Bearer")
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,//是否验证签名,不验证的画可以篡改数据，不安全
        IssuerSigningKey = signingKey,//解密的密钥
        ValidateIssuer = true,//是否验证发行人，就是验证载荷中的Iss是否对应ValidIssuer参数
        ValidIssuer = Configuration["JWT:Issuer"],//发行人
        ValidateAudience = true,//是否验证订阅人，就是验证载荷中的Aud是否对应ValidAudience参数
        ValidAudience = Configuration["JWT:Audience"],//订阅人
        ValidateLifetime = true,//是否验证过期时间，过期了就拒绝访问
        ClockSkew = TimeSpan.Zero,//这个是缓冲过期时间，也就是说，即使我们配置了过期时间，这里也要考虑进去，过期时间+缓冲，默认好像是7分钟，你可以直接设置为0
        RequireExpirationTime = true,
    };
});
<!-- 开始授权认证 -->
app.UseAuthentication();//认证

app.UseAuthorization();//授权
<!-- 7-(4)生成JWT帮助类 -->
<!-- services.AddScoped<JwtHelper>();注册帮助类 -->
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
return new JwtSecurityTokenHandler().WriteToken(securityToken);
