#!/usr/bin/env perl
# Convert a legacy Features/**/UI/Pages/Index.razor file to use the shared CrudPage template.
# This is tuned for AW reference-data pages that share the same structure:
#   - @page "/..."
#   - Page title + caption row + "View history" + "New X" buttons
#   - Filter MudPaper with 1-3 inputs
#   - MudDataGrid<XDto> with Id/Name/ModifiedDate columns + actions
#   - LoadServerDataAsync, OpenCreateAsync, OpenEditAsync, DeleteAsync methods
#
# The script extracts the entity-specific pieces (entity name, dto name, filter fields,
# column list, sort mapping, ownership metadata) and re-emits a CrudPage-based file.
#
# It prints NEEDS-REVIEW for any file that deviates from the expected pattern and leaves
# the file untouched so the caller can convert that one manually.
#
# Usage:
#   perl scripts/convert_to_crudpage.pl <path/to/Index.razor>

use strict;
use warnings;

my $verbose = 0;

sub extract {
    my ($text, $pattern, $default) = @_;
    return $1 if $text =~ /$pattern/;
    return $default;
}

sub convert_file {
    my ($file) = @_;
    open my $in, '<', $file or die "$file: $!";
    local $/ = undef;
    my $text = <$in>;
    close $in;

    # Bail if this file already uses CrudPage.
    if ($text =~ /<CrudPage\s+TDto/) {
        print "SKIP (already converted): $file\n";
        return;
    }

    # --- Extract required metadata ---

    my ($page_route) = $text =~ /^\@page\s+"([^"]+)"/m
        or return fail("no \@page route", $file);

    my ($title) = $text =~ /<PageTitle>([^<]+?)<\/PageTitle>/s
        or return fail("no <PageTitle>", $file);

    my ($caption) = $text =~ /<MudText\s+Typo="Typo\.caption"[^>]*>([^<]+?)<\/MudText>/s;

    my ($history_route) = $text =~ /Href="([^"]+?\/history)"/s
        or return fail("no history Href", $file);

    my ($dto) = $text =~ /<MudDataGrid\s+T="(\w+Dto)"/s
        or return fail("no MudDataGrid T=XDto", $file);
    my $entity = $dto;
    $entity =~ s/Dto$//;

    my ($db_set) = $text =~ /db\.(\w+)\.AsNoTracking\(\)/s
        or return fail("no db.XxxSet.AsNoTracking", $file);

    # Try to extract the dialog class (e.g., AddressTypeDialog).
    my ($dialog) = $text =~ /DialogService\.ShowAsync<(\w+Dialog)>/s
        or return fail("no DialogService.ShowAsync<XDialog>", $file);

    # Try to extract the create/edit/delete snackbar messages.
    my ($created_msg) = $text =~ /Snackbar\.Add\("([^"]+? created\.)"/s;
    my ($updated_msg) = $text =~ /Snackbar\.Add\("([^"]+? updated\.)"/s;
    my ($deleted_msg) = $text =~ /Snackbar\.Add\("([^"]+? deleted\.)"/s;
    $created_msg //= "$entity created.";
    $updated_msg //= "$entity updated.";
    $deleted_msg //= "$entity deleted.";

    # The new-button label.
    my ($new_label) = $text =~ /OnClick="OpenCreateAsync">\s*([^<\n]+?)\s*<\/MudButton>/s;
    $new_label //= "New";

    # EntityNameSingular: extract from the "Delete X?" title or snackbar.
    my ($entity_singular) = $text =~ /"Delete ([^?"]+)\?"/s;
    $entity_singular //= lc($entity);
    $entity_singular =~ s/^\s+|\s+$//g;

    # Edit dialog title pattern. We handle the common "Edit #{row.Id}" / "Edit X #{row.Id}".
    my ($edit_title) = $text =~ /ShowAsync<\w+Dialog>\("([^"]*\{row[^"]*)",\s*parameters/s;
    $edit_title //= "Edit #{row.Id}";

    # --- Extract the filter MudPaper body and the Columns block ---

    # Find the <MudPaper Class="pa-3" Elevation="1">...</MudPaper> that wraps filters.
    my ($filters_block);
    if ($text =~ /<MudPaper\s+Class="pa-3"\s+Elevation="1">\s*(<MudGrid[^>]*>.+?<\/MudGrid>|<MudTextField[^>]*\/>)\s*<\/MudPaper>/s) {
        $filters_block = $1;
    }
    # Some files wrap filters directly in <MudPaper> without a <MudGrid> — wrap those ourselves.
    # Normalize: if we got just <MudTextField ... />, wrap in <MudItem xs="12">.
    if (defined $filters_block && $filters_block =~ /^<MudTextField/) {
        $filters_block = qq(<MudItem xs="12">$filters_block</MudItem>);
    }
    # Strip the outer <MudGrid Spacing="..."> wrapper (CrudPage supplies its own <MudGrid>).
    if (defined $filters_block) {
        $filters_block =~ s/^\s*<MudGrid\b[^>]*>\s*//s;
        $filters_block =~ s/\s*<\/MudGrid>\s*$//s;
    }
    # Remove Immediate= and OnBlur="ApplyFilter" attributes (CrudPage reloads on Apply via
    # callbacks; inputs can stay @bind-Value only).
    if (defined $filters_block) {
        $filters_block =~ s/\s*Immediate="false"//g;
        $filters_block =~ s/\s*OnBlur="ApplyFilter"//g;
    }

    # Extract the <Columns>...</Columns> block.
    my ($columns_block) = $text =~ /<Columns>(.+?)<\/Columns>/s
        or return fail("no <Columns> block", $file);

    # Strip the legacy <HeaderTemplate>...GridColumnHeader.../<\/HeaderTemplate> — CrudPage
    # doesn't wire columnFilters so these headers would be dead.
    $columns_block =~ s/\s*<HeaderTemplate>[^<]*<GridColumnHeader\s[^\/]*\/>\s*<\/HeaderTemplate>//gs;

    # Strip the "<TemplateColumn Title=\"Actions\" Sortable=\"false\">...</TemplateColumn>"
    # block entirely — CrudPage appends its own Actions column.
    $columns_block =~ s/<TemplateColumn\s+Title="Actions"\s+Sortable="false">.*?<\/TemplateColumn>//s;

    # Add explicit T="XDto" / TProperty= annotations to each PropertyColumn since the
    # compiler can't infer across the RenderFragment boundary.
    $columns_block =~ s{<PropertyColumn\s+Property="x\s*=>\s*x\.(\w+)"(\s+Title="[^"]+")(\s+Format="[^"]+")?\s*/?>}{
        my ($prop, $title_attr, $fmt_attr) = ($1, $2, $3 // '');
        qq(<PropertyColumn T="$dto" TProperty="@{[guess_tprop($prop, $text)]}" Property="x => x.$prop"$title_attr$fmt_attr />)
    }gse;

    # Strip orphan </PropertyColumn> tags left over after we self-closed the opener above.
    $columns_block =~ s{\s*</PropertyColumn>\s*\n?}{\n            }gs;

    # TemplateColumn needs T= too.
    $columns_block =~ s{<TemplateColumn\s+Title="([^"]+)"\s+Sortable="false">}{<TemplateColumn T="$dto" Title="$1" Sortable="false">}gs;

    # --- Extract the code-block details: filter fields, LoadServerDataAsync body,
    #     entity removal in DeleteAsync. ---

    # Field declarations (private string? filterXxx; / private int? filterYyy;)
    my @filter_fields;
    while ($text =~ /^\s*private\s+((?:string|int|decimal|bool|DateTime)\??)\s+(filter\w+)\s*(?:=\s*[^;]+)?\s*;/mg) {
        my ($type, $name) = ($1, $2);
        next if $name eq 'filterName' && $type eq 'ColumnFilterState'; # skip
        push @filter_fields, qq(private $type $name;);
    }
    # also catch `private ColumnFilterState columnFilters = new();` — we don't use it.

    # Get the body of LoadServerDataAsync (lines between `private async Task<GridData<XDto>> LoadServerDataAsync(...)`
    # and the matching closing brace). We reproduce it verbatim minus the _loading / InvokeAsync
    # noise — but preserving the query building + sort switch.
    my ($load_body) = $text =~ /private\s+async\s+Task<GridData<\w+Dto>>\s+LoadServerDataAsync\([^)]+\)\s*\{(.+?)\}\s*\n\s*private\s+async\s+Task\s+ApplyFilter/s;
    if (!defined $load_body) {
        return fail("can't isolate LoadServerDataAsync body", $file);
    }

    # Strip the _loading / InvokeAsync scaffolding + ApplyColumnFilters line.
    my $new_load_body = $load_body;
    $new_load_body =~ s/\s*_loading\s*=\s*(?:true|false);\s*//g;
    $new_load_body =~ s/\s*await InvokeAsync\(StateHasChanged\);\s*//g;
    $new_load_body =~ s/\s*query\s*=\s*query\.ApplyColumnFilters\(columnFilters\);\s*\n?//g;
    # Drop try { ... } finally { (emptied) } wrapper since we no longer manage _loading.
    $new_load_body =~ s/^\s*try\s*\{\s*//s;
    $new_load_body =~ s/\s*\}\s*finally\s*\{\s*\}\s*$//s;
    # Indent the body uniformly (4 spaces for code block, 8 for content).
    $new_load_body =~ s/^\s+//;
    $new_load_body =~ s/\s+$//;
    $new_load_body = "\n        " . $new_load_body . "\n    ";

    # --- Get the snackbar / dialog patterns for OnCreate / OnEdit from the existing file.
    #     We don't need the body directly — we use the extracted dialog name + snackbar text.

    # --- Build the new file ---

    my $namespace_root = $entity;  # best-effort; not used currently
    my $new_content = build_new_file(
        page_route       => $page_route,
        title            => $title,
        caption          => $caption,
        history_route    => $history_route,
        dto              => $dto,
        entity           => $entity,
        db_set           => $db_set,
        dialog           => $dialog,
        new_label        => $new_label,
        created_msg      => $created_msg,
        updated_msg      => $updated_msg,
        deleted_msg      => $deleted_msg,
        entity_singular  => $entity_singular,
        edit_title       => $edit_title,
        filters_block    => $filters_block,
        columns_block    => $columns_block,
        filter_fields    => \@filter_fields,
        load_body        => $new_load_body,
        original         => $text,
    );

    open my $out, '>', $file or die "$file: $!";
    print $out $new_content;
    close $out;
    print "OK: $file\n";
}

sub fail {
    my ($reason, $file) = @_;
    print "NEEDS-REVIEW ($reason): $file\n";
}

# Best-effort TProperty guess based on property name.
sub guess_tprop {
    my ($prop, $text) = @_;
    # Look at the record declaration in the *Dtos.cs file to pick the real type.
    # For the common ones we handle inline.
    return 'int'      if $prop =~ /Id$/      && $prop !~ /TaxRateId/;
    return 'DateTime' if $prop =~ /(Date|At)$/;
    return 'bool'     if $prop =~ /^Is\w+$/  || $prop eq 'IsActive' || $prop eq 'OnlineOrderFlag';
    return 'byte'     if $prop eq 'TaxType' || $prop eq 'Status';
    return 'decimal'  if $prop =~ /(Rate|Amt|Total|Cost|Price|Freight|Subtotal|SubTotal|Discount|Tax)$/;
    return 'int?'     if $prop =~ /Id$/;
    return 'string';
}

sub build_new_file {
    my %args = @_;

    # Convert the `aw/xxx/history` route → `aw/xxx` for the back-nav base (not needed; CrudPage builds from history route).

    # Determine the using namespace blocks we still need — keep any @using lines from the
    # original file that aren't "generic bundle" imports.
    my @using;
    while ($args{original} =~ /^\@using\s+([^\s]+)\s*$/mg) {
        my $ns = $1;
        push @using, "\@using $ns";
    }
    # We can simplify by only keeping the Domain + Dtos lines for the specific entity.
    # Extract those that contain the entity's feature subfolder.
    my @kept_using = grep {
        /\.(Domain|Dtos)$/ && /\.$args{entity}\b/
    } @using;
    # If we found exactly the two we expect, use them; otherwise keep all Domain/Dtos usings.
    if (@kept_using < 1) {
        @kept_using = grep { /\.(Domain|Dtos)$/ } @using;
    }

    my $using_block = join "\n", @kept_using;
    my $filter_fields_block = join "\n    ", @{ $args{filter_fields} };

    my $filters_block = $args{filters_block} // '';
    my $filters_tag = $filters_block
        ? "    <Filters>\n        $filters_block\n    </Filters>\n"
        : '';

    my $caption_attr = $args{caption}
        ? qq(\n          Caption="$args{caption}")
        : '';

    my $edit_title = $args{edit_title};

    my $code = <<"EOF";
\@page "$args{page_route}"
\@attribute [Authorize]
$using_block
\@inject IDbContextFactory<ApplicationDbContext> DbFactory
\@inject IDialogService DialogService
\@inject ISnackbar Snackbar

<CrudPage TDto="$args{dto}"
          Title="$args{title}"$caption_attr
          NewButtonLabel="$args{new_label}"
          HistoryRoute="$args{history_route}"
          EditRoles="\@(\$"{AppRoles.Employee},{AppRoles.Manager},{AppRoles.Admin}")"
          DeleteRoles="\@(\$"{AppRoles.Manager},{AppRoles.Admin}")"
          LoadData="LoadServerDataAsync"
          IdOf="row => row.Id"
          OnCreate="OpenCreateAsync"
          OnEdit="OpenEditAsync"
          OnDelete="DeleteAsync"
          EntityNameSingular="$args{entity_singular}">
$filters_tag    <Columns>
$args{columns_block}
    </Columns>
</CrudPage>

\@code {
    $filter_fields_block

    private async Task<GridData<$args{dto}>> LoadServerDataAsync(GridState<$args{dto}> state, CancellationToken ct)
    {$args{load_body}    }

    private async Task OpenCreateAsync()
    {
        var dialog = await DialogService.ShowAsync<$args{dialog}>("$args{new_label}", new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true });
        if ((await dialog.Result) is { Canceled: false }) Snackbar.Add("$args{created_msg}", Severity.Success);
    }

    private async Task OpenEditAsync($args{dto} row)
    {
        var parameters = new DialogParameters<$args{dialog}> { { x => x.Model, row } };
        var dialog = await DialogService.ShowAsync<$args{dialog}>(\$"$edit_title", parameters, new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true });
        if ((await dialog.Result) is { Canceled: false }) Snackbar.Add("$args{updated_msg}", Severity.Success);
    }

    private async Task DeleteAsync($args{dto} row)
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        var entity = await db.$args{db_set}.FirstOrDefaultAsync(t => t.Id == row.Id);
        if (entity is null) { Snackbar.Add("Already gone.", Severity.Warning); return; }
        db.$args{db_set}.Remove(entity);
        try { await db.SaveChangesAsync(); Snackbar.Add("$args{deleted_msg}", Severity.Success); }
        catch (DbUpdateException ex) { Snackbar.Add(\$"Cannot delete: {ex.InnerException?.Message ?? ex.Message}", Severity.Error); }
    }
}
EOF
    return $code;
}

for my $file (@ARGV) {
    convert_file($file);
}
