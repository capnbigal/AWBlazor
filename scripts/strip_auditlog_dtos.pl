#!/usr/bin/env perl
# Strip *AuditLogDto record declarations and ToDto(XxxAuditLog) extension methods
# from Dtos/*.cs files. Both declarations are either single-statement or span multiple
# lines ending with `);`.

use strict;
use warnings;

for my $file (@ARGV) {
    open my $in, '<', $file or die "$file: $!";
    local $/ = undef;
    my $text = <$in>;
    close $in;
    my $orig = $text;

    # Remove `public sealed record XxxAuditLogDto(` through the matching `);`.
    $text =~ s{public\s+sealed\s+record\s+\w*AuditLogDto\s*\([^;]*?\)\s*;\s*\n?}{}gs;

    # Remove `public static XxxAuditLogDto ToDto(this XxxAuditLog a) => new(` through `);`.
    $text =~ s{public\s+static\s+\w*AuditLogDto\s+ToDto\s*\(\s*this\s+\w+AuditLog\s+\w+\s*\)\s*=>\s*new\s*\([^;]*?\)\s*;\s*\n?}{}gs;

    # Also the alternate `=> new ... { ... };` form (less common but possible).
    # Same DOM: we rely on `);` as the terminator. Skip.

    if ($text eq $orig) {
        print "NO-CHANGE: $file\n";
        next;
    }
    open my $out, '>', $file or die "$file: $!";
    print $out $text;
    close $out;
    print "OK: $file\n";
}
