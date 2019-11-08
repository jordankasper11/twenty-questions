using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;

namespace TwentyQuestions.Data.Repositories
{
    public interface IAuthenticationRepository : IRepository
    {
        Task<string> Login(LoginRequest request);

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

        public async Task<string> Login(LoginRequest request)
        {
            var userCredentials = await GetUserCredentials(request);

            if (userCredentials != null && ValidatePassword(request.Password, userCredentials.PasswordSalt, userCredentials.PasswordHash))
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey));
                var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddMinutes(300);

                var claims = new List<Claim>()
                {
                    new Claim("userId", userCredentials.Id.ToString()),
                    new Claim("username", userCredentials.Username),
                    new Claim("email", userCredentials.Email)
                };

                var token = new JwtSecurityToken(claims: claims, expires: expires, signingCredentials: signingCredentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            return null;
        }

        private async Task<UserCredentialsDto> GetUserCredentials(LoginRequest request)
        {
            await EnsureConnectionOpen();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = "User_GetCredentials";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar) { Value = request.Username });

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

                        return userCredentials;
                    }
                }
            }

            return null;
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

