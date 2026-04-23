#!/usr/bin/env python3
"""
Convert MapIntIdCrud<8-generics>(17-params) calls into
MapCrudWithInterceptor<5-generics>(8-params) calls.

Run from repo root; takes file paths as args:
    python3 scripts/convert_mapintidcrud.py <file1> <file2> ...
"""
import re
import sys
from pathlib import Path


PATTERN = re.compile(
    r"""group\.MapIntIdCrud<\s*
    (?P<tentity>[^,]+),\s*
    (?P<tdto>[^,]+),\s*
    (?P<tcreate>[^,]+),\s*
    (?P<tupdate>[^,]+),\s*
    [^,]+,\s*          # TAuditLog
    [^,]+,\s*          # TAuditLogDto
    [^,]+,\s*          # TSnapshot
    (?P<tid>[^>]+)>\s*\(
    \s*entityName:\s*(?P<name>"[^"]+"),
    \s*routePrefix:\s*(?P<prefix>"[^"]+"),
    \s*entitySet:\s*(?P<eset>[^,]+?),
    \s*auditSet:\s*[^,]+?,
    \s*idSelector:\s*(?P<idsel>[^,]+?),
    \s*auditIdSelector:\s*[^,]+?,
    \s*auditChangedDateSelector:\s*[^,]+?,
    \s*auditPrimaryKeySelector:\s*[^,]+?,
    \s*getId:\s*(?P<getid>[^,]+?),
    \s*toDto:\s*(?P<todto>[^,]+?),
    \s*toEntity:\s*(?P<toent>[^,]+?),
    \s*applyUpdate:\s*(?P<apply>\([^)]*\)\s*=>\s*[^,]+?),
    \s*captureSnapshot:\s*[^,]+?,
    \s*recordCreate:\s*[^,]+?,
    \s*recordUpdate:\s*[^,]+?,
    \s*recordDelete:\s*[^,]+?,
    \s*auditToDto:\s*[^)]+?\);""",
    re.VERBOSE | re.DOTALL,
)


def convert(text: str) -> tuple[str, int]:
    def replace(m: re.Match[str]) -> str:
        return (
            f"group.MapCrudWithInterceptor<{m['tentity'].strip()}, {m['tdto'].strip()}, "
            f"{m['tcreate'].strip()}, {m['tupdate'].strip()}, {m['tid'].strip()}>(\n"
            f"            entityName: {m['name']},\n"
            f"            routePrefix: {m['prefix']},\n"
            f"            entitySet: {m['eset'].strip()},\n"
            f"            idSelector: {m['idsel'].strip()},\n"
            f"            getId: {m['getid'].strip()},\n"
            f"            toDto: {m['todto'].strip()},\n"
            f"            toEntity: {m['toent'].strip()},\n"
            f"            applyUpdate: {m['apply'].strip()});"
        )

    new_text, n = PATTERN.subn(replace, text)
    return new_text, n


if __name__ == "__main__":
    for arg in sys.argv[1:]:
        path = Path(arg)
        original = path.read_text(encoding="utf-8")
        new_text, n = convert(original)
        if n == 0:
            print(f"NO-MATCH: {path}")
            continue
        path.write_text(new_text, encoding="utf-8")
        print(f"OK ({n}): {path}")
