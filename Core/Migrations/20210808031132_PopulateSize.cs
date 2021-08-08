using System;
using System.IO;
using ImageInfrastructure.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class PopulateSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // force flush to prevent making a copy of ImageBlobs in WAL
            long count = 0;
            var path = string.IsNullOrEmpty(Arguments.DataPath) ? "Core.db3" : Path.Combine(Arguments.DataPath, "Database", "Core.db3");
            var connectionString = $"Data Source={path};";
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            var command = new SqliteCommand("SELECT Count(1) FROM ImageBlobs", connection);
            count = (long)command.ExecuteScalar();
            connection.Close();
            command.Dispose();
            connection.Dispose();

            for (int i = 0; i < Math.Ceiling(count / 1000D); i++)
            {
                var connection2 = new SqliteConnection(connectionString);
                connection2.Open();
                var newCommand =
                    new SqliteCommand(
                        $"UPDATE ImageBlobs SET Size = length(Data) WHERE ImageBlobId IN (SELECT ImageBlobId FROM ImageBlobs ORDER BY ImageBlobId LIMIT 100 OFFSET {i * 1000})",
                        connection2);
                newCommand.ExecuteNonQuery();
                connection2.Close();
                newCommand.Dispose();
                connection2.Dispose();
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
