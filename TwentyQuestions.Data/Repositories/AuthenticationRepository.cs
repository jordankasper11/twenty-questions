using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Models.Responses;

namespace TwentyQuestions.Data.Repositories
{
    public interface IAuthenticationRepository : IRepository
    {
        Task<LoginResponse> Login(LoginRequest request);

        Task<LoginResponse> RefreshToken(RefreshTokenRequest request);

        UserCredentials HashPassword(string password);

        bool ValidatePassword(string password, string passwordSalt, string passwordHash);
    }

    public class AuthenticationRepository : BaseRepository, IAuthenticationRepository
    {
        private string _securityKey = null;

        public AuthenticationRepository(SqlConnection connection, IRepositoryContext context, string securityKey) : base(connection, context)
        {
            _securityKey = securityKey;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var userCredentials = await GetUserCredentials(request);

            if (userCredentials != null && ValidatePassword(request.Password, userCredentials.PasswordSalt, userCredentials.PasswordHash))
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                if (!String.IsNullOrWhiteSpace(userCredentials.RefreshToken))
                {
                    var refreshToken = tokenHandler.ReadJwtToken(userCredentials.RefreshToken);

                    if (refreshToken.ValidTo < DateTime.UtcNow)
                    {
                        userCredentials.RefreshToken = GetRefreshToken(userCredentials.Id);

                        await SaveRefreshToken(userCredentials.Id, userCredentials.RefreshToken);
                    }
                }
                else
                {
                    userCredentials.RefreshToken = GetRefreshToken(userCredentials.Id);

                    await SaveRefreshToken(userCredentials.Id, userCredentials.RefreshToken);
                }

                var loginResponse = new LoginResponse();

                loginResponse.AccessToken = GetAccessToken(userCredentials);
                loginResponse.RefreshToken = userCredentials.RefreshToken;

                return loginResponse;
            }

            return null;
        }

        public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
        {
            var userCredentials = await GetUserCredentials(request.RefreshToken);

            if (userCredentials != null)
            {
                var loginResponse = new LoginResponse();

                loginResponse.AccessToken = GetAccessToken(userCredentials);

                var token = new JwtSecurityTokenHandler().ReadJwtToken(request.RefreshToken);

                if (token.ValidTo >= DateTime.UtcNow)
                    loginResponse.RefreshToken = request.RefreshToken;
                else
                {
                    var userId = Guid.Parse(token.Claims.Single(c => c.Type == "userId").Value);

                    loginResponse.RefreshToken = GetRefreshToken(userId);

                    await SaveRefreshToken(userId, loginResponse.RefreshToken);
                }

                return loginResponse;
            }

            return null;
        }

        private string GetAccessToken(UserCredentialsDto userCredentials)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(1);

            var claims = new List<Claim>()
            {
                new Claim("userId", userCredentials.Id.ToString()),
                new Claim("username", userCredentials.Username),
                new Claim("email", userCredentials.Email)
            };

            var token = new JwtSecurityToken(claims: claims, expires: expires, signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetRefreshToken(Guid userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(30);

            var claims = new List<Claim>()
            {
                new Claim("userId", userId.ToString())
            };

            var token = new JwtSecurityToken(claims: claims, expires: expires, signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<UserCredentialsDto> GetUserCredentials(string refreshToken)
        {
            return await GetUserCredentials(null, refreshToken);
        }

        private async Task<UserCredentialsDto> GetUserCredentials(LoginRequest request)
        {
            return await GetUserCredentials(request, null);
        }

        private async Task<UserCredentialsDto> GetUserCredentials(LoginRequest request, string refreshToken)
        {
            await EnsureConnectionOpen();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = "User_GetCredentials";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar) { Value = request?.Username });
                sqlCommand.Parameters.Add(new SqlParameter("@RefreshToken", SqlDbType.NVarChar) { Value = refreshToken });

                using (var sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();

                    dataTable.Load(sqlDataReader);

                    if (dataTable.Rows.Count > 0)
                    {
                        var userCredentials = new UserCredentialsDto();

                        userCredentials.Id = dataTable.Rows[0].Field<Guid>("Id");
                        userCredentials.Username = dataTable.Rows[0].Field<string>("Username");
                        userCredentials.Email = dataTable.Rows[0].Field<string>("Email");
                        userCredentials.PasswordHash = dataTable.Rows[0].Field<string>("PasswordHash");
                        userCredentials.PasswordSalt = dataTable.Rows[0].Field<string>("PasswordSalt");
                        userCredentials.RefreshToken = dataTable.Rows[0].Field<string>("RefreshToken");

                        return userCredentials;
                    }
                }
            }

            return null;
        }

        private async Task SaveRefreshToken(Guid userId, string refreshToken)
        {
            await EnsureConnectionOpen();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = "User_SaveRefreshToken";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = userId });
                sqlCommand.Parameters.Add(new SqlParameter("@RefreshToken", SqlDbType.NVarChar) { Value = refreshToken });

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        public UserCredentials HashPassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var userCredentials = new UserCredentials();

                userCredentials.PasswordSalt = Convert.ToBase64String(hmac.Key);
                userCredentials.PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

                return userCredentials;
            }
        }

        public bool ValidatePassword(string password, string passwordSalt, string passwordHash)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(passwordSalt)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password))) == passwordHash;
            }
        }
    }
}

