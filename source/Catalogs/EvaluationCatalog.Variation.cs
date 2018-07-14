﻿using Open.Evaluation.Core;
using Open.Hierarchy;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace Open.Evaluation.Catalogs
{
	using EvalDoubleVariationCatalog = EvaluationCatalog<double>.VariationCatalog;

	public partial class EvaluationCatalog<T>
		where T : IComparable
	{
		private VariationCatalog _variation;
		public VariationCatalog Variation =>
			LazyInitializer.EnsureInitialized(ref _variation, () => new VariationCatalog(this));

		public class VariationCatalog : SubmoduleBase<EvaluationCatalog<T>>
		{
			internal VariationCatalog(EvaluationCatalog<T> source) : base(source)
			{

			}
		}
	}


	public static partial class EvaluationCatalogExtensions
	{

		public static bool IsValidForRemoval<TEval>(this Node<TEval> gene, bool ifRoot = false)
			where TEval : IEvaluate
		{
			if (gene == gene.Root) return ifRoot;
			// Validate worthyness.
			var parent = gene.Parent;
			Debug.Assert(parent != null);

			// Search for potential futility...
			// Basically, if there is no dynamic nodes left after reduction then it's not worth removing.
			return !parent.Any(g => g != gene && !(g.Value is IConstant))
				   && parent.IsValidForRemoval(true);
		}

		/// <summary>
		/// Removes a node from its parent.
		/// </summary>
		/// <param name="catalog">The catalog to use.</param>
		/// <param name="node">The node to remove from the tree.</param>
		/// <param name="newRoot">The resultant root node corrected by .FixHierarchy()</param>
		/// <returns>true if sucessful</returns>
		public static bool TryRemoveValid(
			this EvalDoubleVariationCatalog catalog,
			Node<IEvaluate<double>> node,
			out IEvaluate<double> newRoot)
		{
			Debug.Assert(catalog != null);
			if (node == null) throw new ArgumentNullException(nameof(node));
			if (IsValidForRemoval(node))
			{
				newRoot = catalog.Factory.Recycle(catalog.Catalog.RemoveNode(node)); ;
				return true;
			}
			newRoot = default;
			return false;
		}


		static bool CheckPromoteChildrenValidity(
			IParent parent)
			// Validate worthyness.
			=> parent?.Children.Count == 1;

		public static IEvaluate<double> PromoteChildren(
			this EvalDoubleVariationCatalog catalog,
			Node<IEvaluate<double>> node)
		{
			Debug.Assert(catalog != null);
			if (node == null) throw new ArgumentNullException(nameof(node));
			Contract.EndContractBlock();

			// Validate worthyness.
			if (!CheckPromoteChildrenValidity(node)) return null;

			return catalog.Catalog.ApplyClone(node,
				newNode => newNode.Value = newNode.Children.Single().Value);
		}

		// This should handle the case of demoting a function.
		public static IEvaluate<double> PromoteChildrenAt(
			this EvalDoubleVariationCatalog catalog,
			Node<IEvaluate<double>> root, int descendantIndex)
		{
			Debug.Assert(catalog != null);
			if (root == null) throw new ArgumentNullException(nameof(root));
			Contract.EndContractBlock();

			return PromoteChildren(catalog,
				root.GetDescendantsOfType()
					.ElementAt(descendantIndex));
		}

		public static IEvaluate<double> ApplyFunction(
			this EvalDoubleVariationCatalog catalog,
			Node<IEvaluate<double>> node, char fn)
		{
			Debug.Assert(catalog != null);
			if (node == null) throw new ArgumentNullException(nameof(node));
			Contract.EndContractBlock();

			if (!Registry.Arithmetic.Functions.Contains(fn))
				throw new ArgumentException("Invalid function operator.", nameof(fn));

			return catalog.Catalog.ApplyClone(node, newNode =>
				newNode.Value = catalog.Catalog.GetFunction(fn, newNode.Value));
		}

		public static IEvaluate<double> ApplyFunctionAt(
			this EvalDoubleVariationCatalog catalog,
			Node<IEvaluate<double>> root, int descendantIndex, char fn)
		{
			Debug.Assert(catalog != null);
			if (root == null) throw new ArgumentNullException(nameof(root));
			Contract.EndContractBlock();

			return ApplyFunction(catalog,
				root.GetDescendantsOfType()
					.ElementAt(descendantIndex).Parent, fn);
		}


	}
}