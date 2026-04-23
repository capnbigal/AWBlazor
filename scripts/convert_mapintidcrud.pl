#!/usr/bin/env perl
# Convert MapIntIdCrud<8-generics>(17 named args)  →  MapCrudWithInterceptor<5-generics>(8 named args).

use strict;
use warnings;

sub convert {
    my ($text) = @_;
    my $n = 0;
    my $out = '';
    my $i = 0;
    my $len = length($text);

    while ($i < $len) {
        my $start = index($text, 'group.MapIntIdCrud<', $i);
        if ($start < 0) {
            $out .= substr($text, $i);
            last;
        }
        $out .= substr($text, $i, $start - $i);
        my $after_prefix = $start + length('group.MapIntIdCrud<');

        # Scan for matching '>' — balance angle brackets.
        my $depth = 1;
        my $p = $after_prefix;
        while ($p < $len && $depth > 0) {
            my $c = substr($text, $p, 1);
            if ($c eq '<') { $depth++ }
            elsif ($c eq '>') { $depth-- }
            $p++;
        }
        my $generics = substr($text, $after_prefix, $p - $after_prefix - 1);

        # Skip whitespace, expect '('.
        while ($p < $len && substr($text, $p, 1) =~ /\s/) { $p++ }
        unless (substr($text, $p, 1) eq '(') {
            $out .= "group.MapIntIdCrud<" . $generics . ">";
            $i = $p;
            next;
        }
        $p++; # past '('
        my $body_start = $p;

        # Scan for matching ')' — balance parens.
        my $pdepth = 1;
        while ($p < $len && $pdepth > 0) {
            my $c = substr($text, $p, 1);
            if ($c eq '(') { $pdepth++ }
            elsif ($c eq ')') { $pdepth-- }
            $p++;
        }
        my $body = substr($text, $body_start, $p - $body_start - 1);

        # Consume optional trailing ';'.
        my $trailing = '';
        if ($p < $len && substr($text, $p, 1) eq ';') {
            $trailing = ';';
            $p++;
        }

        # Split generics on top-level commas.
        my @g = split_top_level($generics, ',');
        if (@g != 8) {
            $out .= "group.MapIntIdCrud<$generics>($body)$trailing";
            $i = $p;
            next;
        }
        my ($tentity, $tdto, $tcreate, $tupdate, undef, undef, undef, $tid) = map { trim($_) } @g;

        # Parse named args from the body.
        my %args;
        for my $part (split_top_level($body, ',')) {
            $part = trim($part);
            next unless $part =~ /^(\w+)\s*:\s*(.*)$/s;
            $args{$1} = trim($2);
        }
        my @missing = grep { !exists $args{$_} } qw(entityName routePrefix entitySet idSelector getId toDto toEntity applyUpdate);
        if (@missing) {
            $out .= "group.MapIntIdCrud<$generics>($body)$trailing";
            $i = $p;
            next;
        }

        $out .= "group.MapCrudWithInterceptor<$tentity, $tdto, $tcreate, $tupdate, $tid>(\n"
             .  "            entityName: $args{entityName},\n"
             .  "            routePrefix: $args{routePrefix},\n"
             .  "            entitySet: $args{entitySet},\n"
             .  "            idSelector: $args{idSelector},\n"
             .  "            getId: $args{getId},\n"
             .  "            toDto: $args{toDto},\n"
             .  "            toEntity: $args{toEntity},\n"
             .  "            applyUpdate: $args{applyUpdate})"
             .  $trailing;
        $n++;
        $i = $p;
    }
    return ($out, $n);
}

sub trim {
    my $s = shift;
    $s =~ s/^\s+|\s+$//g;
    return $s;
}

sub split_top_level {
    my ($s, $sep) = @_;
    my @out;
    my $buf = '';
    my ($a, $n, $b) = (0, 0, 0);
    my $len = length($s);
    for (my $i = 0; $i < $len; $i++) {
        my $c = substr($s, $i, 1);
        my $next = $i + 1 < $len ? substr($s, $i + 1, 1) : '';
        my $prev = $i > 0 ? substr($s, $i - 1, 1) : '';
        if    ($c eq '(') { $n++ }
        elsif ($c eq ')') { $n-- }
        elsif ($c eq '<') { $a++ }
        elsif ($c eq '>') {
            # Skip '>' when it's part of a lambda arrow '=>' — don't count it as a closer.
            if ($prev ne '=') { $a-- if $a > 0 }
        }
        elsif ($c eq '[') { $b++ }
        elsif ($c eq ']') { $b-- }
        elsif ($c eq $sep && $n == 0 && $a == 0 && $b == 0) {
            push @out, $buf;
            $buf = '';
            next;
        }
        $buf .= $c;
    }
    push @out, $buf if length $buf;
    return @out;
}

for my $file (@ARGV) {
    open my $in, '<', $file or die "open $file: $!";
    local $/ = undef;
    my $text = <$in>;
    close $in;

    my ($new_text, $n) = convert($text);
    if ($n == 0) {
        print "NO-MATCH: $file\n";
        next;
    }
    open my $out, '>', $file or die "write $file: $!";
    print $out $new_text;
    close $out;
    print "OK ($n): $file\n";
}
