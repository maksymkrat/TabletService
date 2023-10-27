using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        _defaultConnection = _configuration.GetConnectionString("DefaultConnection");

        _connection = new MySqlConnection(_defaultConnection);
    }

    private async Task<int> GetDeviceIdByMAC(string mac)
    {
        try
        {
            var macStrToUIng = Convert.ToUInt64(mac);
            
            var query = $"select id from device where mac = {macStrToUIng}";
            var cmd = new MySqlCommand(query, _connection);
            await _connection.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var deviceId = Convert.ToInt32(reader["id"]);

            await _connection.CloseAsync();

            return deviceId;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task UpdateReceiptActivity(TabletInfoModel model)
    {
        try
        {
            int deviceId = await GetDeviceIdByMAC(model.TabletMAC);
            if (deviceId > 0)
            {
                // get
                // datetime
                await _connection.OpenAsync();
                var query =
                    $"select checks_per_day, change_checks_date from html_content_planning where device_id = {deviceId}";
                var cmd = new MySqlCommand(query, _connection);
                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var checksPerDay = reader["checks_per_day"];
                    var changeDate = reader["change_checks_date"];
                    await _connection.CloseAsync();
                    if (checksPerDay is DBNull || changeDate is DBNull)
                    {
                        await UpdateReceiptActivity(deviceId, 1);
                    }
                    else
                    {
                        DateTime date = (DateTime) changeDate;
                        if (date.Day == DateTime.Now.Day)
                        {
                            await UpdateReceiptActivity(deviceId, (int) checksPerDay + 1);
                        }
                        else
                        {
                            await UpdateReceiptActivity(deviceId, 1);
                        }
                    }
                }
                else
                {
                    await _connection.CloseAsync();
                    await InsertReceiptActivity(deviceId, 1);
                }
            }
        }
        catch (Exception e)
        {
            await _connection.CloseAsync();
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task InsertReceiptActivity(int deviceId, int checksPerDay)
    {
        try
        {
            await _connection.OpenAsync();
            var query =
                $"INSERT INTO html_content_planning (device_id, checks_per_day, change_checks_date) value ({deviceId}, {checksPerDay},now())";
            var cmd = new MySqlCommand(query, _connection);
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

    private async Task UpdateReceiptActivity(int deviceId, int checksPerDay)
    {
        try
        {
            await _connection.OpenAsync();

            var query =
                $"update  html_content_planning  set checks_per_day = {checksPerDay}, change_checks_date = now() where device_id = {deviceId}";
            var cmd = new MySqlCommand(query, _connection);
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
            int id = await GetDeviceIdByMAC(model.TabletMAC);
            if (id > 0)
            {
                await _connection.OpenAsync();
                var query =
                    $"select device_id from html_content_planning where device_id = (select id from device where mac = {Convert.ToUInt64(model.TabletMAC)})";
                var cmd = new MySqlCommand(query, _connection);
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var deviceId = reader["device_id"];
                    await _connection.CloseAsync();

                    if (deviceId is DBNull)
                       await InsertDeviceIP((int) deviceId, model.TabletIP);
                    else
                       await UpdateDeviceIP((int) deviceId, model.TabletIP);
                }
                else
                {
                    await _connection.CloseAsync();
                    await InsertDeviceIP(id, model.TabletIP);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await _connection.CloseAsync();
            throw;
        }
    }

    private async Task InsertDeviceIP(int deviceId, string deviceIp)
    {
        try
        {
            await _connection.OpenAsync();
            var query = $"INSERT INTO html_content_planning (device_id, device_ip, change_ip_date) value ({deviceId}, '{deviceIp}',now())";
            var cmd = new MySqlCommand(query, _connection);
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

    private async Task UpdateDeviceIP(int deviceId, string deviceIp)
    {
        try
        {
            await _connection.OpenAsync();

            var query =
                $"update  html_content_planning  set device_ip = {deviceIp}, change_ip_date = now() where device_id = {deviceId}";
            var cmd = new MySqlCommand(query, _connection);
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


    public async Task<string> GetHTMLTemplate()
    {
        try
        {
            await _connection.OpenAsync();
            var query = $"select HTML from html_template limit 1";
            MySqlCommand cmd = new MySqlCommand(query, _connection);
            DbDataReader reader = await cmd.ExecuteReaderAsync();

            var html = String.Empty;
            while (await reader.ReadAsync())
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

            var query =
                $"SELECT html_template, attachment_path FROM html_content_planning WHERE device_id = (select id from device where mac = {macStrToUIng})";
            MySqlCommand cmd = new MySqlCommand(query, _connection);
            DbDataReader reader = await cmd.ExecuteReaderAsync();

            HtmlResourcesModel htmlResources = new HtmlResourcesModel();
            if (await reader.ReadAsync())
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
            var query = $"select Path from html_attachment";
            var cmd = new MySqlCommand(query, _connection);
            var reader = await cmd.ExecuteReaderAsync();

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