#!/usr/bin/env perl
# Rewrite legacy History.razor pages to use the shared <AuditHistoryGrid EntityType="X" />.
# Preserves the original @page routes, page title, and back-button URL.

use strict;
use warnings;

for my $file (@ARGV) {
    open my $in, '<', $file or die "$file: $!";
    local $/ = undef;
    my $text = <$in>;
    close $in;

    # Extract @page directives — keep all of them.
    my @pages;
    while ($text =~ /^(\@page\s+"[^"]+")\s*$/mg) { push @pages, $1 }

    unless (@pages) {
        print "NO-PAGE: $file\n";
        next;
    }

    # Extract entity name from PagedResult<XAuditLogDto> or db.XAuditLogs.
    my $entity;
    if ($text =~ /PagedResult<(\w+)AuditLogDto>/) {
        $entity = $1;
    } elsif ($text =~ /\bdb\.(\w+)AuditLogs\b/) {
        $entity = $1;
    } else {
        print "NO-ENTITY: $file\n";
        next;
    }

    # Extract back-button URL.
    my $back_url = '';
    if ($text =~ /Navigation\.NavigateTo\("([^"]+)"\)/) { $back_url = $1 }
    elsif ($text =~ /Href="([^"]+)"/) { $back_url = $1 }

    # Extract page title — look for <PageTitle>... and grab the leading text.
    my $title = "$entity history";
    if ($text =~ /<PageTitle>([^<@]+?)(?:@|<)/) {
        my $t = $1;
        $t =~ s/\s+$//;
        $title = $t if $t;
    }

    # Extract the optional [Authorize] attribute so we keep it if it was present.
    my $auth_attr = ($text =~ /^\@attribute\s+\[Authorize\]/m) ? "\@attribute [Authorize]\n" : '';

    # Extract the SlotId parameter if present (route constraint {SlotId:int}).
    my $has_slot = ($text =~ /\{SlotId:int\}/) ? 1 : 0;

    # Build the new content.
    my $back_line = '';
    if ($back_url) {
        $back_line = <<EOF;
        <MudIconButton Icon="\@Icons.Material.Filled.ArrowBack" aria-label="Back" Size="Size.Medium" OnClick="\@(() => Navigation.NavigateTo(\"$back_url\"))" />
EOF
        chomp $back_line;
    }

    my $entity_id_bind = $has_slot
        ? qq(EntityId="\@(SlotId?.ToString())")
        : '';

    my $heading = $has_slot
        ? qq(\@(SlotId is null ? "$title" : \$"History for $entity #{SlotId}"))
        : qq("$title");

    my $slot_param = $has_slot
        ? "    [Parameter] public int? SlotId { get; set; }\n"
        : '';

    my $pages_block = join "\n", @pages;

    my $inject_nav = $back_url ? "\@inject NavigationManager Navigation\n" : '';

    my $new = <<EOF;
$pages_block
$auth_attr$inject_nav
<PageTitle>$title</PageTitle>

<MudStack Spacing="3">
    <MudStack Row="true" AlignItems="AlignItems.Center">
$back_line
        <MudText Typo="Typo.h4">$heading</MudText>
    </MudStack>

    <AuditHistoryGrid EntityType="$entity" $entity_id_bind />
</MudStack>

\@code {
$slot_param}
EOF

    open my $out, '>', $file or die "$file: $!";
    print $out $new;
    close $out;
    print "OK: $file (entity=$entity, slot=$has_slot)\n";
}
