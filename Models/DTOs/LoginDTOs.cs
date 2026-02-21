namespace Bank_Slip_Scanner_App.Models.DTOs

   {
        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public bool KeepSignedIn { get; set; }
        public bool Success { get; internal set; }
    }
        public class LoginResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public UserInfoDto User { get; set; }

        }
        public class UserInfoDto
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string NomComplet { get; set; }
            public string[] Roles { get; set; }
        }
     public class changePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    }


