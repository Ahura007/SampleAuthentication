
namespace SampleAuthentication.SeedWorks
{
	public class PageQuery
	{
		public PageQuery()
		{
			Page = 1;
			PageSize = 10;
		}

		public string Filters { get; set; }

		public string Sorts { get; set; }

		public int Page { get; set; }

		public int PageSize { get; set; }
	}
}
