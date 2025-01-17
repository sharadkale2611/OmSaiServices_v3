﻿using OmSaiModels.Worker;
using OmSaiServices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmSaiServices.Worker.Interfaces
{
    public  interface IWorkerAddressService:IRepository<WorkerAddressModel>
    {
		public List<WorkerAddressModel> GetByWorkerId(int id);
       
    }
}
