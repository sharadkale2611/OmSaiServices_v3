using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmSaiModels.Worker
{
	public class BankDetailModel
	{
		[Key]
		public int BankDetailId { get; set; }

		[Required]
		public int WorkerId { get; set; }
		public string? WorkerName { get; set; }

		[Required]
		public string AccountNumber { get; set; }

		[Required]
		public string IfscCode { get; set; }

		[Required]
		public string BankName { get; set; }

		[Required]
		public string BranchName { get; set; }
		public string BankProofType { get; set; }
		public string BankProofImage { get; set; }
		public string AccountHolderName { get; set; }
		public bool IsVerified { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }


	}
}
