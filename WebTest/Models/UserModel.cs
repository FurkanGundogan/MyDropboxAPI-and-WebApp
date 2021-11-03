using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTest.Models
{
    public class UserModel
    {
        public string email { get; set; }
        public string name { get; set; }
        public string photourl { get; set; }
       
        public UserModel(string email, string name, string photourl)
        {
            this.email = email;
            this.name = name;
            this.photourl = photourl;
            
        }
    }
}