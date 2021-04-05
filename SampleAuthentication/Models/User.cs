using System;
using Microsoft.AspNetCore.Identity;

namespace SampleAuthentication.Models
{
    public class User : IdentityUser
    {
        
        public User(string userName, string name) : base(userName)
        {
            this.Name = name;
            this.RegistrationDateTime = DateTime.Now;
            this.TotalLoginCount = 0;
            this.IsActive = true;
        }

        
        public User(string userName, string name, string externalProvider) : this(userName, name)
        {
            this.ExternalProvider = externalProvider;
        }

        /// <summary>
		/// نام
		/// </summary>
		public string Name { get; set; }

        /// <summary>
        /// تاریخ ثبت نام
        /// </summary>
        public DateTime RegistrationDateTime { get; set; }

        /// <summary>
        /// تاریخ آخرین لاگین
        /// </summary>
        public DateTime? LastLoginDateTime { get; set; }

        /// <summary>
        /// تعداد ورود
        /// </summary>
        public int TotalLoginCount { get; set; }

        /// <summary>
        /// رمز عبور پیشین
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// آیا غیرفعال شده است ؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// ارائه دهنده احراز هویت خارجی مثل گوگل
        /// </summary>
        public string ExternalProvider { get; set; }

   

        /// <summary>
        /// غیر فعال کردن کاربر
        /// </summary>
        public void DeActive()
        {
            this.IsActive = false;
        }

        public void Active()
        {
            this.IsActive = true;
        }

        private User() { }
    }
}
