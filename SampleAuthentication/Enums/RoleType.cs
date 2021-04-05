using System.ComponentModel;

namespace SampleAuthentication.Enums
{
	public enum RoleType
	{
		[Description("مدیر ارشد")]
		SuperAdmin = 1,

		[Description("مشتری")]
		Customer = 2,

		[Description("کاربر صندوق")]
		FundUser,

        [Description("کارگزار")]
        Broker
	}
}