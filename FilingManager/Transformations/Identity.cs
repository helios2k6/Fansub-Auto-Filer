using FileNameParser;
using FilingManager.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.StringMetricsCalculators.Transformations
{
	public sealed class Identity : ITransformation
	{
		public FansubFile Transform(FansubFile file)
		{
			return file.DeepCopy();
		}
	}
}
