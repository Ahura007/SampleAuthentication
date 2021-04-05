using System.Linq;
using System.Text.RegularExpressions;

namespace SampleAuthentication.SeedWorks
{
	/// <summary>
	/// The person's code helpers.
	/// </summary>
	public static class PersonsCodeHelpers
	{
		/// <summary>
		/// The invalid national codes.
		/// </summary>
		public static readonly string[] InvalidNationalCodes =
			{
                // "1111111111", // این کد جدیداً مشخص شده است که قابل قبول می باشد
                "0000000000", "2222222222", "3333333333", "4444444444", "5555555555", "6666666666", "7777777777",
				"8888888888", "9999999999"
			};

		/// <summary>
		/// The national code validator.
		/// </summary>
		/// <param name="nationalCode">
		/// The national code.
		/// </param>
		/// <param name="result">
		/// The result.
		/// </param>
		/// <param name="isForigenPersonAccepted">
		/// The is forigen person accepted.
		/// </param>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		public static bool NationalCodeValidation(string nationalCode, out string result, bool isForigenPersonAccepted = true)
		{
			// در صورتی که کد ملی وارد شده تهی باشد
			if (string.IsNullOrEmpty(nationalCode))
			{
				result = "Null or Empty not allowed.";
				return false;
			}

			// در صورتی که کد ملی ده رقم عددی نباشد
			var regex = new Regex(@"[0-9]{10}");
			if (!regex.IsMatch(nationalCode))
			{
				result = "Should be 10 numbers only.";
				return false;
			}

			// -> اتباع بیگانه
			if (nationalCode.StartsWith("9") && !isForigenPersonAccepted)
			{
				result = "NationalCode of forigen persons not allowed.";
				return false;
			}

			if (InvalidNationalCodes.Contains(nationalCode))
			{
				result = "Should not be a repeated digit.";
				return false;
			}

			// عملیات اعتبار سنجی با الگوریتم
			var check = nationalCode[9].ToInt32();
			var sum = nationalCode.Select((c, i) => c.ToInt32() * (10 - i)).Sum();
			var net = (sum - check) % 11;
			var checkNet = net < 2 ? net : 11 - net;

			if (checkNet == check)
			{
				result = "NationalCode is valid.";
				return true;
			}

			result = $"NationalCode - {nationalCode} - is not valid.";
			return false;
		}

		/// <summary>
		/// The email validation.
		/// </summary>
		/// <param name="emailAddress">
		/// The email address.
		/// </param>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		public static bool EmailValidation(string emailAddress)
		{
			var cellPhoneNumberRegex = new Regex(@"^[^@]+@[^@]+\.[^@\.]+$");
			return cellPhoneNumberRegex.IsMatch(emailAddress);
		}
	}
}