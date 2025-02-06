using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmSaiServices.Worker;
using OmSaiModels.Worker;
using Microsoft.Data.SqlClient;
using System.Data;
using LmsServices.Common;
using OmSaiServices.Common;
using OmSaiServices.Worker.Interfaces;


namespace OmSaiServices.Worker.Implementations
{
	public class BankDetailService : Repository<BankDetailModel>, IBankDetailService
	{
		private readonly string usp_cud;
		private readonly string usp_r;
		private readonly Mapper _mapper;

		public BankDetailService()
		{
			usp_cud = "usp_CreateUpdateDeleteRestore_BankDetails";
			usp_r = "usp_GetAll_BankDetails";
			_mapper = new Mapper();
		}
		public int Create(BankDetailModel model)
		{
			return Create(model, usp_cud, CreateUpdate(model, "create"));
		}

		public void Update(BankDetailModel model)
		{
			Update(model, usp_cud, CreateUpdate(model, "update"));
		}


		public void Delete(int id)
		{
			Delete(usp_cud, DeleteRestore(id));
		}

		public void Restore(int id)
		{
			Restore(usp_cud, DeleteRestore(id));

		}

		public List<BankDetailModel> GetAll(int? Id = null, int? WorkerId = null, int? SiteId = null)
		{

			// Define the mapping function
			var mapEntity = new Func<IDataReader, BankDetailModel>(reader => _mapper.MapEntity<BankDetailModel>(reader));

			return GetAll(usp_r, mapEntity, GetParams(Id, WorkerId, SiteId));
		}


		public List<BankDetailModel> GetAllByWorkerId(int id)
		{

			// Define the mapping function
			var mapEntity = new Func<IDataReader, BankDetailModel>(reader => _mapper.MapEntity<BankDetailModel>(reader));

			return GetAll(usp_r, mapEntity, GetParams(null, id));
		}


		public BankDetailModel GetById(int id)
		{
			// Define the mapping function
			var mapEntity = new Func<IDataReader, BankDetailModel>(reader => _mapper.MapEntity<BankDetailModel>(reader));

			return GetById(id, usp_r, mapEntity, GetParams(id));
		}


		private List<KeyValuePair<string, object>> CreateUpdate(BankDetailModel model, string type)
		{

			var BankDetailId = type == "create" ? 0 : model.BankDetailId;

			return new List<KeyValuePair<string, object>>
			{
				new("@BankDetailId", BankDetailId),
				new("@WorkerId", model.WorkerId),
				new("@AccountNumber", model.AccountNumber),
				new("@IfscCode", model.IfscCode),
				new("@BankName", model.BankName),
				new("@BranchName", model.BranchName),				
				new("@BankProofType", model.BankProofType),
				new("@BankProofImage", model.BankProofImage),
				new("@AccountHolderName", model.AccountHolderName),
				new("@IsVerified", model.IsVerified),
				//new("@CreatedAt", model.CreatedAt),
				//new("@UpdatedAt", model.UpdatedAt)
			};
		}

		private List<KeyValuePair<string, object>> DeleteRestore(int id)
		{

			return new List<KeyValuePair<string, object>>
			{
				new("@BankDetailId", id),

			};

		}

		private SqlParameter[] GetParams(int? id = null, int? WorkerId = null, int? SiteId = null)
		{
			return new SqlParameter[]
			{
				new SqlParameter("@BankDetailId", id),
				new SqlParameter("@WorkerId", WorkerId),
				new SqlParameter("@SiteId", SiteId)
			};
		}

	}
}
