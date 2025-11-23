# DotnetJedi – Place to Discover Better Implementation for Your Algorithm

General Description
DotnetJedi is a structured playground for iterative optimization. It helps you:
- Prototype multiple algorithm variants side by side.
- Validate correctness early.
- Perform statistically sound performance benchmarking across .NET runtime versions.
- Track historical results from different machines to guide improvement.

Flow to Measure an Algorithm
1. Implement Variants
   - Add alternative implementations in a utils/library project with uniform signatures.
2. Correctness & Smoke Tests
   - Use a test project (e.g. NUnit) to assert functional equivalence and obtain a rough performance sanity check.
3. Accurate Benchmarking
   - Use a BenchmarkDotNet project with multi-runtime jobs (e.g. .NET 6/8/9/10) for comparative metrics (mean, throughput, ratios).
4. Result Collection & Iteration
   - Store Markdown/HTML/CSV outputs under a results folder; compare across commits and machines; refine code and repeat.

Implemented Algorithms

| # | Name                              | Description (High-Level)                               | Domain / Usage Examples                          |Benchmark results|
|--:|:----------------------------------|:-------------------------------------------------------|:-------------------------------------------------|:----------------|
| 1 | [RGB24 90° Rotation (3 BPP)](RotateImageUtils/RotationUtils.cs)        | Rotates packed 3-byte-per-pixel buffer 90° clockwise.  | Imaging, pixel buffer transforms, preprocessing  |[RotationBenchmark/Results/](RotationBenchmark/Results)|

(Add more rows as you introduce new algorithms.)

How to Extend
- Add new algorithm file/method.
- Add correctness tests referencing all variants.
- Add a `[Benchmark]` method for each variant.
- Run benchmarp app and measure the performance.
- Commit result artifacts for traceability.

Guiding Principles
- Evidence over assumption: every optimization backed by benchmarks.
- Consistent API surface to simplify enumeration.
- Separation of correctness and performance concerns.
- Multi-runtime insight to catch regressions or JIT improvements.

Result Artifacts
- Located in `YourBenchmark/Results/`
- Naming suggestion: `YYYY.MM.DD_<HostCPU>_<BenchmarkClass>-report-github.md`


Optimize, measure, validate, repeat.