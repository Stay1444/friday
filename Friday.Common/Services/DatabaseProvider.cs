﻿using Dapper;
using Friday.Common.Attributes;
using Friday.Common.Models;
using MySqlConnector;

namespace Friday.Common.Services;

public class DatabaseProvider
{
    private FridayConfiguration _configuration;
    public DatabaseProvider(FridayConfiguration config)
    {
        this._configuration = config;
        SqlMapper.TypeMapProvider = type =>
        {
            // create fallback default type map
            var fallback = new DefaultTypeMap(type);
            return new CustomPropertyTypeMap(type, (t, column) =>
            {
                var property = t.GetProperties().FirstOrDefault(prop =>
                    prop.GetCustomAttributes(true).Any(attr => attr.GetType() == typeof(ColumnName) && ((ColumnName)attr).Name == column));

                // if no property matched - fall back to default type map
                if (property == null)
                {
                    property = fallback.GetMember(column)?.Property;
                }

                return property;
            });
        };
    }
    
    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(_configuration.Database.ConnectionString);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
         var connection = GetConnection();
         await connection.OpenAsync();
         var result = await connection.QueryAsync<T>(sql, parameters);
         await connection.CloseAsync();
         return result;
    }
    
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
    {
        var connection = GetConnection();
        await connection.OpenAsync();
        var result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        await connection.CloseAsync();
        return result;
    }
    
    public async Task ExecuteAsync(string sql, object? parameters = null)
    {
        var connection = GetConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(new CommandDefinition(sql, parameters));
        await connection.CloseAsync();
    }
}

    
