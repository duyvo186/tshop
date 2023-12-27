using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TeaShop.Models.EntityFramework
{
    [Table("tb_Import")]
    public class Import: CommonAbstract
    {
        public Import()
        {
            this.ImportDetails = new HashSet<ImportDetail>();
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        public string WareName { get; set; }
        public int Quantity { get; set; }
        //public int Supplier_Id { get; set; }
        public decimal? Amount { get; set; }
       
        public DateTime? DateImport { get; set; }

        public virtual ICollection<ImportDetail> ImportDetails { get; set; }
     
    }
}