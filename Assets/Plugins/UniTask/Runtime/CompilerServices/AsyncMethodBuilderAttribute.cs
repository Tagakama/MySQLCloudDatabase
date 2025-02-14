﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS0436

namespace System.Runtime.CompilerServices
{
	sealed class AsyncMethodBuilderAttribute : Attribute
	{
		public Type BuilderType { get; }

		public AsyncMethodBuilderAttribute(Type builderType)
		{
			BuilderType = builderType;
		}
	}
}