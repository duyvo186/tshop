using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TeaShop.Models.EntityFramework
{
    [Table("tb_ImportDetail")]
    public class ImportDetail
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ImportId { get; set; }
        public int ProductId { get; set; }
        public int UnitID { get; set; }
        public int SupplierID { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public string PackageNumber { get; set; }
        public DateTime? DateExpired { get; set; }

        public virtual Unit Unit { get; set; }
        public virtual Import Import { get; set; }
        public virtual Product Product { get; set; }
        public virtual Supplier Supplier { get; set; }

    }
}