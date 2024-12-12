using System;
using System.Activities;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using MySqlConnector;

namespace APP_SYP
{
    public class ConsultaDB : CodeActivity
    {
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> Server { get; set; }

        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> User { get; set; }

        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> Password { get; set; }

        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> Database { get; set; }

        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> Query { get; set; }

        [Category("Output")]
        public OutArgument<DataTable> Result { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string server = Server.Get(context);
            string user = User.Get(context);
            string password = Password.Get(context);
            string database = Database.Get(context);
            string query = Query.Get(context);

            DataTable dataTable = new DataTable();

            try
            {
                using var connection = new MySqlConnection($"Server={server};User ID={user};Password={password};Database={database}");
                connection.Open();

                using var command = new MySqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                dataTable.Load(reader);
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                throw new InvalidOperationException($"Error ejecutando la consulta: {ex.Message}");
            }

            Result.Set(context, dataTable);
        }
    }
}