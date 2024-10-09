using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

class Program
{
    #region Conexión a la base de datos
    // ---------------------------- Conexión a la base de datos ---------------------------- //
    static string connectionString = "Data Source=localhost;Initial Catalog=Reservaciones;Integrated Security=True";
    // ------------------------------------------------------------------------------------- //
    #endregion
    #region Main()
    // ---------------------------- Main que invoca al menú al iniciar el sistema ---------------------------- //
    static void Main(string[] args)
    {
        Menu();
    }
    // ------------------------------------------------------------------------------------------------------- //
    #endregion
    #region Menú
    // ---------------------------- Menú Principal ---------------------------- //
    static void Menu()
    {
        try
        {
            Console.WriteLine("┌────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│                              MENÚ PRINCIPAL                            │");
            Console.WriteLine("├────────────────────────────────────────────────────────────────────────┤");
            Console.WriteLine("│                      Seleccione la opción que desee                    │");
            Console.WriteLine("├────────────────────────────────────────────────────────────────────────┤");
            Console.WriteLine("│ 1. Listar habitaciones disponibles                                     │");
            Console.WriteLine("│ 2. Registrar nueva reserva                                             │");
            Console.WriteLine("│ 3. Consultar reservas de un cliente                                    │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────────────┘");


            while (true)
            {
                Console.Write("\nSeleccione una opción: ");
                int opcion = int.Parse(Console.ReadLine());

                switch (opcion)
                {
                    case 1:
                        ListarHabitaciones();
                        break;
                    case 2:
                        RegistrarReserva();
                        break;
                    case 3:
                        ConsultarReservasCliente();
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ocurrió un error: " + ex.Message);
        }
    }
    // ------------------------------------------------------------------------ //
    #endregion
    #region Listar Habitaciones
    // ---------------------------- Lista todas las habitaciones disponibles en base a si están reservadas o no ---------------------------- //
    static void ListarHabitaciones()
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT H.HabitacionID, H.TipoHabitacion, H.PrecioPorNoche FROM Habitaciones H LEFT JOIN Reservas R ON H.HabitacionID = R.HabitacionID AND R.FechaEntrada <= GETDATE() AND R.FechaSalida >= GETDATE() WHERE R.ReservaID IS NULL;";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("┌─────────────────────┬────────────────────────────┬─────────────────────┐");
                    Console.WriteLine("│ Nro Habitación      │ Tipo de Habitación         │ Precio por Noche    │");
                    Console.WriteLine("├─────────────────────┼────────────────────────────┼─────────────────────┤");

                    while (reader.Read())
                    {
                        Console.WriteLine($"│ {reader["HabitacionID"],-19} │ {reader["TipoHabitacion"],-26} │ {reader["PrecioPorNoche"],-19:C} │");
                    }

                    Console.WriteLine("└─────────────────────┴────────────────────────────┴─────────────────────┘");
                }
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"Un error ha ocurrido: {e}");
            throw;
        }
        pregunta();
    }
    // ------------------------------------------------------------------------------------------------------------------------------------- //
    #endregion
    #region Registrar Reserva
    // ---------------------------- Registra reservaciones capturando ciertos datos escenciales ---------------------------- //
    static void RegistrarReserva()
    {

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            int cliente_id;
            int habitacion_id;

            while (true)
            {
                // Pedir ID del cliente
                Console.Write("Ingrese el ID del Cliente: ");
                int clienteID = int.Parse(Console.ReadLine());

                // Validar si el cliente existe
                string queryCliente = "SELECT COUNT(*) FROM Clientes WHERE ClienteID = @ClienteID";
                SqlCommand commandCliente = new SqlCommand(queryCliente, connection);
                commandCliente.Parameters.AddWithValue("@ClienteID", clienteID);
                int clienteExiste = (int)commandCliente.ExecuteScalar();

                if (clienteExiste == 0)
                {
                    Console.WriteLine("El cliente no existe.");
                }else
                {
                    cliente_id = clienteID;
                    break;
                }
            }

            while (true)
            {
                // Pedir ID de la habitación
                Console.Write("Ingrese el ID de la Habitación: ");
                int habitacionID = int.Parse(Console.ReadLine());

                // Validar si la habitación existe
                string queryHabitacion = "SELECT COUNT(*) FROM Habitaciones WHERE HabitacionID = @HabitacionID";
                SqlCommand commandHabitacion = new SqlCommand(queryHabitacion, connection);
                commandHabitacion.Parameters.AddWithValue("@HabitacionID", habitacionID);

                int habitacionExiste = (int)commandHabitacion.ExecuteScalar();

                if (habitacionExiste == 0)
                {
                    Console.WriteLine("La habitación no existe");
                }
                else
                {
                    habitacion_id = habitacionID;
                    break;
                }
            }
            
            //Pedir fechas
            DateTime fechaEntrada = LeerFecha("Ingrese la fecha de entrada (yyyy-mm-dd): ");
            DateTime fechaSalida = LeerFecha("Ingrese la fecha de salida (yyyy-mm-dd): ");

            using (SqlConnection connection2 = new SqlConnection(connectionString))
            {
                connection2.Open();

                // Validar si la habitación existe y está disponible
                string queryValidacion = @"SELECT COUNT(*) FROM Habitaciones h LEFT JOIN Reservas r ON h.HabitacionID = r.HabitacionID WHERE h.HabitacionID = @HabitacionID  AND r.FechaEntrada < @FechaSalida AND r.FechaSalida > @FechaEntrada ";

                using (SqlCommand commandValidacion = new SqlCommand(queryValidacion, connection2))
                {
                    commandValidacion.Parameters.AddWithValue("@HabitacionID", habitacion_id);
                    commandValidacion.Parameters.AddWithValue("@FechaEntrada", fechaEntrada);
                    commandValidacion.Parameters.AddWithValue("@FechaSalida", fechaSalida);

                    int disponibilidad = (int)commandValidacion.ExecuteScalar();

                    if (disponibilidad > 0)
                    {
                        Console.WriteLine("La habitación no está disponible en las fechas seleccionadas.");
                        return;
                    }
                }
            }

                // Llamar al procedimiento almacenado RegistrarReserva
            using (SqlCommand command = new SqlCommand("RegistrarReserva", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Pasar los parámetros al procedimiento almacenado
                command.Parameters.AddWithValue("@ClienteID", cliente_id);
                command.Parameters.AddWithValue("@HabitacionID", habitacion_id);
                command.Parameters.AddWithValue("@FechaEntrada", fechaEntrada);
                command.Parameters.AddWithValue("@FechaSalida", fechaSalida);

                // Ejecutar el procedimiento almacenado
                command.ExecuteNonQuery();

                Console.WriteLine("Reserva registrada con éxito.");
            }
        }

        pregunta();
    }

    // --------------------------------------------------------------------------------------------------------------------- //
    #endregion
    #region Consultar Reservas por Cliente
    // ---------------------------- Consulta las reservaciones filtrandolas por clientes ---------------------------- //
    static void ConsultarReservasCliente()
    {
        Console.Write("\nIngrese el ID del Cliente: ");
        int clienteID = int.Parse(Console.ReadLine());

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = @"SELECT r.FechaEntrada, r.FechaSalida, r.Total, h.HabitacionID FROM Reservas r INNER JOIN Habitaciones h ON r.HabitacionID = h.HabitacionID WHERE ClienteID = @ClienteID AND FechaSalida > GETDATE()";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClienteID", clienteID);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                // Cabecera de la tabla
                Console.WriteLine("┌─────────────────────┬─────────────────────┬─────────────────────┬─────────────────────┐");
                Console.WriteLine("│ Habitación ID       │ Fecha Entrada       │ Fecha Salida        │ Total               │");
                Console.WriteLine("├─────────────────────┼─────────────────────┼─────────────────────┼─────────────────────┤");

                // Leer los datos
                bool hayReservas = false;
                while (reader.Read())
                {
                    hayReservas = true; // Indica que hay al menos una reserva
                    Console.WriteLine($"│ {reader["HabitacionID"],-19} │ {((DateTime)reader["FechaEntrada"]).ToString("yyyy-MM-dd"),-19} │ {((DateTime)reader["FechaSalida"]).ToString("yyyy-MM-dd"),-19} │ {((decimal)reader["Total"]).ToString("C"),-19} │");
                }

                // Cerrar la tabla
                Console.WriteLine("└─────────────────────┴─────────────────────┴─────────────────────┴─────────────────────┘");

                if (!hayReservas)
                {
                    Console.WriteLine("No hay reservas activas para el cliente.");
                }
            }
        }

        pregunta();
    }
    // -------------------------------------------------------------------------------------------------------------- //
    #endregion
    #region Validador de fecha
    // ---------------- Valida que las fechas estén bien establecidas --------------- //
    static DateTime LeerFecha(string mensaje)
    {
        DateTime fecha;
        string formato = "yyyy-MM-dd";
        while (true)
        {
            Console.Write(mensaje);
            string entrada = Console.ReadLine();

            // Intentar analizar la fecha con el formato específico
            if (DateTime.TryParseExact(entrada, formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                return fecha; // Si la fecha es válida, retorna el valor
            }
            else
            {
                Console.WriteLine("Formato de fecha incorrecto. Por favor ingrese la fecha en el formato (yyyy-mm-dd).");
            }
        }
    }
    // ------------------------------------------------------------------------------ //
    #endregion
    #region Pregunta
    // ---------------------------- Pregunta si quiere volver al menú principal ---------------------------- //
    static void pregunta()
    {
        while (true)
        {
            Console.Write("\n¿Desea regresar al menú principal? Y/N: ");
            string opcion = Console.ReadLine().ToLower();

            if (opcion == "y")
            {
                Menu();
                break;
            }
            else if (opcion == "n")
            {
                Console.WriteLine("Saliendo...");
                break;
            }
            else
            {
                Console.WriteLine("Opción no válida. Por favor ingrese 'Y' o 'N'.");
            }
        }
    }
    // -------------------------------------------------------------------------------------------------- //
    #endregion
}
