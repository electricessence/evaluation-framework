﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Open.Evaluation.Tests
{
	public static class Exponent
	{
		[TestClass]
		public class Default : ParseTestBase
		{
			const string FORMAT = "(({0} + {1})^({2} + {3}))";
			public Default() : base(FORMAT) { }

			protected override double Expected
			{
				get
				{
					var x1 = PV[0] + PV[1];
					var x2 = PV[2] + PV[3];
					return Math.Pow(x1, x2);
				}
			}

		}
	}
}
