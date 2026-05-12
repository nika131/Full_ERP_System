using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IPdfExportService
    {
        void GenerateInvoice(InventoryTransaction transaction, string filePath);
    }
}
