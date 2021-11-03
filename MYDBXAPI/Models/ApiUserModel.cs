using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MYDBXAPI.Models
{
    public class ApiUserModel
    {
        public string name { get; set; }
        public string email { get; set; }
        public string photourl { get; set; }
        
        public ApiUserModel(string name, string email,string photourl)
        {
            this.name = name;
            this.email = email;
            this.photourl = photourl;
            
        }

        public ApiUserModel()
        {
        }
    }
}
