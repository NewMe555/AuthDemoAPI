namespace AuthDemoApi.Data.DTOs
{
    public class LoginDto
    {
        //DTO → clean data shape, we don’t expose DB model directly
        public string username { get; set; } = null!;
        public string password { get; set; }=null!;
    }
}
