using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
    public class RegisterUserCommand : ICommandBase
    {
        /// <summary>
        /// نام کاربری
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// کلمه عبور
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// شماره موبایل
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// نیاز به تایید دو عاملی دارد؟
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        
        public void Validate()
        {

        }
    }
}