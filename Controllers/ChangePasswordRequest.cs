namespace Bank_Slip_Scanner_App.Controllers
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; internal set; }
        public string NewPassword { get; internal set; }
    }
}