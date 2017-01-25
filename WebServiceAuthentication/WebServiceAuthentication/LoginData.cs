using System;

namespace WebServiceAuthentication
{
    public struct LoginData
    {
        public string User { get; }
        public string Password { get; }

        public LoginData(string user, string password)
        {
            if (string.IsNullOrEmpty(user))
            {
                throw new NullReferenceException("user");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new NullReferenceException("password");
            }

            this.User = user;
            this.Password = password;
        }
    }
}
