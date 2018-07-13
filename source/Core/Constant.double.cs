﻿/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open.Evaluation/blob/master/LICENSE.txt
 */

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Open.Evaluation.Core
{
	public sealed class Constant : Constant<double>
	{
		Constant(double value) : base(value)
		{ }

		internal new static Constant Create(ICatalog<IEvaluate<double>> catalog, double value)
			=> catalog.Register(value.ToString(CultureInfo.InvariantCulture), k => new Constant(value));

		public override IEvaluate NewUsing(ICatalog<IEvaluate> catalog, double value)
			=> catalog.Register(value.ToString(CultureInfo.InvariantCulture), k => new Constant(value));
	}

	public static partial class ConstantExtensions
	{
		public static Constant GetConstant(
			this ICatalog<IEvaluate<double>> catalog,
			double value)
			=> Constant.Create(catalog, value);

		public static Constant SumOfConstants(
			this ICatalog<IEvaluate<double>> catalog,
			IEnumerable<IConstant<double>> constants)
			=> GetConstant(catalog, constants.Sum(s => s.Value));

		public static Constant SumOfConstants(
			this ICatalog<IEvaluate<double>> catalog,
			double c1, params IConstant<double>[] rest)
			=> GetConstant(catalog, c1 + rest.Sum(s => s.Value));

		public static Constant SumOfConstants(
			this ICatalog<IEvaluate<double>> catalog,
			IConstant<double> c1, params IConstant<double>[] rest)
			=> SumOfConstants(catalog, rest.Concat(c1));

		public static Constant ProductOfConstants(
			this ICatalog<IEvaluate<double>> catalog,
			IEnumerable<IConstant<double>> constants)
			=> ProductOfConstants(catalog, 1, constants);

		public static Constant ProductOfConstants(
			this ICatalog<IEvaluate<double>> catalog,
			IConstant<double> c1, params IConstant<double>[] rest)
			=> ProductOfConstants(catalog, c1.Value, rest);

		public static Constant ProductOfConstants(
			this ICatalog<IEvaluate<double>> catalog,
			double c1, IEnumerable<IConstant<double>> others)
		{
			var result = c1;
			foreach (var c in others)
			{
				result *= c.Value;
			}
			return GetConstant(catalog, result);
		}

		public static Constant ProductOfConstants(
			this ICatalog<IEvaluate<double>> catalog,
			double c1, params IConstant<double>[] rest)
			=> ProductOfConstants(catalog, c1, (IEnumerable<IConstant<double>>)rest);

	}

}
