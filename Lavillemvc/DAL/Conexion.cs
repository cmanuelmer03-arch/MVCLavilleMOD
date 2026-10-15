
using Npgsql;

namespace Lavillemvc.DAL
{
    public class Conexion
    {
        private readonly string _cadenaConexion;

        public Conexion(IConfiguration configuration)
        {
            _cadenaConexion =
                configuration.GetConnectionString(
                    "SupabaseConnection"
                ) ?? throw new Exception(
                    "No se encontró la cadena de conexión."
                );
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_cadenaConexion);
        }
    }
}
