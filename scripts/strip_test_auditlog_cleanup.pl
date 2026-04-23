#!/usr/bin/env perl
# Remove test-cleanup code that references legacy per-entity audit DbSets
# (e.g. `cleanup.XAuditLogs.RemoveRange(...)`, `var audits = cleanup.XAuditLogs.Where(...)`).
# Preserves cleanup code targeting the consolidated `AuditLogs` DbSet (exact match
# on `.AuditLogs` without a preceding type prefix).

use strict;
use warnings;

for my $file (@ARGV) {
    open my $in, '<', $file or die "$file: $!";
    my @lines = <$in>;
    close $in;

    my @out;
    my $removed = 0;
    for my $line (@lines) {
        # Match references to legacy `.<Name>AuditLogs` where <Name> is non-empty.
        # Exclude `.AuditLogs` (standalone — the consolidated DbSet).
        if ($line =~ /\.\w+AuditLogs\b/ && $line !~ /^\s*(?:await\s+)?[\w.]+\.AuditLogs\b/) {
            # Only strip if this is clearly cleanup — i.e. the line participates in RemoveRange/Where
            # patterns we produced. Pattern match on common call shapes.
            if ($line =~ /\.\w+AuditLogs\s*\.\s*(Where|RemoveRange|AsNoTracking|Where|CountAsync)\b/
                || $line =~ /var\s+\w+\s*=\s*[\w.]+\.\w+AuditLogs\s*\./
                || $line =~ /\.\w+AuditLogs\s*\.\s*RemoveRange\(/
            ) {
                $removed++;
                next;
            }
        }
        push @out, $line;
    }
    if ($removed) {
        open my $out, '>', $file or die "$file: $!";
        print $out @out;
        close $out;
        print "STRIPPED $removed line(s): $file\n";
    }
}
