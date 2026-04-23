#!/usr/bin/env perl
# Strip inline audit-service/log calls and rewrite HistoryAsync to the consolidated AuditLog.
# Handles per-file patterns uniformly.

use strict;
use warnings;

for my $file (@ARGV) {
    open my $in, '<', $file or die "open $file: $!";
    local $/ = undef;
    my $text = <$in>;
    close $in;
    my $orig = $text;
    my $changes = 0;

    # 1. Rewrite HistoryAsync return-type + body.
    #    Look for the signature `Task<Ok<PagedResult<XxxAuditLogDto>>>` and matching body; replace
    #    PagedResult<XAuditLogDto> with PagedResult<AuditLog>, query `db.XAuditLogs` → filtered AuditLogs.
    #    We handle each occurrence.
    while ($text =~ /PagedResult<(\w+)AuditLogDto>/g) {
        my $base = $1;
        my $p = pos($text);
        # Replace all three forms in one go for this base:
        $changes += ($text =~ s/PagedResult<\Q$base\EAuditLogDto>/PagedResult<AuditLog>/g);
        $changes += ($text =~ s/db\.\Q$base\EAuditLogs\.AsNoTracking\(\)\.Where\(a => a\.\w+Id == id\)/db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "$base" && a.EntityId == idStr)/);
        # Add idStr inside the method body: find `take = Math.Clamp(take, 1,` line and inject idStr.
        $changes += ($text =~ s/(take = Math\.Clamp\(take, 1, \d+\);)\s*\n(\s+)var q = db\.AuditLogs\.AsNoTracking\(\)\.Where/$1\n$2var idStr = id.ToString();\n$2var q = db.AuditLogs.AsNoTracking().Where/);
        # Drop the .Select(a => a.ToDto()) inside the rows query for the consolidated AuditLog.
        $changes += ($text =~ s/\.Select\(a => a\.ToDto\(\)\)\.ToListAsync\(ct\)/.ToListAsync(ct)/);
        pos($text) = $p;
    }

    # 2. Delete lines that are purely legacy audit bookkeeping.
    my @lines = split /\n/, $text, -1;
    my @out;
    for my $line (@lines) {
        if ($line =~ /\b\w+AuditService\.(CaptureSnapshot|RecordCreate|RecordUpdate|RecordDelete)\s*\(/) {
            $changes++;
            next;
        }
        if ($line =~ /\bdb\.\w+AuditLogs\.Add\s*\(/) {
            $changes++;
            next;
        }
        if ($line =~ /^\s*using\s+AWBlazorApp\.Features\.(Quality|Performance|Purchasing|Sales)\.Audit;\s*$/) {
            $changes++;
            next;
        }
        push @out, $line;
    }
    $text = join "\n", @out;

    # 3. AddWithAuditAsync / DeleteWithAuditAsync call sites are rewritten manually per file
    #    because the DbSet name isn't inferable from context.

    # 4. Ensure `using AWBlazorApp.Shared.Audit;` is present if we added a reference to the AuditLog type.
    if ($text =~ /PagedResult<AuditLog>/ && $text !~ /using\s+AWBlazorApp\.Shared\.Audit;/) {
        if ($text =~ /(using\s+AWBlazorApp\.Shared\.Dtos;)/) {
            $text =~ s{(using\s+AWBlazorApp\.Shared\.Dtos;)}{using AWBlazorApp.Shared.Audit;\n$1};
            $changes++;
        }
    }

    if ($text eq $orig) {
        print "NO-CHANGES: $file\n";
        next;
    }
    open my $out, '>', $file or die "write $file: $!";
    print $out $text;
    close $out;
    print "CHANGED ($changes edits): $file\n";
}
