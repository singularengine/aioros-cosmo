# Coverage document

[../coverage.md](../coverage.md) lists every shadcn/ui component, its parts and its fidelity matrix; the architecture
tests (`tests/Aioros.Cosmo.ArchitectureTests/CoverageTests.cs`) keep it honest.

| AC | Criterion |
| --- | --- |
| COVERAGE-01 | Given docs/coverage.md, when its tables are read, then every row has a known status (`done`, `partial`, `missing`, `n/a`) and every `done` row names at least one criterion. |
| COVERAGE-02 | Given docs/coverage.md, when the criteria its rows and fidelity matrix name are listed, then each exists in docs/acceptance. |
| COVERAGE-03 | Given docs/coverage.md's fidelity matrix, when its cells are read, then each holds only criteria or `n/a` with a reason, and once the document says `Finished: true` none is empty. |
| COVERAGE-04 | Given docs/coverage.md, when it says `Finished: true`, then no row is `partial` or `missing`; while it says `Finished: false`, the open rows are reported. |
| COVERAGE-05 | Given docs/coverage.md, when its `Counts:` line is read, then it states the number of rows of each status and the filled cells of the fidelity matrix exactly. |
| COVERAGE-06 | Given docs/coverage.md's components table, when it is compared with shadcn/ui's documented components in the pinned registry (`tools/shadcn/registry/components.txt`), then every documented component has exactly one row and no row names a component shadcn/ui does not document. |
