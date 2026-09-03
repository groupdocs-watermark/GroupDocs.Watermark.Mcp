# Backlog & Known Issues

Running list of ideas, planned work, and known limitations for the
GroupDocs.Watermark MCP server. Grouped by topic. Terse on purpose — each line is
a ticket, not an essay. `[ ]` = open, `[x]` = shipped (kept for context).

**Current surface (26.9.0):** `add_watermark`, `add_image_watermark`, `search_watermarks`,
`remove_watermarks`, `get_document_info`.

---

## Confirmed defects — external audit, 2026-08-16

Source: black-box test round against `ghcr.io/groupdocs-watermark/watermark-net-mcp:latest`
(26.7.2, licensed), 46 family-wide defects reported and all 46 independently reproduced with
control calls. A later validation round found **zero false positives**.

`S#` = shared core (`GroupDocs.Mcp.Core`) · `M#` = this repo · `P#` = GroupDocs.Watermark library

**Verdict: the core loop is excellent** — add → search finds 3 with coordinates and rotation →
filtered remove → re-search 0. But search is unreliable in both directions: it invents watermarks
that are not there, and cannot see the ones this server itself writes.

### Product library — upstream

- [ ] **P2** Image watermarks are write-only — **Med**.
      *Proof:* `search_watermarks` cannot see the image its own `add_image_watermark` just
      embedded — verified structurally, a `/Subtype /Image` XObject **is** present in the output.
      *Impact:* image watermarks cannot be verified or removed through this server.
      *Fix:* include image watermarks in the search scope.
      **P1 — today `add_image_watermark` produces something nothing can verify or undo.**
- [ ] **P1** Every hyperlink is reported as a watermark — **Med**.
      *Proof:* `search_watermarks` reports 20 hyperlinks as watermarks on a clean document.
      *Impact:* asked *"does this document have watermarks?"*, an agent confidently answers yes and
      lists 20. The library's "possible watermark" hyperlink search may well be intentional; the
      problem is the results carry **no qualifier**.
      *Fix:* mark such hits as *possible/heuristic*, or expose a flag to exclude them, so a caller
      can tell candidates from confirmed watermarks.
      **P1 — the one most likely to reach users as "it says my clean document is watermarked".**
- [ ] **P3** Search results carry `page: null` and duplicate zero-size entries — **Low**.
      Pads the result noise. **P2**

### MCP wrapper — this repo

- [ ] **M1** Both "add" tools share one output name, so you cannot tell the results apart —
      **Info**.
      *Fix:* distinct default names (`_watermarked` / `_image_watermarked`), and document them.
      **P2**

### Shared core — fixed once in `GroupDocs.Mcp.Core`, lands here on the next bump

- [ ] **S1** Passing `fileName` crashes any tool — **High**.
- [ ] **S2** Missing files return an opaque error — **High**; listing capped at 20 entries.
- [ ] **S3** `isError` is set on crashes but not on real failures — **Med**.

Nothing to do in this repo for S1–S3 beyond re-testing after the Core bump.

---

## Known issues & limitations

- The text-watermark round trip is solid: add → search (with coordinates and rotation) → filtered
  remove → re-search returns 0.
- Output collisions dedup to `' (N)'` — the family convention, shared with Merger, Signature and
  Total.
- `search_watermarks` conflates confirmed watermarks with heuristic hyperlink candidates (P1
  above); until that is qualified, treat a positive result as "possible".

---

## Tools & functionality

- [ ] **P2** make `search_watermarks` see image watermarks. **P1**
- [ ] **P1** qualify or filter hyperlink hits. **P1**
- [ ] **M1** distinct default output names for the two add tools. **P2**
- [ ] **P3** drop `page: null` / zero-size duplicate entries from results. **P2**
- [ ] Expose an output `fileName` parameter. **P2**

## Testing & CI

- [ ] **Add-image-then-search test** — the suite has none, which is exactly why P2 shipped. **P1**
- [ ] Clean-document search test: assert a document with hyperlinks and no watermarks reports zero
      confirmed watermarks. **P1**
- [ ] Tighten `ErrorHandlingTests.cs:32-35` — the unknown-file oracle currently accepts
      `(response.IsError ?? false) || contains("not found") || contains("available")`, so **the
      test passes on the exact defect reported**. Assert the promised `Available files:` text.
      **P1**
- [ ] Add the two mandatory probes: the **`fileName`-only form**, and a **missing file**. **P1**
- [ ] Add a `channel: [dnx, docker]` axis — the current matrix is dnx-only. **P1**
- [ ] Per-tool Linux smoke test in image CI. **P1**
- [ ] macOS integration leg hangs (family-wide) — `timeout-minutes: 20` is committed locally but
      unpushed here. Push it, and stream the `dnx` child's stderr to an uploaded file. **P1**

## Documentation & discoverability

- [ ] Document what `search_watermarks` does and does not detect (P1/P2 above). **P1**
- [ ] Document the two output names once M1 lands. **P2**
- [ ] Licensing section covering the metered option once it ships. **P1**

## Platform & infra (longer-term)

- [ ] Metered licensing (`GROUPDOCS_METERED_PUBLIC_KEY` / `_PRIVATE_KEY`) via
      `GroupDocs.Mcp.Core`, plus the `get_license_status` tool. **P1**
- [ ] HTTP/SSE transport for shared/team deploys (stdio stays default). **P2**
- [ ] Remote storage (URL / S3) via `GroupDocs.Mcp.Core`. **P2**

---

*Evidence: `TEMP_ThirdPartyAnalysis/watermark.md` (per-product findings),
`ALL-PRODUCTS-REPORT.md` (10-product sweep), `VALIDATION-REPORT.md` (why the green suites miss
these). Conventions: any behaviour change ships with a `changelog/NNN-*.md` entry and a CalVer
bump. Integration tests target the published NuGet via `dnx`, so new-tool tests only pass once the
matching version is live.*
