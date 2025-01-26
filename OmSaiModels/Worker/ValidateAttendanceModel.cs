using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmSaiModels.Worker
{
	public class ValidateAttendanceModel
	{
		public string? InOutType { get; set; }
		public string? OldStatus { get; set; }
		public string Remark { get; set; }
		public bool IsAttendanceAllowed { get; set; }
		public string? NewStatus { get; set; }
		public string? StatusCode { get; set; }
		public int? TimeDiffInMinutes { get; set; }
		public int? SiteId { get; set; }
		public int? SiteShiftId { get; set; }
		public string? GpsLocation { get; set; }
		
		public List<AttendanceRecord> AttendanceRecords { get; set; }  // Change to List<AttendanceRecord>

	}
}
