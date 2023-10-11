using System.Data.Common;
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

    public async Task UpdateReceiptActivity(TabletInfoModel model)
    {
        try
        {
           await _connection.OpenAsync();
            //get device id
            var macStrToUIng = Convert.ToUInt64(model.TabletMAC);
            var query = $"select id from device where mac = {macStrToUIng}";
            var cmd = new MySqlCommand(query,_connection);
            var reader =await cmd.ExecuteReaderAsync();
           await reader.ReadAsync();
            var deviceId =  Convert.ToInt32(reader["id"]);
            
            await _connection.CloseAsync();
            if (deviceId > 0)
            {
                // get
                // datetime
               await _connection.OpenAsync();
                query = $"select checks_per_day, change_date from tablet_activity_receipt where device_id = {deviceId}";
                 cmd = new MySqlCommand(query,_connection);
                 reader = await cmd.ExecuteReaderAsync();
                 
                 if (await reader.ReadAsync())
                 {
                     var checksPerDay = (int) reader["checks_per_day"];
                     var changeDate = (DateTime) reader["change_date"];
                     await _connection.CloseAsync();
                     
                     if (changeDate.Day == DateTime.Now.Day)
                     {
                         await InsertOrUpdateReceiptActivity(deviceId,checksPerDay +1);
                     }
                     else
                     {
                        await InsertOrUpdateReceiptActivity(deviceId,1);
                     }
                 }
                 else
                 {
                     await InsertOrUpdateReceiptActivity(deviceId,1);
                 }
            }
            
            await _connection.CloseAsync();
        }
        catch (Exception e)
        {
            await _connection.CloseAsync();
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task InsertOrUpdateReceiptActivity(int deviceId, int checksPerDay)
    {

        try
        {
           await _connection.OpenAsync();
            
            var query = $"REPLACE INTO tablet_activity_receipt VALUES({deviceId},{checksPerDay},now())";
            var cmd = new MySqlCommand(query,_connection);
            var result1 = await cmd.ExecuteNonQueryAsync();

            await _connection.CloseAsync();

        }
        catch (Exception e)
        {
            await _connection.CloseAsync();
            Console.WriteLine(e);
            throw;
        }
    }


    public async Task InsertOrUpdateTabletIp(TabletInfoModel model)
    {
        try
        {
            await _connection.OpenAsync();

            var macStrToUIng = Convert.ToUInt64(model.TabletMAC);
            var query = $"REPLACE INTO tablet_info VALUES((select id from device where mac = {macStrToUIng}),'{model.TabletIP}',now(), '') ";
            var cmd = new MySqlCommand(query,_connection);
            var result = await cmd.ExecuteNonQueryAsync();

            await _connection.CloseAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
           await _connection.CloseAsync();
            throw;
        }
        
    }

    
    public async Task<string> GetHTMLTemplate()
    {
        try
        {
           await _connection.OpenAsync();
            var query  = $"select HTML from html_template limit 1";
            MySqlCommand cmd = new MySqlCommand(query, _connection);
            DbDataReader reader =  await cmd.ExecuteReaderAsync();

            var html = String.Empty;
            while (await  reader.ReadAsync())
            { 
                html = (string) reader["HTML"];
            }
            
            await _connection.CloseAsync();
            return html;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            await _connection.CloseAsync();
            throw;
        }
    }

    public async Task<HtmlResourcesModel> GetHtmlResourcesByDeviceId(string mac)
    {
        try
        {
            await _connection.OpenAsync();
            var macStrToUIng = Convert.ToUInt64(mac);
            
            var query  = $"select html_template, attachment_path from html_resources where device_id = (select id from device where mac = {macStrToUIng})";
            MySqlCommand cmd = new MySqlCommand(query, _connection);
            DbDataReader reader = await cmd.ExecuteReaderAsync();

            HtmlResourcesModel htmlResources = new HtmlResourcesModel();
            if(await  reader.ReadAsync())
            { 
                htmlResources.HTML = (string) reader["html_template"];
                htmlResources.AtachmentPath = (string) reader["attachment_path"];
            }
            
           await _connection.CloseAsync();
            return htmlResources;
            
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
           await _connection.CloseAsync();
            throw;
        }
    }

    public async Task<List<string>> GetListHTMLAttachments()
    {
        try
        {
            await _connection.OpenAsync();
            var query  = $"select Path from html_attachment";
            var cmd = new MySqlCommand(query, _connection);
            var reader = await  cmd.ExecuteReaderAsync();

            var attachments = new List<string>();
            while (await reader.ReadAsync())
            { 
                attachments.Add((string) reader["Path"]);
            }
            
            await _connection.CloseAsync();
            return attachments;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            await _connection.CloseAsync();
            throw;
        }
    }
}