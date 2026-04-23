#!/usr/bin/env perl
# Strip `modelBuilder.Entity("...XxxAuditLog", b =>` blocks from EF snapshot/designer files.
# Preserves: AuditLog (consolidated), SecurityAuditLog.
# Each block is delimited by `modelBuilder.Entity(...` at the top and `});` at matching brace depth.

use strict;
use warnings;

sub should_keep {
    my $fqtn = shift;
    return 1 if $fqtn =~ /\.AuditLog$/;            # Shared.Audit.AuditLog
    return 1 if $fqtn =~ /\.SecurityAuditLog$/;    # Identity.Domain.SecurityAuditLog
    return 0;
}

for my $file (@ARGV) {
    open my $in, '<', $file or die "$file: $!";
    my @lines = <$in>;
    close $in;
    my $total = scalar @lines;
    my @out;
    my $removed = 0;
    my $i = 0;

    while ($i < $total) {
        my $line = $lines[$i];
        # Match `modelBuilder.Entity("<fqtn>", b =>`
        if ($line =~ /^\s*modelBuilder\.Entity\("([^"]+)"\s*,\s*b\s*=>\s*$/) {
            my $fqtn = $1;
            if ($fqtn =~ /AuditLog$/ && !should_keep($fqtn)) {
                # Consume until the `});` at depth 0.
                my $depth = 0;
                my $j = $i + 1;
                while ($j < $total) {
                    my $l = $lines[$j];
                    for my $c (split //, $l) {
                        if ($c eq '{') { $depth++ }
                        elsif ($c eq '}') { $depth-- }
                    }
                    $j++;
                    last if $depth == 0;
                }
                # Skip trailing blank line separator.
                if ($j < $total && $lines[$j] =~ /^\s*$/) { $j++ }
                $removed++;
                $i = $j;
                next;
            }
        }
        push @out, $line;
        $i++;
    }

    open my $out, '>', $file or die "$file: $!";
    print $out @out;
    close $out;
    print "REMOVED $removed entity blocks from $file\n";
}
