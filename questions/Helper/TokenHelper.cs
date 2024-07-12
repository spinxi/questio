namespace questions.Helper
{
    public static class TokenHelper
    {
        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        public static bool ValidateToken(string token, string storedToken, DateTime expiry)
        {
            return token == storedToken && DateTime.Now <= expiry;
        }
    }
}
