#!/usr/bin/env perl
# Rewrite bespoke HistoryAsync methods that returned List<XxxAuditLogDto> to
# return List<AuditLog> filtered by EntityType from the consolidated audit.AuditLog.
# Matches:
#   private static async Task<Ok<List<XAuditLogDto>>> HistoryAsync(
#       [many args]
#       CancellationToken ct = default)
#   {
#       [body up to matching `}`]
#   }

use strict;
use warnings;

for my $file (@ARGV) {
    open my $in, '<', $file or die "$file: $!";
    local $/ = undef;
    my $text = <$in>;
    close $in;
    my $orig = $text;

    # Find: `private static async Task<Ok<List<(Word)AuditLogDto>>>(\s*HistoryAsync\s*\(` and
    # the matching method body. We pattern-match the method signature, then scan forward
    # counting braces.
    while ($text =~ /private\s+static\s+async\s+Task<Ok<List<(\w+)AuditLogDto>>>\s+HistoryAsync\s*\(/gs) {
        my $entity_guess = $1;
        my $sig_start = $-[0];
        my $after_sig_open = pos($text);

        # Walk to the `)` that ends the parameter list.
        my $depth = 1;
        my $i = $after_sig_open;
        while ($i < length($text) && $depth > 0) {
            my $c = substr($text, $i, 1);
            if ($c eq '(') { $depth++ }
            elsif ($c eq ')') { $depth-- }
            $i++;
        }
        # Skip whitespace to the opening `{`.
        while ($i < length($text) && substr($text, $i, 1) =~ /\s/) { $i++ }
        next unless substr($text, $i, 1) eq '{';
        my $body_start = $i;
        # Walk to matching `}`.
        $depth = 1;
        my $j = $body_start + 1;
        while ($j < length($text) && $depth > 0) {
            my $c = substr($text, $j, 1);
            if ($c eq '{') { $depth++ }
            elsif ($c eq '}') { $depth-- }
            $j++;
        }

        # Guess the EntityType name — `XyzAuditLogDto` → `Xyz`.
        my $entity = $entity_guess;
        # If the source also queries `db.XAuditLogs` we trust that name.
        my $before_substring = substr($text, $sig_start, $j - $sig_start);
        if ($before_substring =~ /\bdb\.(\w+)AuditLogs\b/) {
            $entity = $1;
        }

        my $new_method = <<EOF;
private static async Task<Ok<List<AWBlazorApp.Shared.Audit.AuditLog>>> HistoryAsync(
        ApplicationDbContext db,
        CancellationToken ct = default)
    {
        var rows = await db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == \"$entity\")
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
EOF
        chomp $new_method;
        substr($text, $sig_start, $j - $sig_start) = $new_method;

        # Reset pos so we re-scan from the start (indices shifted).
        pos($text) = 0;
    }

    if ($text eq $orig) {
        print "NO-CHANGE: $file\n";
        next;
    }
    open my $out, '>', $file or die "$file: $!";
    print $out $text;
    close $out;
    print "OK: $file\n";
}
