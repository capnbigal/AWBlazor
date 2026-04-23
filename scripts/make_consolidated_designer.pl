#!/usr/bin/env perl
# Hand-write the Designer.cs companion for the existing 20260422121400_AddConsolidatedAuditLog
# migration, mirroring the current post-A8 ApplicationDbContextModelSnapshot content. EF only
# uses the Designer at `migrations add` time to diff the prior model; at runtime it ignores
# the Designer and runs Up() from the .cs file. So a forward-dated snapshot in the Designer
# is safe as long as the Up() is historically correct (which it is — we didn't touch the .cs).

use strict;
use warnings;

my $migrations_dir = '/c/Users/capnb/source/repos/AWBlazor/src/AWBlazorApp/Infrastructure/Persistence/Migrations';
my $snapshot_path = "$migrations_dir/ApplicationDbContextModelSnapshot.cs";
my $migration_id = '20260422121400_AddConsolidatedAuditLog';
my $designer_path = "$migrations_dir/${migration_id}.Designer.cs";

open my $sh, '<', $snapshot_path or die "$snapshot_path: $!";
local $/ = undef;
my $designer = <$sh>;
close $sh;

my $eol = $designer =~ /\r\n/ ? "\r\n" : "\n";

my $extra_using = "using Microsoft.EntityFrameworkCore.Migrations;$eol";
$designer =~ s{using Microsoft\.EntityFrameworkCore\.Metadata;\r?\n}{using Microsoft.EntityFrameworkCore.Metadata;$eol$extra_using}
    or die "Could not insert Migrations using";

my $old_decl = join($eol,
    "    [DbContext(typeof(ApplicationDbContext))]",
    "    partial class ApplicationDbContextModelSnapshot : ModelSnapshot",
    "    {",
    "        protected override void BuildModel(ModelBuilder modelBuilder)",
) . $eol;

my $new_decl = join($eol,
    "    [DbContext(typeof(ApplicationDbContext))]",
    qq(    [Migration("$migration_id")]),
    "    partial class AddConsolidatedAuditLog",
    "    {",
    "        /// <inheritdoc />",
    "        protected override void BuildTargetModel(ModelBuilder modelBuilder)",
) . $eol;

my $pos = index($designer, $old_decl);
die "Could not find class declaration to rewrite" if $pos < 0;
substr($designer, $pos, length $old_decl) = $new_decl;

open my $od, '>', $designer_path or die "$designer_path: $!";
print $od $designer;
close $od;
print "WROTE $designer_path\n";
