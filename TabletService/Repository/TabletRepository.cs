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
    
    public async Task<string> GetHTMLTemplate()
    {
        try
        {
            _connection.OpenAsync();
            var query  = $"select HTML from html_template limit 1";
            var cmd = new MySqlCommand(query, _connection);
            MySqlDataReader reader =  cmd.ExecuteReader();

            var html = String.Empty;
            while (reader.Read())
            { 
                html = (string) reader["HTML"];
            }
            
            _connection.CloseAsync();
            return html;
        }
        catch (Exception e)
        {
            _connection.CloseAsync();
            throw;
        }
    }
    
    public async Task<List<string>> GetListHTMLAttachments()
    {
        try
        {
            _connection.OpenAsync();
            var query  = $"select Path from html_attachment";
            var cmd = new MySqlCommand(query, _connection);
            MySqlDataReader reader =  cmd.ExecuteReader();

            var attachments = new List<string>();
            while (reader.Read())
            { 
                attachments.Add((string) reader["Path"]);
            }
            
            _connection.CloseAsync();
            return attachments;
        }
        catch (Exception e)
        {
            _connection.CloseAsync();
            throw;
        }
    }
    
    public void  Upload(IFormFile file)
    {
        try
        {
            string uploads = Path.Combine("D:\\");
            if (file.Length > 0)
            {
                string filePath = Path.Combine(uploads, file.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                     file.CopyTo(fileStream);
                }
            }
        }
        catch (Exception e)
        {
        }
    }
}