using TabletService.Models;
using MySql.Data.MySqlClient;

namespace TabletService.Repository;

public class TabletRepository
{
    private readonly IConfiguration _configuration;
    private readonly string _defaultConnection;
   
    private readonly MySqlConnection _connection;
    
    public TabletRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        _defaultConnection =  _configuration.GetConnectionString("DefaultConnection");
        _connection = new MySqlConnection(_defaultConnection);
    }


    public async Task InsertOrUpdateTabletIp(TabletInfoModel model)
    {
        try
        {
            _connection.OpenAsync();

            var macStrToUIng = Convert.ToUInt64(model.TabletMAC);
            var query = $"REPLACE INTO tablet_info VALUES((select id from device where mac = {macStrToUIng}),'{model.TabletIP}',now(), '') ";
            var cmd = new MySqlCommand(query,_connection);
            cmd.ExecuteNonQueryAsync();

            _connection.CloseAsync();
        }
        catch (Exception e)
        {
            _connection.CloseAsync();
            throw;
        }
        
    }

    public async Task<string> GetPathByContentType(string contentType)
    {
        try
        {
            _connection.OpenAsync();
            var query  = $"select ContentPath from window_content_info where  ContentType = '{contentType}'";
            var cmd = new MySqlCommand(query, _connection);
            MySqlDataReader reader =  cmd.ExecuteReader();

            var path = String.Empty;
            while (reader.Read())
            { 
                path = (string) reader["ContentPath"];
            }
            
            _connection.CloseAsync();
            return path;
        }
        catch (Exception e)
        {
            _connection.CloseAsync();
            throw;
        }
    }
}