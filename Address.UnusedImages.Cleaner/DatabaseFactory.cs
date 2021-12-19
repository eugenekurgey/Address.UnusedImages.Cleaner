// Decompiled with JetBrains decompiler
// Type: Address.UnusedImages.Cleaner.DatabaseFactory
// Assembly: Address.UnusedImages.Cleaner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B187DB93-0B84-44A4-84F5-435C80057FD2
// Assembly location: C:\Users\Eugene Kurgey\Desktop\Decompile.dll

using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Address.UnusedImages.Cleaner
{
    public class DatabaseFactory
    {
        private readonly IConfigurationRoot _configuration;

        public DatabaseFactory(IConfigurationRoot configuration) => this._configuration = configuration;

        public DataSet GetListingPhotos()
        {
            DataSet dataSet = new DataSet();
            try
            {
                string connectionString = this._configuration.GetSection("ConnectionString").Value;
                string listingPhotoDomainId = this._configuration.GetSection("ListingPhotoDomainId").Value;
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    Console.WriteLine("Extracting images paths from database.");
                    
                    sqlConnection.Open();

                    var sql = "SELECT TOP(100) * FROM ListingPhoto lp where lp.ListingPhotoDomainId = " +
                              listingPhotoDomainId + " and (lp.state = 10 or lp.state = 1)";
                    
                    using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
                    {
                        sqlCommand.CommandTimeout = 10000;
                        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                        {
                            sqlDataAdapter.Fill(dataSet);
                        }
                        
                        Console.WriteLine(
                            Environment.NewLine + "Retrieving data from database..." + Environment.NewLine);
                    }
                }

                return dataSet;
            }
            catch (Exception ex)
            {
                Console.WriteLine((object)ex);
                throw;
            }
        }
    }
}