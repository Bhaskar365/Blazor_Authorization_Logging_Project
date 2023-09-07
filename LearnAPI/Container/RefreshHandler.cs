using LearnAPI.Repos;
using LearnAPI.Service;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using LearnAPI.Repos.Models;

namespace LearnAPI.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly LearndataContext context;
        public RefreshHandler(LearndataContext context) 
        {
            this.context = context;
        }
        public async Task<string> GenerateToken(string username)
        {
            var randomNumber = new byte[32];
            using ( var randomNumberGenerator = RandomNumberGenerator.Create()) 
            {
                randomNumberGenerator.GetBytes(randomNumber);
                string refreshToken = Convert.ToBase64String(randomNumber);
                var existingToken = this.context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Userid == username).Result;
                if (existingToken != null)
                {
                    existingToken.Refreshtoken = refreshToken;
                }
                else 
                {
                    await this.context.TblRefreshtokens.AddAsync(new TblRefreshtoken
                    {
                        Userid = username,
                        Tokenid = new Random().Next().ToString(),
                        Refreshtoken = refreshToken
                    });
                }
                await this.context.SaveChangesAsync();

                return refreshToken;
            }
        }
    }
}
