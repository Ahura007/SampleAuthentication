using System.Collections.Generic;

namespace SampleAuthentication.Dtos
{
	public class UserDto
	{
		/// <summary>
		/// شناسه
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// نام کاربری
		/// </summary>
		public string UserName { get; set; }

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
		/// آیا فعال است ؟
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		/// نقش های کاربر
		/// </summary>
		public IList<string> Roles { get; set; }
		/// <summary>
		/// نیاز به تایید دو عاملی است؟
		/// </summary>
        public bool TwoFactorEnabled { get; set; }
    }
}