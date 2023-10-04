﻿using System.Data.Common;
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
        //var mariadbCon = "Server=127.0.0.1;Port=3008;Database=stopper_db_prod;User=root;Password=pass;";
        //var mariadbCon = "server=localhost;port=3008;uid=root;pwd=pass;database=stopper_db_prod";
        //_defaultConnection = mariadbCon;
        _defaultConnection =  _configuration.GetConnectionString("DefaultConnection");
        _connection = new MySqlConnection(_defaultConnection);
    }

    public async Task UpdateReceiptActivity(TabletInfoModel model)
    {
        try
        {
            _connection.Open();

            var macStrToUIng = Convert.ToUInt64(model.TabletMAC);
            var query = $"REPLACE INTO tablet_activity_receipt VALUES((select id from device where mac = {macStrToUIng}),1,now())";
            var cmd = new MySqlCommand(query,_connection);
            var result = cmd.ExecuteNonQuery();
            _connection.Close();

        }
        catch (Exception e)
        {
            _connection.Close();
            Console.WriteLine(e);
            throw;
        }
    }


    public async Task InsertOrUpdateTabletIp(TabletInfoModel model)
    {
        try
        {
            _connection.Open();

            var macStrToUIng = Convert.ToUInt64(model.TabletMAC);
            var query = $"REPLACE INTO tablet_info VALUES((select id from device where mac = {macStrToUIng}),'{model.TabletIP}',now(), '') ";
            var cmd = new MySqlCommand(query,_connection);
            var result =  cmd.ExecuteNonQuery();

            _connection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _connection.Close();
            throw;
        }
        
    }

    
    public async Task<string> GetHTMLTemplate()
    {
        try
        {
            _connection.Open();
            var query  = $"select HTML from html_template limit 1";
            MySqlCommand cmd = new MySqlCommand(query, _connection);
            DbDataReader reader =  cmd.ExecuteReader();

            var html = String.Empty;
            while (  reader.Read())
            { 
                html = (string) reader["HTML"];
            }
            
            _connection.Close();
            return html;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            _connection.Close();
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