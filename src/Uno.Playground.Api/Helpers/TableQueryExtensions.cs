using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Uno.UI.Demo.Api.Helpers
{
	//public static class TableQueryExtensions
	//{
	//	public static TableQuery<T> SelectColumn<T>(
	//		this TableQuery<T> tableQuery,
	//		string column)
	//		where T : ITableEntity, new()
	//	{
	//		if (tableQuery.SelectColumns == null)
	//		{
	//			tableQuery.SelectColumns = new List<string>();
	//		}

	//		tableQuery.SelectColumns.Add(column);

	//		return tableQuery;
	//	}

	//	public static Task<TableQuerySegment<T>> ExecuteQuery<T>(
	//		this CloudTable cloudTable,
	//		TableQuery<T> query,
	//		CancellationToken ct)
	//		where T : ITableEntity, new()
	//	{
	//		return cloudTable.ExecuteQuerySegmentedAsync(
	//			query,
	//			token: null,
	//			requestOptions: null,
	//			operationContext: null,
	//			cancellationToken: ct);
	//	}
	//}
}
