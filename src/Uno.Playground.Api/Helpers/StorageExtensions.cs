using Azure.Data.Tables;

namespace Uno.Playground.Api.Helpers;

public static class StorageExtensions
{
	public static TableClient GetSamplesTableClient(this TableServiceClient service)
	{
		return service.GetTableClient(Constants.SamplesTableName);
	}
}