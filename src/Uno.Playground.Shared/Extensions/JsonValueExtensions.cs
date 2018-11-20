using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Json;
using System.Text;

namespace Uno.Playground.Extensions
{
	public static class JsonValueExtensions
	{
		public static dynamic BuildExpando(this JsonObject rootObject)
		{
			var result = new ExpandoObject() as IDictionary<string, object>;

			foreach (var node in rootObject)
			{
				result[node.Key] = BuildExpandoValue(node.Value);
			}

			return result;
		}

		public static dynamic BuildExpandoValue(this JsonValue value)
		{
			if (value is JsonObject child)
			{
				return BuildExpando(child);
			}
			else if (value is JsonArray array)
			{
				var resultArray = new object[array.Count];

				for (int index = 0; index < array.Count; index++)
				{
					resultArray[index] = BuildExpandoValue(array[index]);
				}

				return resultArray;
			}
			else
			{
				switch (value.JsonType)
				{
					case JsonType.Number:
						return (int)value;

					case JsonType.String:
						return (string)value;

					default:
						throw new NotSupportedException();
				}
			}
		}

	}
}
