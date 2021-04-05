using System.Collections.Generic;

namespace SampleAuthentication.SeedWorks
{
	public class Paging<T>
	{
		public IEnumerable<T> Content { get; set; }
		public int TotalRecord { get; set; }
		public object AdditionalData { get; set; }
		public object PaginationInfo { get; set; }
	}
}
