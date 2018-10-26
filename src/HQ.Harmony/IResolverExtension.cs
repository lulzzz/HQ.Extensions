﻿// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Harmony
{
	public interface IResolverExtension
	{
		bool CanResolve(Lifetime lifetime);
		Func<T> Memoize<T>(IDependencyResolver host, Func<T> f);
		Func<IDependencyResolver, T> Memoize<T>(IDependencyResolver host, Func<IDependencyResolver, T> f);
	}
}