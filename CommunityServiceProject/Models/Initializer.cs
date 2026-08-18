using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.DAL
{
    public class Initializer : CreateDatabaseIfNotExists<Community>
    {
        protected override void Seed(Community context)
        {
            base.Seed(context);
        }
    }
}