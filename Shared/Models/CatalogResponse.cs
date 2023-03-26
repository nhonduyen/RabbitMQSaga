using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class CatalogResponse
    {
        public Guid OrderId { get; set; }
        public Guid CatalogId { get; set; }
        public bool IsSuccess { get; set; }
    }
}
