using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class UpdateUserCommand : ICommandBase
	{
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
		/// نیاز به تایید دوعاملی است؟
		/// </summary>
		public bool TwoFactorEnabled { get; set; }

		/// <summary>
		/// آیا فعال هست؟
		/// </summary>
        public bool IsActive { get; set; }

		
		public void Validate() => new UpdateUserCommandValidator().Validate(this).RaiseExceptionIfRequired();
	}

	public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
	{
		public UpdateUserCommandValidator()
		{
			RuleFor(p => p.FirstName).NotEmpty();
			RuleFor(p => p.LastName).NotEmpty();
			RuleFor(p => p.PhoneNumber).NotEmpty();
		}
	}
}