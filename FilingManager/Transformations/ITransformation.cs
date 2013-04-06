using FileNameParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilingManager.Transformations
{
	interface ITransformation
	{
		FansubFile Transform(FansubFile file);
	}
}
