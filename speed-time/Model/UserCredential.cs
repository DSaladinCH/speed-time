using DSaladin.SpeedTime.Integrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model
{
    [Table("UserCredential")]
    public class UserCredential
    {
        [Key]
        public ServiceType ServiceType { get; set; }
        public byte[] Credential { get; set; }
        public string ServiceUri { get; set; }
    }
}
