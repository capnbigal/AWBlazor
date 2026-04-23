#!/usr/bin/env perl
# Remove using / \@using references to deleted Audit/Application.Services namespaces.
use strict;
use warnings;

my $N = 0;
for my $file (@ARGV) {
    open my $in, '<', $file or die "$file: $!";
    my @lines = <$in>;
    close $in;
    my @out;
    my $changed = 0;
    for my $line (@lines) {
        # C# semi-colon style: optional chained 'using ...; using ...;' segments on one line.
        # Drop each dead segment while keeping alive ones.
        if ($line =~ /\busing\s+AWBlazorApp\.Features\./) {
            my $new = $line;
            $new =~ s{using\s+AWBlazorApp\.Features\.(Quality|Performance)\.Audit;\s*}{}g;
            $new =~ s{using\s+AWBlazorApp\.Features\.(Purchasing|Sales|HumanResources|Person|Production)\.\w+\.Application\.Services;\s*}{}g;
            if ($new =~ /^\s*$/) { $changed++; next; }
            if ($new ne $line) { $changed++; push @out, $new; next; }
        }
        # Razor \@using style.
        if ($line =~ /^\@using\s+AWBlazorApp\.Features\.(Quality|Performance)\.Audit\s*$/) { $changed++; next; }
        if ($line =~ /^\@using\s+AWBlazorApp\.Features\.(Purchasing|Sales|HumanResources|Person|Production)\.\w+\.Application\.Services\s*$/) { $changed++; next; }
        push @out, $line;
    }
    if ($changed) {
        open my $out, '>', $file or die "$file: $!";
        print $out @out;
        close $out;
        print "STRIPPED $changed line(s): $file\n";
        $N++;
    }
}
print "Total files changed: $N\n";
