using LmsServices.Common;
using Microsoft.Data.SqlClient;
using OmSaiModels.Worker;
using OmSaiServices.Worker.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OmSaiServices.Worker.Implementations
{
	public class WorkerAttendanceService : IWorkerAttendanceService
	{
		private readonly Mapper _mapper;

		public WorkerAttendanceService()
		{
			_mapper = new Mapper();
		}


		public ValidateAttendanceModel ValidateAttendance(int WorkerId)
		{
			// Define the input and output parameters
			var parameters = new List<SqlParameter>
			{
				new SqlParameter("@WorkerId", SqlDbType.Int) { Value = WorkerId },
				new SqlParameter("@InOutType", SqlDbType.NVarChar, 10) { Direction = ParameterDirection.Output },
				new SqlParameter("@OldStatus", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output },
				new SqlParameter("@Remark", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output },
				new SqlParameter("@IsAttendanceAllowed", SqlDbType.Bit) { Direction = ParameterDirection.Output },
				new SqlParameter("@NewStatus", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output },
				new SqlParameter("@StatusCode", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output },
				new SqlParameter("@TimeDiffInMinutes", SqlDbType.Int) { Direction = ParameterDirection.Output },

				new SqlParameter("@SiteId", SqlDbType.Int) { Direction = ParameterDirection.Output },
				new SqlParameter("@SiteShiftId", SqlDbType.Int) { Direction = ParameterDirection.Output },
				new SqlParameter("@GpsLocation", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output },
				
			};

			// Execute the stored procedure and retrieve the output values and result set
			var outputs = QueryService.QueryOutputs("usp_ValidateAttendanceByWorkerID", parameters, parameters.Select(p => p.ParameterName).ToList());

			// Initialize AttendanceRecords list
			var attendanceRecords = new List<AttendanceRecord>();

			// Check if the result contains tables (AttendanceRecords) and extract the data
			if (outputs.ContainsKey("AttendanceRecords") && outputs["AttendanceRecords"] is DataTable)
			{
				var table = (DataTable)outputs["AttendanceRecords"];
				foreach (DataRow row in table.Rows)
				{
					attendanceRecords.Add(new AttendanceRecord
					{
						WorkerAttendanceId = Convert.ToInt32(row["WorkerAttendanceId"]),
						WorkerId = Convert.ToInt32(row["WorkerId"]),
						VerifyBy = row["VerifyBy"].ToString(),
						SiteId = Convert.ToInt32(row["SiteId"]),
						ShiftId = Convert.ToInt32(row["ShiftId"]),
						InTime = Convert.ToDateTime(row["InTime"]),
						OutTime = Convert.ToDateTime(row["OutTime"]),
						InSelfiPath = row["InSelfiPath"].ToString(),
						OutSelfiPath = row["OutSelfiPath"].ToString(),
						InGeoLocation = row["InGeoLocation"].ToString(),
						OutGeoLocation = row["OutGeoLocation"].ToString(),
						LateIn = Convert.ToBoolean(row["LateIn"]),
						EarlyOut = Convert.ToBoolean(row["EarlyOut"]), 
						Status = row["Status"].ToString(),
						CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
						IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
						DeletedAt = row["DeletedAt"] != DBNull.Value ? Convert.ToDateTime(row["DeletedAt"]) : (DateTime?)null
					});
				}
			}

			// Map and return the model in a concise way
			return new ValidateAttendanceModel
			{
				InOutType = outputs.ContainsKey("@InOutType") && outputs["@InOutType"] != DBNull.Value
							? outputs["@InOutType"]?.ToString()
							: null,
				OldStatus = outputs.ContainsKey("@OldStatus") && outputs["@OldStatus"] != DBNull.Value
							? outputs["@OldStatus"]?.ToString()
							: null,
				Remark = outputs.ContainsKey("@Remark") && outputs["@Remark"] != DBNull.Value
						 ? outputs["@Remark"]?.ToString()
						 : null,
				IsAttendanceAllowed = outputs.ContainsKey("@IsAttendanceAllowed") && outputs["@IsAttendanceAllowed"] != DBNull.Value &&
									  Convert.ToBoolean(outputs["@IsAttendanceAllowed"]),
				NewStatus = outputs.ContainsKey("@NewStatus") && outputs["@NewStatus"] != DBNull.Value
							? outputs["@NewStatus"]?.ToString()
							: null,
				StatusCode = outputs.ContainsKey("@StatusCode") && outputs["@StatusCode"] != DBNull.Value
							 ? outputs["@StatusCode"]?.ToString()
							 : null,
				TimeDiffInMinutes = outputs.ContainsKey("@TimeDiffInMinutes") && outputs["@TimeDiffInMinutes"] != DBNull.Value
									? Convert.ToInt32(outputs["@TimeDiffInMinutes"])
									: (int?)null, // Nullable int

				SiteId = outputs.ContainsKey("@SiteId") && outputs["@SiteId"] != DBNull.Value
									? Convert.ToInt32(outputs["@SiteId"])
									: (int?)null, // Nullable int

				SiteShiftId = outputs.ContainsKey("@SiteShiftId") && outputs["@SiteShiftId"] != DBNull.Value
									? Convert.ToInt32(outputs["@SiteShiftId"])
									: (int?)null, // Nullable int

				GpsLocation = outputs.ContainsKey("@GpsLocation") && outputs["@GpsLocation"] != DBNull.Value
							? outputs["@GpsLocation"]?.ToString()
							: null,

				AttendanceRecords = attendanceRecords // Add AttendanceRecords as a list
			};

		}

		public void ManageAttendance(WorkerAttendanceModel model )
		{

			// Use DateTime.Now if CurrentTime is null
			model.CurrentTime ??= DateTime.Now;

			var parameters =  new List<KeyValuePair<string, object>>
			{

				new("@WorkerId", model.WorkerId),
				new("@SiteId", model.SiteId),
				new("@ShiftId", model.ShiftId), 
				new("@Status", model.Status),
				new("@SelfieImage", model.SelfieImage),
				new("@GeoLocation", model.GeoLocation),
				new("@CurrentTime", model.CurrentTime),
				new("@InOutType", model.InOutType)
			};

			QueryService.NonQuery("usp_ManageWorkerAttendance", parameters);
		}


		public List<WorkerAttendanceViewModel> GetAll(WorkerAttendanceFilter filter)
		{
			var mapEntity = new Func<IDataReader, WorkerAttendanceViewModel>(reader => _mapper.MapEntity<WorkerAttendanceViewModel>(reader));

			var parameters = new[]
			   {
					new SqlParameter("@WorkerId", filter.WorkerId ?? (object)DBNull.Value),
					new SqlParameter("@SiteId", filter.SiteId ?? (object)DBNull.Value),
					new SqlParameter("@CurrentDate", filter.CurrentDate),
					new SqlParameter("@WorkmanId", filter.WorkmanId),
					new SqlParameter("@RecordCount", filter.RecordCount),

					new SqlParameter("@Month", filter.Month ?? (object)DBNull.Value),
					new SqlParameter("@Year", filter.Year ?? (object)DBNull.Value),
					new SqlParameter("@StartMonth", filter.StartMonth ?? (object)DBNull.Value),
					new SqlParameter("@EndMonth", filter.EndMonth ?? (object)DBNull.Value),
					new SqlParameter("@StartYear", filter.StartYear ?? (object)DBNull.Value),
					new SqlParameter("@EndYear", filter.EndYear ?? (object)DBNull.Value),
					new SqlParameter("@StartDate", filter.StartDate),
					new SqlParameter("@EndDate", filter.EndDate),


				};

			return QueryService.Query("usp_GetAttendanceData", mapEntity, parameters);
		}


		public List<WorkerAttendanceLedgerModel> GetLedger(int? WorkerId = null, int? SiteId = null, int? SiteShiftId = null, int? Year = null, int? Month = null)
		{
			var mapEntity = new Func<IDataReader, WorkerAttendanceLedgerModel>(reader => _mapper.MapEntity<WorkerAttendanceLedgerModel>(reader));


			var parameters = new[]
			   {
					new SqlParameter("@WorkerId", WorkerId ?? (object)DBNull.Value),
					new SqlParameter("@SiteId", SiteId ?? (object)DBNull.Value),
					new SqlParameter("@SiteShiftId", SiteShiftId ?? (object)DBNull.Value),
					new SqlParameter("@Year", Year),
					new SqlParameter("@Month", Month)
				};

			return QueryService.Query("usp_GetAll_AttendanceLedger", mapEntity, parameters);
		}

		public void CreateLedger( int? SiteId = null, int? SiteShiftId = null, int? Year = null, int? Month = null)
		{

			var parameters = new List<KeyValuePair<string, object>>
			{
				new("@SiteId", SiteId ?? (object)DBNull.Value),
				new("@SiteShiftId", SiteShiftId ?? (object)DBNull.Value),
				new("@Year", Year),
				new("@Month", Month)

			};

			 QueryService.NonQuery("usp_GenerateAttendanceLedger",  parameters);
		}


		//TO GET WORKER MANUAL ATTENDANCE
		public List<WorkerAttendanceDetailsViewModel> GetWorkerAttendanceDetails(int? SiteId = null)
		{
			var mapEntity = new Func<IDataReader, WorkerAttendanceDetailsViewModel>(reader => _mapper.MapEntity<WorkerAttendanceDetailsViewModel>(reader));

		
			var parameters = new[]
			   {
					new SqlParameter("@SiteId", SiteId ?? (object)DBNull.Value)
				};
			// Execute stored procedure and return results
			return QueryService.Query("usp_GetWorkerAttendanceDetails", mapEntity, parameters);
		}

		// Method to create or update worker attendance
		public void CreateOrUpdateWorkerAttendance(int workerId, DateTime inTime, DateTime outTime, string status)
		{
			using (var connection = new SqlConnection("Server=A2NWPLSK14SQL-v04.shr.prod.iad2.secureserver.net;Database=om_sai_services_db_v1;User Id=omuser;Password=YSAAS#2024;TrustServerCertificate=True"))
			{
				using (var command = new SqlCommand("usp_CreateUpdateManualWorkerAttendance", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@WorkerId", workerId);
					command.Parameters.AddWithValue("@InTime", inTime);
					command.Parameters.AddWithValue("@OutTime", outTime);
					command.Parameters.AddWithValue("@Status", status);

					connection.Open();
					command.ExecuteNonQuery();
				}
			}
		}


	}
}
