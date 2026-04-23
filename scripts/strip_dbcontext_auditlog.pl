#!/usr/bin/env perl
# Remove legacy *AuditLog DbSets and their builder.Entity<> configs from ApplicationDbContext.
# Preserves:
#   - DbSet<AuditLog> (consolidated)
#   - DbSet<SecurityAuditLog>
#   - DbSet<ToolSlotAuditLog>
#   - AuditLog / SecurityAuditLog / ToolSlotAuditLog entity configs

use strict;
use warnings;

sub should_keep_type {
    my $t = shift;
    return 1 if $t eq 'AuditLog';
    return 1 if $t eq 'SecurityAuditLog';
    return 1 if $t eq 'ToolSlotAuditLog';
    return 0;
}

for my $file (@ARGV) {
    open my $in, '<', $file or die "$file: $!";
    my @lines = <$in>;
    close $in;

    my @out;
    my $i = 0;
    my $total = scalar @lines;
    my $removed = 0;

    while ($i < $total) {
        my $line = $lines[$i];

        # Match DbSet<XxxAuditLog> on a single line.
        if ($line =~ /public\s+DbSet<(\w+AuditLog)>\s+\w+\s*=>/) {
            my $type = $1;
            if (!should_keep_type($type)) {
                $removed++;
                $i++;
                next;
            }
        }

        # Match `builder.Entity<XxxAuditLog>(b =>` followed by `{` on the next line
        # and read until the matching `});`.
        if ($line =~ /^\s*builder\.Entity<(\w+AuditLog)>\s*\(\s*b\s*=>\s*$/) {
            my $type = $1;
            if (!should_keep_type($type)) {
                # Expect the next line to start a block with `{`.
                my $j = $i + 1;
                my $depth = 0;
                # Traverse until we find the `{` and then close the block.
                while ($j < $total) {
                    my $l = $lines[$j];
                    for my $c (split //, $l) {
                        if ($c eq '{') { $depth++ }
                        elsif ($c eq '}') { $depth-- }
                    }
                    $j++;
                    last if $depth == 0;
                }
                # Also drop any trailing empty line that separated this block from the next one.
                if ($j < $total && $lines[$j] =~ /^\s*$/) { $j++ }
                $removed++;
                $i = $j;
                next;
            }
        }

        # Match a single-line `builder.Entity<XxxAuditLog>(...)` call (schema-only form).
        if ($line =~ /^\s*builder\.Entity<(\w+AuditLog)>\s*\(.*?\);/) {
            my $type = $1;
            if (!should_keep_type($type)) {
                $removed++;
                $i++;
                next;
            }
        }

        push @out, $line;
        $i++;
    }

    open my $out, '>', $file or die "$file: $!";
    print $out @out;
    close $out;
    print "STRIPPED $removed entries from $file\n";
}
