using AuthDemoApi.Models;
using Microsoft.IdentityModel.Tokens;  // for signing and validating JWTs
using System.IdentityModel.Tokens.Jwt; // for creating JWT token objects
using System.Security.Claims;          // for adding user identity info in token
using System.Security.Cryptography;    // for generating secure refresh tokens
using System.Text;                     // for encoding secret key

namespace AuthDemoApi.Helper
{
    // JwtService is a helper class that handles creation of:
    //Access Tokens(JWTs) — used to authenticate users for a short time.
    //Refresh Tokens — used to get a new access token when the old one expires.
    //So this class helps your app implement secure login using tokens.
    public class JwtService
    {//The class and constructor
        private readonly IConfiguration _config;

        
        public JwtService(IConfiguration config)
        {
            _config = config;
        }
      //IConfiguration lets you read settings from appsettings.json(like secret key, issuer, audience).

         //Example from appsettings.json:
         
         //"Jwt": {
         //    "Key": "SuperSecretKey123",
         //    "Issuer": "yourapp.com",
         //    "Audience": "yourapp-users"
         //}
            //_config is stored so that other methods in this class can read those values.
        
        //3. GenerateAccessToken(string username)
        //This method creates a JWT access token.
        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,user.Username),
                  new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            // ✅ Claims = small pieces of information about the user.
            //Here, we add one claim — the user’s name.
            //You could add more later, like user roles or email.

            var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:key"]!));
             //✅ Converts your secret key(from config) into bytes — used to digitally sign the token.
             //This ensures no one can tamper with it

            var cred=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            //✅ Sets up the signing algorithm — HMAC SHA256 is the most common one.
            //This will be used to prove that the token came from your server (not someone else).

            var token =new JwtSecurityToken(
                issuer: _config["jwt:Issuer"],
                audience: _config["jwt:Audience"],
                claims:claims,
                expires:DateTime.UtcNow.AddMinutes(5),//✅ 5 minutes
                signingCredentials: cred
                );
               //✅ Creates the actual JWT object with:
               
               //issuer → who created the token(your app)
               
               //audience → who is supposed to use it(your users)
               
               //claims → data about the user(like username)
               
               //expires → when the token will expire(after 5 minutes)
               
               //signingCredentials → digital signature
               
               //So this token is valid for 5 minutes — after that, user must use a refresh token to get a new one.

            
            return new JwtSecurityTokenHandler().WriteToken(token);
               //✅ Converts the token object into a string, like this:

               //eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

                //This string is what you send to the frontend(React, mobile app, etc.) as the Access Token.

        }
            
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            //✅ This creates a random, secure string — used as a Refresh Token.
            
            //RandomNumberGenerator.GetBytes(64) → generates 64 random bytes.
            
            //Convert.ToBase64String() → converts those bytes into a readable string.
            
            //Example output:
            
            //"U29tZVJhbmRvbVN0cmluZ0hleQ=="
            //The refresh token is stored in the database along with the user’s record, and used to issue a new access token when the old one expires.
     
        }
        
    }


}
