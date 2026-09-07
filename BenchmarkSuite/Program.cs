// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using BenchmarkDotNet.Running;

namespace BenchmarkSuite;

  internal sealed class Program
  {
    static void Main()
    {
      var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
  }
