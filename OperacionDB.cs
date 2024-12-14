using System;
using System.Activities;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace COM_SYP
{
    public enum TipoOperacion
    {
        Select,
        Insert,
        Update,
        Delete
    }
    public class OperacionDB : CodeActivity
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
        public InArgument<string> Query { get; set; } // Consulta parametrizada

        [Category("Input")]
        public InArgument<Dictionary<string, object>> Parameters { get; set; } // Diccionario de parámetros

        [Category("Input")]
        [RequiredArgument]
        public InArgument<TipoOperacion> Operacion { get; set; }

        [Category("Output")]
        public OutArgument<DataTable> Result { get; set; } // Para SELECT

        protected override void Execute(CodeActivityContext context)
        {
            string server = Server.Get(context);
            string user = User.Get(context);
            string password = Password.Get(context);
            string database = Database.Get(context);
            string query = Query.Get(context);
            var parameters = Parameters.Get(context);
            var operacion = Operacion.Get(context);

            try
            {
                using var connection = new MySqlConnection($"Server={server};User ID={user};Password={password};Database={database}");
                connection.Open();

                using var command = new MySqlCommand(query, connection);

                // Agregar parámetros a la consulta
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                switch (operacion)
                {
                    case TipoOperacion.Select:
                        using (var reader = command.ExecuteReader())
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(reader);
                            Result.Set(context, dataTable);
                        }
                        break;

                    case TipoOperacion.Insert:
                    case TipoOperacion.Update:
                    case TipoOperacion.Delete:
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new InvalidOperationException($"La operación {operacion} no afectó filas.");
                        }
                        break;

                    default:
                        throw new InvalidOperationException("Operación no válida.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error durante la operación {operacion}: {ex.Message}");
            }
        }
    }
}
