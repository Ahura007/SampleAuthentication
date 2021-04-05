using System.Collections.Generic;
using System.Linq;

namespace SampleAuthentication.SeedWorks
{
	public static class PagingExtentions
	{
		public static IEnumerable<T> ToPagingAndSorting<T>(this IEnumerable<T> dataModels, PageQuery pageQuery)
		{
			return pageQuery.PageSize == 0
				? dataModels
				: dataModels
					.Skip((pageQuery.Page - 1) * pageQuery.PageSize)
					.Take(pageQuery.PageSize);
		}

		public static IQueryable<T> ToPagingAndSorting<T>(this IQueryable<T> dataModels, PageQuery pageQuery)
		{
			return pageQuery.PageSize == 0
				? dataModels
				: dataModels
					.Skip((pageQuery.Page - 1) * pageQuery.PageSize)
					.Take(pageQuery.PageSize);
		}

		public static Paging<T> ToPaging<T>(this IEnumerable<T> data, int totalRecord, PageQuery query)
		{
			return new Paging<T>
			{
				Content = data,
				TotalRecord = totalRecord,
				PaginationInfo = new
				{
					TotalRecord = totalRecord,
					FromRecord = GetFromRecord(totalRecord, query),
					ToRecord = GetToRecord(totalRecord, query)
				}
			};
		}

		public static Paging<T> ToPaging<T>(this IEnumerable<T> data, int totalRecord, PageQuery query, object additionalData)
		{
			return new Paging<T>
			{
				Content = data,
				TotalRecord = totalRecord,
				AdditionalData = additionalData,
				PaginationInfo = new
				{
					TotalRecord = totalRecord,
					FromRecord = GetFromRecord(totalRecord, query),
					ToRecord = GetToRecord(totalRecord, query)
				}
			};
		}

		#region PrivateMethods

		private static int GetFromRecord(int totalRecord, PageQuery query)
		{
			if (totalRecord == 0)
				return 0;

			if (query.Page == 1)
				return 1;

			// Example : TotalRecord : 100 , PageIndex = 5 , PageSize = 10
			// Result : 40
			return (query.Page - 1) * query.PageSize;
		}

		private static int GetToRecord(int totalRecord, PageQuery query)
		{
			if (totalRecord == 0)
				return 0;

			// Example : TotalRecord : 5 , PageSize : 10
			// Result : 5
			if (totalRecord < query.PageSize)
				return totalRecord;

			// Example : TotalRecord : 100 , PageIndex = 5 , PageSize = 10
			// Result : 50
			return query.Page * query.PageSize;
		}

		#endregion
	}
}