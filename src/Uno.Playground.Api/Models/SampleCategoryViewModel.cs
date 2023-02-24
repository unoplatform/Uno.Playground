using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uno.UI.Demo.Api.Models
{
	public class SampleCategoryViewModel
	{
		private readonly SampleCategory _category;

		public SampleCategoryViewModel(SampleCategory category, IEnumerable<Sample> samples)
		{
			_category = category;
			Samples = samples
				.Where(s => s.Category.Equals(category.Id))
				.Select(s => new SampleViewModel(s)).ToArray();
		}

		public string CategoryId => _category.Id;

		public string Title => _category.Title;

		public string DefaultIconPath => _category.PathData;

		public string DefaultIconAccentPath => _category.AccentPathData;

		public SampleViewModel[] Samples { get; }

		[IgnoreDataMember]
		public int SamplesHash
		{
			get
			{
				var sum = 0;
				foreach (var s in Samples)
				{
					unchecked
					{
						sum += GetEtagHash(s.Etag.ToString());
					}
				}

				return sum;
			}
		}

		private static int GetEtagHash(string s)
		{
			unchecked
			{
				var r = 0;

				foreach (var c in s)
				{
					r += c;
				}

				return r;

			}
		}
	}
}
