﻿// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HQ.Cadence.Reporters.Console")]
[assembly: InternalsVisibleTo("HQ.Cadence.AspNetCore")]
[assembly: InternalsVisibleTo("HQ.Cadence.Tests")]

namespace HQ.Cadence
{
	internal class InternalsVisibleTo { }
}