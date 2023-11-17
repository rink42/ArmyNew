﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace ArmyAPI.Data
{
	public class DapperHelper
	{
		private readonly string _connectionString;

		public DapperHelper(string connectionString)
		{
			_connectionString = connectionString;
		}

		public T ExecuteScalar<T>(string query, object parameters = null)
		{
			using (IDbConnection dbConnection = new SqlConnection(_connectionString))
			{
				dbConnection.Open();
				return dbConnection.ExecuteScalar<T>(query, parameters);
			}
		}

		public DataTable ExecuteQuery(string query, object parameters = null)
		{
			using (IDbConnection dbConnection = new SqlConnection(_connectionString))
			{
				dbConnection.Open();
				var dataTable = new DataTable();
				using (var reader = dbConnection.ExecuteReader(query, parameters))
				{
					dataTable.Load(reader);
				}
				return dataTable;
			}
		}

		public DataSet ExecuteQueryMultiple(string query, object parameters = null)
		{
			using (IDbConnection dbConnection = new SqlConnection(_connectionString))
			{
				dbConnection.Open();
				var dataSet = new DataSet();
				using (var reader = dbConnection.ExecuteReader(query, parameters))
				{
					do
					{
						var dataTable = new DataTable();
						dataTable.Load(reader);
						dataSet.Tables.Add(dataTable);
					} while (!reader.IsClosed && reader.NextResult());
				}
				return dataSet;
			}
		}

		public int Execute(string query, object parameters = null)
		{
			using (IDbConnection dbConnection = new SqlConnection(_connectionString))
			{
				dbConnection.Open();
				return dbConnection.Execute(query, parameters);
			}
		}

		public void ExecuteTransaction(List<string> queries, List<object> parametersList)
		{
			using (IDbConnection dbConnection = new SqlConnection(_connectionString))
			{
				dbConnection.Open();
				using (var transaction = dbConnection.BeginTransaction())
				{
					try
					{
						for (int i = 0; i < queries.Count; i++)
						{
							dbConnection.Execute(queries[i], parametersList[i], transaction);
						}
						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						throw;
					}
				}
			}
		}
	}
}
