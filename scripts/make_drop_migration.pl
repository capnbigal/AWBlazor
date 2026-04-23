#!/usr/bin/env perl
# Build the paired Migration.cs + Migration.Designer.cs files for the DropLegacyAuditLogTables
# migration. Body of Designer mirrors the current ApplicationDbContextModelSnapshot.cs.

use strict;
use warnings;

my $migrations_dir = '/c/Users/capnb/source/repos/AWBlazor/src/AWBlazorApp/Infrastructure/Persistence/Migrations';
my $snapshot_path = "$migrations_dir/ApplicationDbContextModelSnapshot.cs";
my $tables_path = '/tmp/drop_tables_final.txt';
my $migration_id = '20260422213554_DropLegacyAuditLogTables';

# 1. Read drop-table list.
open my $th, '<', $tables_path or die "$tables_path: $!";
my @tables = <$th>;
close $th;
chomp @tables;
@tables = grep { length } @tables;

# 2. Read the snapshot.
open my $sh, '<', $snapshot_path or die "$snapshot_path: $!";
local $/ = undef;
my $snapshot = <$sh>;
close $sh;

# 3. Generate the Migration.cs file using idempotent raw SQL so redeploys / partial rollouts
#    don't fail on already-dropped tables.
my $up_body = '';
for my $table (@tables) {
    $up_body .= qq(            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[$table]', N'U') IS NOT NULL DROP TABLE [dbo].[$table];");\n);
}

my $migration_cs = <<"EOF";
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyAuditLogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
$up_body        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new System.NotSupportedException(
                "DropLegacyAuditLogTables cannot be rolled back. The 116 legacy *AuditLog "
                + "tables were retired by this migration; writes have long since moved to "
                + "audit.AuditLog via AuditLogInterceptor. To recover historical rows, restore "
                + "a pre-deployment SQL backup.");
        }
    }
}
EOF

# 4. Generate the Migration.Designer.cs file by surgically editing the snapshot.
#    - add `using Microsoft.EntityFrameworkCore.Migrations;`
#    - rewrite the class wrapper: `ApplicationDbContextModelSnapshot : ModelSnapshot` with
#      `BuildModel` → `DropLegacyAuditLogTables` with `BuildTargetModel` + [Migration(...)]
my $designer = $snapshot;

# Detect line ending style in the snapshot (CRLF vs LF).
my $eol = $designer =~ /\r\n/ ? "\r\n" : "\n";

# Insert the Migrations using directly after the Metadata using line.
my $extra_using = "using Microsoft.EntityFrameworkCore.Migrations;$eol";
$designer =~ s{using Microsoft\.EntityFrameworkCore\.Metadata;\r?\n}{using Microsoft.EntityFrameworkCore.Metadata;$eol$extra_using}
    or die "Could not insert Migrations using";

# Swap the class declaration.
my $old_decl = join($eol,
    "    [DbContext(typeof(ApplicationDbContext))]",
    "    partial class ApplicationDbContextModelSnapshot : ModelSnapshot",
    "    {",
    "        protected override void BuildModel(ModelBuilder modelBuilder)",
) . $eol;

my $new_decl = join($eol,
    "    [DbContext(typeof(ApplicationDbContext))]",
    qq(    [Migration("$migration_id")]),
    "    partial class DropLegacyAuditLogTables",
    "    {",
    "        /// <inheritdoc />",
    "        protected override void BuildTargetModel(ModelBuilder modelBuilder)",
) . $eol;

my $pos = index($designer, $old_decl);
die "Could not find class declaration to rewrite" if $pos < 0;
substr($designer, $pos, length $old_decl) = $new_decl;

# Write both files.
my $cs_path = "$migrations_dir/${migration_id}.cs";
my $designer_path = "$migrations_dir/${migration_id}.Designer.cs";

open my $ocs, '>', $cs_path or die "$cs_path: $!";
print $ocs $migration_cs;
close $ocs;
print "WROTE $cs_path\n";

open my $od, '>', $designer_path or die "$designer_path: $!";
print $od $designer;
close $od;
print "WROTE $designer_path\n";
