﻿using LearnAPI.Modal;
using LearnAPI.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using LearnAPI.Service;

namespace LearnAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly LearndataContext context;
        private readonly JwtSettings jwtSettings;
        private readonly IRefreshHandler refresh;

        public AuthorizeController(LearndataContext context, IOptions<JwtSettings> options, IRefreshHandler refresh)
        {
            this.context = context;
            this.jwtSettings = options.Value;
            this.refresh = refresh;
        }

        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserCred userCred) 
        {
            var user = await this.context.TblUsers.FirstOrDefaultAsync(item => item.Code == userCred.username && item.Password == userCred.password);
            if (user != null)
            {
                //generate token
                var tokenhandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(this.jwtSettings.securityKey);
                var tokendesc = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Code),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddSeconds(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)
                };
                var token = tokenhandler.CreateToken(tokendesc);
                var finalToken = tokenhandler.WriteToken(token);
                return Ok(new TokenResponse() 
                {
                    Token = finalToken,
                    RefreshToken = await this.refresh.GenerateToken(userCred.username)
                });
            }
            else 
            {
                return Unauthorized();
            }
            return Ok("");
        }

        [HttpPost("GenerateRefreshToken")]
        public async Task<IActionResult> GenerateToken([FromBody] TokenResponse token)
        {
            var _refreshToken = await this.context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Refreshtoken == token.RefreshToken);
            if (_refreshToken != null)
            {
                //generate token
                var tokenhandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(this.jwtSettings.securityKey);
                SecurityToken securityToken;
                var principal = tokenhandler.ValidateToken(token.Token, new TokenValidationParameters() 
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenkey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    
                }, out securityToken);

                var _token = securityToken as JwtSecurityToken;
                if (_token != null && _token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    string username = principal.Identity.Name;
                    var _existData = await this.context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Userid == username && item.Refreshtoken == token.RefreshToken);
                    if (_existData != null)
                    {
                        var _newToken = new JwtSecurityToken(
                              claims: principal.Claims.ToArray(),
                              expires: DateTime.Now.AddSeconds(30),
                              signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtSettings.securityKey)),SecurityAlgorithms.HmacSha256)
                            );

                        var _finalToken = tokenhandler.WriteToken(_newToken);
                        return Ok(new TokenResponse()
                        {
                            Token = _finalToken,
                            RefreshToken = await this.refresh.GenerateToken(username)
                        });
                    }
                    else 
                    {
                        return Unauthorized();
                    }
                }
                else 
                {
                     return Unauthorized();
                }
            }
            else
            {
                
            }
            return Ok("");
        }

    }
}
